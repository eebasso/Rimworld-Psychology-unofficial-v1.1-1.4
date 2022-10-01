//using System;//using Verse;//using RimWorld;//using HarmonyLib;//using System.Collections.Generic;//using System.Reflection;//using System.Reflection.Emit;//using System.Linq;

//namespace Psychology.Harmony;

//[HarmonyPatch(typeof(CompAbilityEffect_Reassure), nameof(CompAbilityEffect_Reassure.Apply))]
//public static class CombAbilityEffect_Reassure_ApplyPatch
//{
//    [HarmonyTranspiler]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
//    {
//        bool hasFired = false;
//        bool bool0Found = false;
//        bool bool1Found = false;
//        bool bool0;
//        bool bool1;

//        List<CodeInstruction> clist = codes.ToList();
//        List<OpCode> opcodeList = new List<OpCode> { OpCodes.Brfalse, OpCodes.Brfalse_S, OpCodes.Brtrue, OpCodes.Brtrue_S };

//        for (int i = 0; i < clist.Count(); i++)
//        {
//            yield return clist[i];
//            if (i == clist.Count() - 2)
//            {
//                yield return new CodeInstruction(OpCodes.Ldloc_1);
//            }
            
//        }
//        if (hasFired != true)
//        {
//            Log.Error("Psychology: failed to patch CompAbilityEffect_Counsel.Apply, bool0 found: " + bool0Found + ", bool1 found: " + bool1Found);
//        }

//    }

//    //[HarmonyTranspiler]
//    //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
//    //{
//    //    bool hasFired = false;
//    //    bool bool0Found = false;
//    //    bool bool1Found = false;
//    //    bool bool0;
//    //    bool bool1;

//    //    List<CodeInstruction> clist = codes.ToList();
//    //    List<OpCode> opcodeList = new List<OpCode> { OpCodes.Brfalse, OpCodes.Brfalse_S, OpCodes.Brtrue, OpCodes.Brtrue_S };

//    //    for (int i = 0; i < clist.Count(); i++)
//    //    {
//    //        yield return clist[i];
//    //        if (i < 1 || hasFired)
//    //        {
//    //            continue;
//    //        }
//    //        bool0 = clist[i - 1].Calls(AccessTools.Method(typeof(Rand), nameof(Rand.Chance)));
//    //        bool1 = opcodeList.Contains(clist[i].opcode);
//    //        if (bool0)
//    //        {
//    //            bool0Found = true;
//    //        }
//    //        if (bool1)
//    //        {
//    //            bool1Found = true;
//    //        }
//    //        if (bool0 && bool1 && hasFired != true)
//    //        {
//    //            yield return new CodeInstruction(OpCodes.Ldloc_0);
//    //            yield return CodeInstruction.Call(typeof(CombAbilityEffect_Reassure_ApplyPatch), nameof(BoostRatingsTowardsPawnsIdeo));
//    //            hasFired = true;
//    //        }
//    //    }
//    //    if (hasFired != true)
//    //    {
//    //        Log.Error("Psychology: failed to patch CompAbilityEffect_Counsel.Apply, bool0 found: " + bool0Found + ", bool1 found: " + bool1Found);
//    //    }

//    //}

//    public static void BoostRatingsTowardsPawnsIdeo(Pawn casterPawn, Pawn targetPawn)
//    {
//        PsycheHelper.Comp(targetPawn).Psyche.BoostRatingsTowardsIdeo(targetPawn.Ideo, true);
//    }

//}

//[HarmonyPatch(typeof(CompAbilityEffect_Reassure), nameof(CompAbilityEffect_Reassure.ExtraLabelMouseAttachment))]
//public static class CompAbilityEffect_Reassure_ExtraLabelMouseAttachmentPatch
//{
//    public static void Postfix(LocalTargetInfo target, string __result)
//    {
//        if (__result == null)
//        {
//            return;
//        }
//        Pawn pawn = target.Pawn;
//        __result += "\n\n" + PsycheHelper.Comp(pawn).Psyche.IdeoAbilityEffectOnPsycheTooltip();

//    }
//}

