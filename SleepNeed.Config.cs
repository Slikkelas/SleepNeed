using ProtoBuf;
using SleepNeed.Energy;
using SleepNeed.Hud;
using SleepNeed.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace SleepNeed.Config

{
    
    public class IModConfig
    {
    }

    
    public class ConfigClient : IModConfig
    {
        public string Energy_Bar_X { get; set; } = "                                        POSITION X-AXIS (ENERGY BAR)            -  Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float EnergyBarX { get; set; }
        public string Energy_Bar_Y { get; set; } = "                                        POSITION Y-AXIS (ENERGY BAR)            -  Enter a positive value in increments at +2 to move the bar down. Enter a negative value ´to move the bar upwards";
        public float EnergyBarY { get; set; }
        public string Sleepiness_Bar_X { get; set; } = "                                    POSITION X-AXIS (SLEEPINESS BAR)        -  Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float SleepinessBarX { get; set; }
        public string Sleepiness_Bar_Y { get; set; } = "                                    POSITION Y-AXIS (SLEEPINESS BAR)        -  Enter a positive value in increments at +2 to move the bar to the right. Enter a negative value ´to move the bar to the left";
        public float SleepinessBarY { get; set; }
        public string Energy_Bar_Fill_Direction_Right_To_Left { get; set; } = "             FILL DIRECTION (ENERGY BAR)             -  False = Bar will fill the opposite direction of (Right To Left) right side of the bar towards the left side.";
        public bool EnergyBarFillDirectionRightToLeft { get; set; } = false;
        public string Sleepiness_Bar_Fill_Direction_Right_To_Left { get; set; } = "         FILL DIRECTION (SLEEPINESS BAR)         -  true = Bar will fill from right side of the bar towards the left side.";
        public bool SleepinessBarFillDirectionRightToLeft { get; set; } = true;
        public string Sleepiness_Bar_Visible { get; set; } = "                              HIDE SLEEPINESS BAR                     -  Should the Sleepiness bar be vissible?";
        public bool SleepinessBarVisible { get; set; } = true;
        public string Hide_Sleepiness_Bar_At { get; set; } = "                              HIDE VALUE                              -  If sleepiness is below set value, then the bar hides. (If set to 3, the bar will hide when sleepiness is below 3. Default is 0 so it never hides.)";
        public float HideSleepinessBarAt { get; set; }
        public string Energy_Bar_Color { get; set; } = "                                    COLOR                                   -  Change the color of the HUD bars, by entering a new Hexidecimal ¨HEX¨ value of a color you'd like. Google it when in doubt how to, and it dosn't matter if it's upper or lower case letters in the config file.";
        public string EnergyBarColor { get; set; } = ModGuiStyle.EnergyBarColor.ToHex();
        public string SleepinessBarColor { get; set; } = ModGuiStyle.SleepinessBarColor.ToHex();
        public string SleepinessOverloadColor { get; set; } = ModGuiStyle.SleepinessOverloadColor.ToHex();
        public string InvigorationBarColor { get; set; } = ModGuiStyle.InvigorationBarColor.ToHex();


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
            this.SleepinessOverloadColor = previousConfig.SleepinessOverloadColor;
            this.InvigorationBarColor = previousConfig.InvigorationBarColor;
            this.SleepinessBarVisible = previousConfig.SleepinessBarVisible;
            this.HideSleepinessBarAt = previousConfig.HideSleepinessBarAt;
            
        }
    }

    
    public class ConfigServer : IModConfig
    {
        // Energy
        public string Energy_Category { get; set; } = "▁   ▂   ▃   ▄   ▅   ▆   ▇   █ - 🏃 - ENERGY - 🏃 - █    ▇   ▆   ▅   ▄   ▃   ▂   ▁";
        public string Max_Energy { get; set; } = "                                          MAX ENERGY                                                🗘 (900)          -  Set the base max energy amount.";
        public float MaxEnergy { get; set; } = 900f;
        public string Energy_Modifiers { get; set; } = "            ----------------------  ENERGY MODIFIERS  ----------------------                  -  Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public string Energy_Speed_Modifier { get; set; } = "                               OVERALL                         -  ENERGY COST            🗘 (0.0)          -  Modifier for how fast energy drains. If this is 0.0, the speed is determined by world start config HungerSpeedModifier = 1. If you wanna reduce energy cost you can then only go as low as 0.1 (0.5 = Energy drains 50% slower)";
        public float EnergySpeedModifier { get; set; } // Modifier for how fast energy drains. If this is not set, the speed is determined by GlobalConstants.HungerSpeedModifier
        public string Movement_Energy_Cost_Modifier { get; set; } = "                       MOVEMENT                        -  ENERGY COST            🗘 (1.0)          -  Modifier for movement like walking or sneaking. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float MovementEnergyCostModifier { get; set; } = 1.0f; // Modifier for movement energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Sprinting_Jumping_Energy_Cost_Modifier { get; set; } = "              SPRINTING                       -  ENERGY COST            🗘 (1.0)          -  Modifier for sprinting and jumping. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float SprintingJumpingEnergyCostModifier { get; set; } = 1.0f; // Modifier for sprinting and jumping energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string No_Tools_Work_Energy_Cost_Modifier { get; set; } = "                  NO TOOLS WORK                   -  ENERGY COST            🗘 (1.0)          -  Modifier for using the hands without any tools. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float NoToolsWorkEnergyCostModifier { get; set; } = 1.0f; // Modifier for work without tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Light_Tools_Energy_Cost_Modifier { get; set; } = "                    LIGHT TOOLS WORK                -  ENERGY COST            🗘 (1.0)          -  Modifier for using small tools like knife, chisel, etc. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float LightToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for light tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Medium_Tools_Energy_Cost_Modifier { get; set; } = "                   MEDIUM TOOLS WORK               -  ENERGY COST            🗘 (1.0)          -  Modifier for using tools like axe, shovel, pickaxe etc. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float MediumToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for medium tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Heavy_Tools_Energy_Cost_Modifier { get; set; } = "                    HEAVY TOOLS WORK                -  ENERGY COST            🗘 (1.0)          -  Modifier for using heavy stuff like warhammer, poleaxe etc. (If those things ever get in the game, it's sertainly in the API). Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float HeavyToolsEnergyCostModifier { get; set; } = 1.0f; // Modifier for heavy tools energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Weapons_Energy_Cost_Modifier { get; set; } = "                        WEAPONS                         -  ENERGY COST            🗘 (1.0)          -  Modifier for using weapons like sword, spear, bow, shield etc. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float WeaponsEnergyCostModifier { get; set; } = 1.0f; // Modifier for weapons energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Attack_Energy_Cost_Modifier { get; set; } = "                         ATTACK                          -  ENERGY COST            🗘 (1.0)          -  Modifier for an aditional cost from doing an attack. It's hard to swing a weapon fighting for your life.... Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float AttackEnergyCostModifier { get; set; } = 1.0f; // Modifier for attack energy cost, 1.0f means no change, 0.5f means half the energy cost, 2.0f means double the energy cost.
        public string Sitting_Relaxing_Speed_Modifier { get; set; } = "                     RELAXING                        -  ENERGY GAIN SPEED      🗘 (1.0)          -  Modifier for how fast energy regenerates when sitting. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.0 = 100%, 0.5 = 50%)";
        public float SittingRelaxingSpeedModifier { get; set; } = 1.0f; // Modifier for how fast energy regenerates when sitting.
        public string WaterSpa_Relaxing_Speed_Modifier { get; set; } = "                    RELAXING SPA                    -  ENERGY GAIN SPEED      🗘 (1.45)         -  Modifier for how fast energy regenerates when sitting in water when outside temperature is above 20C. Example: 1.0 means no change, 0.5 means half the energy cost, 2.0 means double the energy cost. (1.45 = 145%, 0.5 = 50%)";
        public float WaterSpaRelaxingSpeedModifier { get; set; } = 1.45f; // Modifier for how fast energy regenerates when sitting.
        public string Drain_Energy_When_Healing { get; set; } = "                           HEALING DRAINS                  -  MOD MECHANIC           🗘 (false)        -  Should energy drain when healing? (This is only healing from sources, dosn't affect Healing Regeneration. If SlowTox is enabled, energy will tank when accumulated healing kicks in) Default = false";
        public bool DrainEnergyWhenHealing { get; set; } = false;
        public string Energy_Drain_From_Healing_Modifier { get; set; } = "                  HEALING DRAINS                  -  ENERGY COST            🗘 (1.0)          -  Modifier for the energy cost from using healing remedies like poultice. Yes it takes a toll on you getting hurt....";
        public float EnergyDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the energy drain from healing, 1.0f means no change, 0.5f means half the energy drain, 2.0f means double the energy drain.
        public string Drain_Energy_When_Taking_Damage { get; set; } = "                     DAMAGE DRAINS                   -  MOD MECHANIC           🗘 (true)         -  Should the player lose energy in relation to the amount of damage taken?";
        public bool DrainEnergyWhenTakingDamage { get; set; } = true;
        public string Energy_Drain_From_Damage_Multiplier { get; set; } = "                 DAMAGE DRAINS                   -  ENERGY COST            🗘 (1.0)          -  When taking damage energy is drained, because it takes a toll on you getting bit by a bear...";
        public float EnergyDrainFromDamageMultiplier { get; set; } = 1.0f;
        public string Energy_Game_Settings { get; set; } = "         ---------------------- ENERGY GAME PLAY  ----------------------                  -  These settings defines how energy is affecting gameplay";
        public string Enable_Adrenaline_Rush { get; set; } = "                              ADRENALINE RUSH                 -  MOD MECHANIC           🗘 (true)         -  Should the energy drain from animal, creature or player damage be delayed during fight or flight?";
        public bool EnableAdrenalineRush { get; set; } = true;
        public string Adrenaline_Duration { get; set; } = "                                 ADRENALINE DURATION             -  TIME                   🗘 (60)           -  For how long in real life seconds should the accumulated energy drain be delayed after taking attack damage?";
        public float AdrenalineDuration { get; set; } = 60f;
        public string Disable_BlockBreake_When_Sitting { get; set; } = "                    SITTING BLOCKBREAKE DISABLED    -  MOD MECHANIC           🗘 (true)         -  If you are sitting down aka. relaxing, then you can't breake blocks if this is enabled true. This is because you shouldn't be able to gain energy while doing work.";
        public bool DisableBlockBreakeWhenSitting { get; set; } = true;
        public string Hunger_Level_Matters { get; set; } = "                                HUNGER MATTERS                  -  MOD MECHANIC           🗘 (true)         -  This makes hunger/satiety a part of the energy mechanic. An example is, that low satiety increases the energy rate so you consume more energy when being hungry. Look at description to understand what hunger affects.";
        public bool HungerLevelMatters { get; set; } = true; // Should hunger have an impact on the energy rate?
        public string Only_Die_From_No_Energy { get; set; } = "                             HUNGER REWORK                   -  GAME MECHANIC          🗘 (true)         -  If enabled the player dosn't take damage when the hunger-bar reaches 0. Instead the player is starving and consuming way more energy, nutrition and invigoration. When nutrition, hunger and energy is 0, you die....";
        public bool OnlyDieFromNoEnergy { get; set; } = true; // If true, the player will only die from no energy, so if the player has no energy, they will die, but if they have energy, they will not die from hunger.
        public string Damage_If_No_Energy_And_Starving { get; set; } = "                    DAMAGE FATIGUE + STARVING       -  DAMAGE / TICK          🗘 (0.5)          -  Damage pr. tick. Only applied if OnlyDieFromNoEnergy = true.";
        public float DamageIfNoEnergyAndStarving { get; set; } = 0.5f; // Damage pr. tick
        public string Energy_Kills { get; set; } = "        IF HUNGER MATTERS IS DISABLED:  ENERGY KILLS                    -  MOD MECHANIC           🗘 (true)         -  This is only valid, if Hunger Matters is disabled. If HungerLevelMatters = false, then you take damage when energy reaches 0.";
        public bool EnergyKills { get; set; } = true; // Only used if Hunger Matters is disabled
        public string Damage_If_No_Energy_And_Hunger_Does_Not_Matter { get; set; } = "      DAMAGE FATIGUE                  -  DAMAGE / TICK          🗘 (0.25)         -  Damage pr. tick if hunger matters is disabled and energy reaches 0.";
        public float DamageIfNoEnergyAndHungerDoesNotMatter { get; set; } = 0.25f; // If hunger matters is disabled, this is the applied damage when no energy.
        public string Enable_Nutrient_Factor { get; set; } = "                              NUTRITION                       -  MOD MECHANIC           🗘 (true)         -  This makes nutrition an important part of overall health. Nutrition is the 5 bars in character menu like fruit, protein etc. (Read description to understand how it is implemented in this mod)";
        public bool EnableNutrientFactor { get; set; } = true; // Enable the nutrient factor, which will factor in invigoration based on the nutrient level of the player.
        public string Nutrition_Loss_When_Starving_Modifier { get; set; } = "               NUTRITION LOSS                  -  NUTRITION COST         🗘 (10.0)         -  If hunger-bar is empty, you are starving and that drains your nutrition values at an increased rate. This simulates that starving in real life makes your body using the resources it has.";
        public float NutritionLossWhenStarvingModifier { get; set; } = 10.0f; // Multiplier for how fast nutrition values should drain when starving.
        public string Hunger_Rate_Reduction_From_High_Energy { get; set; } = "              HUNGER RATE LOW                 -  STAT CHANGE            🗘 (0.5)          -  The percentage by which the hunger rate is reduced when energy is above 70%. This is gradual and the value is the final percentage when energy is 100%. (0.5 = 50% reduction from base hunger rate.)";
        public float HungerRateReductionFromHighEnergy { get; set; } = 0.5f; // The percentage by which the hunger rate is reduced from high energy, 0.5 = 50% reduction from base hunger rate.
        public string Hunger_Rate_Gain_From_Low_Energy { get; set; } = "                    HUNGER RATE HIGH                -  STAT CHANGE            🗘 (3.5)          -  The percentage by which the hunger rate is increased when energy is below 30%. This is gradual and the value is the final percentage when energy is 0%. (3.5 = 350% increase in hunger rate. This is added to the default 100% so 450% is the final value.)";
        public float HungerRateGainFromLowEnergy { get; set; } = 3.5f; // The factor by which the hunger rate is gained from low energy, 3.5 = 350% increase in hunger rate.
        public string Energy_After_Revival { get; set; } = "                                REVIVAL                         -  MOD MECHANIC           🗘 (0.5)          -  The amount of energy you have after revival. This is the percentage of the max energy capacity. (0.5 = 50%)";
        public float EnergyAfterRevival { get; set; } = 0.5f;
        public string Energy_Rate_Settings { get; set; } = "        ----------------------  ENERGY RATE  ----------------------                       -  Settings related to energy rate, energy rate increases or decreases the energy cost.";
        public string Hunger_Energy_Rate_Debuff { get; set; } = "                           HUNGER DEBUFF                   -  STAT CHANGE            🗘 (2500)         -  Maximum increase of energy rate caused by hunger. This value is the final energy rate when hunger reaches 0. (Set in percent; 1000 = 1000%)";
        public float HungerEnergyrateDebuff { get; set; } = 2500f; // Put in the wanted end percentage. 1000 = 1000%
        public string Hunger_Energy_Rate_Debuff_Start_Ratio { get; set; } = "               HUNGER THRESHOLD                -  MOD MECHANIC           🗘 (0.3)          -  When should hunger affect energy rate? Default is 0.3 which means the debuff starts when satiety is at 30% and downwards to 0";
        public float HungerEnergyrateDebuffStartRatio { get; set; } = 0.3f; // When should hunger matter? 0.3 = at 30% satiety and downwards.
        public string Body_Temperature_Matters { get; set; } = "                            TEMPERATURE MATTERS             -  MOD MECHANIC           🗘 (true)         -  This will make the body temperature of the player affect the energy rate, so if the player is too cold or too hot, they will have increased energy rate and will drain energy faster. (< 37, > 37.8)";
        public bool BodyTemperatureMatters { get; set; } = true; // Enable the body temperature matters, which will make the body temperature of the player matter for the energy level, so if the player is too cold or too hot, they will have increased energy rate and will drain energy faster.
        public string Energy_Rate_Per_Degrees { get; set; } = "                             TEMPERATURE DEBUFF              -  STAT CHANGE            🗘 (250)          -  This is the increased energy rate pr. degree C when being cold. If body temp is above 37.8 C then this value is doubled. This is because body temp above normal is very serious in real life and is rare in the game to ever happen. (250 = 250% pr. degree)";
        public float EnergyRatePerDegrees { get; set; } = 250f; // 45 = 450% So energy rate will increase 450% per degree of body temp outside normal core temp.
        public string Sprinting_Warmth { get; set; } = "                                    TEMPERATURE SPRINTING/JUMPING   -  STAT CHANGE            🗘 (0.06)         -  Sprinting or jumping while freezing raises the body temperature by (0.06) degrees pr. second";
        public float SprintingWarmth { get; set; } = 0.06f;
        public string Sleepiness_Energy_Rate_Debuff { get; set; } = "                       SLEEPINESS DEBUFF               -  STAT CHANGE            🗘 (3000)         -  When in an overloaded state of sleepiness, how much should the increase in energy rate be, when fully overloaded? (Set the final percentage; 3000 = 3000%) :: This is gradually increased from overload starts till it's full. See settings for sleepiness to get a hint on how overload is set.";
        public float SleepinessEnergyrateDebuff { get; set; } = 3000f; // Put in the wanted end percentage. 3000 = 3000%
        public string Energy_Stats_Settings { get; set; } = "       ----------------------  STAT CHANGES  ----------------------                      -  Stats affected by the current energy level. All values are either added as boost when above 70% energy or subtracted as debuff when below 30% energy.";
        public string Enable_Energy_Depended_WalkSpeed { get; set; } = "                    WALK SPEED                      -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on walkspeed?";
        public bool EnableEnergyDependedWalkSpeed { get; set; } = true;
        public string WalkSpeed_From_Energy { get; set; } = "                               WALK SPEED STATS                -  STAT CHANGE            🗘 (0.15 - 0.45)  -  How much should walkspeed be affected? (0.15 = 15%)";
        public float WalkSpeedBoostFromEnergy { get; set; } = 0.15f; // 0.15 = 15% added to base walkspeed
        public float WalkSpeedDebuffFromEnergy { get; set; } = 0.45f; // 0.45 = 45% subtracted from base walk speed.
        public string Enable_Armor_WalkSpeed_Affectedness { get; set; } = "                 ARMOR WALK SPEED                -  MOD MECHANIC           🗘 (true)         -  Should energy affect the walkspeed when wearing armor? This is only touching the debuff value to walkspeed when wearing an armor.";
        public bool EnableEnergyDependedArmorWalkSpeedAffectedness { get; set; } = true;
        public string Armor_Walkspeed_From_Energy { get; set; } = "                         ARMOR SPEED STATS               -  STAT CHANGE            🗘 (0.25 - 0.45)  -  How much should walkspeed be affected when wearing armor? (0.25 = 25%); Value is either added or subtracted from the original debuff to walkspeed comming from the armor itself.";
        public float ArmorWalkSpeedAffectednessBoostFromEnergy { get; set; } = 0.25f;
        public float ArmorWalkSpeedAffectednessDebuffFromEnergy { get; set; } = 0.45f;
        public string Enable_Energy_Depended_Jump_Height { get; set; } = "                  JUMP HEIGHT                     -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on jump height?";
        public bool EnableEnergyDependedJumpHeight { get; set; } = true; // Enable the energy depended jump speed, which will make the jump speed of the player depend on the current energy level of the player, so if the player has low energy, the jump speed will be lower.
        public string Jump_Height_From_High_Energy { get; set; } = "                        JUMP HEIGHT STATS               -  STAT CHANGE            🗘 (0.44 - 0.6)   -  The values are not so simple as percentages, so you will have to try for yourself.";
        public float JumpHeightBoostFromEnergy { get; set; } = 0.44f; // 0.6 equals 2 block jump height. 0.39 equals 2 block jump height when refreshed boost is 1.35.
        public float JumpHeightDebuffFromEnergy { get; set; } = 0.6f; // 0.6 equals to less than 1 block jump height.
        public string Enable_Energy_Depended_Tool_MiningSpeed { get; set; } = "             MINING SPEED                    -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on mining speed? (This changes the time it takes for a block to break, and it applies to all tools like axe, shovel etc.)";
        public bool EnableEnergyDependedToolMiningSpeed { get; set; } = true; // Enable the energy depended tool mining speed, which will make the mining speed of tools depend on the current energy level of the player, so if the player has low energy, the mining speed will be lower.
        public string Tool_MiningSpeed_From_Energy { get; set; } = "                        MINING SPEED STATS              -  STAT CHANGE            🗘 (0.2 - 0.95)   -  The values are not so simple either, try for yourself.";
        public float ToolMiningSpeedBoostFromEnergy { get; set; } = 0.2f; // The mining speed boost from high energy, value is added to base game value.
        public float ToolMiningSpeedDebuffFromEnergy { get; set; } = 0.95f; // The mining speed debuff from low energy, value is subtracted from the base game value.
        public string Enable_Energy_Depended_Melee_Weapon_Damage { get; set; } = "          MELEE WEAPON DAMAGE             -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on melee damage output?";
        public bool EnableEnergyDependedMeleeWeaponDamage { get; set; } = true;
        public string Melee_Weapon_Damage_From_Energy { get; set; } = "                     MELEE WEAPON DAMAGE STATS       -  STAT CHANGE            🗘 (0.35 - 0.65)  -  How much should melee damage be affected? (0.15 = 15%)";
        public float MeleeWeaponDamageBoostFromEnergy { get; set; } = 0.35f;
        public float MeleeWeaponDamageDebuffFromEnergy { get; set; } = 0.65f;
        public string Enable_Energy_Depended_Ranged_Weapon_Damage { get; set; } = "         RANGED WEAPON DAMAGE            -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on ranged damage output?";
        public bool EnableEnergyDependedRangedWeaponDamage { get; set; } = true;
        public string Ranged_Weapon_Damage_From_Energy { get; set; } = "                    RANGED WEAPON DAMAGE STATS      -  STAT CHANGE            🗘 (0.25 - 0.45)  -  How much should ranged damage be affected? (0.15 = 15%)";
        public float RangedWeaponDamageBoostFromEnergy { get; set; } = 0.25f;
        public float RangedWeaponDamageDebuffFromEnergy { get; set; } = 0.45f;
        public string Enable_Energy_Depended_Ranged_Weapon_Speed { get; set; } = "          RANGED WEAPON SPEED             -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on drawing speed on ranged weapons?";
        public bool EnableEnergyDependedRangedWeaponSpeed { get; set; } = true;
        public string Ranged_Weapon_Speed_From_Energy { get; set; } = "                     RANGED WEAPON SPEED STATS       -  STAT CHANGE            🗘 (0.25 - 0.75)  -  How much should ranged speed be affected? (0.15 = 15%)";
        public float RangedWeaponSpeedBoostFromEnergy { get; set; } = 0.25f; // 0.25 = 25%
        public float RangedWeaponSpeedDebuffFromEnergy { get; set; } = 0.75f;
        public string Enable_Energy_Depended_Bow_Drawing_Strength { get; set; } = "         BOW DRAWING STRENGTH            -  MOD MECHANIC           🗘 (true)         -  Should energy have any impact on drawing strength when using a bow?";
        public bool EnableEnergyDependedBowDrawingStrength { get; set; } = true;
        public string Bow_Drawing_Strength_From_Energy { get; set; } = "                    BOW DRAWING STRENGTH STATS      -  STAT CHANGE            🗘 (0.35 - 0.65)  -  How much should drawing strength be affected? (0.15 = 15%)";
        public float BowDrawingStrengthBoostFromEnergy { get; set; } = 0.35f;
        public float BowDrawingStrengthDebuffFromEnergy { get; set; } = 0.65f;
        public string Enable_Energy_Depended_Animal_Harvesting_Time { get; set; } = "       BUTCHERING TIME                 -  GAME MECHANIC          🗘 (true)         -  Should energy have any impact on the time it takes to skin an animal?";
        public bool EnableEnergyDependedAnimalHarvestingTime { get; set; } = true;
        public string Animal_Harvesting_Time_From_Energy { get; set; } = "                  BUTCHERING TIME STATS           -  STAT CHANGE            🗘 (0.15 - 0.85)  -  How much should animal harvesting time be affected? (0.15 = 15%)";
        public float AnimalHarvestingTimeBoostFromEnergy { get; set; } = 0.15f;
        public float AnimalHarvestingTimeDebuffFromEnergy { get; set; } = 0.85f;
        public string Refreshed_Energy_Boost_Multiplier { get; set; } = "                   REFRESHED ENERGY BOOST          -  STAT CHANGE BOOST      🗘 (1.35)         -  When sleepiness is completely full, you are in a refreshed state. This will boost all stats above by 1.35 = 35% (Refreshed threshold is set at sleepiness settings.)";
        public float RefreshedEnergyBoostMultiplier { get; set; } = 1.35f; // Multiplier for boosting stat values when refreshed from sleepiness being less than set value.
        // Sleepiness
        public string Sleepiness_Category { get; set; } = " ▁  ▂   ▃   ▄   ▅   ▆   ▇   █ - 💤 -  SLEEPINESS  - 💤 - █    ▇   ▆   ▅   ▄   ▃   ▂   ▁";
        public string Max_Sleepiness { get; set; } = "                                      MAX SLEEPINESS                                            🗘 (14)           -  What is the max amount of sleepiness, before being overloaded? This value should be seen as hours.";
        public float MaxSleepiness { get; set; } = 14f;
        public string Sleepiness_Capacity_Overload { get; set; } = "                        CAPACITY OVERLOAD                                         🗘 (0.8)          -  When sleepiness is above max, you are in an overloaded state of sleepiness. How big of a percentage of max should that overloaded state be? (0.8 = 80%) of max sleepiness capacity that can be overloaded. So if max sleepiness capacity is 14h then the player can be overloaded for an aditional 11.2h.)";
        public float SleepinessCapacityOverload { get; set; } = 0.8f; // Percentage (0.8 = 80%) of max sleepiness capacity that can be overloaded. So if max sleepiness capacity is 12h then the player can be overloaded for an aditional 10h.
        public string Feeling_Refreshed_Hours { get; set; } = "                             REFRESHED DURATION                                        🗘 (3)            -  The number of hours the player will feel refreshed starting from 0 sleepiness. This will give a buff to the player for the set amount of hours (3 = 3h) after sleeping. All stats from energy is affected by the boost set in RefreshedEnergyBoostMultiplier";
        public float FeelingRefreshedHours { get; set; } = 3f; // The number of hours the player will feel refreshed after sleeping, this will give a buff to the player for the set amount of hours (3 = 3h) after sleeping, so they will feel more energized and ready to go.
        public string Sleepiness_Stats { get; set; } = "             ---------------------- OVERLOAD STAT CHANGES  ----------------------             -  When in an overloaded state, these stats are changed.";
        public string Sleepiness_WeaponsAcc { get; set; } = "                               RANGED WEAPON ACCURACY          -  STAT CHANGE            🗘 (0.95)         -  How much should ranged weapon accuracy be affected when in an overloaded state? The value is the end value at maximum overload. Example: 0.95 means the player only has 5% accuracy when completely overloaded.";
        public float SleepinessRangedWeaponsAccDebuff { get; set; } = 0.95f;
        public string Sleepiness_Stats_When_Energy_Off { get; set; } = "            - 🤚 - ONLY IF ENERGY IS DISABLED - 🤚 -                         -  If energy is disabled, these stats will be affected by sleepiness alone. If energy is enabled these stats do nothing. How much should the following stats be affected by being in an overloaded state of sleepiness? (0.15 = 15%) :: To disable the stats just set them to 0.";
        
        public string Sleepiness_Walkspeed { get; set; } = "                                WALKSPEED                       -  STAT CHANGE            🗘 (0.65)         -  If energy mechanic is disabled in config, how much should walkspeed be affected from being in an overloaded state?";
        public float SleepinessWalkSpeedDebuff { get; set; } = 0.65f;
        public string Sleepiness_WeaponsSpeed { get; set; } = "                             RANGED WEAPON SPEED             -  STAT CHANGE            🗘 (0.9)          -  If energy mechanic is disabled in config, how much should ranged weapon speed be affected from being in an overloaded state?";
        public float SleepinessRangedWeaponsSpeedDebuff { get; set; } = 0.9f;
        public string Stats_ENERGY_DISABLED_EndOfLine { get; set; } = "------------------------------------------------------------------";
        public string Sleepiness_After_Revival { get; set; } = "                            REVIVAL                         -  MOD MECHANIC           🗘 (0.5)          -  The amount of sleepiness you have after revival. This is the percentage of the maximum capacity including the overload. (0.5 = 50%)";
        public float SleepinessAfterRevival { get; set; } = 0.5f;
        public string Gain_Sleepiness_When_Healing { get; set; } = "                        HEALING MAKES SLEEPY            -  MOD MECHANIC           🗘 (true)         -  Sleepiness increases when healing from all sources e.g. poultice. This is to simulate that when you are hurt and finally recovering, you get sleepy and need to rest. If you are using the mod SlowTox beware that when you drink alcohol you will get sleepy, due to the accumulated healing from intoxication.";
        public bool GainSleepinessWhenHealing { get; set; } = true;
        public string GainSleepiness_WhenHealing_Modifier { get; set; } = "                 HEALING MAKES SLEEPY            -  SLEEPINESS GAIN        🗘 (1.0)          -  Modifier for how much sleepiness is gained from healing.";
        public float GainSleepinessWhenHealingModifier { get; set; } = 1.0f;
        // Invigoration
        public string Invigoration_Category { get; set; } = " ▁  ▂   ▃   ▄   ▅   ▆   ▇   █ - 💪 -  INVIGORATION  - 💪 - █    ▇   ▆   ▅   ▄   ▃   ▂   ▁";
        public string Lose_Invigoration_When_Dying { get; set; } = "                        REVIVAL                         -  MOD MECHANIC           🗘 (true)         -  If enabled, this will reset the invigoration level to 0 when the player dies, so they will have to regain it again.";
        public bool LoseInvigorationWhenDying { get; set; } = true; // Lose invigoration when dying, this will reset the invigoration level to 0 when the player dies, so they will have to regain it again.
        public string Drain_Invigoration_When_Healing { get; set; } = "                     HEALING DRAINS                  -  MOD MECHANIC           🗘 (false)        -  If enabled, this will hurt the invigoration when the player heals with remedies, so they will have to regain it again. This is useful for balancing the game, so players can't just spam healing and keep their invigoration level high.";
        public bool DrainInvigorationWhenHealing { get; set; } = false; // Lose invigoration when healing, this will hurt the invigoration when the player heals, so they will have to regain it again. This is useful for balancing the game, so players can't just spam healing and keep their invigoration level high.
        public string Invigoration_Drain_From_Healing_Modifier { get; set; } = "            HEALING DRAINS                  -  INVIGORATION COST      🗘 (1.0)          -  This is a modifier for the invigoration drain from healing, 1.0f means no change, 0.5f means half the invigoration drain, 2.0f means double the invigoration drain.";
        public float InvigorationDrainFromHealingModifier { get; set; } = 1.0f; // Default is (MaxEnergy / 10), This is a modifier for the invigoration drain from healing, 1.0f means no change, 0.5f means half the invigoration drain, 2.0f means double the invigoration drain.
        public string Drain_From_Damage_Modifier { get; set; } = "                          DAMAGE DRAINS                   -  INVIGORATION COST      🗘 (1.0)          -  Modifier of how much invigoration that will be drained when the player takes damage while invigorated.";
        public float InvigoratedDrainFromDamageMultiplier { get; set; } = 1.0f; // Percentage of Invigoration that will be drained when the player takes damage while invigorated, 0.1f means 10% of CurrentEnergy will be drained when the player takes damage while invigorated.
        public string Enable_Invigorated_Health_Boost { get; set; } = "                     HEALTH BOOST                    -  MOD MECHANIC           🗘 (true)         -  Should the player gain extra health points when in a good overall health? (Overall health is a mean ratio of invigoration and nutrition)";
        public bool EnableInvigoratedHealthBoost { get; set; } = true; // Should the player gain extra health points with invigoration?
        public string Health_Boost_Percentage_Of_Max_Health { get; set; } = "               HEALTH BOOST                    -  STAT CHANGE            🗘 (0.5)          -  How much health should be given at max overall health? Base max health = 15 by default. (15 * 0.5 = 7.5 hp)";
        public float InvigoratedHealthBoostPercentageOfMaxHealth { get; set; } = 0.5f; // 7.5 health points if full invigoration and well nutritious if max health is 15 hp.
        public string Healing_Effectiveness_From_Overall_Health { get; set; } = "           HEALING EFFECTIVENESS           -  MOD MECHANIC           🗘 (true)         -  Should the player gain healing effectiveness when in a good overall health?";
        public bool EnableInvigoratedHealingEffectiveness { get; set; } = true; // Healing effectiveness increases with invigoration.
        public string Healing_Effectiveness_Modifier { get; set; } = "                      HEALING EFFECTIVENESS           -  STAT CHANGE            🗘 (1.0)          -  1.0 = default + 100% = 200% Healing Effectiveness";
        public float InvigoratedHealingEffectivenessModifier { get; set; } = 1.0f; // 1.0 = default + 100% = 200% Healing Effectiveness 
        public string Enable_Invigorated_Max_Energy_Boost { get; set; } = "                 MAX ENERGY BOOST                -  MOD MECHANIC           🗘 (true)         -  Should the player gain increased max energy capacity when in a good overall health?";
        public bool EnableInvigoratedMaxEnergyBoost { get; set; } = true; // Should max energy increase with invigoration?
        public string Max_Energy_Boost_Modifier { get; set; } = "                           MAX ENERGY BOOST                -  CAPACITY INCREASE      🗘 (1.0)          -  Default has a final value for max energy when in good overall health equal to 1500. Setting this to 1.1 sets it 10% higher = 1650";
        public float InvigoratedMaxEnergyBoostModifier { get; set; } = 1.0f; // 1.0 means final value for max energy when fully invigorated equals to 1500.
        public string Enable_Invigorated_Lung_Capacity_Boost { get; set; } = "              LUNG CAPACITY BOOST             -  MOD MECHANIC           🗘 (true)         -  Should the player gain increased lung capacity when in a good overall health?";
        public bool EnableInvigoratedLungCapacityBoost { get; set; } = true; // Enable the invigorated lung capacity boost, which will boost the lung capacity of the player based on the configured lung capacity in the config file.
        public string Lung_Capacity_Boost_Percentage { get; set; } = "                      LUND CAPACITY BOOST             -  CAPACITY INCREASE      🗘 (0.5)          -  (0.5 = 50%) of base lung capacity, so if the base lung capacity is 40.000 (40s), then it will be 60s when in good overall health.";
        public float InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity { get; set; } = 0.5f; // 50% of base lung capacity, so if the base lung capacity is 40.000 (40s), then it will be 60s.
        // During sleep
        public string Sleeping_Category { get; set; } = " ▁  ▂   ▃   ▄   ▅   ▆   ▇  - 🛏 - 🛌 -  SLEEP  - 🛌 - 🛏 -  ▇   ▆   ▅   ▄   ▃   ▂   ▁";
        public string Sleep_Regeneration_Factor { get; set; } = "                           REGENERATION SLEEPINESS         -  SLEEPINESS DRAIN       🗘 (1.0)          -  Modifier for how much sleep should decrease sleepiness. If you can't get sleepiness down enough while sleeping, then set this value higher than 1.";
        public float SleepRegenerationFactor { get; set; } = 1.0f; // Multiplier for how much sleepiness should decrease sleepiness.
        public string Sleep_Boost_From_High_Energy { get; set; } = "                        SLEEP BOOST                     -  SLEEPINESS DRAIN       🗘 (0.35)         -  The factor by which having high energy increases sleep efficiency. (Value is not correlating, so be carefull. Higher value means more buff.)";
        public float SleepBoostFromHighEnergy { get; set; } = 0.35f; // The factor by which having high energy increases sleep efficiency. (Value is not correlating, so be carefull)
        public string Sleep_Boost_From_Low_Energy { get; set; } = "                         SLEEP DEBUFF                    -  SLEEPINESS DRAIN       🗘 (0.5)          -  The factor by which having low energy decreases sleep efficiency. (Value is not correlating, so be carefull. Higher value means less debuff.)";
        public float SleepDebuffFromLowEnergy { get; set; } = 0.5f; // The factor by which low energy decreases sleep efficiency.
        public string Energy_Restored_By_Sleeping_Modifier { get; set; } = "                REGENERATION ENERGY             -  ENERGY GAIN            🗘 (0.75)         -  Factor for how fast energy regenerates when sleeping. (Value is not correlating, so be carefull)";
        public float EnergyRestoredBySleepingModifier { get; set; } = 0.75f; // Factor for how fast energy regenerates when sleeping.
        public string Energy_From_Sleep_When_Refreshed_Modifier { get; set; } = "           REFRESHED SLEEP                 -  ENERGY GAIN            🗘 (1.35)         -  When sleepiness is fully drained, and you are still sleeping, you then gain additional energy while sleeping. (1.35 = 35% increase in energy restored)";
        public float EnergyFromSleepWhenRefreshedModifier { get; set; } = 1.35f; // The factor by which the energy is regenerated from sleeping when sleepiness is fully drained.
        public string Delay_Seconds { get; set; } = "                                       DELAY SLEEPINESS                -  TIME                   🗘 (25)           -  This is used to delay sleepiness gain after waking up for X amount of real life seconds.";
        public float DelaySeconds { get; set; } = 25f;
        public string Tiredness_After_Sleep { get; set; } = "                               TIREDNESS VALUE                 -  TIME                   🗘 (5)            -  Tiredness is the vanilla mechanic that determines when you can sleep again, after waking up. Tiredness goes up as time goes and when it hits a value of 8, the player is allowed to sleep. Setting this to 5 results in a 4 in-game hours delay after waking up, before the player can sleep again for all bed types. Set this to 8 or above to be able to sleep as soon as you want after waking up.";
        public float TirednessAfterSleep { get; set; } = 5f;
        public float TirednessAfterRevival { get; set; } = 5f;
        public string Disable_Vanilla_Tiredness { get; set; } = "                           DISABLE VANILLA TIREDNESS       -  VANILLA MECHANIC       🗘 (false)        -  Disable the vanilla tiredness mechanic if set to true. This means the player can now sleep any time they want.";
        public bool DisableTiredness { get; set; } = false;
        public string Disable_Behaviors { get; set; } = "           ----------------------  DISABLE MOD BEHAVIORS  ---------------------                                -  Here you can turn off Energy or Sleepiness and only keep the behavior you want. You can also turn off all stat changes if you don't want the mod touching any stats at all.";
        public string Disable_Behaviors_Note { get; set; } = "                   -----   🕱 🚫  WARNING! 🚫 🕱   -----       -  Only Enable/Disable Energy or Sleepiness when out of game, so the world can load with the new config";
        public string Enable_Energy { get; set; } = "                                       ENABLE ENERGY                   -  MOD SYSTEM             🗘 (true)         -  You can disable the energy system and only keep the sleepiness, there will bee slight changes to the mod and what it affects.";
        public bool EnableEnergy { get; set; } = true;
        public string Enable_Sleepiness { get; set; } = "                                   ENABLE SLEEPINESS               -  MOD SYSTEM             🗘 (true)         -  You can disable the sleepiness system and only keep the energy, there will bee slight changes to the mod and what it affects.";
        public bool EnableSleepiness { get; set; } = true;
        public string Disable_StatChanges { get; set; } = "                                 DISABLE ALL STATS CHANGES       -  MOD SYSTEM             🗘 (false)        -  Disable all changes to stats affected by this mod. If you like managing the HUD bars, but don't care for the buff and debuffs to stats.";
        public bool DisableStatChanges { get; set; } = false;
        public SyncedConfig ToSyncedConfig()
        {
            return new SyncedConfig
            {
                // Master Switches
                EnableEnergy = this.EnableEnergy,
                EnableSleepiness = this.EnableSleepiness,
                DisableStatChanges = this.DisableStatChanges,

                // Energy
                MaxEnergy = this.MaxEnergy,
                EnergySpeedModifier = this.EnergySpeedModifier,
                MovementEnergyCostModifier = this.MovementEnergyCostModifier,
                SprintingJumpingEnergyCostModifier = this.SprintingJumpingEnergyCostModifier,
                NoToolsWorkEnergyCostModifier = this.NoToolsWorkEnergyCostModifier,
                LightToolsEnergyCostModifier = this.LightToolsEnergyCostModifier,
                MediumToolsEnergyCostModifier = this.MediumToolsEnergyCostModifier,
                HeavyToolsEnergyCostModifier = this.HeavyToolsEnergyCostModifier,
                WeaponsEnergyCostModifier = this.WeaponsEnergyCostModifier,
                AttackEnergyCostModifier = this.AttackEnergyCostModifier,
                SittingRelaxingSpeedModifier = this.SittingRelaxingSpeedModifier,
                WaterSpaRelaxingSpeedModifier = this.WaterSpaRelaxingSpeedModifier,
                DrainEnergyWhenHealing = this.DrainEnergyWhenHealing,
                EnergyDrainFromHealingModifier = this.EnergyDrainFromHealingModifier,
                DrainEnergyWhenTakingDamage = this.DrainEnergyWhenTakingDamage,
                EnergyDrainFromDamageMultiplier = this.EnergyDrainFromDamageMultiplier,
                EnableAdrenalineRush = this.EnableAdrenalineRush,
                AdrenalineDuration = this.AdrenalineDuration,
                DisableBlockBreakeWhenSitting = this.DisableBlockBreakeWhenSitting,

                // Hunger & Nutrition
                HungerLevelMatters = this.HungerLevelMatters,
                OnlyDieFromNoEnergy = this.OnlyDieFromNoEnergy,
                DamageIfNoEnergyAndStarving = this.DamageIfNoEnergyAndStarving,
                EnergyKills = this.EnergyKills,
                DamageIfNoEnergyAndHungerDoesNotMatter = this.DamageIfNoEnergyAndHungerDoesNotMatter,
                EnableNutrientFactor = this.EnableNutrientFactor,
                NutritionLossWhenStarvingModifier = this.NutritionLossWhenStarvingModifier,
                HungerRateReductionFromHighEnergy = this.HungerRateReductionFromHighEnergy,
                HungerRateGainFromLowEnergy = this.HungerRateGainFromLowEnergy,
                EnergyAfterRevival = this.EnergyAfterRevival,

                // Energy Rates
                HungerEnergyrateDebuff = this.HungerEnergyrateDebuff,
                HungerEnergyrateDebuffStartRatio = this.HungerEnergyrateDebuffStartRatio,
                BodyTemperatureMatters = this.BodyTemperatureMatters,
                EnergyRatePerDegrees = this.EnergyRatePerDegrees,
                SprintingWarmth = this.SprintingWarmth,
                SleepinessEnergyrateDebuff = this.SleepinessEnergyrateDebuff,

                // Stats
                EnableEnergyDependedWalkSpeed = this.EnableEnergyDependedWalkSpeed,
                WalkSpeedBoostFromEnergy = this.WalkSpeedBoostFromEnergy,
                WalkSpeedDebuffFromEnergy = this.WalkSpeedDebuffFromEnergy,
                EnableEnergyDependedArmorWalkSpeedAffectedness = this.EnableEnergyDependedArmorWalkSpeedAffectedness,
                ArmorWalkSpeedAffectednessBoostFromEnergy = this.ArmorWalkSpeedAffectednessBoostFromEnergy,
                ArmorWalkSpeedAffectednessDebuffFromEnergy = this.ArmorWalkSpeedAffectednessDebuffFromEnergy,
                EnableEnergyDependedJumpHeight = this.EnableEnergyDependedJumpHeight,
                JumpHeightBoostFromEnergy = this.JumpHeightBoostFromEnergy,
                JumpHeightDebuffFromEnergy = this.JumpHeightDebuffFromEnergy,
                EnableEnergyDependedToolMiningSpeed = this.EnableEnergyDependedToolMiningSpeed,
                ToolMiningSpeedBoostFromEnergy = this.ToolMiningSpeedBoostFromEnergy,
                ToolMiningSpeedDebuffFromEnergy = this.ToolMiningSpeedDebuffFromEnergy,
                EnableEnergyDependedMeleeWeaponDamage = this.EnableEnergyDependedMeleeWeaponDamage,
                MeleeWeaponDamageBoostFromEnergy = this.MeleeWeaponDamageBoostFromEnergy,
                MeleeWeaponDamageDebuffFromEnergy = this.MeleeWeaponDamageDebuffFromEnergy,
                EnableEnergyDependedRangedWeaponDamage = this.EnableEnergyDependedRangedWeaponDamage,
                RangedWeaponDamageBoostFromEnergy = this.RangedWeaponDamageBoostFromEnergy,
                RangedWeaponDamageDebuffFromEnergy = this.RangedWeaponDamageDebuffFromEnergy,
                EnableEnergyDependedRangedWeaponSpeed = this.EnableEnergyDependedRangedWeaponSpeed,
                RangedWeaponSpeedBoostFromEnergy = this.RangedWeaponSpeedBoostFromEnergy,
                RangedWeaponSpeedDebuffFromEnergy = this.RangedWeaponSpeedDebuffFromEnergy,
                EnableEnergyDependedBowDrawingStrength = this.EnableEnergyDependedBowDrawingStrength,
                BowDrawingStrengthBoostFromEnergy = this.BowDrawingStrengthBoostFromEnergy,
                BowDrawingStrengthDebuffFromEnergy = this.BowDrawingStrengthDebuffFromEnergy,
                EnableEnergyDependedAnimalHarvestingTime = this.EnableEnergyDependedAnimalHarvestingTime,
                AnimalHarvestingTimeBoostFromEnergy = this.AnimalHarvestingTimeBoostFromEnergy,
                AnimalHarvestingTimeDebuffFromEnergy = this.AnimalHarvestingTimeDebuffFromEnergy,
                RefreshedEnergyBoostMultiplier = this.RefreshedEnergyBoostMultiplier,

                // Sleepiness
                MaxSleepiness = this.MaxSleepiness,
                SleepinessCapacityOverload = this.SleepinessCapacityOverload,
                FeelingRefreshedHours = this.FeelingRefreshedHours,
                SleepinessRangedWeaponsAccDebuff = this.SleepinessRangedWeaponsAccDebuff,
                SleepinessWalkSpeedDebuff = this.SleepinessWalkSpeedDebuff,
                SleepinessRangedWeaponsSpeedDebuff = this.SleepinessRangedWeaponsSpeedDebuff,
                SleepinessAfterRevival = this.SleepinessAfterRevival,
                GainSleepinessWhenHealing = this.GainSleepinessWhenHealing,
                GainSleepinessWhenHealingModifier = this.GainSleepinessWhenHealingModifier,

                // Invigoration
                LoseInvigorationWhenDying = this.LoseInvigorationWhenDying,
                DrainInvigorationWhenHealing = this.DrainInvigorationWhenHealing,
                InvigorationDrainFromHealingModifier = this.InvigorationDrainFromHealingModifier,
                InvigoratedDrainFromDamageMultiplier = this.InvigoratedDrainFromDamageMultiplier,
                EnableInvigoratedHealthBoost = this.EnableInvigoratedHealthBoost,
                InvigoratedHealthBoostPercentageOfMaxHealth = this.InvigoratedHealthBoostPercentageOfMaxHealth,
                EnableInvigoratedHealingEffectiveness = this.EnableInvigoratedHealingEffectiveness,
                InvigoratedHealingEffectivenessModifier = this.InvigoratedHealingEffectivenessModifier,
                EnableInvigoratedMaxEnergyBoost = this.EnableInvigoratedMaxEnergyBoost,
                InvigoratedMaxEnergyBoostModifier = this.InvigoratedMaxEnergyBoostModifier,
                EnableInvigoratedLungCapacityBoost = this.EnableInvigoratedLungCapacityBoost,
                InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity = this.InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity,

                // Sleep Regeneration
                SleepRegenerationFactor = this.SleepRegenerationFactor,
                SleepBoostFromHighEnergy = this.SleepBoostFromHighEnergy,
                SleepDebuffFromLowEnergy = this.SleepDebuffFromLowEnergy,
                EnergyRestoredBySleepingModifier = this.EnergyRestoredBySleepingModifier,
                EnergyFromSleepWhenRefreshedModifier = this.EnergyFromSleepWhenRefreshedModifier,
                DelaySeconds = this.DelaySeconds,
                TirednessAfterSleep = this.TirednessAfterSleep,
                TirednessAfterRevival = this.TirednessAfterRevival,
                DisableTiredness = this.DisableTiredness
            };
        }

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
            this.WaterSpaRelaxingSpeedModifier = previousConfig.WaterSpaRelaxingSpeedModifier;
            this.AttackEnergyCostModifier = previousConfig.AttackEnergyCostModifier;
            this.HeavyToolsEnergyCostModifier = previousConfig.HeavyToolsEnergyCostModifier;
            this.MediumToolsEnergyCostModifier = previousConfig.MediumToolsEnergyCostModifier;
            this.LightToolsEnergyCostModifier = previousConfig.LightToolsEnergyCostModifier;
            this.WeaponsEnergyCostModifier = previousConfig.WeaponsEnergyCostModifier;
            this.NoToolsWorkEnergyCostModifier = previousConfig.NoToolsWorkEnergyCostModifier;
            this.MovementEnergyCostModifier = previousConfig.MovementEnergyCostModifier;
            this.SprintingJumpingEnergyCostModifier = previousConfig.SprintingJumpingEnergyCostModifier;


            // Energy related stats
            this.DisableBlockBreakeWhenSitting = previousConfig.DisableBlockBreakeWhenSitting;
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
            this.EnableEnergyDependedMeleeWeaponDamage = previousConfig.EnableEnergyDependedMeleeWeaponDamage;
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
            this.DrainEnergyWhenTakingDamage = previousConfig.DrainEnergyWhenTakingDamage;
            this.EnergyDrainFromDamageMultiplier = previousConfig.EnergyDrainFromDamageMultiplier;
            this.InvigoratedDrainFromDamageMultiplier = previousConfig.InvigoratedDrainFromDamageMultiplier;
            this.LoseInvigorationWhenDying = previousConfig.LoseInvigorationWhenDying;
            this.EnergyAfterRevival = previousConfig.EnergyAfterRevival;
            this.DrainEnergyWhenHealing = previousConfig.DrainEnergyWhenHealing;
            this.DrainInvigorationWhenHealing = previousConfig.DrainInvigorationWhenHealing;
            this.InvigorationDrainFromHealingModifier = previousConfig.InvigorationDrainFromHealingModifier;
            this.EnergyDrainFromHealingModifier = previousConfig.EnergyDrainFromHealingModifier;
            this.EnableAdrenalineRush = previousConfig.EnableAdrenalineRush;
            this.AdrenalineDuration = previousConfig.AdrenalineDuration;

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
            this.SprintingWarmth = previousConfig.SprintingWarmth;

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
            this.GainSleepinessWhenHealing = previousConfig.GainSleepinessWhenHealing;
            this.GainSleepinessWhenHealingModifier = previousConfig.GainSleepinessWhenHealingModifier;
            this.DelaySeconds = previousConfig.DelaySeconds;
            this.TirednessAfterSleep = previousConfig.TirednessAfterSleep;
            this.TirednessAfterRevival = previousConfig.TirednessAfterRevival;
            this.DisableTiredness = previousConfig.DisableTiredness;

            // Master Switches
            this.EnableEnergy = previousConfig.EnableEnergy;
            this.EnableSleepiness = previousConfig.EnableSleepiness;
            this.DisableStatChanges = previousConfig.DisableStatChanges;
        }


    }

    
    [ProtoContract]
    public class SyncedConfig : IModConfig
    {

        // Master Switches
        [ProtoMember(1)] public bool EnableEnergy { get; set; } // = true;
        [ProtoMember(2)] public bool EnableSleepiness { get; set; } // = true;
        [ProtoMember(3)] public bool DisableStatChanges { get; set; }

        // Energy Data
        [ProtoMember(4)] public float MaxEnergy { get; set; }
        [ProtoMember(5)] public float EnergySpeedModifier { get; set; }
        [ProtoMember(6)] public float MovementEnergyCostModifier { get; set; }
        [ProtoMember(7)] public float SprintingJumpingEnergyCostModifier { get; set; }
        [ProtoMember(8)] public float NoToolsWorkEnergyCostModifier { get; set; }
        [ProtoMember(9)] public float LightToolsEnergyCostModifier { get; set; }
        [ProtoMember(10)] public float MediumToolsEnergyCostModifier { get; set; }
        [ProtoMember(11)] public float HeavyToolsEnergyCostModifier { get; set; }
        [ProtoMember(12)] public float WeaponsEnergyCostModifier { get; set; }
        [ProtoMember(13)] public float AttackEnergyCostModifier { get; set; }
        [ProtoMember(14)] public float SittingRelaxingSpeedModifier { get; set; }
        [ProtoMember(15)] public float WaterSpaRelaxingSpeedModifier { get; set; }
        [ProtoMember(16)] public bool DrainEnergyWhenHealing { get; set; }
        [ProtoMember(17)] public float EnergyDrainFromHealingModifier { get; set; }
        [ProtoMember(18)] public bool DrainEnergyWhenTakingDamage { get; set; }
        [ProtoMember(19)] public float EnergyDrainFromDamageMultiplier { get; set; }
        [ProtoMember(20)] public bool EnableAdrenalineRush { get; set; }
        [ProtoMember(21)] public float AdrenalineDuration { get; set; }
        [ProtoMember(22)] public bool DisableBlockBreakeWhenSitting { get; set; }

        // Hunger & Nutrition
        [ProtoMember(23)] public bool HungerLevelMatters { get; set; }
        [ProtoMember(24)] public bool OnlyDieFromNoEnergy { get; set; }
        [ProtoMember(25)] public float DamageIfNoEnergyAndStarving { get; set; }
        [ProtoMember(26)] public bool EnergyKills { get; set; }
        [ProtoMember(27)] public float DamageIfNoEnergyAndHungerDoesNotMatter { get; set; }
        [ProtoMember(28)] public bool EnableNutrientFactor { get; set; }
        [ProtoMember(29)] public float NutritionLossWhenStarvingModifier { get; set; }
        [ProtoMember(30)] public float HungerRateReductionFromHighEnergy { get; set; }
        [ProtoMember(31)] public float HungerRateGainFromLowEnergy { get; set; }
        [ProtoMember(32)] public float EnergyAfterRevival { get; set; }

        // Energy Rates
        [ProtoMember(33)] public float HungerEnergyrateDebuff { get; set; }
        [ProtoMember(34)] public float HungerEnergyrateDebuffStartRatio { get; set; }
        [ProtoMember(35)] public bool BodyTemperatureMatters { get; set; }
        [ProtoMember(36)] public float EnergyRatePerDegrees { get; set; }
        [ProtoMember(37)] public float SleepinessEnergyrateDebuff { get; set; }

        // Stats
        [ProtoMember(38)] public bool EnableEnergyDependedWalkSpeed { get; set; }
        [ProtoMember(39)] public float WalkSpeedBoostFromEnergy { get; set; }
        [ProtoMember(40)] public float WalkSpeedDebuffFromEnergy { get; set; }
        [ProtoMember(41)] public bool EnableEnergyDependedArmorWalkSpeedAffectedness { get; set; }
        [ProtoMember(42)] public float ArmorWalkSpeedAffectednessBoostFromEnergy { get; set; }
        [ProtoMember(43)] public float ArmorWalkSpeedAffectednessDebuffFromEnergy { get; set; }
        [ProtoMember(44)] public bool EnableEnergyDependedJumpHeight { get; set; }
        [ProtoMember(45)] public float JumpHeightBoostFromEnergy { get; set; }
        [ProtoMember(46)] public float JumpHeightDebuffFromEnergy { get; set; }
        [ProtoMember(47)] public bool EnableEnergyDependedToolMiningSpeed { get; set; }
        [ProtoMember(48)] public float ToolMiningSpeedBoostFromEnergy { get; set; }
        [ProtoMember(49)] public float ToolMiningSpeedDebuffFromEnergy { get; set; }
        [ProtoMember(50)] public bool EnableEnergyDependedMeleeWeaponDamage { get; set; }
        [ProtoMember(51)] public float MeleeWeaponDamageBoostFromEnergy { get; set; }
        [ProtoMember(52)] public float MeleeWeaponDamageDebuffFromEnergy { get; set; }
        [ProtoMember(53)] public bool EnableEnergyDependedRangedWeaponDamage { get; set; }
        [ProtoMember(54)] public float RangedWeaponDamageBoostFromEnergy { get; set; }
        [ProtoMember(55)] public float RangedWeaponDamageDebuffFromEnergy { get; set; }
        [ProtoMember(56)] public bool EnableEnergyDependedRangedWeaponSpeed { get; set; }
        [ProtoMember(57)] public float RangedWeaponSpeedBoostFromEnergy { get; set; }
        [ProtoMember(58)] public float RangedWeaponSpeedDebuffFromEnergy { get; set; }
        [ProtoMember(59)] public bool EnableEnergyDependedBowDrawingStrength { get; set; }
        [ProtoMember(60)] public float BowDrawingStrengthBoostFromEnergy { get; set; }
        [ProtoMember(61)] public float BowDrawingStrengthDebuffFromEnergy { get; set; }
        [ProtoMember(62)] public bool EnableEnergyDependedAnimalHarvestingTime { get; set; }
        [ProtoMember(63)] public float AnimalHarvestingTimeBoostFromEnergy { get; set; }
        [ProtoMember(64)] public float AnimalHarvestingTimeDebuffFromEnergy { get; set; }
        [ProtoMember(65)] public float RefreshedEnergyBoostMultiplier { get; set; }

        // Sleepiness
        [ProtoMember(66)] public float MaxSleepiness { get; set; }
        [ProtoMember(67)] public float SleepinessCapacityOverload { get; set; }
        [ProtoMember(68)] public float FeelingRefreshedHours { get; set; }
        [ProtoMember(69)] public float SleepinessRangedWeaponsAccDebuff { get; set; }
        [ProtoMember(70)] public float SleepinessWalkSpeedDebuff { get; set; }
        [ProtoMember(71)] public float SleepinessRangedWeaponsSpeedDebuff { get; set; }
        [ProtoMember(72)] public float SleepinessAfterRevival { get; set; }
        [ProtoMember(73)] public bool GainSleepinessWhenHealing { get; set; }
        [ProtoMember(74)] public float GainSleepinessWhenHealingModifier { get; set; }

        // Invigoration
        [ProtoMember(75)] public bool LoseInvigorationWhenDying { get; set; }
        [ProtoMember(76)] public bool DrainInvigorationWhenHealing { get; set; }
        [ProtoMember(77)] public float InvigorationDrainFromHealingModifier { get; set; }
        [ProtoMember(78)] public float InvigoratedDrainFromDamageMultiplier { get; set; }
        [ProtoMember(79)] public bool EnableInvigoratedHealthBoost { get; set; }
        [ProtoMember(80)] public float InvigoratedHealthBoostPercentageOfMaxHealth { get; set; }
        [ProtoMember(81)] public bool EnableInvigoratedHealingEffectiveness { get; set; }
        [ProtoMember(82)] public float InvigoratedHealingEffectivenessModifier { get; set; }
        [ProtoMember(83)] public bool EnableInvigoratedMaxEnergyBoost { get; set; }
        [ProtoMember(84)] public float InvigoratedMaxEnergyBoostModifier { get; set; }
        [ProtoMember(85)] public bool EnableInvigoratedLungCapacityBoost { get; set; }
        [ProtoMember(86)] public float InvigoratedLungCapacityBoostPercentageOfConfigLungCapacity { get; set; }

        // Sleeping
        [ProtoMember(87)] public float SleepRegenerationFactor { get; set; }
        [ProtoMember(88)] public float SleepBoostFromHighEnergy { get; set; }
        [ProtoMember(89)] public float SleepDebuffFromLowEnergy { get; set; }
        [ProtoMember(90)] public float EnergyRestoredBySleepingModifier { get; set; }
        [ProtoMember(91)] public float EnergyFromSleepWhenRefreshedModifier { get; set; }
        [ProtoMember(92)] public float DelaySeconds { get; set; }
        [ProtoMember(93)] public float SprintingWarmth { get; set; }
        [ProtoMember(94)] public float TirednessAfterSleep { get; set; }
        [ProtoMember(95)] public float TirednessAfterRevival { get; set; }
        [ProtoMember(96)] public bool DisableTiredness { get; set; }

        public SyncedConfig()
        {
        }

        public SyncedConfig Clone() 
        {
            return (SyncedConfig)this.MemberwiseClone();
        }
    }



    public static class ModConfig
    {
        public static T ReadConfig<T>(ICoreAPI api, string jsonConfig) where T : IModConfig
        {
            T config;
            try
            {
                config = api.LoadModConfig<T>(jsonConfig);
            }
            catch (System.Exception e)
            {
                api.Logger.Error("Failed to load custom config for {0}. Error: {1}", jsonConfig, e.Message);
                config = null;
            }

            if (config == null)
            {
                
                config = (T)Activator.CreateInstance(typeof(T), api, null);

                
                string updatePath = Path.Combine(api.GetOrCreateDataPath("ModConfig"), jsonConfig);

                if (!System.IO.File.Exists(updatePath))
                {
                    api.StoreModConfig(config, jsonConfig);
                }
            }
            else
            {
                
                api.StoreModConfig(config, jsonConfig);
            }

            return config;
        }

        public static void WriteConfig<T>(ICoreAPI api, string jsonConfig, T config) where T : IModConfig
        {
            api.StoreModConfig(config, jsonConfig);
        }

        
        public static string GetConfigPath(ICoreAPI api)
        {
            return Path.Combine(api.GetOrCreateDataPath("ModConfig"), "SleepNeed");
        }
    }
}
