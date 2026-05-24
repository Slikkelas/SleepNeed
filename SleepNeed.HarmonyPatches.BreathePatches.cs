using HarmonyLib;
using SleepNeed.Energy;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using System;

namespace SleepNeed.HarmonyPatches.BreathePatches
{
    [HarmonyPatch(typeof(EntityBehaviorBreathe))]
    public static class BreathePatches
    {
        // Helper to access the private maxOxygen field
        private static readonly AccessTools.FieldRef<EntityBehaviorBreathe, float> MaxOxygenField =
            AccessTools.FieldRefAccess<EntityBehaviorBreathe, float>("maxOxygen");

        private static float GetSleepNeedBoost(Entity entity)
        {
            var energyBehavior = entity.GetBehavior<EntityBehaviorEnergy>();
            return energyBehavior != null ? energyBehavior.BoostLungCapacity : 0f;
        }

        // PREFIX: Blocks AD from forcing Oxygen down to 40k when we are boosted
        [HarmonyPatch("Oxygen", MethodType.Setter)]
        [HarmonyPrefix]
        public static bool OxygenSetter_Prefix(EntityBehaviorBreathe __instance, float value)
        {
            float boost = GetSleepNeedBoost(__instance.entity);
            if (boost <= 0) return true;

            float baseMax = MaxOxygenField(__instance);

            // AD Check: Is something trying to set oxygen exactly to the Base Max?
            bool isSettingToBase = Math.Abs(value - baseMax) < 1f;

            // Are we currently boosted well above that base?
            ITreeAttribute oxygenTree = __instance.entity.WatchedAttributes.GetTreeAttribute("oxygen");
            if (oxygenTree == null) return true;

            float currentOxygen = oxygenTree.GetFloat("currentoxygen", 0f);
            bool isCurrentlyBoosted = currentOxygen > (baseMax + 10f);

            // If we are boosted (46k), and AD says "Set Oxygen to 40000", it blocks it.
            if (isSettingToBase && isCurrentlyBoosted)
            {
                return false;
            }

            return true;
        }

        // SETTER POSTFIX: Visuals (HUD Bar Size)
        [HarmonyPatch("MaxOxygen", MethodType.Setter)]
        [HarmonyPostfix]
        public static void MaxOxygenSetter_Postfix(EntityBehaviorBreathe __instance, float value)
        {
            ITreeAttribute oxygenTree = __instance.entity.WatchedAttributes.GetTreeAttribute("oxygen");
            if (oxygenTree != null)
            {
                // Safely store the clean, unboosted base value
                oxygenTree.SetFloat("sleepneed_basemax", value);

                float boost = GetSleepNeedBoost(__instance.entity);
                if (boost > 0)
                {
                    // Update the client tree so the bar looks correct
                    oxygenTree.SetFloat("maxoxygen", value + boost);
                    __instance.entity.WatchedAttributes.MarkPathDirty("oxygen");
                }
            }
        }

        // TICK PREFIX: Logic Injection (Using Safe Base)
        [HarmonyPatch("OnGameTick")]
        [HarmonyPrefix]
        public static void OnGameTick_Prefix(EntityBehaviorBreathe __instance, ref float __state)
        {
            float currentField = MaxOxygenField(__instance);
            float boost = GetSleepNeedBoost(__instance.entity);

            ITreeAttribute oxygenTree = __instance.entity.WatchedAttributes.GetTreeAttribute("oxygen");

            // Retrieve our clean base, or default to the current field if it's not initialized yet
            float cleanBase = currentField;
            if (oxygenTree != null && oxygenTree.HasAttribute("sleepneed_basemax"))
            {
                cleanBase = oxygenTree.GetFloat("sleepneed_basemax");
            }
            else if (oxygenTree != null)
            {
                oxygenTree.SetFloat("sleepneed_basemax", cleanBase);
            }

            // Pass the clean base to the Postfix
            __state = cleanBase;

            if (boost > 0)
            {
                // Apply boost safely for this tick only
                MaxOxygenField(__instance) = cleanBase + boost;
            }
            else
            {
                MaxOxygenField(__instance) = cleanBase;
            }
        }

        // TICK POSTFIX: Cleanup & Downward Clamping
        [HarmonyPatch("OnGameTick")]
        [HarmonyPostfix]
        public static void OnGameTick_Postfix(EntityBehaviorBreathe __instance, float __state)
        {
            // Always restore the field to the Clean Base (Keeps AD happy and prevents data leaks)
            MaxOxygenField(__instance) = __state;

            // Handle Boost Decrease (Clamping)
            float boost = GetSleepNeedBoost(__instance.entity);
            if (boost > 0)
            {
                float effectiveMax = __state + boost;

                // If current oxygen is higher than our new effective max, clamp it down.
                if (__instance.Oxygen > effectiveMax)
                {
                    __instance.Oxygen = effectiveMax;
                }
            }
        }

        // Note: Check_Postfix has been intentionally removed. If a "Check" method requires 
        // patching for AD compatibility, it MUST use a Prefix/Postfix pair exactly like 
        // OnGameTick to prevent permanent field deduction.
    }
}