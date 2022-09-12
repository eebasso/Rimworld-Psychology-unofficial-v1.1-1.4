using System;using Verse;using RimWorld;using HarmonyLib;using System.Collections.Generic;using System.Reflection;using System.Reflection.Emit;using System.Linq;

namespace Psychology.Harmony;
[HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]public static class SkillRecord_Learn_Patch{    [HarmonyTranspiler]    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)    {        List<CodeInstruction> codeList = codes.ToList();        FieldInfo levelInt = AccessTools.Field(typeof(SkillRecord), "levelInt");        for (int i = 0; i < codeList.Count(); i++)
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
                yield return CodeInstruction.LoadField(typeof(SkillRecord), "pawn");
                yield return CodeInstruction.Call(typeof(SkillRecord_Learn_Patch), nameof(SkillRecord_Learn_Patch.AdjustRatings), new Type[] { typeof(Pawn) });
            }
        }
    }

    public static void AdjustRatings(Pawn pawn)
    {
        if (!PsychologySettings.speciesDict.TryGetValue(pawn.def.defName, out SpeciesSettings settings))
        {
            return;
        }
        if (settings?.enablePsyche == false)
        {
            return;
        }
        if (PsycheHelper.Comp(pawn)?.Psyche == null)
        {
            return;
        }
        PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
    }
}
