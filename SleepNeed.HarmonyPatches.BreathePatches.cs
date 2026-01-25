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
            float boost = GetSleepNeedBoost(__instance.entity);
            if (boost > 0)
            {
                // Update the client tree so the bar looks correct
                ITreeAttribute oxygenTree = __instance.entity.WatchedAttributes.GetTreeAttribute("oxygen");
                if (oxygenTree != null)
                {
                    oxygenTree.SetFloat("maxoxygen", value + boost);
                    __instance.entity.WatchedAttributes.MarkPathDirty("oxygen");
                }
            }
        }

        // TICK PREFIX: Logic Injection (with Recursion Fix)
        [HarmonyPatch("OnGameTick")]
        [HarmonyPrefix]
        public static void OnGameTick_Prefix(EntityBehaviorBreathe __instance, ref float __state)
        {
            float currentField = MaxOxygenField(__instance);
            float boost = GetSleepNeedBoost(__instance.entity);

            if (boost > 0)
            {
                ITreeAttribute oxygenTree = __instance.entity.WatchedAttributes.GetTreeAttribute("oxygen");

                // FIX: Default to currentField if the attribute is missing to prevent 0 val
                float treeMax = currentField;
                if (oxygenTree != null && oxygenTree.HasAttribute("maxoxygen"))
                {
                    treeMax = oxygenTree.GetFloat("maxoxygen");
                }

                // Safety check: If treeMax read as 0 somehow, force it to currentField
                if (treeMax <= 0.1f) treeMax = currentField;

                // If Field == Tree, it's boosted/dirty. Subtract boost to get base.
                if (Math.Abs(currentField - treeMax) < 100f)
                {
                    __state = currentField - boost;
                }
                // If Field is wildly high (recursion bug), reset to Tree - Boost.
                else if (currentField > treeMax + 100f)
                {
                    __state = treeMax - boost;
                }
                // Otherwise, it's clean.
                else
                {
                    __state = currentField;
                }

                // Apply boost
                MaxOxygenField(__instance) = __state + boost;
            }
            else
            {
                __state = currentField;
            }
        }

        // TICK POSTFIX: Cleanup & Downward Clamping
        [HarmonyPatch("OnGameTick")]
        [HarmonyPostfix]
        public static void OnGameTick_Postfix(EntityBehaviorBreathe __instance, float __state)
        {
            // Restore the field to Clean Base (Keeps AD happy)
            MaxOxygenField(__instance) = __state;

            // Handle Boost Decrease (Clamping)
            float boost = GetSleepNeedBoost(__instance.entity);
            if (boost > 0)
            {
                float effectiveMax = __state + boost;

                // If current oxygen is higher than our new effective max (plus tiny tolerance), clamp it down.
                // This handles cases where health/energy drops, lowering the boost.
                if (__instance.Oxygen > effectiveMax) // removed  + 0.1f
                {
                    __instance.Oxygen = effectiveMax;
                }
            }
        }

        // CHECK POSTFIX: Prevent permanent pollution from Tree sync
        [HarmonyPatch("Check")]
        [HarmonyPostfix]
        public static void Check_Postfix(EntityBehaviorBreathe __instance)
        {
            float boost = GetSleepNeedBoost(__instance.entity);
            if (boost > 0)
            {
                float currentField = MaxOxygenField(__instance);
                MaxOxygenField(__instance) = currentField - boost;
            }
        }
    }
}