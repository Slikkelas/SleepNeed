using SleepNeed.Energy;
using SleepNeed.Util;
using SleepNeed.Hud;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using System.IO;
using System.Runtime.InteropServices;

namespace SleepNeed.Config

{
    
    public class IModConfig
    {
    }

    
    public class ConfigClient : IModConfig
    {
        
        public float EnergyBarX { get; set; }

        public float EnergyBarY { get; set; }

        public float SleepinessBarX { get; set; }

        public float SleepinessBarY { get; set; }

        public bool EnergyBarFillDirectionRightToLeft { get; set; } = false;

        public bool SleepinessBarFillDirectionRightToLeft { get; set; } = true;

        public bool SleepinessBarVisible { get; set; } = true;

        public float HideSleepinessBarAt { get; set; }

        public string EnergyBarColor { get; set; } = ModGuiStyle.EnergyBarColor.ToHex();

        public string SleepinessBarColor { get; set; } = ModGuiStyle.SleepinessBarColor.ToHex();

        
        public ConfigClient(ICoreAPI api, ConfigClient previousConfig = null)
        {
            if (previousConfig == null)
            {
                return;
            }
            this.EnergyBarX = previousConfig.EnergyBarX;
            this.EnergyBarY = previousConfig.EnergyBarY;
            this.SleepinessBarX = previousConfig.SleepinessBarX;
            this.SleepinessBarY = previousConfig.SleepinessBarY;
            this.EnergyBarFillDirectionRightToLeft = previousConfig.EnergyBarFillDirectionRightToLeft;
            this.SleepinessBarFillDirectionRightToLeft = previousConfig.SleepinessBarFillDirectionRightToLeft;
            this.EnergyBarColor = previousConfig.EnergyBarColor;
            this.SleepinessBarColor = previousConfig.SleepinessBarColor;
            this.SleepinessBarVisible = previousConfig.SleepinessBarVisible;
            this.HideSleepinessBarAt = previousConfig.HideSleepinessBarAt;
            
        }
    }

    
    public class ConfigServer : SyncedConfig
    {
        
        public float MaxEnergy { get; set; } = 900f;

        public bool EnergyKills { get; set; } = true; // Only used if Hunger Matters is disabled

        public float DamageIfNoEnergyAndHungerDoesNotMatter { get; set; } = 0.15f; // If hunger matters is disabled, this is the applied damage when no energy.

        public float DamageIfNoEnergyAndStarving { get; set; } = 0.5f; // Damage pr. tick 

        public float EnergyDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the energy drain from healing, 1.0f means no change, 0.5f means half the energy drain, 2.0f means double the energy drain.

        public float InvigorationDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the invigoration drain from healing, 1.0f means no change, 0.5f means half the invigoration drain, 2.0f means double the invigoration drain.

        public bool LoseInvigorationWhenDying { get; set; } = true; // Lose invigoration when dying, this will reset the invigoration level to 0 when the player dies, so they will have to regain it again.
        
        public bool DrainInvigorationWhenHealing { get; set; } = true; // Lose invigoration when healing, this will hurt the invigoration when the player heals, so they will have to regain it again. This is useful for balancing the game, so players can't just spam healing and keep their invigoration level high.

        public float EnergySpeedModifier { get; set; } // Modifier for how fast energy drains. If this is not set, the speed is determined by GlobalConstants.HungerSpeedModifier

        public float SittingRelaxingSpeedModifier { get; set; } = 1.0f; // Modifier for how fast energy regenerates when sitting.

        public float AttackEnergyCostModifier { get; set; } = 1.0f; // Modifier for attack energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public float HeavyToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for heavy tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        
        public float MediumToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for medium tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        
        public float LightToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for light tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public float WeaponsEnergyCostModifier { get; set; } = 1.0f; // Modifier for weapons energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public float NoToolsWorkEnergyCostModifier { get; set; } = 1.0f; // Modifier for work without tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public float MovementEnergyCostModifier { get; set; } = 1.0f; // Modifier for movement energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public float SprintingJumpingEnergyCostModifier { get; set; } = 1.0f; // Modifier for sprinting and jumping energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.

