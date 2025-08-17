using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using SleepNeed.Systems;
using SleepNeed.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;
using static HarmonyLib.Code;

namespace SleepNeed.Energy
{
    
    public class EntityBehaviorEnergy : EntityBehavior
    {
        
        public float GetEnergySpeedModifier
        {
            get
            {
                if (ConfigSystem.ConfigServer.EnergySpeedModifier != 0f)
                {
                    return ConfigSystem.ConfigServer.EnergySpeedModifier;
                }
                return GlobalConstants.HungerSpeedModifier;
            }
        }

        
        public override string PropertyName()
        {
            return this.AttributeKey;
        }

        
        private string AttributeKey
        {
            get
            {
                return BtCore.Modid + ":energy";
            }
        }

        
        public float EnergyLossDelay
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("energylossdelay", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("energylossdelay", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                
            }
        }

        
        public float CurrentEnergy
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                return Math.Min((energyTree != null) ? energyTree.GetFloat("currentenergylevel", 0f) : this.MaxEnergy, this.MaxEnergy);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("currentenergylevel", Math.Clamp(value, 0f, this.MaxEnergy));
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                


            }
        }

        
        public float MaxEnergyModifier
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("maxenergymodifier", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("maxenergymodifier", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                
            }
        }

        
        public float MaxEnergy
        {
            get
            {
                float maxEnergy = (float)Math.Round((double)(ConfigSystem.ConfigServer.MaxEnergy + ((this.MaxEnergyModifier * ConfigSystem.ConfigServer.MaxEnergy) / 1.5)));
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("maxenergy", maxEnergy);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                return maxEnergy;
            }
        }

        
        public float Invigorated
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("invigorated", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("invigorated", Math.Clamp(value, 0f, ConfigSystem.ConfigServer.MaxEnergy));
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                


            }
        }

        
        public float Energyloss
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("energyloss", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("energyloss", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                
            }
        }

        
        public EntityBehaviorEnergy(Entity entity) : base(entity)
        {
            this._entityAgent = (entity as EntityAgent);
        }

        
        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            this._energyTree = this.entity.WatchedAttributes.GetTreeAttribute(this.AttributeKey);
            this._api = this.entity.World.Api;
            EntityBehaviorHealth healthBehavior = this.entity.GetBehavior<EntityBehaviorHealth>();
            if (this._energyTree == null || this._energyTree.GetFloat("maxenergy", 0f) == 0f)
            {
                this.entity.WatchedAttributes.SetAttribute(this.AttributeKey, this._energyTree = new TreeAttribute());
                this.MaxEnergyModifier = typeAttributes["maxenergymodifier"].AsFloat(0f);
                this.CurrentEnergy = Math.Min(typeAttributes["currentenergylevel"].AsFloat(this.MaxEnergy), this.MaxEnergy);
                
                this.EnergyLossDelay = 0f;
                this.Invigorated = 0f;
                this.Energyloss = 0f;
                
            }
            this.lungCapacity = this.entity.World.Config.GetAsInt("lungCapacity", 40000);
            this.num3 = this.GetEnergySpeedModifier / 15f; // Prøv at tweake med denne faktor (SE HEEEER!!!)
            this.num4 = this.GetEnergySpeedModifier / 5f;
            // Play with tickrate, it impacts how often the energy is updated. It has an impact on how it registers the hoursPassed for energy consumption.
            if (this.entity is EntityPlayer)
            {
                this._energylistenerId = this.entity.World.RegisterGameTickListener(new Action<float>(this.SlowTick), 600, 0);
            }
            this.entity.Stats.Register(BtCore.Modid + ":energyrate", (EnumStatBlendType)2);
            this.UpdateEnergyBoosts();




        }

        
        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            this.entity.World.UnregisterGameTickListener(this._energylistenerId);
        }

        
        public override void DidAttack(DamageSource source, EntityAgent targetEntity, ref EnumHandling handled)
        {
            this.ConsumeEnergy(100f * ConfigSystem.ConfigServer.AttackEnergyCostModifier);
        }

        
        public virtual void ConsumeEnergy(float amount)
        {
            this.ReduceEnergy(amount / 10f);
        }



        
        public void ReceiveEnergyBySleeping(Entity entity)
        {
            // Skal måske fjerne "if (entity == null) return;"
            if (entity == null) return;
            var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
            if (sleepiness == null || !sleepiness.IsSleepingNow)
            {
                return;
            }
            if (sleepiness.LastHoursPassed <= 0f) return; // Only restore if time has passed
            
            bool wasFull = this.CurrentEnergy >= this.MaxEnergy;
            this.SleepRatio = 1f - sleepiness.SleepinessRatio;
            this.EnergyRestored = sleepiness.LastHoursPassed * ((this.MaxEnergy * 0.08f) * (ConfigSystem.ConfigServer.EnergyRestoredBySleepingModifier + this.SleepRatio)); // Adjust multiplier as needed
            this.CurrentEnergy = Math.Clamp(this.CurrentEnergy + ((this.EnergyRestored + sleepiness.EnergyRestoredfromSleepiness) + ((this.EnergyRestored + sleepiness.EnergyRestoredfromSleepiness) * this.OverallHealthRatio)), 0f, this.MaxEnergy);
            if ((double)this.CurrentEnergy > 0.6 * (double)this.MaxEnergy && this.SleepRatio > 0.7f)
            {
                this.Invigorated = Math.Clamp(this.Invigorated + (((this.CurrentEnergy * 0.08f) * this.OverallHealthRatio) * this.SleepRatio), 0f, ConfigSystem.ConfigServer.MaxEnergy); 
            }
            if (!wasFull)
            {
                this.EnergyLossDelay = Math.Max(this.EnergyLossDelay, 10f);
            }
            
        }

        


        
        public override void OnGameTick(float deltaTime)
        {
            EntityPlayer player = this.entity as EntityPlayer;



            if (player != null && player.Player != null && this._entityAgent != null)
            {
                var invMan = player.Player.InventoryManager;
                if (invMan != null)
                {
                try
                {
                    EnumTool? activeTool = invMan.ActiveTool;
                    if (activeTool != null)
                    {
                        if ((this._entityAgent.Controls.LeftMouseDown && (activeTool == EnumTool.Warhammer || activeTool == EnumTool.Halberd || activeTool == EnumTool.Polearm || activeTool == EnumTool.Poleaxe || activeTool == EnumTool.Javelin || activeTool == EnumTool.Pike)))
                        {
                            this._heavyToolWorkCounter++;
                        }
                        if ((this._entityAgent.Controls.LeftMouseDown && (activeTool == EnumTool.Pickaxe || activeTool == EnumTool.Axe || activeTool == EnumTool.Shovel || activeTool == EnumTool.Hammer || activeTool == EnumTool.Sickle || activeTool == EnumTool.Hoe || activeTool == EnumTool.Saw || activeTool == EnumTool.Scythe)))
                        {
                            this._mediumToolWorkCounter++;
                        }
                        if ((this._entityAgent.Controls.LeftMouseDown && (activeTool == EnumTool.Knife || activeTool == EnumTool.Shears || activeTool == EnumTool.Chisel || activeTool == EnumTool.Sling || activeTool == EnumTool.Wrench || activeTool == EnumTool.Probe || activeTool == EnumTool.Meter || activeTool == EnumTool.Drill || activeTool == EnumTool.Firearm)))
                        {
                            this._lightToolWorkCounter++;
                        }
                        if ((this._entityAgent.Controls.LeftMouseDown && (activeTool == EnumTool.Sword || activeTool == EnumTool.Spear || activeTool == EnumTool.Bow || activeTool == EnumTool.Crossbow || activeTool == EnumTool.Shield || activeTool == EnumTool.Club || activeTool == EnumTool.Mace || activeTool == EnumTool.Staff)))
                        {
                            this._weaponToolWorkCounter++;
                        }
                    }
                }
                    catch (NullReferenceException e)
                    {
                        // Log the exception with more context
                        _api?.Logger.Error($"[SleepNeed] NullReferenceException in OnGameTick when accessing ActiveTool: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
            if (this._entityAgent != null)
            {
                if (this._entityAgent.Controls.LeftMouseDown || this._entityAgent.Controls.RightMouseDown)
                {
                    this._workCounter++;
                }
                if (this._entityAgent.Controls.TriesToMove)
                {
                    this._moveCounter++;
                }
                if (this._entityAgent.Controls.Sprint)
                {
                    this._sprintCounter++;
                }
                if (this._entityAgent.Controls.Jump)
                {
                    this._jumpCounter++;
                }
                if (this._entityAgent.Controls.FloorSitting)
                {
                    this._sittingCounter++;
                }
            }
                this._energyCounter += deltaTime;
                if ((double)this._energyCounter <= 10.0)
                {
                    return;
                }
                float num2 = this.entity.Api.World.Calendar.SpeedOfTime * this.entity.Api.World.Calendar.CalendarSpeedMul;
                
                
                if ((double)this._heavyToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (25.0 + (double)this._heavyToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.HeavyToolsEnergyCostModifier);
                }
                if ((double)this._mediumToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (14.0 + (double)this._mediumToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.MediumToolsEnergyCostModifier);
                }
                if ((double)this._lightToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (8.0 + (double)this._lightToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.LightToolsEnergyCostModifier);
                }
                if ((double)this._weaponToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (12.0 + (double)this._weaponToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.WeaponsEnergyCostModifier);
                }
                if ((double)this._workCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (6.0 + (double)this._workCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.NoToolsWorkEnergyCostModifier);
                }
                if ((double)this._moveCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (4.0 + (double)this._moveCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.MovementEnergyCostModifier);
                }
                if ((double)this._sprintCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (28.0 + (double)this._sprintCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.SprintingJumpingEnergyCostModifier);
                }
                if ((double)this._jumpCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (32.0 + (double)this._jumpCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.ConfigServer.SprintingJumpingEnergyCostModifier);
                }
                if ((double)this._sittingCounter > 0.0 && this.HotOrCold)
                {
                    this.RegainEnergy(this.num4 * (float)(1.2000000476837158 * (8.0 + (double)this._sittingCounter / 15.0) / 10.0) * 0.01f * num2);
                }
                else if ((double)this._sittingCounter > 0.0)
                {
                    // (* 1f * num2) Change 1f as desired. Above it is 0.1f instead of "energyrate"
                    this.RegainEnergy(this.num4 * (float)(1.2000000476837158 * (8.0 + (double)this._sittingCounter / 15.0) / 10.0) * 1f * num2);
                }
                this._energyCounter = 0;
                this._heavyToolWorkCounter = 0;
                this._mediumToolWorkCounter = 0;
                this._lightToolWorkCounter = 0;
                this._weaponToolWorkCounter = 0;
                this._workCounter = 0;
                this._moveCounter = 0;
                this._sprintCounter = 0;
                this._jumpCounter = 0;
                this._sittingCounter = 0;
                
        }

        
        private bool ReduceEnergy(float satLossMultiplier)
        {
            var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
            if (sleepiness != null && sleepiness.IsSleepingNow)
            {
                // Skip energy reduction while sleeping
                return false;
            }
            bool flag = false;
            satLossMultiplier *= this.GetEnergySpeedModifier;
            if (ConfigSystem.ConfigServer.EnableNutrientFactor && this.Starving) // Evt. tilføj ((double)this.CurrentEnergy < 0.2 * (double)this.MaxEnergy)
            {
                EntityBehaviorHunger nutrition = this.entity.GetBehavior<EntityBehaviorHunger>();
                if (nutrition != null)
                {
                    // Fruit
                    if (nutrition.FruitLevel > 0f)
                    {
                        nutrition.FruitLevel = Math.Max(0f, nutrition.FruitLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                    // Vegetable
                    if (nutrition.VegetableLevel > 0f)
                    {
                        nutrition.VegetableLevel = Math.Max(0f, nutrition.VegetableLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                    // Protein
                    if (nutrition.ProteinLevel > 0f)
                    {
                        nutrition.ProteinLevel = Math.Max(0f, nutrition.ProteinLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                    // Grain
                    if (nutrition.GrainLevel > 0f)
                    {
                        nutrition.GrainLevel = Math.Max(0f, nutrition.GrainLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                    // Dairy
                    if (nutrition.DairyLevel > 0f)
                    {
                        nutrition.DairyLevel = Math.Max(0f, nutrition.DairyLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                    if (this.Invigorated > 0f)
                    {
                        this.Invigorated = Math.Max(0f, this.Invigorated - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.ConfigServer.NutritionLossWhenStarvingModifier));
                    }
                }
            }
            if ((double)this.EnergyLossDelay > 0.0)
            {
                this.EnergyLossDelay -= 10f * satLossMultiplier;
                flag = true;
            }
            else if (this.CurrentEnergy < 0.3f * ConfigSystem.ConfigServer.MaxEnergy)
            {
                this.Invigorated = Math.Max(0f, this.Invigorated - ((satLossMultiplier * (this.EnergyRate * 10f)) * Math.Max(0.1f, (1f - this.OverallHealthRatio))));
                
            }
            if (flag)
            {
                this._energyCounter -= 10f;
                return true;
            }
            if ((double)this.CurrentEnergy > 0.0)
            {
                this.CurrentEnergy = Math.Max(0f, this.CurrentEnergy - (satLossMultiplier * (this.EnergyRate)));
                
            }
            return false;
        }

        
        private bool RegainEnergy(float satLossMultiplier)
        {
            bool flag = false;
            satLossMultiplier *= this.GetEnergySpeedModifier;
            
            if ((double)this.EnergyLossDelay > 0.0)
            {
                this.EnergyLossDelay -= 10f * satLossMultiplier;
                flag = true;
            }
            else if (this.CurrentEnergy > 0.6f * ConfigSystem.ConfigServer.MaxEnergy && this.SleepRatio > 0.7f)
            {
                this.Invigorated = Math.Max(0f, this.Invigorated + ((satLossMultiplier * this.OverallHealthRatio)) * ConfigSystem.ConfigServer.SittingRelaxingSpeedModifier);
            }
            if (flag)
            {
                this._energyCounter -= 10f;
                return true;
            }
            if ((double)this.CurrentEnergy >= 0.0)
            {
                this.CurrentEnergy = Math.Max(0f, this.CurrentEnergy + ((satLossMultiplier * (1f + this.OverallHealthRatio)) * ConfigSystem.ConfigServer.SittingRelaxingSpeedModifier));
                
            }
            return false;
        }

        
        public void UpdateEnergyBoosts()
        {
            this.UpdateEnergyStatBoosts();
            this.UpdateEnergyHealthBoost();
        }

        
        
        
        private void UpdateEnergyStatBoosts()
        {
            
            if (this.entity == null || this.entity.Stats == null) 
            {
                return; 
            }


            if (ConfigSystem.ConfigServer.HungerLevelMatters) 
            {
                if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                {
                    this.entity.Stats.Remove("hungerrate", "fatigue");
                }
                else if (this.EnergyRatio >= 0.7f)
                {
                    this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.ConfigServer.HungerRateReductionFromHighEnergy * ((1f - 2f * (this.EnergyRatioHighEnergy))), false);
                }
                else if (this.EnergyRatio <= 0.3f)
                {
                    this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.ConfigServer.HungerRateGainFromLowEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy))), false);                    
                }
            }
            var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
            if (sleepiness != null)
            {
                bool isRefreshed = sleepiness.SleepinessRatio <= sleepiness.RefreshedThreshold;
                bool notRefreshed = sleepiness.SleepinessRatio >= sleepiness.RefreshedThreshold;
                if (ConfigSystem.ConfigServer.EnableEnergyDependedToolMiningSpeed) 
                {

                    
                        if (isRefreshed && this.EnergyRatio >= 0.7f)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", ((ConfigSystem.ConfigServer.ToolMiningSpeedBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                        {
                            this.entity.Stats.Remove("miningSpeedMul", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= 0.7f)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.ConfigServer.ToolMiningSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= 0.3f)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.ConfigServer.ToolMiningSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    
                
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedJumpHeight) 
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.EnergyJumpBoostStat = 0f;
                        this.entity.Stats.Set("jumpHeightMul", "fatigue", ((ConfigSystem.ConfigServer.JumpHeightBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                        this.EnergyJumpBoostStat = 0f;
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.EnergyJumpBoostStat = 0f;
                        this.entity.Stats.Set("jumpHeightMul", "fatigue", (ConfigSystem.ConfigServer.JumpHeightBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false); // this.EnergyRatioHighEnergy = Math.Clamp(0.5f * (this.CurrentEnergy - (0.7f * this.MaxEnergy)) / (0.3f * this.MaxEnergy) + 0.5f, 0.5f, 1f);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                        this.EnergyJumpBoostStat = (ConfigSystem.ConfigServer.JumpHeightDebuffFromEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy)))); // this.EnergyRatioLowEnergy = Math.Clamp((0.5f / (0.3f * this.MaxEnergy)) * this.CurrentEnergy, 0f, 0.5f);
                    }

                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedWalkSpeed) 
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("walkspeed", "fatigue", ((ConfigSystem.ConfigServer.WalkSpeedBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("walkspeed", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.ConfigServer.WalkSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.ConfigServer.WalkSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }

                    
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedRangedWeaponSpeed) 
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", ((ConfigSystem.ConfigServer.RangedWeaponSpeedBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("rangedWeaponsSpeed", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedRangedWeaponDamage)
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", ((ConfigSystem.ConfigServer.RangedWeaponDamageBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("rangedWeaponsDamage", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedMeleeWeaponDamage)
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", ((ConfigSystem.ConfigServer.MeleeWeaponDamageBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("meleeWeaponsDamage", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.MeleeWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.MeleeWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedArmorWalkSpeedAffectedness)
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", ((ConfigSystem.ConfigServer.ArmorWalkSpeedAffectednessBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("armorWalkSpeedAffectedness", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.ConfigServer.ArmorWalkSpeedAffectednessBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.ConfigServer.ArmorWalkSpeedAffectednessDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedBowDrawingStrength)
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("bowDrawingStrength", "fatigue", ((ConfigSystem.ConfigServer.BowDrawingStrengthBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("bowDrawingStrength", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.ConfigServer.BowDrawingStrengthBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.ConfigServer.BowDrawingStrengthDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedAnimalHarvestingTime)
                {
                    if (isRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("animalHarvestingTime", "fatigue", ((ConfigSystem.ConfigServer.AnimalHarvestingTimeBoostFromEnergy * ConfigSystem.ConfigServer.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if ((this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > 0.3f))
                    {
                        this.entity.Stats.Remove("animalHarvestingTime", "fatigue");
                    }
                    else if (notRefreshed && this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.ConfigServer.AnimalHarvestingTimeBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.ConfigServer.AnimalHarvestingTimeDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
            }
            // If Sleepiness is disabled
            else
            {
                if (ConfigSystem.ConfigServer.EnableEnergyDependedToolMiningSpeed)
                {


                    
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("miningSpeedMul", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.ConfigServer.ToolMiningSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.ConfigServer.ToolMiningSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedJumpHeight)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("jumpHeightMul", "fatigue", (ConfigSystem.ConfigServer.JumpHeightBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.EnergyJumpBoostStat = (ConfigSystem.ConfigServer.JumpHeightDebuffFromEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy))));
                    }

                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedWalkSpeed)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("walkspeed", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.ConfigServer.WalkSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.ConfigServer.WalkSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedRangedWeaponSpeed)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("rangedWeaponsSpeed", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedRangedWeaponDamage)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("rangedWeaponsDamage", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.RangedWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }


                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedMeleeWeaponDamage)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("meleeWeaponsDamage", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.MeleeWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.ConfigServer.MeleeWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedArmorWalkSpeedAffectedness)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("armorWalkSpeedAffectedness", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.ConfigServer.ArmorWalkSpeedAffectednessBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.ConfigServer.ArmorWalkSpeedAffectednessDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedBowDrawingStrength)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("bowDrawingStrength", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.ConfigServer.BowDrawingStrengthBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.ConfigServer.BowDrawingStrengthDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
                if (ConfigSystem.ConfigServer.EnableEnergyDependedAnimalHarvestingTime)
                {
                    if (this.EnergyRatio > 0.3f && this.EnergyRatio < 0.7f)
                    {
                        this.entity.Stats.Remove("animalHarvestingTime", "fatigue");
                    }
                    else if (this.EnergyRatio >= 0.7f)
                    {
                        this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.ConfigServer.AnimalHarvestingTimeBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                    }
                    else if (this.EnergyRatio <= 0.3f)
                    {
                        this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.ConfigServer.AnimalHarvestingTimeDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                    }
                }
            }


        }
        
        public void UpdateEnergyHealthBoost()
        {
            EntityBehaviorBreathe breatheBehavior = this.entity.GetBehavior<EntityBehaviorBreathe>();
            
            
                if (breatheBehavior != null && ConfigSystem.ConfigServer.EnableInvigoratedLungCapacityBoost)
                {
                    float lungBoost = this.lungCapacity + this.BoostLungCapacity;
                    if (lungBoost != breatheBehavior.MaxOxygen)
                    {
                    breatheBehavior.MaxOxygen = lungBoost;
                    }
                    if (breatheBehavior.HasAir && breatheBehavior.Oxygen > breatheBehavior.MaxOxygen)
                    {
                       breatheBehavior.Oxygen = Math.Clamp(breatheBehavior.Oxygen, 0f, breatheBehavior.MaxOxygen); // Ensure oxygen does not exceed max capacity
                    }
                    
                }
                else if (breatheBehavior != null && !ConfigSystem.ConfigServer.EnableInvigoratedLungCapacityBoost)
                {
                    if (breatheBehavior.MaxOxygen != this.lungCapacity)
                    {
                        breatheBehavior.MaxOxygen = this.lungCapacity;
                    }
                }

                if (this.entity.Stats != null && ConfigSystem.ConfigServer.EnableInvigoratedHealthBoost)
                {
                    this.entity.Stats.Set("maxhealthExtraPoints", BtCore.Modid + ":energyhealth", this.BoostHealthPoints, false);
                }
                else if (this.entity.Stats != null && !ConfigSystem.ConfigServer.EnableInvigoratedHealthBoost)
                {
                    this.entity.Stats.Remove("maxhealthExtraPoints", BtCore.Modid + ":energyhealth");
                }

                if (this.entity.Stats != null && ConfigSystem.ConfigServer.EnableInvigoratedHealingEffectiveness)
                {
                    this.entity.Stats.Set("healingeffectivness", BtCore.Modid + ":energyhealth", this.BoostHealingEffectivness, false);
                }
                else if (this.entity.Stats != null && !ConfigSystem.ConfigServer.EnableInvigoratedHealingEffectiveness)
                {
                    this.entity.Stats.Remove("healingeffectivness", BtCore.Modid + ":energyhealth");
                }
                
        }


        // Token: 0x06000065 RID: 101 RVA: 0x00004BC8 File Offset: 0x00002DC8
        private void SlowTick(float dt)
        {
            EntityBehaviorHunger hunger = this.entity.GetBehavior<EntityBehaviorHunger>();
            this.EnergyRate = this.entity.Stats.GetBlended("energyrate");
            this.EnergyRatio = (this.MaxEnergy > 0f) ? (this.CurrentEnergy / ConfigSystem.ConfigServer.MaxEnergy) : 0f;
            this.InvigoRatio = this.Invigorated / ConfigSystem.ConfigServer.MaxEnergy;
            var healthBehavior = this.entity.GetBehavior<EntityBehaviorHealth>();
            if (healthBehavior != null && healthBehavior.MaxHealthModifiers != null)
            {
                healthBehavior.MaxHealthModifiers.TryGetValue("nutrientHealthMod", out this.NutrientHealthMod);
                
            }

            if (ConfigSystem.ConfigServer.EnableNutrientFactor && this.InvigoRatio > 0f)
            {
                this.InvigoratedNutrientRatio = (this.InvigoRatio + (this.NutrientHealthMod / 12.5f)) / 2f;
            }
            else
            {
                this.InvigoratedNutrientRatio = this.InvigoRatio;
            }


            if (ConfigSystem.ConfigServer.EnableNutrientFactor)
            {
                this.InvigoratedNutrientRatioForHunger = (this.InvigoRatio + (this.NutrientHealthMod / 12.5f)) / 2f;
                this.NutrientRatio = this.NutrientHealthMod / 12.5f;
            }

            if (this.CurrentEnergy <= 0f)
            {
                this.Fatigued = true;
            }
            else
            {
                this.Fatigued = false;
            }

            if (hunger != null)
            {
                this.SatRatio = hunger.Saturation / hunger.MaxSaturation;
                if (ConfigSystem.ConfigServer.HungerLevelMatters)
                {
                    if (hunger.Saturation <= 0f)
                    {
                        this.Starving = true;
                        if (this.NutrientRatio <= 0f)
                        {
                            this.Famine = true;
                        }


                    }
                    else if (hunger.Saturation > 0f)
                    {
                        this.Starving = false;
                        this.Famine = false;
                    }
                    if (this.SatRatio <= ConfigSystem.ConfigServer.HungerEnergyrateDebuffStartRatio)
                    {
                        float lowHungerRatio = 1f - (Math.Clamp((1f / (ConfigSystem.ConfigServer.HungerEnergyrateDebuffStartRatio * hunger.MaxSaturation)) * hunger.Saturation, 0f, 1f));
                        float lowHungerEnergyrate = ((ConfigSystem.ConfigServer.HungerEnergyrateDebuff / 100f) - 1f) * lowHungerRatio;
                        this.entity.Stats.Set(BtCore.Modid + ":energyrate", "hungryrate", lowHungerEnergyrate, false);
                    }
                    else
                    {
                        this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "hungryrate");
                    }

                }
                else if (!ConfigSystem.ConfigServer.HungerLevelMatters)
                {
                    this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "hungryrate");
                }
            }
            this.HealthRatio = Math.Clamp(healthBehavior.Health, 0f, healthBehavior.BaseMaxHealth) / healthBehavior.BaseMaxHealth;
            this.OverallHealthRatio = ((this.InvigoRatio + this.NutrientRatio) / 2f) * this.HealthRatio;
            this.EnergyRatioHighEnergy = Math.Clamp(0.5f * (this.CurrentEnergy - (0.7f * ConfigSystem.ConfigServer.MaxEnergy)) / (0.3f * ConfigSystem.ConfigServer.MaxEnergy) + 0.5f, 0.5f, 1f);
            this.EnergyRatioHighEnergyPart2 = (1f - 2f * (1f - this.EnergyRatioHighEnergy));
            this.EnergyRatioLowEnergy = Math.Clamp((0.5f / (0.3f * ConfigSystem.ConfigServer.MaxEnergy)) * this.CurrentEnergy, 0f, 0.5f);
            this.EnergyRatioLowEnergyPart2 = (1f - 2f * (1f - this.EnergyRatioLowEnergy));
            this.BoostLungCapacity = (ConfigSystem.ConfigServer.InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity * (int)(this.lungCapacity / 100)) * (this.OverallHealthRatio * 100f);
            this.BoostHealthPoints = ((ConfigSystem.ConfigServer.InvigoratedHealthBoostPercentageOfMaxHealth * healthBehavior.BaseMaxHealth) / 100f) * (this.OverallHealthRatio * 100f);
            this.BoostHealingEffectivness = this.OverallHealthRatio * ConfigSystem.ConfigServer.InvigoratedHealingEffectivenessModifier;
            if (ConfigSystem.ConfigServer.EnableInvigoratedMaxEnergyBoost)
            {
                this.MaxEnergyModifier = this.OverallHealthRatio * ConfigSystem.ConfigServer.InvigoratedMaxEnergyBoostModifier;
                
            }
            else
            {
                this.MaxEnergyModifier = 0f;
            }
            
            if (ConfigSystem.ConfigServer.EnableEnergyloss && this.Energyloss > 0f)
            {
                this.entity.Stats.Set(BtCore.Modid + ":energyrate", "energyloss", this.Energyloss, false);
                this.Energyloss = Math.Max(0f, this.Energyloss - 0.02f * this.EnergyRatio * (float)(((double)Math.Abs(this.CurrentEnergy - this.MaxEnergy) < 0.0001) ? 5 : 1));
            }
            else
            {
                this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "energyloss");
            }

            EntityBehaviorBodyTemperature tempBehavior = this.entity.GetBehavior<EntityBehaviorBodyTemperature>();
            if ((ConfigSystem.ConfigServer.BodyTemperatureMatters && tempBehavior != null) && (tempBehavior.CurBodyTemperature < tempBehavior.NormalBodyTemperature || tempBehavior.CurBodyTemperature > tempBehavior.NormalBodyTemperature + 8.0f))
            {
                this.HotOrCold = true;
            }
            else
            {
                this.HotOrCold = false;
            }

            if ((ConfigSystem.ConfigServer.BodyTemperatureMatters && tempBehavior != null) && this.HotOrCold)
            {
                if (tempBehavior.CurBodyTemperature < tempBehavior.NormalBodyTemperature)
                {
                    this.TemperatureDifference = Math.Max(0f, (tempBehavior.NormalBodyTemperature - tempBehavior.CurBodyTemperature) * 6f);

                }
                else if (tempBehavior.CurBodyTemperature > tempBehavior.NormalBodyTemperature + 8.0f)
                {
                    this.TemperatureDifference = Math.Max(0f, (tempBehavior.CurBodyTemperature - tempBehavior.NormalBodyTemperature) * 15f);

                }
                this.EnergyRateUpdate = (this.entity.World.Api.ModLoader.GetModSystem<RoomRegistry>(true).GetRoomForPosition(this.entity.Pos.AsBlockPos).ExitCount == 0) ? 0f : (((0.01f * this.TemperatureDifference * (ConfigSystem.ConfigServer.EnergyRatePerDegrees * (1f - this.OverallHealthRatio)))) / (1f + (float)Math.Exp((double)(-(double)this.TemperatureDifference))));
                this.entity.Stats.Set(BtCore.Modid + ":energyrate", "resistheat", this.EnergyRateUpdate, false);


            }
            else
            {
                this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "resistheat");
            }
            this.ReceiveEnergyBySleeping(this.entity);
            this.UpdateEnergyBoosts();
            if ((double)this.CurrentEnergy > 0.0)
            {
                return;
            }
            // Her kan tilføjes et check for om hunger matters, så man tager damage som før uden hunger check.
            
            if (ConfigSystem.ConfigServer.HungerLevelMatters && this.Famine)
            {
                this.entity.ReceiveDamage(new DamageSource
                {
                    Source = EnumDamageSource.Unknown,
                    Type = EnumDamageType.Suffocation
                }, ConfigSystem.ConfigServer.DamageIfNoEnergyAndStarving);
            }

            // If hunger dosn't matter
            if (ConfigSystem.ConfigServer.EnergyKills && !ConfigSystem.ConfigServer.HungerLevelMatters)
            {
                this.entity.ReceiveDamage(new DamageSource
                {
                    Source = EnumDamageSource.Unknown,
                    Type = EnumDamageType.Suffocation
                }, ConfigSystem.ConfigServer.DamageIfNoEnergyAndHungerDoesNotMatter);
            }


        }

        // Checks to apply penalty from dying.
        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            EntityBehaviorHealth health = this.entity.GetBehavior<EntityBehaviorHealth>();
            if (damageSource.Type == EnumDamageType.Heal)
            {
                this.EnergyLossDelay = 60f; 
                this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - ((ConfigSystem.ConfigServer.MaxEnergy / 10f) * ConfigSystem.ConfigServer.EnergyDrainFromHealingModifier), 0f, this.MaxEnergy);
                if (ConfigSystem.ConfigServer.DrainInvigorationWhenHealing)
                {
                    this.Invigorated = Math.Clamp(this.Invigorated - ((ConfigSystem.ConfigServer.MaxEnergy / 5f) * ConfigSystem.ConfigServer.InvigorationDrainFromHealingModifier), 0f, ConfigSystem.ConfigServer.MaxEnergy);
                }
                 
            }
            
            if (damageSource.Source == EnumDamageSource.Revive)
            {
                this.EnergyLossDelay = 60f; 
                this.CurrentEnergy = this.MaxEnergy / 2f; // Set energy to half
                if (ConfigSystem.ConfigServer.LoseInvigorationWhenDying)
                {
                    this.Invigorated = 0f; // Lose all invigoration
                }
                 
            }
            
            else
            {
                
                if (damage > 0f)
                {
                    
                    if (!this.Starving && !this.Fatigued)
                    {
                        this.EnergyLossDelay = 60f; 
                        this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - ((this.CurrentEnergy * ConfigSystem.ConfigServer.EnergyDrainFromDamageMultiplier) + 10f), 0f, this.MaxEnergy);
                        this.Invigorated = Math.Clamp(this.Invigorated - (this.Invigorated * (ConfigSystem.ConfigServer.InvigoratedDrainFromDamageMultiplier + 0.05f)), 0f, ConfigSystem.ConfigServer.MaxEnergy);
                    }
                    else if (this.Starving && !this.Fatigued)
                    {
                        this.EnergyLossDelay = 60f; 
                        this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - ((this.CurrentEnergy * ConfigSystem.ConfigServer.EnergyDrainFromDamageMultiplier) + 15f), 0f, this.MaxEnergy);
                        this.Invigorated = Math.Clamp(this.Invigorated - (this.Invigorated * (ConfigSystem.ConfigServer.InvigoratedDrainFromDamageMultiplier + 10f)), 0f, ConfigSystem.ConfigServer.MaxEnergy);
                    }
                    else if (!this.Starving && this.Fatigued)
                    {
                        this.EnergyLossDelay = 60f; 
                        this.Invigorated = Math.Clamp(this.Invigorated - (this.Invigorated * (ConfigSystem.ConfigServer.InvigoratedDrainFromDamageMultiplier + 0.15f)), 0f, ConfigSystem.ConfigServer.MaxEnergy);
                    }
                    else if (this.Starving && this.Fatigued)
                    {
                        this.EnergyLossDelay = 60f; 
                        this.Invigorated = Math.Clamp(this.Invigorated - (this.Invigorated * (ConfigSystem.ConfigServer.InvigoratedDrainFromDamageMultiplier + 0.35f)), 0f, ConfigSystem.ConfigServer.MaxEnergy);
                    }
                    

                }
                
            }
        }



        
        private ITreeAttribute _energyTree;

        private EntityAgent _entityAgent;


        public float EnergyJumpBoostStat
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("energyjumpbooststat", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("energyjumpbooststat", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);



            }
        }

        private bool HotOrCold;

        private float num3;
        private float num4;

        private float EnergyRate;

        private float EnergyRateUpdate;

        public float EnergyrateHungerFactor;

        public float EnergyRatioHighEnergy;

        public float EnergyRatioHighEnergyPart2;

        public float EnergyRatioLowEnergy;

        public float EnergyRatioLowEnergyPart2;

        private float NutrientHealthMod;

        private float HealthRatio;

        private float InvigoratedNutrientRatio;

        private float InvigoratedNutrientRatioForHunger;

        public float SatRatio;

        public float EnergyRatio;

        private float InvigoRatio;

        private float NutrientRatio;

        public float OverallHealthRatio;

        private float SleepRatio;

        private float BoostLungCapacity;

        private float BoostHealthPoints;

        private float BoostHealingEffectivness;

        private int lungCapacity;

        public float EnergyRatioForSleepiness { get; private set; }

        private float TemperatureDifference;

        public float EnergyRestored { get; private set; }

        public bool Starving { get; private set; }

        public bool Famine { get; private set; }

        public bool Fatigued { get; private set; }

        private float _energyCounter;

        private int _sprintCounter;

        private int _jumpCounter;

        private int _sittingCounter;

        private int _moveCounter;

        private int _workCounter;

        private int _heavyToolWorkCounter;

        private int _mediumToolWorkCounter;

        private int _lightToolWorkCounter;

        private int _weaponToolWorkCounter;

        private long _energylistenerId;

        private ICoreAPI _api;


        // Currently not used as it is hard to balance
        public StatMultiplier HungerrateMultiplierHighEnergy = new StatMultiplier
        {
            Multiplier = ConfigSystem.ConfigServer.HungerRateReductionFromHighEnergy,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = true
        };
        public StatMultiplier HungerrateMultiplierLowEnergy = new StatMultiplier
        {
            Multiplier = ConfigSystem.ConfigServer.HungerRateGainFromLowEnergy,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };

    }

    


}
