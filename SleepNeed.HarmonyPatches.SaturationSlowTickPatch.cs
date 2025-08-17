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
    public static class SaturationSlowTickPatch
    {
        public static IEnumerable<CodeInstruction> HungerDamageTranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            // We will search for the starvation check pattern as seen in your IL and C# code:
            // 1. call to 'get_Saturation'
            // 2. ldc.r4 0f
            // 3. bgt.un.s (a branch instruction)

            for (int i = 0; i < codes.Count; i++)
            {
                // Look for the instruction that loads the constant 0f.
                if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0f)
                {
                    // Now, check the surrounding instructions to confirm this is the starvation check.
                    // The previous instruction should be the call to get_Saturation().
                    if (i > 0 && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo prevMethod && prevMethod.Name == "get_Saturation")
                    {
                        // Found the correct instruction. We'll change the operand to -1f.
                        codes[i].operand = -1f;
                        Console.WriteLine("Successfully patched SlowTick to change starvation damage threshold to -1f!");
                        break; // We assume only one such constant to patch.
                    }
                }
            }

            return codes;
        }
    }
}