        public bool BodyTemperatureMatters { get; set; } = true; // Enable the body temperature matters, which will make the body temperature of the player matter for the energy level, so if the player is too cold or too hot, they will have increased energy rate and will drain energy faster. 

        public float EnergyRatePerDegrees { get; set; } = 45f; // 45 = 450% So energy rate will increase 450% per degree of body temp outside normal core temp.
        
        public bool EnableInvigoratedHealthBoost { get; set; } = true; // Should the player gain extra health points with invigoration?

        public float InvigoratedHealthBoostPercentageOfMaxHealth { get; set; } = 0.5f; // 7.5 health points if full invigoration and well nutritious if max health is 15 hp.

        public bool EnableInvigoratedLungCapacityBoost { get; set; } = true; // Enable the invigorated lung capacity boost, which will boost the lung capacity of the player based on the configured lung capacity in the config file.

        public float InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity { get; set; } = 0.5f; // 50% of base lung capacity, so if the base lung capacity is 40.000 (40s), then it will be 60s.

        public bool EnableInvigoratedHealingEffectiveness { get; set; } = true; // Healing effectiveness increases with invigoration.

        public float InvigoratedHealingEffectivenessModifier { get; set; } = 1.0f; // 1.0 = default + 100% = 200% Healing Effectiveness 

        public bool EnableInvigoratedMaxEnergyBoost { get; set; } = true; // Should max energy increase with invigoration?

        public float InvigoratedMaxEnergyBoostModifier { get; set; } = 1.0f; // 1.0 means final value for max energy when fully invigorated equals to 1500.


        public bool EnableNutrientFactor { get; set; } = true; // Enable the nutrient factor, which will factor in invigoration based on the nutrient level of the player.

        public bool HungerLevelMatters { get; set; } = true; // Should hunger have an impact on the energy rate?

        public float NutritionLossWhenStarvingModifier { get; set; } = 10.0f; // Multiplier for how fast nutrition values should drain when starving.

        public bool OnlyDieFromNoEnergy { get; set; } = true; // If true, the player will only die from no energy, so if the player has no energy, they will die, but if they have energy, they will not die from hunger.

        public bool EnableEnergyloss { get; set; } = true; // As of now this has no effect.

        public float EnergyDrainFromDamageMultiplier { get; set; } = 0.1f; // Percentage of CurrentEnergy that will be drained when the player takes damage, 0.1f means 10% of CurrentEnergy will be drained when the player takes damage.

        public float InvigoratedDrainFromDamageMultiplier { get; set; } = 0.1f; // Percentage of Invigoration that will be drained when the player takes damage while invigorated, 0.1f means 10% of CurrentEnergy will be drained when the player takes damage while invigorated.

        public bool EnableEnergyDependedToolMiningSpeed { get; set; } = true; // Enable the energy depended tool mining speed, which will make the mining speed of tools depend on the current energy level of the player, so if the player has low energy, the mining speed will be lower. 

        public bool EnableEnergyDependedJumpHeight { get; set; } = true; // Enable the energy depended jump speed, which will make the jump speed of the player depend on the current energy level of the player, so if the player has low energy, the jump speed will be lower.

        public bool EnableEnergyDependedWalkSpeed { get; set; } = true; 

        public bool EnableEnergyDependedRangedWeaponSpeed { get; set; } = true;

        public bool EnableEnergyDependedRangedWeaponDamage { get; set; } = true;

        public bool EnableEnergyDependedMeleeWeaponDamage { get; set; } = true;

        public bool EnableEnergyDependedArmorWalkSpeedAffectedness { get; set; } = true;

        public bool EnableEnergyDependedBowDrawingStrength { get; set; } = true;

        public bool EnableEnergyDependedAnimalHarvestingTime { get; set; } = true;

        public float ToolMiningSpeedBoostFromEnergy { get; set; } = 0.2f; // The mining speed boost from high energy, value is added to base game value.

        public float ToolMiningSpeedDebuffFromEnergy { get; set; } = 0.95f; // The mining speed debuff from low energy, value is subtracted from the base game value.

