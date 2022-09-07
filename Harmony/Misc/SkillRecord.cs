﻿using System;

namespace Psychology.Harmony;
[HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
        {
            yield return codeList[i];
            if (i < 3)
            {
                continue;
            }
            bool bool0 = codeList[i - 3].LoadsField(levelInt);
            bool bool1 = codeList[i - 2].opcode == OpCodes.Ldc_I4;
            bool bool2 = codeList[i - 1].opcode == OpCodes.Add || codeList[i - 1].opcode == OpCodes.Sub;
            bool bool3 = codeList[i - 0].StoresField(levelInt);
            if (bool0 && bool1 && bool2 && bool3)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                CodeInstruction.LoadField(typeof(SkillRecord), "pawn");
                CodeInstruction.Call(typeof(SkillRecord_Learn_Patch), nameof(SkillRecord_Learn_Patch.AdjustRatings), new Type[] { typeof(Pawn) });
            }
        }
    }

    public static void AdjustRatings(Pawn pawn)
    {
        if (!PsychologySettings.speciesDict.ContainsKey(pawn.def.defName) || !PsychologySettings.speciesDict[pawn.def.defName].enablePsyche)
        {
            return;
        }
        PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
    }
}