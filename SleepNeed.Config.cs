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
        public string Energy_Bar_X { get; set; } = "Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float EnergyBarX { get; set; }

        public string Energy_Bar_Y { get; set; } = "Enter a positive value in increments at +2 to move the bar down. Enter a negative value ´to move the bar upwards";
        public float EnergyBarY { get; set; }
        public string Sleepiness_Bar_X { get; set; } = "Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float SleepinessBarX { get; set; }
        public string Sleepiness_Bar_Y { get; set; } = "Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float SleepinessBarY { get; set; }
        public string Energy_Bar_Fill_Direction_Right_To_Left { get; set; } = "False = Bar will fill the opposite direction of (Right To Left) right side of the bar towards the left side.";
        public bool EnergyBarFillDirectionRightToLeft { get; set; } = false;
        public string Sleepiness_Bar_Fill_Direction_Right_To_Left { get; set; } = "true = Bar will fill from right side of the bar towards the left side.";
        public bool SleepinessBarFillDirectionRightToLeft { get; set; } = true;
        public string Sleepiness_Bar_Visible { get; set; } = "Should the Sleepiness bar be vissible?";
        public bool SleepinessBarVisible { get; set; } = true;
        public string Hide_Sleepiness_Bar_At { get; set; } = "If sleepiness is below set value, then the bar hides. (If set to 3, the bar will hide when sleepiness is below 3. Default is 0 so it never hides.)";
        public float HideSleepinessBarAt { get; set; }
        public string Energy_Bar_Color { get; set; } = "Change the color of the HUD bars, by entering a new Hexidecimal ¨HEX¨ value of a color you'd like. Google it when in doubt how to, and it dosn't matter if it's upper or lower case letters in the config file.";
        public string EnergyBarColor { get; set; } = ModGuiStyle.EnergyBarColor.ToHex();

        public string SleepinessBarColor { get; set; } = ModGuiStyle.SleepinessBarColor.ToHex();

        public string SleepinessOverloadColor { get; set; } = ModGuiStyle.SleepinessOverloadColor.ToHex();

        
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
        // Energy
        public string Energy_Category { get; set; } = "....::::: ENERGY :::::....";
        public string Max_Energy { get; set; } = "Set the base max energy amount";
        public float MaxEnergy { get; set; } = 900f;
        public string Energy_Modifiers { get; set; } = "----- Modifiers: Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public string Energy_Speed_Modifier { get; set; } = "Modifier for how fast energy drains. If this is not set, the speed is determined by world start config HungerSpeedModifier = 1.";
        public float EnergySpeedModifier { get; set; } // Modifier for how fast energy drains. If this is not set, the speed is determined by GlobalConstants.HungerSpeedModifier
        public string Movement_Energy_Cost_Modifier { get; set; } = "Modifier for movement like walking or sneaking.";
        public float MovementEnergyCostModifier { get; set; } = 1.0f; // Modifier for movement energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Sprinting_Jumping_Energy_Cost_Modifier { get; set; } = "Modifier for sprinting and jumping.";
        public float SprintingJumpingEnergyCostModifier { get; set; } = 1.0f; // Modifier for sprinting and jumping energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string No_Tools_Work_Energy_Cost_Modifier { get; set; } = "Modifier for using the hands without any tools.";
        public float NoToolsWorkEnergyCostModifier { get; set; } = 1.0f; // Modifier for work without tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Light_Tools_Energy_Cost_Modifier { get; set; } = "Modifier for using small tools like knife, chisel, etc.";
        public float LightToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for light tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Medium_Tools_Energy_Cost_Modifier { get; set; } = "Modifier for using tools like axe, shovel, pickaxe etc.";
        public float MediumToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for medium tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Heavy_Tools_Energy_Cost_Modifier { get; set; } = "Modifier for using heavy stuff like warhammer, poleaxe etc. (If those things ever get in the game, it's sertainly in the API).";
        public float HeavyToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for heavy tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Weapons_Energy_Cost_Modifier { get; set; } = "Modifier for using weapons like sword, spear, bow, shield etc.";
        public float WeaponsEnergyCostModifier { get; set; } = 1.0f; // Modifier for weapons energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Attack_Energy_Cost_Modifier { get; set; } = "Modifier for an aditional cost from doing an attack. It's hard to swing a weapon fighting for your life....";
        public float AttackEnergyCostModifier { get; set; } = 1.0f; // Modifier for attack energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Energy_Drain_From_Healing_Modifier { get; set; } = "Modifier for the energy cost from using healing remedies like poultice. Yes it takes a toll on you getting hurt....";
        public float EnergyDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the energy drain from healing, 1.0f means no change, 0.5f means half the energy drain, 2.0f means double the energy drain.
        public string Sitting_Relaxing_Speed_Modifier { get; set; } = "Modifier for how fast energy regenerates when sitting.";
        public float SittingRelaxingSpeedModifier { get; set; } = 1.0f; // Modifier for how fast energy regenerates when sitting.
        public string Energy_Drain_From_Damage_Multiplier { get; set; } = "When taking damage energy is drained, because it takes a toll on you getting bit by a bear...";
        public float EnergyDrainFromDamageMultiplier { get; set; } = 1.0f;
        public string Energy_Game_Settings { get; set; } = "----- These settings defines how energy is affecting gameplay";
        public string Hunger_Level_Matters { get; set; } = "This makes hunger/satiety a part of the energy mechanic. An example is, that low satiety increases the energy rate so you consume more energy when being hungry. Look at description to understand what hunger affects.";
        public bool HungerLevelMatters { get; set; } = true; // Should hunger have an impact on the energy rate?
        public string Only_Die_From_No_Energy { get; set; } = "If enabled the player dosn't take damage when the hunger-bar reaches 0. Instead the player is starving and consuming way more energy, nutrition and invigoration. When nutrition, hunger and energy is 0, you die....";
        public bool OnlyDieFromNoEnergy { get; set; } = true; // If true, the player will only die from no energy, so if the player has no energy, they will die, but if they have energy, they will not die from hunger.
        public string Damage_If_No_Energy_And_Starving { get; set; } = "Damage pr. tick. Only applied if OnlyDieFromNoEnergy = true.";
        public float DamageIfNoEnergyAndStarving { get; set; } = 0.5f; // Damage pr. tick
        public string Energy_Kills { get; set; } = "This is only valid, if Hunger Matters is disabled. If HungerLevelMatters = false, then you take damage when energy reaches 0.";
        public bool EnergyKills { get; set; } = true; // Only used if Hunger Matters is disabled
        public string Damage_If_No_Energy_And_Hunger_Does_Not_Matter { get; set; } = "Damage pr. tick if hunger matters is disabled and energy reaches 0.";
        public float DamageIfNoEnergyAndHungerDoesNotMatter { get; set; } = 0.25f; // If hunger matters is disabled, this is the applied damage when no energy.
        public string Enable_Nutrient_Factor { get; set; } = "This makes nutrition an important part of overall health. Nutrition is the 5 bars in character menu like fruit, protein etc. (Read description to understand how it is implemented in this mod)";
        public bool EnableNutrientFactor { get; set; } = true; // Enable the nutrient factor, which will factor in invigoration based on the nutrient level of the player.
        public string Nutrition_Loss_When_Starving_Modifier { get; set; } = "If hunger-bar is empty, you are starving and that drains your nutrition values at an increased rate. This simulates that starving in real life makes your body using the resources it has.";
        public float NutritionLossWhenStarvingModifier { get; set; } = 10.0f; // Multiplier for how fast nutrition values should drain when starving.
        public string Hunger_Rate_Reduction_From_High_Energy { get; set; } = "The percentage by which the hunger rate is reduced when energy is above 70%. This is gradual and the value is the final percentage when energy is 100%. (0.5 = 50% reduction from base hunger rate.)";
        public float HungerRateReductionFromHighEnergy { get; set; } = 0.5f; // The percentage by which the hunger rate is reduced from high energy, 0.5 = 50% reduction from base hunger rate.
        public string Hunger_Rate_Gain_From_Low_Energy { get; set; } = "The percentage by which the hunger rate is increased when energy is below 30%. This is gradual and the value is the final percentage when energy is 0%. (3.5 = 350% increase in hunger rate. This is added to the default 100% so 450% is the final value.)";
        public float HungerRateGainFromLowEnergy { get; set; } = 3.5f; // The factor by which the hunger rate is gained from low energy, 3.5 = 350% increase in hunger rate.
        public string Energy_After_Revival { get; set; } = "The amount of energy you have after revival. This is the percentage of the max energy capacity. (0.5 = 50%)";
        public float EnergyAfterRevival { get; set; } = 0.5f;
        public string Energy_Rate_Settings { get; set; } = "----- Settings related to energy rate, energy rate increases or decreases the energy cost.";
        public string Hunger_Energy_Rate_Debuff { get; set; } = "Maximum increase of energy rate caused by hunger. This value is the final energy rate when hunger reaches 0. (Set in percent; 1000 = 1000%)";
        public float HungerEnergyrateDebuff { get; set; } = 2500f; // Put in the wanted end percentage. 1000 = 1000%
        public string Hunger_Energy_Rate_Debuff_Start_Ratio { get; set; } = "When should hunger affcet energy rate? Default is 0.3 which means the debuff start at 30% and downwards to 0";
        public float HungerEnergyrateDebuffStartRatio { get; set; } = 0.3f; // When should hunger matter? 0.3 = at 30% satiety and downwards.
        public string Body_Temperature_Matters { get; set; } = "This will make the body temperature of the player affect the energy rate, so if the player is too cold or too hot, they will have increased energy rate and will drain energy faster. (< 37, > 37.8)";
        public bool BodyTemperatureMatters { get; set; } = true; // Enable the body temperature matters, which will make the body temperature of the player matter for the energy level, so if the player is too cold or too hot, they will have increased energy rate and will drain energy faster.
        public string Energy_Rate_Per_Degrees { get; set; } = "This is the increased energy rate pr. degree C when being cold. If body temp is above 37.8 C then this value is doubled. This is because body temp above normal is very serious in real life and is rare in the game to ever happen. (450 = 450% pr. degree)";
        public float EnergyRatePerDegrees { get; set; } = 450f; // 45 = 450% So energy rate will increase 450% per degree of body temp outside normal core temp.
        public string Sleepiness_Energy_Rate_Debuff { get; set; } = "When in an overloaded state of sleepiness, how much should the increase in energy rate be, when fully overloaded? (Set the final percentage; 3000 = 3000%) :: This is gradually increased from overload starts till it's full. See settings for sleepiness to get a hint on how overload is set.";
        public float SleepinessEnergyrateDebuff { get; set; } = 3000f; // Put in the wanted end percentage. 3000 = 3000%
        public string Energy_Stats_Settings { get; set; } = "----- Stats affected by the current energy level. All values are either added as boost when above 70% energy or subtracted as debuff when below 30% energy.";
        public string Enable_Energy_Depended_WalkSpeed { get; set; } = "Should energy have any impact on walkspeed?";
        public bool EnableEnergyDependedWalkSpeed { get; set; } = true;
        public string WalkSpeed_From_Energy { get; set; } = "How much should walkspeed be affected? (0.15 = 15%)";
        public float WalkSpeedBoostFromEnergy { get; set; } = 0.15f; // 0.15 = 15% added to base walkspeed
        public float WalkSpeedDebuffFromEnergy { get; set; } = 0.45f; // 0.45 = 45% subtracted from base walk speed.
        public string Enable_Energy_Depended_Armor_WalkSpeed_Affectedness { get; set; } = "Should energy affect the walkspeed when wearing armor? This is only touching the debuff value to walkspeed when wearing an armor.";
        public bool EnableEnergyDependedArmorWalkSpeedAffectedness { get; set; } = true;
        public string Armor_Walkspeed_From_Energy { get; set; } = "How much should walkspeed be affected when wearing armor? (0.25 = 25%); Value is either added or subtracted from the original debuff to walkspeed comming from the armor itself.";
        public float ArmorWalkSpeedAffectednessBoostFromEnergy { get; set; } = 0.25f;
        public float ArmorWalkSpeedAffectednessDebuffFromEnergy { get; set; } = 0.45f;
        public string Enable_Energy_Depended_Jump_Height { get; set; } = "Should energy have any impact on jump height?";
        public bool EnableEnergyDependedJumpHeight { get; set; } = true; // Enable the energy depended jump speed, which will make the jump speed of the player depend on the current energy level of the player, so if the player has low energy, the jump speed will be lower.
        public string Jump_Height_From_High_Energy { get; set; } = "The values are not so simple as percentages, so you will have to try for yourself.";
        public float JumpHeightBoostFromEnergy { get; set; } = 0.44f; // 0.6 equals 2 block jump height. 0.39 equals 2 block jump height when refreshed boost is 1.35.
        public float JumpHeightDebuffFromEnergy { get; set; } = 0.6f; // 0.6 equals to less than 1 block jump height.
        public string Enable_Energy_Depended_Tool_MiningSpeed { get; set; } = "Should energy have any impact on mining speed? (This changes the time it takes for a block to break, and it applies to all tools like axe, shovel etc.)";
        public bool EnableEnergyDependedToolMiningSpeed { get; set; } = true; // Enable the energy depended tool mining speed, which will make the mining speed of tools depend on the current energy level of the player, so if the player has low energy, the mining speed will be lower.
        public string Tool_MiningSpeed_From_Energy { get; set; } = "The values are not so simple either, try for yourself.";
        public float ToolMiningSpeedBoostFromEnergy { get; set; } = 0.2f; // The mining speed boost from high energy, value is added to base game value.
        public float ToolMiningSpeedDebuffFromEnergy { get; set; } = 0.95f; // The mining speed debuff from low energy, value is subtracted from the base game value.
        public string Enable_Energy_Depended_Melee_Weapon_Damage { get; set; } = "Should energy have any impact on melee damage output?";
        public bool EnableEnergyDependedMeleeWeaponDamage { get; set; } = true;
        public string Melee_Weapon_Damage_From_Energy { get; set; } = "How much should melee damage be affected? (0.15 = 15%)";
        public float MeleeWeaponDamageBoostFromEnergy { get; set; } = 0.35f;
        public float MeleeWeaponDamageDebuffFromEnergy { get; set; } = 0.65f;
        public string Enable_Energy_Depended_Ranged_Weapon_Damage { get; set; } = "Should energy have any impact on ranged damage output?";
        public bool EnableEnergyDependedRangedWeaponDamage { get; set; } = true;
        public string Ranged_Weapon_Damage_From_Energy { get; set; } = "How much should ranged damage be affected? (0.15 = 15%)";
        public float RangedWeaponDamageBoostFromEnergy { get; set; } = 0.25f;
        public float RangedWeaponDamageDebuffFromEnergy { get; set; } = 0.45f;
        public string Enable_Energy_Depended_Ranged_Weapon_Speed { get; set; } = "Should energy have any impact on drawing speed on ranged weapons?";
        public bool EnableEnergyDependedRangedWeaponSpeed { get; set; } = true;
        public string Ranged_Weapon_Speed_From_Energy { get; set; } = "How much should ranged speed be affected? (0.15 = 15%)";
        public float RangedWeaponSpeedBoostFromEnergy { get; set; } = 0.25f; // 0.25 = 25%
        public float RangedWeaponSpeedDebuffFromEnergy { get; set; } = 0.75f;
        public string Enable_Energy_Depended_Bow_Drawing_Strength { get; set; } = "Should energy have any impact on drawing strenght when using a bow?";
        public bool EnableEnergyDependedBowDrawingStrength { get; set; } = true;
        public string Bow_Drawing_Strength_From_Energy { get; set; } = "How much should drawing strength be affected? (0.15 = 15%)";
        public float BowDrawingStrengthBoostFromEnergy { get; set; } = 0.35f;
        public float BowDrawingStrengthDebuffFromEnergy { get; set; } = 0.65f;
        public string Enable_Energy_Depended_Animal_Harvesting_Time { get; set; } = "Should energy have any impact on the time it takes to skin an animal?";
        public bool EnableEnergyDependedAnimalHarvestingTime { get; set; } = true;
        public string Animal_Harvesting_Time_From_Energy { get; set; } = "How much should animal harvesting time be affected? (0.15 = 15%)";
        public float AnimalHarvestingTimeBoostFromEnergy { get; set; } = 0.15f;
        public float AnimalHarvestingTimeDebuffFromEnergy { get; set; } = 0.85f;
        public string Refreshed_Energy_Boost_Multiplier { get; set; } = "When sleepiness is completely full, you are in a refreshed state. This will boost all stats above by 1.35 = 35%";
        public float RefreshedEnergyBoostMultiplier { get; set; } = 1.35f; // Multiplier for boosting stat values when refreshed from sleepiness being less than set value.
        // Sleepiness
        public string Sleepiness_Category { get; set; } = "....::::: SLEEPINESS :::::....";
        public string Max_Sleepiness { get; set; } = "What is the max amount of sleepiness, before being overloaded? This value should be seen as hours.";
        public float MaxSleepiness { get; set; } = 14f;
        public string Sleepiness_Capacity_Overload { get; set; } = "When sleepiness is above max, you are in an overloaded state of sleepiness. How big of a percentage of max should that overloaded state be? (0.8 = 80%) of max sleepiness capacity that can be overloaded. So if max sleepiness capacity is 14h then the player can be overloaded for an aditional 11.2h.)";
        public float SleepinessCapacityOverload { get; set; } = 0.8f; // Percentage (0.8 = 80%) of max sleepiness capacity that can be overloaded. So if max sleepiness capacity is 12h then the player can be overloaded for an aditional 10h.
        public string Feeling_Refreshed_Hours { get; set; } = "The number of hours the player will feel refreshed after sleeping, this will give a buff to the player for the set amount of hours (3 = 3h) after sleeping. All stats from energy is affected by the boost set in RefreshedEnergyBoostMultiplier";
        public float FeelingRefreshedHours { get; set; } = 3f; // The number of hours the player will feel refreshed after sleeping, this will give a buff to the player for the set amount of hours (3 = 3h) after sleeping, so they will feel more energized and ready to go.
        public string Energy_Disabled_Stats { get; set; } = "If energy is disabled, these stats will be affected by sleepiness alone. If energy is enabled these stats do nothing.";
        public string Sleepiness_Stats_When_Energy_Off { get; set; } = "How much should the following stats be affected by being in an overloaded state of sleepiness? (0.15 = 15%) :: To disable the stats just set them to 0.";
        public float SleepinessWalkSpeedDebuff { get; set; } = 0.65f;
        public float SleepinessRangedWeaponsAccDebuff { get; set; } = 0.95f;
        public float SleepinessRangedWeaponsSpeedDebuff { get; set; } = 0.9f;
        public string Sleepiness_After_Revival { get; set; } = "The amount of sleepiness you have after revival. This is the percentage of the maximum capacity including the overload. (0.5 = 50%)";
        public float SleepinessAfterRevival { get; set; } = 0.5f;
        // Invigoration
        public string Invigoration_Category { get; set; } = "....::::: INVIGORATION :::::....";
        public string Lose_Invigoration_When_Dying { get; set; } = "If enabled, this will reset the invigoration level to 0 when the player dies, so they will have to regain it again.";
        public bool LoseInvigorationWhenDying { get; set; } = true; // Lose invigoration when dying, this will reset the invigoration level to 0 when the player dies, so they will have to regain it again.
        public string Drain_Invigoration_When_Healing { get; set; } = "If enabled, this will hurt the invigoration when the player heals with remedies, so they will have to regain it again. This is useful for balancing the game, so players can't just spam healing and keep their invigoration level high.";
        public bool DrainInvigorationWhenHealing { get; set; } = true; // Lose invigoration when healing, this will hurt the invigoration when the player heals, so they will have to regain it again. This is useful for balancing the game, so players can't just spam healing and keep their invigoration level high.
        public string Invigoration_Drain_From_Healing_Modifier { get; set; } = "This is a modifier for the invigoration drain from healing, 1.0f means no change, 0.5f means half the invigoration drain, 2.0f means double the invigoration drain.";
        public float InvigorationDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the invigoration drain from healing, 1.0f means no change, 0.5f means half the invigoration drain, 2.0f means double the invigoration drain.
        public string Drain_From_Damage_Modifier { get; set; } = "Modifier of how much invigoration that will be drained when the player takes damage while invigorated.";
        public float InvigoratedDrainFromDamageMultiplier { get; set; } = 1.0f; // Percentage of Invigoration that will be drained when the player takes damage while invigorated, 0.1f means 10% of CurrentEnergy will be drained when the player takes damage while invigorated.
        public string Enable_Invigorated_Health_Boost { get; set; } = "Should the player gain extra health points when in a good overall health? (Overall health is a mean ratio of invigoration and nutrition)";
        public bool EnableInvigoratedHealthBoost { get; set; } = true; // Should the player gain extra health points with invigoration?
        public string Health_Boost_Percentage_Of_Max_Health { get; set; } = "How much health should be given at max overall health? Base max health = 15 by default. (15 * 0.5 = 7.5 hp)";
        public float InvigoratedHealthBoostPercentageOfMaxHealth { get; set; } = 0.5f; // 7.5 health points if full invigoration and well nutritious if max health is 15 hp.
        public string Healing_Effectiveness_From_Overall_Health { get; set; } = "Should the player gain healing effectiveness when in a good overall health?";
        public bool EnableInvigoratedHealingEffectiveness { get; set; } = true; // Healing effectiveness increases with invigoration.
        public string Healing_Effectiveness_Modifier { get; set; } = "1.0 = default + 100% = 200% Healing Effectiveness";
        public float InvigoratedHealingEffectivenessModifier { get; set; } = 1.0f; // 1.0 = default + 100% = 200% Healing Effectiveness 
        public string Enable_Invigorated_Max_Energy_Boost { get; set; } = "Should the player gain increased max energy capacity when in a good overall health?";
        public bool EnableInvigoratedMaxEnergyBoost { get; set; } = true; // Should max energy increase with invigoration?
        public string Max_Energy_Boost_Modifier { get; set; } = "1.0 is default and has a final value for max energy when in good overall health equal to 1500.";
        public float InvigoratedMaxEnergyBoostModifier { get; set; } = 1.0f; // 1.0 means final value for max energy when fully invigorated equals to 1500.
        public string Enable_Invigorated_Lung_Capacity_Boost { get; set; } = "Should the player gain increased lung capacity when in a good overall health?";
        public bool EnableInvigoratedLungCapacityBoost { get; set; } = true; // Enable the invigorated lung capacity boost, which will boost the lung capacity of the player based on the configured lung capacity in the config file.
        public string Lung_Capacity_Boost_Percentage { get; set; } = "(0.5 = 50%) of base lung capacity, so if the base lung capacity is 40.000 (40s), then it will be 60s when in good overall health.";
        public float InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity { get; set; } = 0.5f; // 50% of base lung capacity, so if the base lung capacity is 40.000 (40s), then it will be 60s.
        // During sleep
        public string Sleeping_Category { get; set; } = "....::::: SLEEPING :::::....";
        public string Sleep_Regeneration_Factor { get; set; } = "Modifier for how much sleep should decrease sleepiness. If you can't get sleepiness down enough while sleeping, then set this value higher than 1.";
        public float SleepRegenerationFactor { get; set; } = 1.0f; // Multiplier for how much sleepiness should decrease sleepiness.
        public string Sleep_Boost_From_High_Energy { get; set; } = "The factor by which having high energy increases sleep efficiency. (Value is not correlating, so be carefull. Higher value means more buff.)";
        public float SleepBoostFromHighEnergy { get; set; } = 0.35f; // The factor by which having high energy increases sleep efficiency. (Value is not correlating, so be carefull)
        public string Sleep_Boost_From_Low_Energy { get; set; } = "The factor by which having low energy decreases sleep efficiency. (Value is not correlating, so be carefull. Higher value means less debuff.)";
        public float SleepDebuffFromLowEnergy { get; set; } = 0.5f; // The factor by which low energy decreases sleep efficiency.
        public string Energy_Restored_By_Sleeping_Modifier { get; set; } = "Factor for how fast energy regenerates when sleeping. (Value is not correlating, so be carefull)";
        public float EnergyRestoredBySleepingModifier { get; set; } = 0.75f; // Factor for how fast energy regenerates when sleeping.
        public string Energy_From_Sleep_When_Refreshed_Modifier { get; set; } = "When sleepiness is fully drained, and you are still sleeping, you then gain additional energy while sleeping. (1.35 = 35% increase in energy restored)";
        public float EnergyFromSleepWhenRefreshedModifier { get; set; } = 1.35f; // The factor by which the energy is regenerated from sleeping when sleepiness is fully drained.
        public string Disable_Behaviors { get; set; } = "Here you can turn off Energy or Sleepiness. You can also turn off all stat changes if you'd like, and only keep the systems without changes to walkspeed, jump height and all the other stats related to energy and sleepiness.";
        


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
            this.MaxSleepiness = previousConfig.MaxSleepiness;
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
            this.SleepinessAfterRevival = previousConfig.SleepinessAfterRevival;

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