        public float JumpHeightBoostFromEnergy { get; set; } = 0.39f; // 0.6 equals 2 block jump height. 0.39 equals 2 block jump height when refreshed boost is 1.35.

        public float JumpHeightDebuffFromEnergy { get; set; } = 0.6f; // 0.6 equals to less than 1 block jump height.

        public float WalkSpeedBoostFromEnergy { get; set; } = 0.15f; // 0.15 = 15% added to base walkspeed

        public float WalkSpeedDebuffFromEnergy { get; set; } = 0.45f; // 0.45 = 45% subtracted from base walk speed.

        public float RangedWeaponSpeedBoostFromEnergy { get; set; } = 0.25f; // 0.25 = 25%

        public float RangedWeaponSpeedDebuffFromEnergy { get; set; } = 0.75f; 

        public float RangedWeaponDamageBoostFromEnergy { get; set; } = 0.25f; 

        public float RangedWeaponDamageDebuffFromEnergy { get; set; } = 0.45f; 

        public float MeleeWeaponDamageBoostFromEnergy { get; set; } = 0.35f; 

        public float MeleeWeaponDamageDebuffFromEnergy { get; set; } = 0.65f; 

        public float ArmorWalkSpeedAffectednessBoostFromEnergy { get; set; } = 0.25f; 

        public float ArmorWalkSpeedAffectednessDebuffFromEnergy { get; set; } = 0.45f; 

        public float BowDrawingStrengthBoostFromEnergy { get; set; } = 0.35f; 

        public float BowDrawingStrengthDebuffFromEnergy { get; set; } = 0.65f; 

        public float AnimalHarvestingTimeBoostFromEnergy { get; set; } = 0.15f; 

        public float AnimalHarvestingTimeDebuffFromEnergy { get; set; } = 0.85f; 

        public float RefreshedEnergyBoostMultiplier { get; set; } = 1.35f; // Multiplier for boosting stat values when refreshed from sleepiness being less than set value.

        // If Energy is disabled
        public float SleepinessWalkSpeedDebuff { get; set; } = 0.65f;

        public float SleepinessRangedWeaponsAccDebuff { get; set; } = 0.95f;

        public float SleepinessRangedWeaponsSpeedDebuff { get; set; } = 0.9f;
        // ^^

        public float SleepinessEnergyrateDebuff { get; set; } = 3000f; // Put in the wanted end percentage. 3000 = 3000%

        public float HungerEnergyrateDebuff { get; set; } = 1000f; // Put in the wanted end percentage. 1000 = 1000%

        public float HungerEnergyrateDebuffStartRatio { get; set; } = 0.3f; // When should hunger matter? 0.3 = at 30% satiety and downwards.

        public float SleepinessCapacityOverload { get; set; } = 0.8f; // Percentage (0.8 = 80%) of max sleepiness capacity that can be overloaded. So if max sleepiness capacity is 12h then the player can be overloaded for an aditional 10h.

        public float SleepRegenerationFactor { get; set; } = 0.75f; // Multiplier for how much sleep should regenerate energy and decrease sleepiness.

        public float SleepBoostFromHighEnergy { get; set; } = 0.35f; // The factor by which having high energy increases sleep efficiency. (Value is not correlating, so be carefull)

        public float SleepDebuffFromLowEnergy { get; set; } = 1.35f; // The factor by which low energy decreases sleep efficiency.

        public float EnergyFromSleepWhenRefreshedModifier { get; set; } = 1.35f; // The factor by which the energy is regenerated from sleeping when sleepiness is fully drained.

        public float FeelingRefreshedHours { get; set; } = 3f; // The number of hours the player will feel refreshed after sleeping, this will give a buff to the player for the set amount of hours (3 = 3h) after sleeping, so they will feel more energized and ready to go.

        public float EnergyRestoredBySleepingModifier { get; set; } = 0.75f; // Factor for how fast energy regenerates when sleeping.

        public float HungerRateReductionFromHighEnergy { get; set; } = 0.5f; // The percentage by which the hunger rate is reduced from high energy, 0.5 = 50% reduction from base hunger rate.

