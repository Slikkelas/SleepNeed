using HarmonyLib;
using SleepNeed.Energy;
using SleepNeed.Sleepiness;
using SleepNeed.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace SleepNeed.HarmonyPatches.MiningWhenSittingPatch
{
    [HarmonyPatch(typeof(Block), "OnGettingBroken")]
    public class StopMiningWhileSittingPatch
    {
        
        // Kører FØR Block.OnGettingBroken.
        // Forhindrer metoden i at køre (og dermed stoppe mining), hvis spilleren sidder.
        // 'false' (spring original metode over) hvis spilleren sidder.
        // 'true' (kør original metode) hvis spilleren står op.
        
        public static bool Prefix(ref float __result, IPlayer player, float remainingResistance)
        {
            if (player?.Entity == null)
            {
                return true;
            }
            bool playerIsBuildingGroundBed = false;
            bool playerIsBuildingStatus = false;
            ITreeAttribute energyTree = player.Entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            ITreeAttribute sleepinessTree = player.Entity.WatchedAttributes.GetTreeAttribute("sleepneed:sleepiness");
            if (energyTree != null)
            {
                playerIsBuildingStatus = energyTree.GetBool("buildinggroundbed");
                if (playerIsBuildingStatus)
                {
                    playerIsBuildingGroundBed = true;
                }
            }
            else if (energyTree == null && sleepinessTree != null)
            {
                playerIsBuildingStatus = sleepinessTree.GetBool("buildinggroundbed");
                if (playerIsBuildingStatus)
                {
                    playerIsBuildingGroundBed = true;
                }
            }
            else
            {
                return true;
            }


            bool disableBlockBreakeWhenSitting = false;
            if (ConfigSystem.SyncedConfig == null)
            {
                return true;
            }
            else if (ConfigSystem.SyncedConfig.DisableBlockBreakeWhenSitting)
            {
                disableBlockBreakeWhenSitting = ConfigSystem.SyncedConfig.DisableBlockBreakeWhenSitting;
            }


            if (disableBlockBreakeWhenSitting || playerIsBuildingGroundBed)
            {
                EntityAgent agent = player.Entity as EntityAgent;
                // Tjekker om spilleren eksisterer og sidder på gulvet
                if (agent != null && agent.Controls != null && agent.Controls.FloorSitting)
                {
                    // Sætter resultatet til den nuværende resistance (så den ikke ændres)
                    // Dette forhindrer, at modstand falder til 0.
                    __result = remainingResistance;

                    // Returnerer 'false' for at springe den originale OnGettingBroken-metode HELT over.
                    // Der sker ingen mining-fremgang.
                    return false;
                }
            }
            // Returner 'true' for at lade den originale metode køre normalt.
            // Spilleren står op, så mining er tilladt.
            return true;
        }
    }
}