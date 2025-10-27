using SleepNeed.Systems;
using SleepNeed.Energy;
using SleepNeed.Util;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;
using Vintagestory.API.Config;
using SleepNeed.Config;


namespace SleepNeed.Sleepiness
{
    public class EntityBehaviorSleepiness : EntityBehavior
    {
        public override string PropertyName()
        {
            return this.AttributeKey;
        }

        private string AttributeKey
        {
            get
            {
                return BtCore.Modid + ":sleepiness";
            }
        }

        public float SleepinessCapacityOverload
        {
            get
            {
                float sleepinesscapacityOverload = (float)Math.Round((double)(ConfigSystem.ConfigServer.SleepinessCapacityOverload * this.SleepinessCapacity));
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("sleepinesscapacityoverload", sleepinesscapacityOverload);
                }
                
                return sleepinesscapacityOverload;
            }
        }

        public float SleepinessCapacity
        {
            get
            {
                float sleepinessCapacity = (float)Math.Round((double)(this.SleepinessCapacityModifier * this._hoursPerDay * this.ConfigCapacity));
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("sleepinesscapacity", sleepinessCapacity);
                }
                
                return sleepinessCapacity;
            }
        }

        public float CurrentSleepinessLevel
        {
            get
            {
                
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                return Math.Min((sleepinessTree != null) ? sleepinessTree.GetFloat("currentsleepinesslevel", 0f) : 0f, this.EffectiveSleepinessCapacity);
            }
            set
            {
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("currentsleepinesslevel", Math.Min(value, this.EffectiveSleepinessCapacity));
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
        }

        public float SleepinessCapacityModifier
        {
            get
            {
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree == null)
                {
                    return 1f;
                }
                return sleepinessTree.GetFloat("sleepinesscapacitymodifier", 0f);
            }
            set
            {
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("sleepinesscapacitymodifier", value);
                }
                
            }
        }

        public float EffectiveSleepinessCapacity
        {
            get
            {
                float effectiveSleepinessCapacity = this.SleepinessCapacity + this.SleepinessCapacityOverload;
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("effectivesleepinesscapacity", effectiveSleepinessCapacity);
                }
                
                return effectiveSleepinessCapacity;
            
            }
        }

        public EntityBehaviorSleepiness(Entity entity) : base(entity)
        {
        }


        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {

             
            this._sleepinessTree = this.entity.WatchedAttributes.GetTreeAttribute(this.AttributeKey);
            this._api = this.entity.World.Api;
            
            if (this._sleepinessTree == null || this._sleepinessTree.GetFloat("sleepinesscapacity", 0f) == 0f || this._sleepinessTree.GetFloat("sleepinesscapacityoverload", 0f) == 0f)
            {
                this.entity.WatchedAttributes.SetAttribute(this.AttributeKey, this._sleepinessTree = new TreeAttribute());
                this.CurrentSleepinessLevel = typeAttributes["currentsleepinesslevel"].AsFloat(0f);
                this.SleepinessCapacityModifier = typeAttributes["sleepinesscapacitymodifier"].AsFloat(1f);
            }

            // Play with tickrate. It impacts the decrease in sleepiness when sleeping and the increase when not sleeping.
            if (this.entity is EntityPlayer)
            {
                this._sleepinesslistenerId = this.entity.World.RegisterGameTickListener(new Action<float>(this.SlowTick), 300, 0);
            }
            this._hoursTotal = this.entity.World.Calendar.TotalHours;
            this._hoursPerDay = this.entity.World.Calendar.HoursPerDay;
            this.ConfigCapacity = ConfigSystem.ConfigServer.MaxSleepiness / this._hoursPerDay;




        }
        
        public override void OnGameTick(float deltaTime)
        {
            if (this.HasRevivedSleepiness)
            {
                this.CurrentSleepinessLevel = this.EffectiveSleepinessCapacity * ConfigSystem.ConfigServer.SleepinessAfterRevival;
            }
        }

        private void SlowTick(float dt)
        {
            EntityPlayer player = this.entity as EntityPlayer;
            this.RefreshedThreshold = ConfigSystem.ConfigServer.FeelingRefreshedHours / this.EffectiveSleepinessCapacity;
            this.OverloadThreshold = 1f - (this.SleepinessCapacityOverload / this.EffectiveSleepinessCapacity);
            float hoursPassed = (float)(this.entity.World.Calendar.TotalHours - this._hoursTotal);
            this.LastHoursPassed = hoursPassed;
            // Detect transition from sleeping to awake
            bool wasSleeping = this._isSleepingNow;
            UpdateIsSleepingNow();
            bool isSleeping = this._isSleepingNow;

            if (wasSleeping && !isSleeping)
            {
                this._justWokeUp = true;
                this._wakeDelayTimer = 0f;
            }

            // Handle delay after waking up
            if (this._justWokeUp || this.HasRevivedSleepiness)
            {
                this._wakeDelayTimer += dt;
                if (this._wakeDelayTimer < WakeDelaySeconds)
                {
                    // Skip the "awake" logic during the delay
                    this._hoursTotal = this.entity.World.Calendar.TotalHours;
                    return;
                }
                else
                {
                    this._justWokeUp = false; // Delay finished, resume normal logic
                    this.HasRevivedSleepiness = false;
                }
            }
            
            if (hoursPassed > 0.0f && this._isSleepingNow == false)
            {
                if (ConfigSystem.ConfigServer.EnableEnergy)
                {
                    // This is to slow down sleepiness gain if ou are healthy and well. May ease up early game.
                    this.CurrentSleepinessLevel = GameMath.Clamp(this.CurrentSleepinessLevel + hoursPassed * Math.Clamp(this.SleepinessFactor, 0.55f, 0.90f), 0f, this.EffectiveSleepinessCapacity); // Changed from 0.60 to 0.85, wich is in the current 2.0.0 version
                }
                else
                {
                    // * 0.75f is the rate at which sleepiness increases per hour when not sleeping
                    this.CurrentSleepinessLevel = GameMath.Clamp(this.CurrentSleepinessLevel + hoursPassed * 0.70f, 0f, this.EffectiveSleepinessCapacity);
                }
            }
            this.SleepinessRatio = this.CurrentSleepinessLevel / this.EffectiveSleepinessCapacity;
            this.SleepinessOverloadRatio = Math.Clamp(this.OverloadThreshold * (this.CurrentSleepinessLevel - (this.OverloadThreshold * this.EffectiveSleepinessCapacity)) / ((this.SleepinessCapacityOverload / this.EffectiveSleepinessCapacity) * this.EffectiveSleepinessCapacity) + this.OverloadThreshold, this.OverloadThreshold, 1f);
            
            if (hoursPassed > 0.0f && this._isSleepingNow == true)
            {
                // * 1.5f is the rate at which sleepiness decreases per hour when sleeping
                if (ConfigSystem.ConfigServer.EnableEnergy)
                {
                    var energy = entity.GetBehavior<SleepNeed.Energy.EntityBehaviorEnergy>();
                    if (energy != null)
                    {
                        this.SleepinessFactor = (1f - (((1f - (this.SleepinessRatio)) + energy.EnergyRatio + energy.OverallHealthRatio) / 3f)); // Used for Sleepiness gain
                        if (energy.EnergyRatio >= 0.0f && energy.EnergyRatio < 0.7f) // From 0.1 to 0.0
                        {
                            this.SleepEnergyModifier = Math.Clamp(ConfigSystem.ConfigServer.SleepDebuffFromLowEnergy + ((energy.EnergyRatio + energy.SatRatio) / 2f), 0.5f, 1.0f); // Changed 0.35f to 0.4f
                        }
                        else if (energy.EnergyRatio >= 0.7f)
                        {
                            this.SleepEnergyModifier = 1f + (ConfigSystem.ConfigServer.SleepBoostFromHighEnergy * ((energy.EnergyRatioHighEnergyPart2 + energy.SatRatio) / 2f));
                        }
                        // else if (energy.EnergyRatio <= 0.1f)
                        // {
                        //    this.SleepEnergyModifier = 1f + (ConfigSystem.ConfigServer.SleepDebuffFromLowEnergy * energy.EnergyRatioLowEnergyPart2);
                        // }

                        if (energy.SatRatio <= 0.2f)
                        {
                            this.CurrentSleepinessLevel = Math.Max(this.CurrentSleepinessLevel - (((hoursPassed * ConfigSystem.ConfigServer.SleepRegenerationFactor) + (hoursPassed * energy.OverallHealthRatio)) * this.SleepEnergyModifier), ConfigSystem.ConfigServer.FeelingRefreshedHours);
                        }
                        else
                        {
                            this.CurrentSleepinessLevel = Math.Max(this.CurrentSleepinessLevel - (((hoursPassed * ConfigSystem.ConfigServer.SleepRegenerationFactor) + (hoursPassed * energy.OverallHealthRatio)) * this.SleepEnergyModifier), 0f);
                        }
                        float energyRestored = energy.EnergyRestored * (1f - this.SleepinessRatio);
                        if (energyRestored > 0f)
                        {
                            float energyRestoredfromSleepiness = energyRestored;
                            if (this.SleepinessRatio <= this.RefreshedThreshold)
                            {
                                energyRestoredfromSleepiness = energyRestored * ConfigSystem.ConfigServer.EnergyFromSleepWhenRefreshedModifier;
                            }
                            this.EnergyRestoredfromSleepiness = energyRestored + (energyRestoredfromSleepiness * energy.OverallHealthRatio);
                        }
                        else
                        {
                            this.EnergyRestoredfromSleepiness = 0f;
                        }
                    }
                    
                }
                else
                {
                    this.CurrentSleepinessLevel = Math.Max(this.CurrentSleepinessLevel - hoursPassed * ConfigSystem.ConfigServer.SleepRegenerationFactor, 0f);
                }
                
            }
            
            this._hoursTotal = this.entity.World.Calendar.TotalHours;
            EntityStats stats = this.entity?.Stats;
            SyncedTreeAttribute watchedAttributes = this.entity?.WatchedAttributes;
            if (stats == null || watchedAttributes == null)
            {
                return;
            }
            this.WalkSpeedMultiplier.Multiplier = ConfigSystem.ConfigServer.SleepinessWalkSpeedDebuff;
            this.RangedWeaponsAccMultiplier.Multiplier = ConfigSystem.ConfigServer.SleepinessRangedWeaponsAccDebuff;
            this.RangedWeaponsSpeedMultiplier.Multiplier = ConfigSystem.ConfigServer.SleepinessRangedWeaponsSpeedDebuff;
            if (!this.IsOverloaded())
            {
                this.IsOverloadedForEnergy = false;
                if (this.SleepinessRatio <= this.RefreshedThreshold)
                {
                    if (!ConfigSystem.ConfigServer.DisableStatChanges)
                    {
                        this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", ConfigSystem.ConfigServer.SleepinessRangedWeaponsAccDebuff * 0.385f, false);
                    }
                    
                    if (ConfigSystem.ConfigServer.EnableEnergy)
                    {
                        var energy = entity.GetBehavior<SleepNeed.Energy.EntityBehaviorEnergy>();
                        if (!energy.Starving)
                        {
                            float energyrate = (this.entity.Stats.GetBlended("energyrate"));
                            this.entity.Stats.Set(BtCore.Modid + ":energyrate", "sleepinessfull", -energyrate * (1f - Math.Clamp(this.SleepinessRatio / this.RefreshedThreshold, 0.0f, 1.0f)), false);
                            
                        }
                    }
                    this._sleepinessStatsRemove = false;
                }
                else if (this.SleepinessRatio > this.RefreshedThreshold && !this._sleepinessStatsRemove)
                {
                    this.entity.Stats.Remove("rangedWeaponsAcc", "sleepinessfull");
                    this.entity.Stats.Remove("walkspeed", "sleepinessfull");
                    this.entity.Stats.Remove("rangedWeaponsSpeed", "sleepinessfull");
                    if (ConfigSystem.ConfigServer.EnableEnergy)
                    {
                        this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "sleepinessfull");
                    }
                    this._sleepinessStatsRemove = true;
                }
                
            }
            else if (this.IsOverloaded())
            {
                this.IsOverloadedForEnergy = true;
                this._sleepinessStatsRemove = false;
                float energyrateSleepinessFactor = ((ConfigSystem.ConfigServer.SleepinessEnergyrateDebuff / 100f) - 1f) * this.SleepinessOverloadRatio;

                if (!ConfigSystem.SyncedConfig.DisableStatChanges)
                {
                    this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", this.RangedWeaponsAccMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                }
                
                
                if (ConfigSystem.ConfigServer.EnableEnergy)
                {
                    this.entity.Stats.Set(BtCore.Modid + ":energyrate", "sleepinessfull", energyrateSleepinessFactor, false);
                }
                else
                {
                    if (!ConfigSystem.SyncedConfig.DisableStatChanges)
                    {
                        this.entity.Stats.Set("walkspeed", "sleepinessfull", this.WalkSpeedMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                        this.entity.Stats.Set("rangedWeaponsSpeed", "sleepinessfull", this.RangedWeaponsSpeedMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                    }
                    
                }

                    
            }
            


        }

        private bool IsOverloaded()
        {
            return this.CurrentSleepinessLevel > this.SleepinessCapacity;
        }

        

        private void UpdateIsSleepingNow()
        {
            if (this.entity == null)
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

        // Public property to read the current sleeping state
        public bool IsSleepingNow
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


        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            this.entity.World.UnregisterGameTickListener(this._sleepinesslistenerId);
        }

        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            if (damageSource.Source == EnumDamageSource.Revive)
            {
                this.HasRevivedSleepiness = true;
            }

            EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
            if (damageSource.Type == EnumDamageType.Heal && tirednessBehavior != null)
            {
                tirednessBehavior.Tiredness = Math.Max(0f, tirednessBehavior.Tiredness + damage);
                if (ConfigSystem.ConfigServer.GainSleepinessWhenHealing)
                {
                    this.CurrentSleepinessLevel += (damage / 2f) * ConfigSystem.ConfigServer.GainSleepinessWhenHealingModifier; // Add config to let the player choose the sleepiness gain rate.
                }
            }
        }


        private ITreeAttribute _sleepinessTree;

        private ICoreAPI _api;

        public Random Rand;

        private bool _sleepinessStatsRemove;

        private bool HasRevivedSleepiness;

        private float SleepinessFactor;

        private float ConfigCapacity;

        private float SleepinessOverloadRatio;

        public float OverloadThreshold;

        public bool IsOverloadedForEnergy;

        public float SleepinessRatio;

        public float RefreshedThreshold;

        private float SleepEnergyModifier;
        public float LastHoursPassed { get; private set; }

        public float EnergyRestoredfromSleepiness { get; private set; }

        private double _hoursTotal;

        private float _hoursPerDay;

        private bool sleepinessIsSet;

        private bool _justWokeUp = false;
        private float _wakeDelayTimer = 0f;
        private float WakeDelaySeconds { get; } = ConfigSystem.ConfigServer.DelaySeconds; // Set your desired delay in seconds

        private long _sleepinesslistenerId;

        private EntityAgent _entityAgent;

        // Private field to store the sleeping state
        private bool _isSleepingNow = false;

        
        public StatMultiplier WalkSpeedMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.ConfigServer.SleepinessWalkSpeedDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
        public StatMultiplier RangedWeaponsAccMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.ConfigServer.SleepinessRangedWeaponsAccDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
        public StatMultiplier RangedWeaponsSpeedMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.ConfigServer.SleepinessRangedWeaponsSpeedDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
    }


}
