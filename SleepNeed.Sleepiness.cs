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
                float sleepinesscapacityOverload = (float)Math.Round((double)(this.SleepinessCapacityModifier * ConfigSystem.ConfigServer.SleepinessCapacityOverload * this.SleepinessCapacity));
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("sleepinesscapacityoverload", sleepinesscapacityOverload);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
                return sleepinesscapacityOverload;
            }
        }

        public float SleepinessCapacity
        {
            get
            {
                float sleepinessCapacity = (float)Math.Round((double)(this.SleepinessCapacityModifier * this._hoursPerDay / 2f));
                ITreeAttribute sleepinessTree = this._sleepinessTree;
                if (sleepinessTree != null)
                {
                    sleepinessTree.SetFloat("sleepinesscapacity", sleepinessCapacity);
                }
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
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
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
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
                this.entity.WatchedAttributes.MarkPathDirty(this.AttributeKey);
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
                _justWokeUp = true;
                _wakeDelayTimer = 0f;
            }

            // Handle delay after waking up
            if (_justWokeUp)
            {
                _wakeDelayTimer += dt;
                if (_wakeDelayTimer < WakeDelaySeconds)
                {
                    // Skip the "awake" logic during the delay
                    this._hoursTotal = this.entity.World.Calendar.TotalHours;
                    return;
                }
                else
                {
                    _justWokeUp = false; // Delay finished, resume normal logic
                }
            }
            
            if (hoursPassed > 0.0f && this._isSleepingNow == false)
            {
                // * 0.75f is the rate at which sleepiness increases per hour when not sleeping
                this.CurrentSleepinessLevel = GameMath.Clamp(this.CurrentSleepinessLevel + hoursPassed * 0.75f, 0f, this.EffectiveSleepinessCapacity);
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
                        if (energy.EnergyRatio > 0.1f && energy.EnergyRatio < 0.7f)
                        {
                            this.SleepEnergyModifier = 1f;
                        }
                        else if (energy.EnergyRatio >= 0.7f)
                        {
                            this.SleepEnergyModifier = 1f + (ConfigSystem.ConfigServer.SleepBoostFromHighEnergy * energy.EnergyRatioHighEnergyPart2);
                        }
                        else if (energy.EnergyRatio <= 0.1f)
                        {
                            this.SleepEnergyModifier = 1f + (ConfigSystem.ConfigServer.SleepDebuffFromLowEnergy * energy.EnergyRatioLowEnergyPart2);
                        }

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
                    
                    
                    this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", ConfigSystem.ConfigServer.SleepinessRangedWeaponsAccDebuff * 0.385f, false);
                    if (ConfigSystem.ConfigServer.EnableEnergy)
                    {
                        var energy = entity.GetBehavior<SleepNeed.Energy.EntityBehaviorEnergy>();
                        if (!energy.Starving)
                        {
                            float energyrate = (this.entity.Stats.GetBlended("energyrate"));
                            this.entity.Stats.Set(BtCore.Modid + ":energyrate", "sleepinessfull", -energyrate * (1f - Math.Clamp(this.SleepinessRatio / this.RefreshedThreshold, 0.0f, 1.0f)), false);
                            
                        }
                    }
                    
                }
                else
                {
                    this.entity.Stats.Remove("rangedWeaponsAcc", "sleepinessfull");
                    this.entity.Stats.Remove("walkspeed", "sleepinessfull");
                    this.entity.Stats.Remove("rangedWeaponsSpeed", "sleepinessfull");
                    if (ConfigSystem.ConfigServer.EnableEnergy)
                    {
                        this.entity.Stats.Remove(BtCore.Modid + ":energyrate", "sleepinessfull");
                    }
                    
                }
                
            }
            else if (this.IsOverloaded())
            {
                this.IsOverloadedForEnergy = true;
                float energyrateSleepinessFactor = ((ConfigSystem.ConfigServer.SleepinessEnergyrateDebuff / 100f) - 1f) * this.SleepinessOverloadRatio;
                
                this.entity.Stats.Set("rangedWeaponsAcc", "sleepinessfull", this.RangedWeaponsAccMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                
                if (ConfigSystem.ConfigServer.EnableEnergy)
                {
                    this.entity.Stats.Set(BtCore.Modid + ":energyrate", "sleepinessfull", energyrateSleepinessFactor, false);
                }
                else
                {
                    this.entity.Stats.Set("walkspeed", "sleepinessfull", this.WalkSpeedMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
                    this.entity.Stats.Set("rangedWeaponsSpeed", "sleepinessfull", this.RangedWeaponsSpeedMultiplier.CalcModifier(this.SleepinessOverloadRatio), false);
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




        private ITreeAttribute _sleepinessTree;

        private ICoreAPI _api;

        public Random Rand;

        private float SleepinessOverloadRatio;

        private float OverloadThreshold;

        public bool IsOverloadedForEnergy;

        public float SleepinessRatio;

        public float RefreshedThreshold;

        private float SleepEnergyModifier;
        public float LastHoursPassed { get; private set; }

        public float EnergyRestoredfromSleepiness { get; private set; }

        private double _hoursTotal;

        private float _hoursPerDay;

        private bool _justWokeUp = false;
        private float _wakeDelayTimer = 0f;
        private const float WakeDelaySeconds = 5f; // Set your desired delay in seconds

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
