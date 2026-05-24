using Newtonsoft.Json.Linq;
using SleepNeed.Sleepiness;
using SleepNeed.Systems;
using SleepNeed.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static HarmonyLib.Code;
using static System.Net.Mime.MediaTypeNames;

namespace SleepNeed.Energy
{
    
    public class EntityBehaviorEnergy : EntityBehavior
    {
        
        public float GetEnergySpeedModifier
        {
            get
            {
                if (ConfigSystem.SyncedConfig.EnergySpeedModifier != 0f)
                {
                    return ConfigSystem.SyncedConfig.EnergySpeedModifier;
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
                float maxEnergy = (float)Math.Round((double)(ConfigSystem.SyncedConfig.MaxEnergy + ((this.MaxEnergyModifier * ConfigSystem.SyncedConfig.MaxEnergy) / 1.5)));
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
                    energyTree.SetFloat("invigorated", Math.Clamp(value, 0f, ConfigSystem.SyncedConfig.MaxEnergy));
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                


            }
        }


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
                // this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
        }

        

        public bool Adrenaline
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return false;
                }
                return energyTree.GetBool("adrenalineindicator", false);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetBool("adrenalineindicator", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
        }

        public float OverallHealthRatio
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("overallhealth", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("overallhealth", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
        }

         public bool PlayerIsBuildingGroundBed
         {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return false;
                }
                return energyTree.GetBool("buildinggroundbed", false);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetBool("buildinggroundbed", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
         }
        
        public float RelaxingSpeed
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return 0f;
                }
                return energyTree.GetFloat("relaxingspeed", 0f);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetFloat("relaxingspeed", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
        }