        public float HungerRateGainFromLowEnergy { get; set; } = 3.5f; // The factor by which the hunger rate is gained from low energy, 3.5 = 350% increase in hunger rate.

        public ConfigServer(ICoreAPI api, ConfigServer previousConfig = null)
        {
            if (previousConfig == null)
            {
                return;
            }
            // Energy
            this.MaxEnergy = previousConfig.MaxEnergy;
            this.EnergySpeedModifier = previousConfig.EnergySpeedModifier;
            this.SittingRelaxingSpeedModifier = previousConfig.SittingRelaxingSpeedModifier;
            this.AttackEnergyCostModifier = previousConfig.AttackEnergyCostModifier;
            this.HeavyToolsEnergyCostModifier = previousConfig.HeavyToolsEnergyCostModifier;
            this.MediumToolsEnergyCostModifier = previousConfig.MediumToolsEnergyCostModifier;
            this.LightToolsEnergyCostModifier = previousConfig.LightToolsEnergyCostModifier;
            this.WeaponsEnergyCostModifier = previousConfig.WeaponsEnergyCostModifier;
            this.NoToolsWorkEnergyCostModifier = previousConfig.NoToolsWorkEnergyCostModifier;
            this.MovementEnergyCostModifier = previousConfig.MovementEnergyCostModifier;
            this.SprintingJumpingEnergyCostModifier = previousConfig.SprintingJumpingEnergyCostModifier;
            

            // Energy related stats
            this.EnableEnergyDependedToolMiningSpeed = previousConfig.EnableEnergyDependedToolMiningSpeed;
            this.ToolMiningSpeedBoostFromEnergy = previousConfig.ToolMiningSpeedBoostFromEnergy;
            this.ToolMiningSpeedDebuffFromEnergy = previousConfig.ToolMiningSpeedDebuffFromEnergy;
            this.EnableEnergyDependedJumpHeight = previousConfig.EnableEnergyDependedJumpHeight;
            this.JumpHeightBoostFromEnergy = previousConfig.JumpHeightBoostFromEnergy;
            this.JumpHeightDebuffFromEnergy = previousConfig.JumpHeightDebuffFromEnergy;
            this.EnableEnergyDependedWalkSpeed = previousConfig.EnableEnergyDependedWalkSpeed;
            this.WalkSpeedBoostFromEnergy = previousConfig.WalkSpeedBoostFromEnergy;
            this.WalkSpeedDebuffFromEnergy = previousConfig.WalkSpeedDebuffFromEnergy;
            this.HungerRateReductionFromHighEnergy = previousConfig.HungerRateReductionFromHighEnergy;
            this.HungerRateGainFromLowEnergy = previousConfig.HungerRateGainFromLowEnergy;
            this.EnableEnergyDependedRangedWeaponSpeed = previousConfig.EnableEnergyDependedRangedWeaponSpeed;
            this.RangedWeaponSpeedBoostFromEnergy = previousConfig.RangedWeaponSpeedBoostFromEnergy;
            this.RangedWeaponSpeedDebuffFromEnergy = previousConfig.RangedWeaponSpeedDebuffFromEnergy;
            this.EnableEnergyDependedRangedWeaponDamage = previousConfig.EnableEnergyDependedRangedWeaponDamage;
            this.RangedWeaponDamageBoostFromEnergy = previousConfig.RangedWeaponDamageBoostFromEnergy;
            this.RangedWeaponDamageDebuffFromEnergy = previousConfig.RangedWeaponDamageDebuffFromEnergy;
            this.EnableEnergyDependedMeleeWeaponDamage = previousConfig.EnableEnergyDependedRangedWeaponDamage;
            this.MeleeWeaponDamageBoostFromEnergy = previousConfig.MeleeWeaponDamageBoostFromEnergy;
            this.MeleeWeaponDamageDebuffFromEnergy = previousConfig.MeleeWeaponDamageDebuffFromEnergy;
            this.EnableEnergyDependedArmorWalkSpeedAffectedness = previousConfig.EnableEnergyDependedArmorWalkSpeedAffectedness;
            this.ArmorWalkSpeedAffectednessBoostFromEnergy = previousConfig.ArmorWalkSpeedAffectednessBoostFromEnergy;
            this.ArmorWalkSpeedAffectednessDebuffFromEnergy = previousConfig.ArmorWalkSpeedAffectednessDebuffFromEnergy;
            this.EnableEnergyDependedBowDrawingStrength = previousConfig.EnableEnergyDependedBowDrawingStrength;
            this.BowDrawingStrengthBoostFromEnergy = previousConfig.BowDrawingStrengthBoostFromEnergy;
            this.BowDrawingStrengthDebuffFromEnergy = previousConfig.BowDrawingStrengthDebuffFromEnergy;
            this.EnableEnergyDependedAnimalHarvestingTime = previousConfig.EnableEnergyDependedAnimalHarvestingTime;
            this.AnimalHarvestingTimeBoostFromEnergy = previousConfig.AnimalHarvestingTimeBoostFromEnergy;
            this.AnimalHarvestingTimeDebuffFromEnergy = previousConfig.AnimalHarvestingTimeDebuffFromEnergy;
            this.RefreshedEnergyBoostMultiplier = previousConfig.RefreshedEnergyBoostMultiplier;

            // Invigoration
            this.EnableInvigoratedHealthBoost = previousConfig.EnableInvigoratedHealthBoost;
            this.InvigoratedHealthBoostPercentageOfMaxHealth = previousConfig.InvigoratedHealthBoostPercentageOfMaxHealth;
            this.EnableInvigoratedLungCapacityBoost = previousConfig.EnableInvigoratedLungCapacityBoost;
            this.InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity = previousConfig.InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity;
            this.EnableInvigoratedHealingEffectiveness = previousConfig.EnableInvigoratedHealingEffectiveness;
            this.InvigoratedHealingEffectivenessModifier = previousConfig.InvigoratedHealingEffectivenessModifier;
            this.EnableInvigoratedMaxEnergyBoost = previousConfig.EnableInvigoratedMaxEnergyBoost;
            this.InvigoratedMaxEnergyBoostModifier = previousConfig.InvigoratedMaxEnergyBoostModifier;

            // Damage
            this.EnergyKills = previousConfig.EnergyKills;
            this.DamageIfNoEnergyAndHungerDoesNotMatter = previousConfig.DamageIfNoEnergyAndHungerDoesNotMatter;
            this.EnergyDrainFromDamageMultiplier = previousConfig.EnergyDrainFromDamageMultiplier;
            this.InvigoratedDrainFromDamageMultiplier = previousConfig.InvigoratedDrainFromDamageMultiplier;
            this.LoseInvigorationWhenDying = previousConfig.LoseInvigorationWhenDying;
            this.DrainInvigorationWhenHealing = previousConfig.DrainInvigorationWhenHealing;
            this.InvigorationDrainFromHealingModifier = previousConfig.InvigorationDrainFromHealingModifier;
            this.EnergyDrainFromHealingModifier = previousConfig.EnergyDrainFromHealingModifier;

            // Hunger & Nutrition
            this.HungerLevelMatters = previousConfig.HungerLevelMatters;
            this.HungerEnergyrateDebuff = previousConfig.HungerEnergyrateDebuff;
            this.HungerEnergyrateDebuffStartRatio = previousConfig.HungerEnergyrateDebuffStartRatio;
            this.OnlyDieFromNoEnergy = previousConfig.OnlyDieFromNoEnergy;
            this.DamageIfNoEnergyAndStarving = previousConfig.DamageIfNoEnergyAndStarving;
            this.EnableNutrientFactor = previousConfig.EnableNutrientFactor;
            this.NutritionLossWhenStarvingModifier = previousConfig.NutritionLossWhenStarvingModifier;

            // Temperature
            this.BodyTemperatureMatters = previousConfig.BodyTemperatureMatters;
            this.EnergyRatePerDegrees = previousConfig.EnergyRatePerDegrees;

            // Sleepiness
            this.SleepinessCapacityOverload = previousConfig.SleepinessCapacityOverload;
            this.SleepRegenerationFactor = previousConfig.SleepRegenerationFactor;
            this.EnergyRestoredBySleepingModifier = previousConfig.EnergyRestoredBySleepingModifier;
            this.SleepBoostFromHighEnergy = previousConfig.SleepBoostFromHighEnergy;
            this.SleepDebuffFromLowEnergy = previousConfig.SleepDebuffFromLowEnergy;
            this.EnergyFromSleepWhenRefreshedModifier = previousConfig.EnergyFromSleepWhenRefreshedModifier;
            this.FeelingRefreshedHours = previousConfig.FeelingRefreshedHours;
            this.SleepinessEnergyrateDebuff = previousConfig.SleepinessEnergyrateDebuff;
            this.SleepinessWalkSpeedDebuff = previousConfig.SleepinessWalkSpeedDebuff;
            this.SleepinessRangedWeaponsAccDebuff = previousConfig.SleepinessRangedWeaponsAccDebuff;
            this.SleepinessRangedWeaponsSpeedDebuff = previousConfig.SleepinessRangedWeaponsSpeedDebuff;

            // Master Switches
            base.EnableEnergy = previousConfig.EnableEnergy;
            base.EnableSleepiness = previousConfig.EnableSleepiness;
            base.ResetModBoosts = previousConfig.ResetModBoosts;
        }


    }

    
    [ProtoContract]
    public class SyncedConfig : IModConfig
    {
        
