using HarmonyLib;
using SleepNeed.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace SleepNeed.HarmonyPatches.SaturationSlowTickPatch
{
    [HarmonyPatch(typeof(EntityBehaviorHunger), "SlowTick")]
    public static class SaturationSlowTickPatch
    {
        // Prefix: Kører før den originale SlowTick metode
        // __state bruges til at sende data fra Prefix til Postfix
        public static void Prefix(EntityBehaviorHunger __instance, out float __state)
        {
            __state = -1f; // Default "flag" værdi (betyder "vi gjorde intet")

            if (ConfigSystem.SyncedConfig == null)
            {
                return;
            }
            else if (ConfigSystem.SyncedConfig.EnableEnergy && ConfigSystem.SyncedConfig.OnlyDieFromNoEnergy)
            {
                // Tjekker om spilleren faktisk sulter (Saturation <= 0)
                if (__instance.Saturation <= 0f)
                {
                    // Gemmer den rigtige værdi (fx 0, eller -5 hvis en anden mod tillader negativ mæthed)
                    __state = __instance.Saturation;

                    // Sætter mætheden til en mikroskopisk værdi over 0.
                    // Dette snyder den originale metode til at tro, at vi ikke sulter,
                    // så den hopper over "if (Saturation <= 0)" blokken og dermed skades-kaldet.
                    __instance.Saturation = 0.0001f;
                }
            }
        }

        // Postfix: Kører efter den originale metode er færdig
        public static void Postfix(EntityBehaviorHunger __instance, float __state)
        {
            // Hvis vi ændrede værdien i Prefix (dvs. state er forskellig fra -1)
            if (__state != -1f)
            {
                // Gendanner den rigtige værdi med det samme.
                // Da dette sker i samme tick, og før data sendes til klienten, vil spilleren aldrig se "0.0001" på sin bar, og logikken forbliver intakt.
                __instance.Saturation = __state;
            }
        }
    }
}
