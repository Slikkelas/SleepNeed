using SleepNeed.Config;
using SleepNeed.Energy;
using SleepNeed.Systems;
using SleepNeed.Util;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;
using static System.Net.Mime.MediaTypeNames;


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
                float sleepinesscapacityOverload = (float)Math.Round((double)(ConfigSystem.SyncedConfig.SleepinessCapacityOverload * this.SleepinessCapacity));
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
         public bool PlayerIsBuildingGroundBed
         {
            get
            {
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree == null)
                {
                    return false;
                }
                return sleepinessTree.GetBool("buildinggroundbed", false);
            }
            set
            {
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetBool("buildinggroundbed", value);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
            }
         }
        

        public EntityBehaviorSleepiness(Entity entity) : base(entity)
        {
        }


        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            if (!ConfigSystem.SyncedConfig.EnableSleepiness)
            {
                return;
            }
            this._sleepinessTree = this.entity.WatchedAttributes.GetTreeAttribute(this.AttributeKey);
            this._api = this.entity.World.Api;
            this.capi = this._api as ICoreClientAPI;
            this.sapi = this._api as ICoreServerAPI;
            bool isClient = this.entity.World.Side == EnumAppSide.Client;
            bool invalidTree = this._sleepinessTree == null || this._sleepinessTree.GetFloat("sleepinesscapacity", 0f) == 0f || this._sleepinessTree.GetFloat("sleepinesscapacityoverload", 0f) == 0f;
            if (invalidTree && (!isClient || this._sleepinessTree == null))
            {
                if (this._sleepinessTree == null)
                {
                    this.entity.WatchedAttributes.SetAttribute(this.AttributeKey, this._sleepinessTree = new TreeAttribute());
                }

                this.CurrentSleepinessLevel = typeAttributes["currentsleepinesslevel"].AsFloat(0f);
                this.SleepinessCapacityModifier = typeAttributes["sleepinesscapacitymodifier"].AsFloat(1f);
            }
            
            if (this._sleepinesslistenerId != 0)
            {
                this.entity.World.UnregisterGameTickListener(this._sleepinesslistenerId);
            }
            if (this.entity is EntityPlayer)
            {
                this._sleepinesslistenerId = this.entity.World.RegisterGameTickListener(new Action<float>(this.SlowTick), 300, 0);
            }
            this._hoursTotal = this.entity.World.Calendar.TotalHours;
            this._hoursPerDay = this.entity.World.Calendar.HoursPerDay;
            if (ConfigSystem.SyncedConfig != null)
            {
                this.ConfigCapacity = ConfigSystem.SyncedConfig.MaxSleepiness / this._hoursPerDay;
            }
            else
            {
                this.ConfigCapacity = 14f / this._hoursPerDay;
            }
        }
        
        public override void OnGameTick(float deltaTime)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableSleepiness)
            {
                if (this.HasRevivedSleepiness)
                {
                    this.CurrentSleepinessLevel = this.EffectiveSleepinessCapacity * ConfigSystem.SyncedConfig.SleepinessAfterRevival;
                    if (this.entity != null && !ConfigSystem.SyncedConfig.EnableEnergy)
                    {
                        EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                        if (tirednessBehavior != null)
                        {
                            tirednessBehavior.Tiredness = ConfigSystem.SyncedConfig.TirednessAfterRevival;
                        }
                    }
                }
            } 
        }

        private void SlowTick(float dt)
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableSleepiness)
            {
                if (this.entity != null && ConfigSystem.SyncedConfig.DisableTiredness && !ConfigSystem.SyncedConfig.EnableEnergy)
                {
                    EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                    if (tirednessBehavior != null)
                    {
                        tirednessBehavior.Tiredness = 10f;
                    }
                }
                EntityPlayer player = this.entity as EntityPlayer;
                this.RefreshedThreshold = ConfigSystem.SyncedConfig.FeelingRefreshedHours / this.EffectiveSleepinessCapacity;
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
                        this._hoursTotal = this.entity.World.Calendar.TotalHours;
                        return;
                    }
                    else
                    {
                        this._justWokeUp = false; // Delay finished, resume normal logic
                        this.HasRevivedSleepiness = false;
                        if (this.entity != null)
                        {
                            EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                            if (tirednessBehavior != null)
                            {
                                tirednessBehavior.Tiredness = ConfigSystem.SyncedConfig.TirednessAfterSleep;
                            }
                        }
                    }
                }

                if (hoursPassed > 0.0f && this._isSleepingNow == false)
                {
                    if (ConfigSystem.SyncedConfig.EnableEnergy)
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
                    if (ConfigSystem.SyncedConfig.EnableEnergy)
                    {
                        var energy = entity.GetBehavior<SleepNeed.Energy.EntityBehaviorEnergy>();
                        if (energy != null)
                        {
                            this.SleepinessFactor = (1f - (((1f - (this.SleepinessRatio)) + energy.EnergyRatio + energy.OverallHealthRatio) / 3f)); // Used for Sleepiness gain
                            if (energy.EnergyRatio >= 0.0f && energy.EnergyRatio < 0.7f) // From 0.1 to 0.0
                            {
                                this.SleepEnergyModifier = Math.Clamp(ConfigSystem.SyncedConfig.SleepDebuffFromLowEnergy + ((energy.EnergyRatio + energy.SatRatio) / 2f), 0.5f, 1.0f); // Changed 0.35f to 0.4f
                            }
                            else if (energy.EnergyRatio >= 0.7f)
                            {
                                this.SleepEnergyModifier = 1f + (ConfigSystem.SyncedConfig.SleepBoostFromHighEnergy * ((energy.EnergyRatioHighEnergyPart2 + energy.SatRatio) / 2f));
                            }
                            // else if (energy.EnergyRatio <= 0.1f)
                            // {
                            //    this.SleepEnergyModifier = 1f + (ConfigSystem.SyncedConfig.SleepDebuffFromLowEnergy * energy.EnergyRatioLowEnergyPart2);
                            // }

                            if (energy.SatRatio <= 0.2f)
                            {
                                this.CurrentSleepinessLevel = Math.Max(this.CurrentSleepinessLevel - (((hoursPassed * ConfigSystem.SyncedConfig.SleepRegenerationFactor) + (hoursPassed * energy.OverallHealthRatio)) * this.SleepEnergyModifier), ConfigSystem.SyncedConfig.FeelingRefreshedHours);
                            }
                            else
                            {
                                this.CurrentSleepinessLevel = Math.Max(this.CurrentSleepinessLevel - (((hoursPassed * ConfigSystem.SyncedConfig.SleepRegenerationFactor) + (hoursPassed * energy.OverallHealthRatio)) * this.SleepEnergyModifier), 0f);
                            }
                            float energyRestored = energy.EnergyRestored * (1f - this.SleepinessRatio);
                            if (energyRestored > 0f)
                            {
                                float energyRestoredfromSleepiness = energyRestored;
                                if (this.SleepinessRatio <= this.RefreshedThreshold)
                                {
                                    energyRestoredfromSleepiness = energyRestored * ConfigSystem.SyncedConfig.EnergyFromSleepWhenRefreshedModifier;
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
                        var healthBehavior = this.entity.GetBehavior<EntityBehaviorHealth>();
                        EntityBehaviorHunger hunger = this.entity.GetBehavior<EntityBehaviorHunger>();
                        float healthRatio = 0f;
                        float satRatio = 0f;
                        float nutrientRatio = 0f;
                        float sleepFactor = 0f;
                        if (healthBehavior != null && hunger != null)
                        {
                            healthRatio = Math.Clamp(healthBehavior.Health, 0f, healthBehavior.BaseMaxHealth) / healthBehavior.BaseMaxHealth;
                            satRatio = hunger.Saturation / hunger.MaxSaturation;
                            nutrientRatio = ((hunger.FruitLevel / hunger.MaxSaturation) + (hunger.VegetableLevel / hunger.MaxSaturation) + (hunger.ProteinLevel / hunger.MaxSaturation) + (hunger.GrainLevel / hunger.MaxSaturation) + (hunger.DairyLevel / hunger.MaxSaturation)) / 5f;
                            sleepFactor = ((healthRatio + satRatio + nutrientRatio) / 3f) * this.SleepinessRatio;
                        }
                        this.CurrentSleepinessLevel -= Math.Max((hoursPassed + sleepFactor) * ConfigSystem.SyncedConfig.SleepRegenerationFactor, 0f);
                    }

                }

                this._hoursTotal = this.entity.World.Calendar.TotalHours;
                EntityStats stats = this.entity?.Stats;
                SyncedTreeAttribute watchedAttributes = this.entity?.WatchedAttributes;
                if (stats == null || watchedAttributes == null)
                {
                    return;
                }
                this.WalkSpeedMultiplier.Multiplier = ConfigSystem.SyncedConfig.SleepinessWalkSpeedDebuff;
                this.RangedWeaponsAccMultiplier.Multiplier = ConfigSystem.SyncedConfig.SleepinessRangedWeaponsAccDebuff;
                this.RangedWeaponsSpeedMultiplier.Multiplier = ConfigSystem.SyncedConfig.SleepinessRangedWeaponsSpeedDebuff;

                this._statsDelayTimer += dt;
                

                if (!this.IsOverloaded())
                {
                    this.IsOverloadedForEnergy = false;
                    if (this.SleepinessRatio <= this.RefreshedThreshold)
                    {
                        if (this._statsDelayTimer > 10f)
                        {
                            if (!ConfigSystem.SyncedConfig.DisableStatChanges)
                            {
                                this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", ConfigSystem.SyncedConfig.SleepinessRangedWeaponsAccDebuff * 0.385f, false);
                            }

                            if (ConfigSystem.SyncedConfig.EnableEnergy)
                            {
                                var energy = entity.GetBehavior<SleepNeed.Energy.EntityBehaviorEnergy>();
                                if (!energy.Starving)
                                {
                                    float baseEnergyRate = 1.0f;
                                    float reductionFactor = (1f - Math.Clamp(this.SleepinessRatio / this.RefreshedThreshold, 0.0f, 1.0f));
                                    this.entity.Stats.Set(BtCore.Modid + ":energyrate", "sleepinessfull", -baseEnergyRate * reductionFactor, false);
                                }
                            }
                            this._sleepinessStatsRemove = false;
                            this._statsDelayTimer = 0f;
                        }
                    }
                    else if (this.SleepinessRatio > this.RefreshedThreshold && !this._sleepinessStatsRemove)
                    {
                        this.entity.Stats.Remove("rangedWeaponsAcc", "sleepinessfull");
                        this.entity.Stats.Remove("walkspeed", "sleepinessfull");
                        this.entity.Stats.Remove("rangedWeaponsSpeed", "sleepinessfull");
                        if (ConfigSystem.SyncedConfig.EnableEnergy)
                        {
                            this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "sleepinessfull");
                        }
                        this._sleepinessStatsRemove = true;
                        this._statsDelayTimer = 0f;
                    }
                }
                else if (this.IsOverloaded())
                {
                    this.IsOverloadedForEnergy = true;
                    this._sleepinessStatsRemove = false;
                    float energyrateSleepinessFactor = ((ConfigSystem.SyncedConfig.SleepinessEnergyrateDebuff / 100f) - 1f) * this.SleepinessOverloadRatio;
                    if (this._statsDelayTimer > 10f)
                    {
                        if (!ConfigSystem.SyncedConfig.DisableStatChanges)
                        {
                            this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", this.RangedWeaponsAccMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                        }


                        if (ConfigSystem.SyncedConfig.EnableEnergy)
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

                        this._statsDelayTimer = 0f;
                    }
                }
            }
        }

        private bool IsOverloaded()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            else if (ConfigSystem.SyncedConfig.EnableSleepiness)
            {
                return this.CurrentSleepinessLevel > this.SleepinessCapacity;
            }
            return false;
        }

        

        private void UpdateIsSleepingNow()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableSleepiness)
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
            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableSleepiness)
            {
                if (damageSource.Source == EnumDamageSource.Revive)
                {
                    this.HasRevivedSleepiness = true;
                }
                if (this.entity != null)
                {
                    EntityBehaviorTiredness tirednessBehavior = this.entity.GetBehavior<EntityBehaviorTiredness>();
                    if ((damageSource.Type == EnumDamageType.Heal && damageSource.Source == EnumDamageSource.Block) && tirednessBehavior != null)
                    {
                        tirednessBehavior.Tiredness = Math.Max(0f, tirednessBehavior.Tiredness + damage);
                    }
                    else if (damageSource.Type == EnumDamageType.Heal && ConfigSystem.SyncedConfig.GainSleepinessWhenHealing)
                    {
                        this.CurrentSleepinessLevel += (damage / 2f) * ConfigSystem.SyncedConfig.GainSleepinessWhenHealingModifier; 
                        tirednessBehavior.Tiredness = Math.Max(0f, tirednessBehavior.Tiredness + damage);
                    }
                }
            }
        }


        private ITreeAttribute _sleepinessTree;

        private ICoreAPI _api;

        private ICoreClientAPI capi;

        private ICoreServerAPI sapi;

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
        private float _statsDelayTimer = 0f;
        private float WakeDelaySeconds { get; } = ConfigSystem.SyncedConfig.DelaySeconds; 

        private long _sleepinesslistenerId;

        private EntityAgent _entityAgent;

        // Private field to store the sleeping state
        private bool _isSleepingNow = false;

     
        public StatMultiplier WalkSpeedMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.SyncedConfig.SleepinessWalkSpeedDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
        public StatMultiplier RangedWeaponsAccMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.SyncedConfig.SleepinessRangedWeaponsAccDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
        public StatMultiplier RangedWeaponsSpeedMultiplier = new StatMultiplier
        {
            Multiplier = ConfigSystem.SyncedConfig.SleepinessRangedWeaponsSpeedDebuff,
            Centering = EnumUpOrDown.Centered,
            Curve = EnumBuffCurve.Linear,
            Inverted = false
        };
    }


}