        [ProtoMember(1, IsRequired = true)]
        public bool EnableEnergy { get; set; } = true;

        
        [ProtoMember(2, IsRequired = true)]
        public bool EnableSleepiness { get; set; } = true;

        
        [ProtoMember(16, IsRequired = true)]
        public bool ResetModBoosts { get; set; }

        
        public SyncedConfig()
        {
        }

        
        public SyncedConfig(ICoreAPI api, SyncedConfig previousConfig = null)
        {
            if (previousConfig == null)
            {
                return;
            }
            this.EnableEnergy = previousConfig.EnableEnergy;
            this.EnableSleepiness = previousConfig.EnableSleepiness;
            this.ResetModBoosts = previousConfig.ResetModBoosts;

        }

        
        public SyncedConfig Clone()
        {
            return new SyncedConfig
            {
                EnableEnergy = this.EnableEnergy,
                EnableSleepiness = this.EnableSleepiness,
                ResetModBoosts = this.ResetModBoosts,

            };
        }
    }


    
    public static class ModConfig
    {
        
        public static T ReadConfig<T>(ICoreAPI api, string jsonConfig) where T : IModConfig
        {
            T config;
            try
            {
                config = ModConfig.LoadConfig<T>(api, jsonConfig);
                if (config == null)
                {
                    ModConfig.GenerateConfig<T>(api, jsonConfig);
                    config = ModConfig.LoadConfig<T>(api, jsonConfig);
                }
                else
                {
                    ModConfig.GenerateConfig<T>(api, jsonConfig, config);
                }
            }
            catch
            {
                ModConfig.GenerateConfig<T>(api, jsonConfig);
                config = ModConfig.LoadConfig<T>(api, jsonConfig);
            }
            return config;
        }

        
        public static void WriteConfig<T>(ICoreAPI api, string jsonConfig, T config) where T : IModConfig
        {
            ModConfig.GenerateConfig<T>(api, jsonConfig, config);
        }

        
        private static T LoadConfig<T>(ICoreAPI api, string jsonConfig) where T : IModConfig
        {
            return api.LoadModConfig<T>(jsonConfig);
        }

        
        private static void GenerateConfig<T>(ICoreAPI api, string jsonConfig, T previousConfig = default(T)) where T : IModConfig
        {
            api.StoreModConfig<T>(ModConfig.CloneConfig<T>(api, previousConfig), jsonConfig);
        }

        
        private static T CloneConfig<T>(ICoreAPI api, T config = default(T)) where T : IModConfig
        {
            return (T)Activator.CreateInstance(typeof(T), new object[]
            {
                api,
                config
            });
        }

        
        public static string GetConfigPath(ICoreAPI api)
        {
            return Path.Combine(api.GetOrCreateDataPath("ModConfig"), "SleepNeed");
        }
    }
}
