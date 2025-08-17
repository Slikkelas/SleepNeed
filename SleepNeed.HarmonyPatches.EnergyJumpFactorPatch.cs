using HarmonyLib;
using SleepNeed.Hud;
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

namespace SleepNeed.HarmonyPatches.EnergyJumpFactorPatch
{
    public static class EnergyJumpFactorPatch // Make it static
    {
        // --- Helper Method to Calculate energyJumpFactor ---
        // This method will be called by the patched IL.
        // It takes the EntityPlayer instance to calculate the custom jump factor.
        public static float GetEnergyJumpFactor(EntityPlayer player)
        {
            
            if (player == null)
            {
                return 0f; // No energy jump factor if not a player
            }
            ITreeAttribute treeAttribute = player.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute == null)
            {
                return 0f; // No energy data available
            }

            float energyJumpBoostStat = treeAttribute.GetFloat("energyjumpbooststat", 0f); // Provide a default value
            return energyJumpBoostStat;

        }

        public static IEnumerable<CodeInstruction> JumpFactorTranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            // --- Identify Local Variable Index ---
            const int entityPlayerLocalIndex = 8; // Corrected index

            // --- Find the 'ldsfld GlobalConstants.BaseJumpForce' instruction ---
            int targetInstructionIndex = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld &&
                    codes[i].operand is FieldInfo field &&
                    field.Name == "BaseJumpForce" &&
                    field.DeclaringType == typeof(GlobalConstants))
                {
                    targetInstructionIndex = i;
                    break;
                }
            }

            if (targetInstructionIndex != -1)
            {
                // Create the new sequence of instructions to replace the original Ldsfld
                List<CodeInstruction> replacementInstructions = new List<CodeInstruction>();

                // 1. Load the original GlobalConstants.BaseJumpForce value onto the stack.
                replacementInstructions.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(GlobalConstants), nameof(GlobalConstants.BaseJumpForce))));
                // Stack: [BaseJumpForce (float)]

                // 2. Load the entityPlayer instance onto the stack.
                replacementInstructions.Add(new CodeInstruction(OpCodes.Ldloc_S, entityPlayerLocalIndex));
                // Stack: [BaseJumpForce (float)], [entityPlayer (ref)]

                // 3. Call our helper method to get the energyJumpFactor (returns float).
                replacementInstructions.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnergyJumpFactorPatch), nameof(GetEnergyJumpFactor))));
                // Stack: [BaseJumpForce (float)], [energyJumpFactor (float)]

                // 4. Add the energyJumpFactor to BaseJumpForce.
                replacementInstructions.Add(new CodeInstruction(OpCodes.Sub));
                // Stack: [(BaseJumpForce - energyJumpFactor) (float)]

                // Now, replace the single original Ldsfld instruction with our new sequence.
                codes.RemoveAt(targetInstructionIndex);
                codes.InsertRange(targetInstructionIndex, replacementInstructions);
            }
            else
            {
                Console.WriteLine("Harmony Transpiler Error: Could not find 'GlobalConstants.BaseJumpForce' load instruction for replacement. EnergyJumpFactor patch failed!");
            }

            return codes;
        }
    }
}