        public bool RelaxingType
        {
            get
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree == null)
                {
                    return false;
                }
                return energyTree.GetBool("relaxingtype", false);
            }
            set
            {
                ITreeAttribute energyTree = this._energyTree;
                if (energyTree != null)
                {
                    energyTree.SetBool("relaxingtype", value);
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
            this.capi = this._api as ICoreClientAPI;
            this.sapi = this._api as ICoreServerAPI;
            bool isClient = this.entity.World.Side == EnumAppSide.Client;
            bool invalidTree = this._energyTree == null || this._energyTree.GetFloat("maxenergy", 0f) == 0f;
            if (invalidTree && (!isClient || this._energyTree == null))
            {
                if (this._energyTree == null)
                {
                    this.entity.WatchedAttributes.SetAttribute(this.AttributeKey, this._energyTree = new TreeAttribute());
                }
                
                this.MaxEnergyModifier = typeAttributes["maxenergymodifier"].AsFloat(0f);
                this.CurrentEnergy = Math.Min(typeAttributes["currentenergylevel"].AsFloat(this.MaxEnergy), this.MaxEnergy);
                this.EnergyLossDelay = 0f;
                this.Invigorated = 0f;
            }
            this.lungCapacity = this.entity.World.Config.GetAsInt("lungCapacity", 40000);
            EntityBehaviorEnergy._sprintSpeedMultiplier = GlobalConstants.SprintSpeedMultiplier;
            
            this.num3 = this.GetEnergySpeedModifier / 20f; // Prøv at tweake med denne faktor (SE HEEEER!!!)
            this.num4 = this.GetEnergySpeedModifier / 5f;
            if (this._energylistenerId != 0)
            {
                this.entity.World.UnregisterGameTickListener(this._energylistenerId);
            }
            if (this.entity is EntityPlayer)
            {
                this._energylistenerId = this.entity.World.RegisterGameTickListener(new Action<float>(this.SlowTick), 600, 0);
            }
            this.entity.Stats.Register(BtCore.Modid + ":energyrate", (EnumStatBlendType)2);
            this.updatedStatsFromStart = false;
            this.PreviousEnergyAmount = -999;
            this._hoursTotal = this.entity.World.Calendar.TotalHours;
            this._hoursPerDay = this.entity.World.Calendar.HoursPerDay;
            EntityBehaviorHunger nutrition = this.entity.GetBehavior<EntityBehaviorHunger>();
            if (nutrition != null)
            {
                this._previousFruitLevel = nutrition.FruitLevel;
            }
        }

        
        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            this.entity.World.UnregisterGameTickListener(this._energylistenerId);
        }

        
        public override void DidAttack(DamageSource source, EntityAgent targetEntity, ref EnumHandling handled)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                this.ConsumeEnergy(this._weaponToolWorkCounter * ConfigSystem.SyncedConfig.AttackEnergyCostModifier);
            }  
        }

        
        public virtual void ConsumeEnergy(float amount)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                this.ReduceEnergy(amount * this.EnergyRate);
            }
        }


        public override void OnGameTick(float deltaTime)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                EntityPlayer player = this.entity as EntityPlayer;
                if (this.HasRevived)
                {
                    this.CurrentEnergy = ConfigSystem.SyncedConfig.MaxEnergy * ConfigSystem.SyncedConfig.EnergyAfterRevival;
                    this.HasRevived = false;
                    if (this.entity != null)
                    {
                        EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                        if (tirednessBehavior != null)
                        {
                            tirednessBehavior.Tiredness = ConfigSystem.SyncedConfig.TirednessAfterRevival;
                        }
                    }
                    return;
                }

                EntityBehaviorBodyTemperature bodyTemp = this.entity.GetBehavior<EntityBehaviorBodyTemperature>();

                if (player != null && player.Player != null && this._entityAgent != null)
                {
                    var invMan = player.Player.InventoryManager;
                    if (invMan != null && invMan.ActiveHotbarSlot != null)
                    {
                        try
                        {
                            // Checking if the hotbar slot has a valid item before getting the tool type
                            if (invMan.ActiveHotbarSlot.Itemstack != null)
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
                        }
                        catch (NullReferenceException e)
                        {
                            _api?.Logger.Error($"[SleepNeed] NullReferenceException in OnGameTick when accessing ActiveTool: {e.Message}\n{e.StackTrace}");
                        }
                    }
                }
                if (this._entityAgent != null)
                {
                    bool isSprinting = this._entityAgent.Controls.Sprint;
                    bool isMoving = this._entityAgent.Controls.TriesToMove;
                    if (this._entityAgent.Controls.LeftMouseDown || this._entityAgent.Controls.RightMouseDown)
                    {
                        this._workCounter++;
                    }
                    if (isMoving)
                    {
                        this._moveCounter++;
                    }
                    if (isSprinting && isMoving) // added this._entityAgent.Controls.TriesToMove
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
                    if (bodyTemp != null && ((isSprinting && isMoving) || this._entityAgent.Controls.Jump))
                    {
                        // Only warm up if we aren't already overheating (e.g., cap at 37 degrees or normal body temp)
                        if (bodyTemp.CurBodyTemperature < bodyTemp.NormalBodyTemperature)
                        {
                            // 'deltaTime' ensures it rises smoothly regardless of frame rate.
                            // Adjust '0.5f' to change how fast they warm up.
                            float heatGain = ConfigSystem.SyncedConfig.SprintingWarmth * deltaTime;

                            bodyTemp.CurBodyTemperature += heatGain;
                        }
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
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (25.0 + (double)this._heavyToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.HeavyToolsEnergyCostModifier);
                }
                if ((double)this._mediumToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (18.0 + (double)this._mediumToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.MediumToolsEnergyCostModifier);
                }
                if ((double)this._lightToolWorkCounter > 0.0 && !this._entityAgent.Controls.FloorSitting)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (8.0 + (double)this._lightToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.LightToolsEnergyCostModifier);
                }
                else if ((double)this._lightToolWorkCounter > 0.0 && this._entityAgent.Controls.FloorSitting)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (8.0 + (double)this._lightToolWorkCounter / 15.0) / 10.0) * (this.EnergyRate * (1f - this.EnergyRatio)) * num2) * ConfigSystem.SyncedConfig.LightToolsEnergyCostModifier);
                }
                if ((double)this._weaponToolWorkCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (15.0 + (double)this._weaponToolWorkCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.WeaponsEnergyCostModifier);
                }
                if ((double)this._workCounter > 0.0 && !this._entityAgent.Controls.FloorSitting)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (6.0 + (double)this._workCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.NoToolsWorkEnergyCostModifier);
                }
                else if ((double)this._workCounter > 0.0 && this._entityAgent.Controls.FloorSitting)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (6.0 + (double)this._workCounter / 15.0) / 10.0) * (this.EnergyRate * (1f - this.EnergyRatio)) * num2) * ConfigSystem.SyncedConfig.NoToolsWorkEnergyCostModifier);
                }
                if ((double)this._moveCounter > 0.0 && this.CurrentEnergy > 0.3f * ConfigSystem.SyncedConfig.MaxEnergy)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (4.0 + (double)this._moveCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.MovementEnergyCostModifier);
                }
                else if ((double)this._moveCounter > 0.0 && this.CurrentEnergy < 0.3f * ConfigSystem.SyncedConfig.MaxEnergy)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * ((this.EnergyRatio * 10f) + (double)this._moveCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.MovementEnergyCostModifier);
                }
                if ((double)this._sprintCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (20.0 + (double)this._sprintCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.SprintingJumpingEnergyCostModifier);
                }
                if ((double)this._jumpCounter > 0.0)
                {
                    this.ReduceEnergy((this.num3 * (float)(1.2000000476837158 * (18.0 + (double)this._jumpCounter / 15.0) / 10.0) * this.EnergyRate * num2) * ConfigSystem.SyncedConfig.SprintingJumpingEnergyCostModifier);
                }
                // if ((double)this._sittingCounter > 0.0 && this.HotOrCold)
                // {
                //    this.RegainEnergy(this.num4 * (float)(1.2000000476837158 * (8.0 + (double)this._sittingCounter / 15.0) / 10.0) * 0.01f * num2);
                // }
                // else if ((double)this._sittingCounter > 0.0)
                if ((double)this._sittingCounter > 0.0 && ConfigSystem.SyncedConfig.EnableSleepiness)
                {
                    // Changed to 7.0 from 8.0
                    this.RegainEnergy(this.num4 * (float)(1.2000000476837158 * (7.0 + (double)this._sittingCounter / 15.0) / 10.0) * Math.Max(1f, ((this.EnergyRate / 10f) * this.SleepRatio)) * num2);
                }
                else if (((double)this._sittingCounter > 0.0 && !ConfigSystem.SyncedConfig.EnableSleepiness))
                {
                    this.RegainEnergy(this.num4 * (float)(1.2000000476837158 * (7.0 + (double)this._sittingCounter / 15.0) / 10.0) * Math.Max(1f, this.EnergyRate / 10f) * num2);
                }
                else if (!this._entityAgent.Controls.FloorSitting)
                {
                    this.RegainEnergy(0f);
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
            
                
        }

        
        private bool ReduceEnergy(float satLossMultiplier)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (ConfigSystem.SyncedConfig.EnableSleepiness)
                {
                    var sleepiness = entity.GetBehavior<EntityBehaviorSleepiness>();
                    if (sleepiness != null && sleepiness.IsSleepingNow)
                    {
                        // Skip energy reduction while sleeping
                        return false;
                    }
                }
                else
                {
                    if (this.IsSleepingNow)
                    {
                        // Skip energy reduction while sleeping
                        return false;
                    }
                }

                bool flag = false;
                satLossMultiplier *= this.GetEnergySpeedModifier;
                if (ConfigSystem.SyncedConfig.EnableNutrientFactor && this.Starving) // Evt. tilføj ((double)this.CurrentEnergy < 0.2 * (double)this.MaxEnergy)
                {
                    EntityBehaviorHunger nutrition = this.entity.GetBehavior<EntityBehaviorHunger>();
                    if (nutrition != null)
                    {
                        // Fruit
                        if (nutrition.FruitLevel > 0f)
                        {
                            nutrition.FruitLevel = Math.Max(0f, nutrition.FruitLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                        // Vegetable
                        if (nutrition.VegetableLevel > 0f)
                        {
                            nutrition.VegetableLevel = Math.Max(0f, nutrition.VegetableLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                        // Protein
                        if (nutrition.ProteinLevel > 0f)
                        {
                            nutrition.ProteinLevel = Math.Max(0f, nutrition.ProteinLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                        // Grain
                        if (nutrition.GrainLevel > 0f)
                        {
                            nutrition.GrainLevel = Math.Max(0f, nutrition.GrainLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                        // Dairy
                        if (nutrition.DairyLevel > 0f)
                        {
                            nutrition.DairyLevel = Math.Max(0f, nutrition.DairyLevel - ((satLossMultiplier * this.EnergyRate) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                        if (this.Invigorated > 0f)
                        {
                            this.Invigorated = Math.Max(0f, this.Invigorated - ((satLossMultiplier * (this.EnergyRate * 10f)) * ConfigSystem.SyncedConfig.NutritionLossWhenStarvingModifier));
                        }
                    }
                }
                if ((double)this.EnergyLossDelay > 0.0)
                {
                    this.EnergyLossDelay -= 10f * satLossMultiplier;
                    flag = true;
                }
                else if (this.CurrentEnergy < 0.3f * ConfigSystem.SyncedConfig.MaxEnergy)
                {

                    this.Invigorated = Math.Max(0f, this.Invigorated * (this.CurrentEnergy / (ConfigSystem.SyncedConfig.MaxEnergy * 0.3f))); // Math.Max(0f, this.Invigorated - ((satLossMultiplier * (this.EnergyRate * 10f)) * Math.Max(0.1f, (1f - this.OverallHealthRatio))));

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
            return false;
        }

        
        private bool RegainEnergy(float satLossMultiplier)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                bool flag = false;
                satLossMultiplier *= this.GetEnergySpeedModifier;
                var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
                if (ConfigSystem.SyncedConfig.EnableSleepiness && sleepiness != null)
                {

                    float weatherTemp = this.entity.World.BlockAccessor.GetClimateAt(this.entity.Pos.AsBlockPos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, this.entity.World.Calendar.TotalDays).Temperature;
                    bool isInside = this.entity.World.Api.ModLoader.GetModSystem<RoomRegistry>(true).GetRoomForPosition(this.entity.Pos.AsBlockPos).ExitCount == 0;
                    bool relaxingType = this.RelaxingType;
                    Block blockAtFeet = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos, 0);
                    Block liquidAtFeet = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos, 2);
                    bool isBoilingWater = blockAtFeet.LiquidCode == "boilingwater" || liquidAtFeet.LiquidCode == "boilingwater";
                    if (!isBoilingWater && this.entity.FeetInLiquid)
                    {
                        Block blockBelow = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos.DownCopy(), 2);
                        if (blockBelow.LiquidCode == "boilingwater") isBoilingWater = true;
                    }
                    bool relaxingStatusBoilingWelness = this._entityAgent.Controls.FloorSitting && this.entity.FeetInLiquid && isBoilingWater;
                    bool relaxingStatusWelness = this._entityAgent.Controls.FloorSitting && this.entity.FeetInLiquid && !isBoilingWater && ((weatherTemp >= 20f) || isInside);
                    bool relaxingStatusChilling = this._entityAgent.Controls.FloorSitting && !this.entity.FeetInLiquid;

                    if ((double)this.EnergyLossDelay > 0.0)
                    {
                        this.EnergyLossDelay -= 10f * satLossMultiplier;
                        flag = true;
                    }
                    else if (this.CurrentEnergy > 0.5f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && sleepiness.CurrentSleepinessLevel < 0.5f * ConfigSystem.SyncedConfig.MaxSleepiness && !sleepiness.IsOverloadedForEnergy && !this.Starving && relaxingStatusWelness && !relaxingStatusBoilingWelness)
                    {
                        this.Invigorated = Math.Max(0f, this.Invigorated + (((satLossMultiplier * ((this.SleepRatio * this.OverallHealthRatio) * this.EnergyRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier) * ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier));
                    }
                    else if (this.CurrentEnergy > 0.5f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && sleepiness.CurrentSleepinessLevel < 0.5f * ConfigSystem.SyncedConfig.MaxSleepiness && !sleepiness.IsOverloadedForEnergy && !this.Starving && relaxingStatusBoilingWelness)
                    {
                        this.Invigorated = Math.Max(0f, this.Invigorated + (((satLossMultiplier * ((this.SleepRatio * this.OverallHealthRatio) * this.EnergyRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier) * (ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f)));
                    }
                    // else if (this.CurrentEnergy > 0.6f * ConfigSystem.SyncedConfig.MaxEnergy && this.SleepRatio > 0.7f && !sleepiness.IsOverloadedForEnergy && !this.Starving)
                    // {
                    //    this.Invigorated = Math.Max(0f, this.Invigorated + ((satLossMultiplier * ((this.SleepRatio * this.OverallHealthRatio) * this.EnergyRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                    // }


                    if (flag)
                    {
                        this._energyCounter -= 10f;
                        return true;
                    }
                    if (!this.Starving)
                    {
                        float energyGained = 0f;
                        float relaxingSpeed = this.RelaxingSpeed;
                        float currentEnergy = this.CurrentEnergy;
                        // < 40% energy
                        if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy <= (ConfigSystem.SyncedConfig.MaxEnergy * 0.4f) && !sleepiness.IsOverloadedForEnergy)
                        {
                            energyGained = Math.Max(0f, (((satLossMultiplier * (this.SleepRatio + this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(energyGained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.02f));
                            }
                        }
                        // > 60% energy
                        else if (this.CurrentEnergy > 0.6f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && !sleepiness.IsOverloadedForEnergy)
                        {
                            energyGained = Math.Max(0f, ((satLossMultiplier * this.OverallHealthRatio) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(energyGained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.08f));
                            }
                        }
                        // 40% < state < 60% energy
                        else if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && !sleepiness.IsOverloadedForEnergy)
                        {
                            energyGained = Math.Max(0f, ((satLossMultiplier * ((this.SleepRatio + this.OverallHealthRatio) / 2f)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(energyGained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.05f));
                            }
                        }
                        // Overloaded + > 75% energy
                        else if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy > 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && sleepiness.IsOverloadedForEnergy) // (1f - sleepiness.OverloadThreshold) This is to reverse it, because in sleepiness behavior it is used differently.
                        {
                            float sleepinessDrained = Math.Max(0f, ((satLossMultiplier * ((this.SleepRatio + this.OverallHealthRatio) * this.EnergyRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            energyGained = Math.Max(0f, (((satLossMultiplier * (this.SleepRatio * this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(sleepinessDrained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.09f));
                            }
                        }
                        // Overloaded
                        else if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && sleepiness.IsOverloadedForEnergy) // (1f - sleepiness.OverloadThreshold) This is to reverse it, because in sleepiness behavior it is used differently.
                        {
                            float sleepinessDrained = Math.Max(0f, ((satLossMultiplier * ((this.SleepRatio + this.OverallHealthRatio) * this.EnergyRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            energyGained = Math.Max(0f, (((satLossMultiplier * (this.SleepRatio * this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(sleepinessDrained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.06f));
                            }
                        }
                        else if (this.CurrentEnergy > 0.75f * ConfigSystem.SyncedConfig.MaxEnergy)
                        {
                            float sleepinessDrained = Math.Max(0f, (((satLossMultiplier * (this.SleepRatio + this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                sleepinessDrained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(sleepinessDrained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.08f));
                            }
                        }
                        // Backup if everything else is not true
                        else if ((double)this.CurrentEnergy >= 0.0)
                        {
                            energyGained = Math.Max(0f, ((satLossMultiplier * (this.SleepRatio * this.OverallHealthRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier));
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }

                            if (sleepiness.CurrentSleepinessLevel > 0.7f * ConfigSystem.SyncedConfig.MaxSleepiness)
                            {
                                sleepiness.CurrentSleepinessLevel -= Math.Max(0f, sleepiness.CurrentSleepinessLevel * Math.Clamp(energyGained / ConfigSystem.SyncedConfig.MaxEnergy, 0f, 0.01f));
                            }
                        }
                        
                        // Hent verdens hastighed (/time csm 0.5 = 1 real life second = 30 in game seconds)
                        float speedOfTime = this.entity.World.Calendar.SpeedOfTime;
                        float realSecondsPassed = 10f; // Det interval _energyCounter kører på

                        // Hvor mange in-game timer svarer de 10 sekunder til?
                        // (10 sek * 30 speed) / 3600 sekunder på en time
                        float gameHoursPassed = (realSecondsPassed * speedOfTime) / 3600f;

                        // Undgå division med 0 (hvis tiden står stille)
                        if (gameHoursPassed > 0)
                        {
                            // Beregn hastighed: Energi / Tid
                            this.RelaxingSpeed = energyGained / gameHoursPassed;
                        }
                        else
                        {
                            this.RelaxingSpeed = 0f;
                        }
                        this.CurrentEnergy += Math.Max(0f, energyGained);
                        if (this._entityAgent.Controls.FloorSitting && this.RelaxingSpeed > 0f)
                        {
                            if (relaxingStatusWelness || relaxingStatusBoilingWelness)
                            {
                                this.RelaxingType = true;
                            }
                            else if (relaxingStatusChilling)
                            {
                                this.RelaxingType = false;
                            }
                        }
                        else
                        {
                            if (this.RelaxingSpeed > 0f)
                            {
                                this.RelaxingSpeed = 0f;
                            }
                        }
                    }
                }
                // If sleepiness is disabled
                else if (!ConfigSystem.SyncedConfig.EnableSleepiness)
                {
                    float weatherTemp = this.entity.World.BlockAccessor.GetClimateAt(this.entity.Pos.AsBlockPos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, this.entity.World.Calendar.TotalDays).Temperature;
                    bool isInside = this.entity.World.Api.ModLoader.GetModSystem<RoomRegistry>(true).GetRoomForPosition(this.entity.Pos.AsBlockPos).ExitCount == 0;
                    bool relaxingType = this.RelaxingType; 
                    Block blockAtFeet = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos, 0);
                    Block liquidAtFeet = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos, 2);
                    bool isBoilingWater = blockAtFeet.LiquidCode == "boilingwater" || liquidAtFeet.LiquidCode == "boilingwater";
                    if (!isBoilingWater && this.entity.FeetInLiquid)
                    {
                        Block blockBelow = this.entity.World.BlockAccessor.GetBlock(this.entity.Pos.AsBlockPos.DownCopy(), 2);
                        if (blockBelow.LiquidCode == "boilingwater") isBoilingWater = true;
                    }
                    bool relaxingStatusBoilingWelness = this._entityAgent.Controls.FloorSitting && this.entity.FeetInLiquid && isBoilingWater;
                    bool relaxingStatusWelness = this._entityAgent.Controls.FloorSitting && this.entity.FeetInLiquid && !isBoilingWater && ((weatherTemp >= 20f) || isInside);
                    bool relaxingStatusChilling = this._entityAgent.Controls.FloorSitting && !this.entity.FeetInLiquid;

                    if ((double)this.EnergyLossDelay > 0.0)
                    {
                        this.EnergyLossDelay -= 10f * satLossMultiplier;
                        flag = true;
                    }
                    else if (this.CurrentEnergy > 0.5f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && !this.Starving && relaxingStatusWelness && !relaxingStatusBoilingWelness)
                    {
                        this.Invigorated = Math.Max(0f, this.Invigorated + (((satLossMultiplier * (this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier) * ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier);
                    }
                    else if (this.CurrentEnergy > 0.5f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy && !this.Starving && relaxingStatusBoilingWelness)
                    {
                        this.Invigorated = Math.Max(0f, this.Invigorated + (((satLossMultiplier * (this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier) * (ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f));
                    }
                    // else if (this.CurrentEnergy > 0.6f * ConfigSystem.SyncedConfig.MaxEnergy && !this.Starving)
                    // {
                    //    this.Invigorated = Math.Max(0f, this.Invigorated + ((satLossMultiplier * (this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier);
                    // }


                    if (flag)
                    {
                        this._energyCounter -= 10f;
                        return true;
                    }
                    if (!this.Starving)
                    {
                        float energyGained = 0f;
                        if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy <= (ConfigSystem.SyncedConfig.MaxEnergy * 0.4f))
                        {
                            energyGained = (((satLossMultiplier * (1f + this.OverallHealthRatio))) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier);
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }
                        }
                        else if (this.CurrentEnergy > 0.6f * ConfigSystem.SyncedConfig.MaxEnergy && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy)
                        {
                            energyGained = ((satLossMultiplier * (this.OverallHealthRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier);
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }
                        }
                        else if ((double)this.CurrentEnergy >= 0.0 && this.CurrentEnergy < 0.75f * ConfigSystem.SyncedConfig.MaxEnergy)
                        {
                            energyGained = Math.Clamp((satLossMultiplier * (0.5f + this.OverallHealthRatio)) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier, (satLossMultiplier * 0.5f) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier, (satLossMultiplier * 1f) * ConfigSystem.SyncedConfig.SittingRelaxingSpeedModifier);
                            if (relaxingStatusWelness && !relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier;
                            }
                            else if (relaxingStatusBoilingWelness)
                            {
                                energyGained *= ConfigSystem.SyncedConfig.WaterSpaRelaxingSpeedModifier * 1.5f;
                            }
                        }
                        // Hent verdens hastighed (/time csm 0.5 = 1 real life second = 30 in game seconds)
                        float speedOfTime = this.entity.World.Calendar.SpeedOfTime;
                        float realSecondsPassed = 10f; // Det interval _energyCounter kører på

                        // Hvor mange in-game timer svarer de 10 sekunder til?
                        // (10 sek * 30 speed) / 3600 sekunder på en time
                        float gameHoursPassed = (realSecondsPassed * speedOfTime) / 3600f;

                        // Undgå division med 0 (hvis tiden står stille)
                        if (gameHoursPassed > 0)
                        {
                            // Beregn hastighed: Energi / Tid
                            this.RelaxingSpeed = energyGained / gameHoursPassed;
                        }
                        else
                        {
                            this.RelaxingSpeed = 0f;
                        }
                        this.CurrentEnergy += Math.Max(0f, energyGained);
                        if (this._entityAgent.Controls.FloorSitting && this.RelaxingSpeed > 0f)
                        {
                            if (relaxingStatusWelness || relaxingStatusBoilingWelness)
                            {
                                this.RelaxingType = true;
                            }
                            else if (relaxingStatusChilling)
                            {
                                this.RelaxingType = false;
                            }
                        }
                        else
                        {
                            if (this.RelaxingSpeed > 0f)
                            {
                                this.RelaxingSpeed = 0f;
                            }
                        }
                    }

                }
            }
            
                
            return false;
        }

        public void ReceiveEnergyBySleeping(Entity entity)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (entity == null || !ConfigSystem.SyncedConfig.EnableSleepiness) return;
                var sleepiness = entity.GetBehavior<EntityBehaviorSleepiness>();
                if (sleepiness == null || !sleepiness.IsSleepingNow)
                {
                    return;
                }
                if (sleepiness.LastHoursPassed <= 0f) return; // Only restore if time has passed

                bool wasFull = this.CurrentEnergy >= this.MaxEnergy;
                this.EnergyRestored = sleepiness.LastHoursPassed * ((this.MaxEnergy * (this.SatRatio / 10f)) * (ConfigSystem.SyncedConfig.EnergyRestoredBySleepingModifier + Math.Clamp((this.SatRatio / 0.3f), 0f, 1f))); // Changed from: sleepiness.LastHoursPassed * ((this.MaxEnergy * (this.SleepRatio / 10f)) * (ConfigSystem.SyncedConfig.EnergyRestoredBySleepingModifier + this.SleepRatio))
                this.CurrentEnergy = Math.Clamp(this.CurrentEnergy + ((this.EnergyRestored + sleepiness.EnergyRestoredfromSleepiness) + ((this.EnergyRestored + sleepiness.EnergyRestoredfromSleepiness) * this.OverallHealthRatio)), 0f, this.MaxEnergy);
                if ((double)this.CurrentEnergy > 0.6 * (double)this.MaxEnergy && this.SleepRatio > 0.7f && !sleepiness.IsOverloadedForEnergy && !this.Starving)
                {
                    this.Invigorated = Math.Clamp(this.Invigorated + (((this.CurrentEnergy * (this.SatRatio / 10f)) * this.OverallHealthRatio) * this.SleepRatio), 0f, ConfigSystem.SyncedConfig.MaxEnergy); // Changed from: this.Invigorated + (((this.CurrentEnergy * (this.SleepRatio / 10f)) * this.OverallHealthRatio) * this.SleepRatio
                }
                if (!wasFull)
                {
                    this.EnergyLossDelay = Math.Max(this.EnergyLossDelay, 10f);
                }
            }
        }


        public void UpdateEnergyBoosts()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (ConfigSystem.SyncedConfig.DisableStatChanges)
                {
                    return;
                }
                this.UpdateEnergyStatBoosts();
                this.UpdateEnergyHealthBoost();
            } 
        }

        
        
        
        private void UpdateEnergyStatBoosts()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (this.entity == null || this.entity.Stats == null)
                {
                    return;
                }
                float upperRatio = ConfigSystem.SyncedConfig.UpperRatio;
                float lowerRatio = ConfigSystem.SyncedConfig.LowerRatio;
                var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
                if (ConfigSystem.SyncedConfig.HungerLevelMatters)
                {
                    if (ConfigSystem.SyncedConfig.EnableSleepiness && sleepiness != null)
                    {
                        if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || sleepiness.IsSleepingNow)
                        {
                            this.entity.Stats.Remove("hungerrate", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.SyncedConfig.HungerRateReductionFromHighEnergy * ((1f - 2f * (this.EnergyRatioHighEnergy))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.SyncedConfig.HungerRateGainFromLowEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy))), false);
                        }
                    }
                    else
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("hungerrate", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.SyncedConfig.HungerRateReductionFromHighEnergy * ((1f - 2f * (this.EnergyRatioHighEnergy))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("hungerrate", "fatigue", ConfigSystem.SyncedConfig.HungerRateGainFromLowEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy))), false);
                        }
                    }

                    if (this.SatRatio <= ConfigSystem.SyncedConfig.HungerEnergyrateDebuffStartRatio)
                    {
                        this.entity.Stats.Set(BtCore.Modid + ":energyrate", "hungryrate", this.lowHungerEnergyrate, false);
                        this._energyrateHungerRemoved = false;
                    }
                    else if (this.SatRatio > ConfigSystem.SyncedConfig.HungerEnergyrateDebuffStartRatio && !this._energyrateHungerRemoved)
                    {
                        this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "hungryrate");
                        this._energyrateHungerRemoved = true;
                    }
                }
                else if (!ConfigSystem.SyncedConfig.HungerLevelMatters)
                {
                    this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "hungryrate");
                }



                if (ConfigSystem.SyncedConfig.BodyTemperatureMatters && this.HotOrCold)
                {
                    this.entity.Stats.Set(BtCore.Modid + ":energyrate", "resistheat", this.EnergyRateUpdate, false);
                    this._resistheatRemoved = false;
                }
                else if (!this.HotOrCold && !this._resistheatRemoved)
                {
                    this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "resistheat");
                    this._resistheatRemoved = true;
                }

                if (ConfigSystem.SyncedConfig.EnableSleepiness && sleepiness != null)
                {
                    bool isRefreshed = sleepiness.SleepinessRatio <= sleepiness.RefreshedThreshold;
                    bool notRefreshed = sleepiness.SleepinessRatio >= sleepiness.RefreshedThreshold;
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedToolMiningSpeed)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", ((ConfigSystem.SyncedConfig.ToolMiningSpeedBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("miningSpeedMul", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.SyncedConfig.ToolMiningSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.SyncedConfig.ToolMiningSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedJumpHeight)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.EnergyJumpBoostStat = 0f;
                            this.entity.Stats.Set("jumpHeightMul", "fatigue", ((ConfigSystem.SyncedConfig.JumpHeightBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                            this.EnergyJumpBoostStat = 0f;
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.EnergyJumpBoostStat = 0f;
                            this.entity.Stats.Set("jumpHeightMul", "fatigue", (ConfigSystem.SyncedConfig.JumpHeightBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false); // this.EnergyRatioHighEnergy = Math.Clamp(0.5f * (this.CurrentEnergy - (0.7f * this.MaxEnergy)) / (0.3f * this.MaxEnergy) + 0.5f, 0.5f, 1f);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                            this.EnergyJumpBoostStat = (ConfigSystem.SyncedConfig.JumpHeightDebuffFromEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy)))); // this.EnergyRatioLowEnergy = Math.Clamp((0.5f / (0.3f * this.MaxEnergy)) * this.CurrentEnergy, 0f, 0.5f);
                        }

                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedWalkSpeed)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("walkspeed", "fatigue", ((ConfigSystem.SyncedConfig.WalkSpeedBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("walkspeed", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.SyncedConfig.WalkSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.SyncedConfig.WalkSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedRangedWeaponSpeed)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", ((ConfigSystem.SyncedConfig.RangedWeaponSpeedBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("rangedWeaponsSpeed", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedRangedWeaponDamage)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", ((ConfigSystem.SyncedConfig.RangedWeaponDamageBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("rangedWeaponsDamage", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedMeleeWeaponDamage)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", ((ConfigSystem.SyncedConfig.MeleeWeaponDamageBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("meleeWeaponsDamage", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.MeleeWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.MeleeWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedArmorWalkSpeedAffectedness)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", ((ConfigSystem.SyncedConfig.ArmorWalkSpeedAffectednessBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("armorWalkSpeedAffectedness", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.SyncedConfig.ArmorWalkSpeedAffectednessBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.SyncedConfig.ArmorWalkSpeedAffectednessDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedBowDrawingStrength)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("bowDrawingStrength", "fatigue", ((ConfigSystem.SyncedConfig.BowDrawingStrengthBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("bowDrawingStrength", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.SyncedConfig.BowDrawingStrengthBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.SyncedConfig.BowDrawingStrengthDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedAnimalHarvestingTime)
                    {
                        if (isRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("animalHarvestingTime", "fatigue", ((ConfigSystem.SyncedConfig.AnimalHarvestingTimeBoostFromEnergy * ConfigSystem.SyncedConfig.RefreshedEnergyBoostMultiplier) * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if ((this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio) || (sleepiness.IsOverloadedForEnergy && this.EnergyRatio > lowerRatio))
                        {
                            this.entity.Stats.Remove("animalHarvestingTime", "fatigue");
                        }
                        else if (notRefreshed && this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.SyncedConfig.AnimalHarvestingTimeBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.SyncedConfig.AnimalHarvestingTimeDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                }
                // If Sleepiness is disabled
                else
                {
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedToolMiningSpeed)
                    {



                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("miningSpeedMul", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.SyncedConfig.ToolMiningSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("miningSpeedMul", "fatigue", (ConfigSystem.SyncedConfig.ToolMiningSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedJumpHeight)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("jumpHeightMul", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("jumpHeightMul", "fatigue", (ConfigSystem.SyncedConfig.JumpHeightBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.EnergyJumpBoostStat = (ConfigSystem.SyncedConfig.JumpHeightDebuffFromEnergy * ((1f - 2f * (this.EnergyRatioLowEnergy))));
                        }

                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedWalkSpeed)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("walkspeed", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.SyncedConfig.WalkSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("walkspeed", "fatigue", (ConfigSystem.SyncedConfig.WalkSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedRangedWeaponSpeed)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("rangedWeaponsSpeed", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponSpeedBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsSpeed", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponSpeedDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedRangedWeaponDamage)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("rangedWeaponsDamage", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("rangedWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.RangedWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }


                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedMeleeWeaponDamage)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("meleeWeaponsDamage", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.MeleeWeaponDamageBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("meleeWeaponsDamage", "fatigue", (ConfigSystem.SyncedConfig.MeleeWeaponDamageDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedArmorWalkSpeedAffectedness)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("armorWalkSpeedAffectedness", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.SyncedConfig.ArmorWalkSpeedAffectednessBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("armorWalkSpeedAffectedness", "fatigue", (ConfigSystem.SyncedConfig.ArmorWalkSpeedAffectednessDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedBowDrawingStrength)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("bowDrawingStrength", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.SyncedConfig.BowDrawingStrengthBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("bowDrawingStrength", "fatigue", (ConfigSystem.SyncedConfig.BowDrawingStrengthDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                    if (ConfigSystem.SyncedConfig.EnableEnergyDependedAnimalHarvestingTime)
                    {
                        if (this.EnergyRatio > lowerRatio && this.EnergyRatio < upperRatio)
                        {
                            this.entity.Stats.Remove("animalHarvestingTime", "fatigue");
                        }
                        else if (this.EnergyRatio >= upperRatio)
                        {
                            this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.SyncedConfig.AnimalHarvestingTimeBoostFromEnergy * ((1f - 2f * (1f - this.EnergyRatioHighEnergy)))), false);
                        }
                        else if (this.EnergyRatio <= lowerRatio)
                        {
                            this.entity.Stats.Set("animalHarvestingTime", "fatigue", (ConfigSystem.SyncedConfig.AnimalHarvestingTimeDebuffFromEnergy * ((1f - 2f * (1f - this.EnergyRatioLowEnergy)))), false);
                        }
                    }
                }
            }
        }
        
        public void UpdateEnergyHealthBoost()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (this.entity.World.Side == EnumAppSide.Client)
                {
                    return;
                }

                this.BoostLungCapacity = 0f;
                if (ConfigSystem.SyncedConfig.EnableInvigoratedLungCapacityBoost)
                {
                    this.BoostLungCapacity = (ConfigSystem.SyncedConfig.InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity * (int)(this.lungCapacity / 100)) * (this.OverallHealthRatio * 100f);
                }

                
                EntityBehaviorBreathe breathe = this.entity.GetBehavior<EntityBehaviorBreathe>();
                if (breathe != null)
                {
                    // Triggers the Harmony Setter Patch without changing the Base value
                    breathe.MaxOxygen = breathe.MaxOxygen;
                }

                if (this.entity.Stats != null && ConfigSystem.SyncedConfig.EnableInvigoratedHealthBoost)
                {
                    this.entity.Stats.Set("maxhealthExtraPoints", BtCore.Modid + ":energyhealth", this.BoostHealthPoints, false);
                }
                else if (this.entity.Stats != null && !ConfigSystem.SyncedConfig.EnableInvigoratedHealthBoost)
                {
                    this.entity.Stats.Remove("maxhealthExtraPoints", BtCore.Modid + ":energyhealth");
                }

                if (this.entity.Stats != null && ConfigSystem.SyncedConfig.EnableInvigoratedHealingEffectiveness)
                {
                    this.entity.Stats.Set("healingeffectivness", BtCore.Modid + ":energyhealth", this.BoostHealingEffectivness, false);
                }
                else if (this.entity.Stats != null && !ConfigSystem.SyncedConfig.EnableInvigoratedHealingEffectiveness)
                {
                    this.entity.Stats.Remove("healingeffectivness", BtCore.Modid + ":energyhealth");
                }
            }   
        }

        // Sleep System If Sleepiness Is Disabled
        private void UpdateIsSleepingNow()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (this.entity == null || ConfigSystem.SyncedConfig.EnableSleepiness)
                {
                    return;
                }
                EntityBehaviorTiredness ebt = this.entity.GetBehavior<EntityBehaviorTiredness>();
                if (ebt != null && ebt.IsSleeping != false)
                {
                    this._isSleepingNow = true;

                }
                else
                {
                    this._isSleepingNow = false;
                }
            }
        }

        // Public property to read the current sleeping state
        private bool IsSleepingNow
        {
            get
            {
                return this._isSleepingNow;
            }
            set
            {
                this._isSleepingNow = value;
            }
        }

        private bool _isSleepingNow = false;
        private double _hoursTotal;
        private float _hoursPerDay;
        private bool _justWokeUp = false;
        public float LastHoursPassed { get; private set; }
        

        


        private void SlowTick(float dt)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (this.entity != null && ConfigSystem.SyncedConfig.FruitDelaysEnergyReduction)
                {
                    EntityBehaviorHunger nutrition = this.entity.GetBehavior<EntityBehaviorHunger>();
                    if (nutrition != null)
                    {
                        if (this._previousFruitLevel < 0f)
                        {
                            this._previousFruitLevel = nutrition.FruitLevel;
                        }

                        float fruitLevelChange = nutrition.FruitLevel - this._previousFruitLevel;

                        if (fruitLevelChange > 0f)
                        {
                            float fruitDelayMultiplier = ConfigSystem.SyncedConfig.FruitDelayMultiplier;
                            this.EnergyLossDelay += (fruitLevelChange * fruitDelayMultiplier);
                        }

                        this._previousFruitLevel = nutrition.FruitLevel;
                    }
                }
                
                if (this.entity != null && ConfigSystem.SyncedConfig.IndefiniteSleepDuration)
                {
                    EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                    if (tirednessBehavior != null)
                    {
                        tirednessBehavior.Tiredness = 10f;
                    }
                }
                if (!ConfigSystem.SyncedConfig.EnableSleepiness)
                {
                    EntityPlayer player = this.entity as EntityPlayer;

                    float hoursPassed = (float)(this.entity.World.Calendar.TotalHours - this._hoursTotal);
                    this.LastHoursPassed = hoursPassed;
                    // Detect transition from sleeping to awake
                    bool wasSleeping = this._isSleepingNow;
                    UpdateIsSleepingNow();
                    bool isSleeping = this._isSleepingNow;

                    if (wasSleeping && !isSleeping)
                    {
                        this._justWokeUp = true;
                    }
                    if (this._justWokeUp)
                    {
                        this._justWokeUp = false; 

                        if (this.entity != null)
                        {
                            EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                            if (tirednessBehavior != null)
                            {
                                tirednessBehavior.Tiredness = ConfigSystem.SyncedConfig.TirednessAfterSleep;
                            }

                            if (this.entity.World.Rand.NextDouble() < ConfigSystem.SyncedConfig.LuckySleepChance) // 0.001 = 0.1% probability
                            {
                                this.CurrentEnergy = this.MaxEnergy;
                                this.Invigorated = ConfigSystem.SyncedConfig.MaxEnergy;
                            }
                        }
                    }

                    
                    if (hoursPassed > 0.0f && this._isSleepingNow == true)
                    {
                        if (this.LastHoursPassed >= 0f)
                        {
                            bool wasFull = this.CurrentEnergy >= this.MaxEnergy;
                            this.EnergyRestored = this.LastHoursPassed * ((this.MaxEnergy * (this.SatRatio / 10f)) * (ConfigSystem.SyncedConfig.EnergyRestoredBySleepingModifier + Math.Clamp((this.SatRatio / 0.3f), 0f, 1f))); // Changed from: sleepiness.LastHoursPassed * ((this.MaxEnergy * (this.SleepRatio / 10f)) * (ConfigSystem.SyncedConfig.EnergyRestoredBySleepingModifier + this.SleepRatio))
                            this.CurrentEnergy = Math.Clamp(this.CurrentEnergy + ((this.EnergyRestored) + ((this.EnergyRestored) * this.OverallHealthRatio)), 0f, this.MaxEnergy);
                            if ((double)this.CurrentEnergy > 0.6 * (double)this.MaxEnergy && !this.Starving)
                            {
                                this.Invigorated = Math.Clamp(this.Invigorated + (((this.CurrentEnergy * (this.SatRatio / 10f)) * this.OverallHealthRatio) * this.EnergyRatio), 0f, ConfigSystem.SyncedConfig.MaxEnergy); // Changed from: this.Invigorated + (((this.CurrentEnergy * (this.SleepRatio / 10f)) * this.OverallHealthRatio) * this.SleepRatio
                            }
                            if (!wasFull)
                            {
                                this.EnergyLossDelay = Math.Max(this.EnergyLossDelay, 10f);
                            }
                        }
                    }

                    this._hoursTotal = this.entity.World.Calendar.TotalHours;
                }
                this.EnergyRate = this.entity.Stats.GetBlended("energyrate");
                this.EnergyRatio = (this.MaxEnergy > 0f) ? (this.CurrentEnergy / ConfigSystem.SyncedConfig.MaxEnergy) : 0f;
                this.InvigoRatio = this.Invigorated / ConfigSystem.SyncedConfig.MaxEnergy;
                EntityBehaviorHunger hunger = this.entity.GetBehavior<EntityBehaviorHunger>();
                var sleepiness = entity.GetBehavior<SleepNeed.Sleepiness.EntityBehaviorSleepiness>();
                if (ConfigSystem.SyncedConfig.EnableSleepiness && sleepiness != null)
                {
                    this.SleepRatio = 1f - sleepiness.SleepinessRatio;
                }
                else
                {
                    this.SleepRatio = 1f;
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
                    if (ConfigSystem.SyncedConfig.EnableNutrientFactor)
                    {
                        this.NutrientRatio = ((hunger.FruitLevel / hunger.MaxSaturation) + (hunger.VegetableLevel / hunger.MaxSaturation) + (hunger.ProteinLevel / hunger.MaxSaturation) + (hunger.GrainLevel / hunger.MaxSaturation) + (hunger.DairyLevel / hunger.MaxSaturation)) / 5f;
                    }

                    if (ConfigSystem.SyncedConfig.HungerLevelMatters)
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
                        if (this.SatRatio <= ConfigSystem.SyncedConfig.HungerEnergyrateDebuffStartRatio)
                        {
                            float lowHungerRatio = 1f - (Math.Clamp((1f / (ConfigSystem.SyncedConfig.HungerEnergyrateDebuffStartRatio * hunger.MaxSaturation)) * hunger.Saturation, 0f, 1f));
                            this.lowHungerEnergyrate = ((ConfigSystem.SyncedConfig.HungerEnergyrateDebuff / 100f) - 1f) * lowHungerRatio;

                        }
                    }

                }

                var healthBehavior = this.entity.GetBehavior<EntityBehaviorHealth>();
                if (healthBehavior != null)
                {
                    this.HealthRatio = Math.Clamp(healthBehavior.Health, 0f, healthBehavior.BaseMaxHealth) / healthBehavior.BaseMaxHealth;
                }

                this.OverallHealthRatio = ((this.InvigoRatio + this.NutrientRatio) / 2f) * this.HealthRatio; // (this.InvigoRatio + this.NutrientRatio) / 2f)
                this.EnergyRatioHighEnergy = Math.Clamp(0.5f * (this.CurrentEnergy - (0.7f * ConfigSystem.SyncedConfig.MaxEnergy)) / (0.3f * ConfigSystem.SyncedConfig.MaxEnergy) + 0.5f, 0.5f, 1f);
                this.EnergyRatioHighEnergyPart2 = (1f - 2f * (1f - this.EnergyRatioHighEnergy));
                this.EnergyRatioLowEnergy = Math.Clamp((0.5f / (0.3f * ConfigSystem.SyncedConfig.MaxEnergy)) * this.CurrentEnergy, 0f, 0.5f);
                this.EnergyRatioLowEnergyPart2 = (1f - 2f * (1f - this.EnergyRatioLowEnergy));
                
                if (healthBehavior != null)
                {
                    this.BoostHealthPoints = ((ConfigSystem.SyncedConfig.InvigoratedHealthBoostPercentageOfMaxHealth * healthBehavior.BaseMaxHealth) / 100f) * (this.OverallHealthRatio * 100f);
                }
                this.BoostHealingEffectivness = this.OverallHealthRatio * ConfigSystem.SyncedConfig.InvigoratedHealingEffectivenessModifier;
                if (ConfigSystem.SyncedConfig.EnableInvigoratedMaxEnergyBoost)
                {
                    this.MaxEnergyModifier = this.OverallHealthRatio * ConfigSystem.SyncedConfig.InvigoratedMaxEnergyBoostModifier;

                }
                else
                {
                    this.MaxEnergyModifier = 0f;
                }

                EntityBehaviorBodyTemperature tempBehavior = this.entity.GetBehavior<EntityBehaviorBodyTemperature>();
                float WeatherTemp = this.entity.World.BlockAccessor.GetClimateAt(this.entity.Pos.AsBlockPos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, this.entity.World.Calendar.TotalDays).Temperature;
                if ((ConfigSystem.SyncedConfig.BodyTemperatureMatters && tempBehavior != null) && (tempBehavior.CurBodyTemperature < tempBehavior.NormalBodyTemperature || WeatherTemp > tempBehavior.NormalBodyTemperature))
                {
                    this.HotOrCold = true;
                }
                else
                {
                    this.HotOrCold = false;
                }

                if ((ConfigSystem.SyncedConfig.BodyTemperatureMatters && tempBehavior != null) && this.HotOrCold)
                {
                    if (tempBehavior.CurBodyTemperature < tempBehavior.NormalBodyTemperature)
                    {
                        this.TemperatureDifference = Math.Max(0f, (tempBehavior.NormalBodyTemperature - tempBehavior.CurBodyTemperature));

                    }
                    else if ((WeatherTemp > tempBehavior.NormalBodyTemperature) && !this.entity.FeetInLiquid)
                    {
                        this.TemperatureDifference = Math.Max(0f, (WeatherTemp - tempBehavior.NormalBodyTemperature));

                    }
                    else if ((WeatherTemp > tempBehavior.NormalBodyTemperature) && this.entity.FeetInLiquid)
                    {
                        this.TemperatureDifference = 0f;

                    }
                    this.EnergyRateUpdate = (this.entity.World.Api.ModLoader.GetModSystem<RoomRegistry>(true).GetRoomForPosition(this.entity.Pos.AsBlockPos).ExitCount == 0) ? 0f : ((Math.Abs(((ConfigSystem.SyncedConfig.EnergyRatePerDegrees / 100f) - 1f)) * this.TemperatureDifference) * Math.Max(0.1f, (1f - this.OverallHealthRatio)));



                }
                this.ReceiveEnergyBySleeping(this.entity);
                bool energyChanged = Math.Abs(this.PreviousEnergyAmount - this.CurrentEnergy) > 0.01f;
                if (energyChanged)
                {
                    this.UpdateEnergyBoosts();
                    this.PreviousEnergyAmount = this.CurrentEnergy;
                    this._startStatsTimer = 0f;
                }
                this._startStatsTimer += dt;
                if (!this.updatedStatsFromStart)
                {
                    if (this._startStatsTimer >= 5f)
                    {
                        this.UpdateEnergyBoosts();
                        this.updatedStatsFromStart = true;
                        this._startStatsTimer = 0f;
                    }
                }
                else if (this._startStatsTimer >= 10f)
                {
                    this.UpdateEnergyBoosts();
                    this._startStatsTimer = 0f;
                }


                // adding a sprint prevent if fatigued
                if (this.Fatigued && !this.wasFatigued)
                {
                    this.wasFatigued = true;
                    GlobalConstants.SprintSpeedMultiplier = 1.0;
                }
                else if (!this.Fatigued && this.wasFatigued)
                {
                    this.wasFatigued = false;
                    GlobalConstants.SprintSpeedMultiplier = EntityBehaviorEnergy._sprintSpeedMultiplier;
                }


                // Adrenaline
                if (this.IsFighting)
                {
                    this.FightDuration += dt;
                    if (this.FightDuration >= ConfigSystem.SyncedConfig.AdrenalineDuration)
                    {
                        this.CurrentEnergy -= this.AccumulatedDamageEnergy;
                        this.CurrentEnergy -= this.AccumulatedDamageInvigoration;
                        this.AccumulatedDamageEnergy = 0f;
                        this.AccumulatedDamageInvigoration = 0f;
                        this.FightDuration = 0f;
                        this.IsFighting = false;
                        if (!this.IsFighting)
                        {
                            this.ShowAdrenalineMessage("sn_adrenaline_stop", Lang.Get(BtCore.Modid + ":sn_adrenaline_stop"));
                            this.Adrenaline = this.IsFighting;
                        }
                    }
                    else
                    {
                        return;
                    }
                }



                if ((double)this.CurrentEnergy > 0.0)
                {
                    return;
                }
                // Her kan tilføjes et check for om hunger matters, så man tager damage som før uden hunger check.

                if (ConfigSystem.SyncedConfig.HungerLevelMatters && this.Famine)
                {
                    this.entity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Unknown,
                        Type = EnumDamageType.Suffocation
                    }, ConfigSystem.SyncedConfig.DamageIfNoEnergyAndStarving);
                }

                // If hunger dosn't matter
                if (ConfigSystem.SyncedConfig.EnergyKills && !ConfigSystem.SyncedConfig.HungerLevelMatters)
                {
                    this.entity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Unknown,
                        Type = EnumDamageType.Suffocation
                    }, ConfigSystem.SyncedConfig.DamageIfNoEnergyAndHungerDoesNotMatter);
                }
            }
        }

        // Checks to apply penalty from dying.
        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                if (damageSource.Source == EnumDamageSource.Revive)
                {
                    this.HasRevived = true;
                    this.EnergyLossDelay = 120f;
                    if (ConfigSystem.SyncedConfig.LoseInvigorationWhenDying)
                    {
                        this.Invigorated = 0f; // Lose all invigoration
                    }
                    this.Fatigued = false;
                    this.AccumulatedDamageEnergy = 0f;
                    this.AccumulatedDamageInvigoration = 0f;
                }

                var healthBehavior = this.entity.GetBehavior<EntityBehaviorHealth>();
                if (damageSource.Type == EnumDamageType.Heal && damageSource.Source == EnumDamageSource.Block)
                {
                    this.EnergyLossDelay = 0f;
                }
                else if (damageSource.Type == EnumDamageType.Heal && healthBehavior != null)
                {
                    this.EnergyLossDelay = 60f;
                    if (ConfigSystem.SyncedConfig.DrainEnergyWhenHealing)
                    {
                        this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - (((this.CurrentEnergy / 2f) * ((((damage) / healthBehavior.MaxHealth) + (1f - this.HealthRatio)) / 2f)) * ConfigSystem.SyncedConfig.EnergyDrainFromHealingModifier), 0f, this.MaxEnergy);
                    }
                    if (ConfigSystem.SyncedConfig.DrainInvigorationWhenHealing)
                    {
                        this.Invigorated = Math.Clamp(this.Invigorated - ((ConfigSystem.SyncedConfig.MaxEnergy / 5f) * ConfigSystem.SyncedConfig.InvigorationDrainFromHealingModifier), 0f, ConfigSystem.SyncedConfig.MaxEnergy);
                    }
                }
                else if (healthBehavior != null && ConfigSystem.SyncedConfig.DrainEnergyWhenTakingDamage && ConfigSystem.SyncedConfig.EnableAdrenalineRush && (damageSource.Type == EnumDamageType.SlashingAttack || damageSource.Type == EnumDamageType.BluntAttack || damageSource.Type == EnumDamageType.PiercingAttack))
                {

                    this.FightDuration = 0f;
                    this.IsFighting = true;
                    this.DamageToEnergyDuringFight = Math.Clamp(((this.CurrentEnergy * (damage / healthBehavior.MaxHealth)) * ConfigSystem.SyncedConfig.EnergyDrainFromDamageMultiplier), 0f, this.MaxEnergy);
                    this.DamageToInvigorationDuringFight = Math.Clamp(((this.Invigorated * (2f * (damage / healthBehavior.MaxHealth))) * ConfigSystem.SyncedConfig.InvigoratedDrainFromDamageMultiplier), 0f, ConfigSystem.SyncedConfig.MaxEnergy);
                    this.AccumulatedDamageEnergy += this.DamageToEnergyDuringFight;
                    this.AccumulatedDamageInvigoration += this.DamageToInvigorationDuringFight;
                    this.EnergyLossDelay = 240f;
                    if (this.IsFighting)
                    {
                        this.ShowAdrenalineMessage("sn_adrenaline_start", Lang.Get(BtCore.Modid + ":sn_adrenaline_start"));
                        this.Adrenaline = this.IsFighting;
                    }

                }
                else
                {
                    if (ConfigSystem.SyncedConfig.DrainEnergyWhenTakingDamage && damage > 0f && healthBehavior != null)
                    {

                        if (!this.Starving && !this.Fatigued)
                        {
                            this.EnergyLossDelay = 120f;
                            this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - ((this.CurrentEnergy * (damage / healthBehavior.MaxHealth)) * ConfigSystem.SyncedConfig.EnergyDrainFromDamageMultiplier), 0f, this.MaxEnergy);
                            this.Invigorated = Math.Clamp(this.Invigorated - ((this.Invigorated * (2f * (damage / healthBehavior.MaxHealth))) * ConfigSystem.SyncedConfig.InvigoratedDrainFromDamageMultiplier), 0f, ConfigSystem.SyncedConfig.MaxEnergy);
                        }
                        else if (this.Starving && !this.Fatigued)
                        {
                            this.EnergyLossDelay = 120f;
                            this.CurrentEnergy = Math.Clamp(this.CurrentEnergy - ((this.MaxEnergy * (damage / healthBehavior.MaxHealth)) * ConfigSystem.SyncedConfig.EnergyDrainFromDamageMultiplier), 0f, this.MaxEnergy);
                            this.Invigorated = Math.Clamp(this.Invigorated - ((this.MaxEnergy * (2f * (damage / healthBehavior.MaxHealth))) * ConfigSystem.SyncedConfig.InvigoratedDrainFromDamageMultiplier), 0f, ConfigSystem.SyncedConfig.MaxEnergy);
                        }
                        else if (!this.Starving && this.Fatigued)
                        {
                            this.EnergyLossDelay = 120f;
                            this.Invigorated = 0f;
                        }
                        else if (this.Starving && this.Fatigued)
                        {
                            this.EnergyLossDelay = 120f;
                            this.Invigorated = 0f;
                            EntityBehaviorHunger nutrition = this.entity.GetBehavior<EntityBehaviorHunger>();
                            if (nutrition != null)
                            {
                                // Fruit
                                if (nutrition.FruitLevel > 0f)
                                {
                                    nutrition.FruitLevel = Math.Clamp(nutrition.FruitLevel - (nutrition.FruitLevel * (damage / healthBehavior.MaxHealth)), 0f, nutrition.MaxSaturation);
                                }
                                // Vegetable
                                if (nutrition.VegetableLevel > 0f)
                                {
                                    nutrition.VegetableLevel = Math.Clamp(nutrition.VegetableLevel - (nutrition.VegetableLevel * (damage / healthBehavior.MaxHealth)), 0f, nutrition.MaxSaturation);
                                }
                                // Protein
                                if (nutrition.ProteinLevel > 0f)
                                {
                                    nutrition.ProteinLevel = Math.Clamp(nutrition.ProteinLevel - (nutrition.ProteinLevel * (damage / healthBehavior.MaxHealth)), 0f, nutrition.MaxSaturation);
                                }
                                // Grain
                                if (nutrition.GrainLevel > 0f)
                                {
                                    nutrition.GrainLevel = Math.Clamp(nutrition.GrainLevel - (nutrition.GrainLevel * (damage / healthBehavior.MaxHealth)), 0f, nutrition.MaxSaturation);
                                }
                                // Dairy
                                if (nutrition.DairyLevel > 0f)
                                {
                                    nutrition.DairyLevel = Math.Clamp(nutrition.DairyLevel - (nutrition.DairyLevel * (damage / healthBehavior.MaxHealth)), 0f, nutrition.MaxSaturation);
                                }

                            }

                        }


                    }

                }
            }
        }

        private void ShowAdrenalineMessage(string notificationCode, string message)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy)
            {
                // Loggen viser, at vi er på serveren, så vi tjekker kun for server-siden.
                if (this.sapi != null)
                {
                    // 1. Opret pakken
                    var packet = new AdrenalineMessagePacket
                    {
                        Code = notificationCode,
                        Message = message
                    };

                    // 2. Få fat i kanalen
                    var channel = this.sapi.Network.GetChannel("SleepNeedAdrenaline") as IServerNetworkChannel;

                    // 3. Find spilleren og send KUN til dem
                    if (this.entity is EntityPlayer player && player.Player is IServerPlayer serverPlayer)
                    {
                        channel?.SendPacket(packet, new IServerPlayer[] { serverPlayer });

                        // Log til server-main.log
                        this._api.Logger.Notification($"[SleepNeed: Adrenaline-LOG] Server sending message: {notificationCode} to player {serverPlayer.PlayerName}.");
                    }
                }
            }
        }



        private ITreeAttribute _energyTree;

        private ICoreClientAPI capi;

        private ICoreServerAPI sapi;

        private EntityAgent _entityAgent;

        private float PreviousEnergyAmount;

        private bool updatedStatsFromStart;

        private float FightDuration;

        private float AccumulatedDamageEnergy;

        private float AccumulatedDamageInvigoration;

        private float DamageToEnergyDuringFight;

        private float DamageToInvigorationDuringFight;

        private bool wasFatigued;

        private static double _sprintSpeedMultiplier;
        private bool HasRevived = false;

        private bool IsFighting;

        private bool HotOrCold;

        private bool _resistheatRemoved;

        private float num3;
        private float num4;

        private float EnergyRate;

        private float EnergyRateUpdate;

        public float EnergyrateHungerFactor;

        private bool _energyrateHungerRemoved;

        private float lowHungerEnergyrate;

        public float EnergyRatioHighEnergy;

        public float EnergyRatioHighEnergyPart2;

        public float EnergyRatioLowEnergy;

        public float EnergyRatioLowEnergyPart2;

        private float NutrientHealthMod;

        private float HealthRatio;

        public float SatRatio;

        public float EnergyRatio;

        private float InvigoRatio;

        private float NutrientRatio;

        private float SleepRatio;

        public float BoostLungCapacity;

        private float BoostHealthPoints;

        private float BoostHealingEffectivness;

        private int lungCapacity;

        public float EnergyRatioForSleepiness { get; private set; }

        private float TemperatureDifference;

        public float EnergyRestored { get; private set; }

        public bool Starving { get; private set; }

        public bool Famine { get; private set; }

        public bool Fatigued { get; private set; }

        private float _startStatsTimer;

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

        private float _previousFruitLevel = -1f;

        private ICoreAPI _api;

        


        // Currently not used as it is hard to balance
        public StatMultiplier HungerrateMultiplierHighEnergy = new StatMultiplier
        {
            Multiplier = ConfigSystem.SyncedConfig.HungerRateReductionFromHighEnergy,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = true
        };
        public StatMultiplier HungerrateMultiplierLowEnergy = new StatMultiplier
        {
            Multiplier = ConfigSystem.SyncedConfig.HungerRateGainFromLowEnergy,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };

    }
}
