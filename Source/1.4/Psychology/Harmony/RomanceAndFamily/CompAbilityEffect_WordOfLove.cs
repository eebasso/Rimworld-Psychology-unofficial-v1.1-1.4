using RimWorld;
using Verse;
using System;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Collections.Generic;using System.Reflection;using System.Reflection.Emit;using System.Linq;

namespace Psychology.Harmony;

public static class CompAbilityEffect_WordOfLove_KinseyEnabledPatches
{
  public static IEnumerable<CodeInstruction> ValidateTarget_Transpiler(IEnumerable<CodeInstruction> codes)
  {
    return RomancePatchUtility.ChangeLogicForSexualityTraits(codes, "ValidateTarget_Transpiler patch unsuccessful.");
  }

  public static void ValidateTarget_Postfix(CompAbilityEffect_WordOfLove __instance, LocalTargetInfo target, LocalTargetInfo ___selectedTarget, ref bool __result)
  {
    if (__result != true)
    {
      return;
    }
    Pawn pawn = ___selectedTarget.Pawn;
    Pawn pawn2 = target.Pawn;
    if (!PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(pawn2))
    {
      return;
    }
    if (!SpeciesHelper.RomanceEnabled(pawn, true) || !SpeciesHelper.RomanceEnabled(pawn2, true))
    {
      // No underage dating
      __result = false;
      return;
    }

    Gender gender = pawn.gender;
    Gender gender2 = pawn2.gender;
    int kinsey2 = PsycheHelper.Comp(pawn2).Sexuality.kinseyRating;
    bool compat2 = gender == gender2 ? kinsey2 > 0 : kinsey2 < 6;
    if (!compat2)
    {
      // ToDo: maybe change this to a different message that says that the ability can be cast, but the loved pawn cannot recipropate feelings of the pawn in love
      Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(pawn2, pawn), pawn2, MessageTypeDefOf.CautionInput, historical: false);
    }
  }

  public static IEnumerable<CodeInstruction> Valid_Transpiler(IEnumerable<CodeInstruction> codes)
  {
    return RomancePatchUtility.ChangeLogicForSexualityTraits(codes, "Valid_Transpiler patch unsuccessful.");
  }

}

//[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget))]
//public static class CompAbilityEffect_WordOfLove_ValidateTarget_Patch
//{
//    // Used a prefix to prevent incorrect messages being sent to the player
//    [HarmonyPrefix]
//    public static bool ValidateTarget(ref bool __result, LocalTargetInfo target, CompAbilityEffect_WordOfLove __instance, LocalTargetInfo ___selectedTarget)
//    {
//        if (!PsychologySettings.enableKinsey)
//        {
//            return true;
//        }
//        //Traverse.Create(__instance).Field("selectedTarget").GetValue<Pawn>()
//        //Pawn inLovePawn = __instance.selectedTarget.Pawn;
//        Pawn inLovePawn = ___selectedTarget.Pawn;
//        Pawn lovedPawn = target.Pawn;
//        if (inLovePawn == lovedPawn)
//        {
//            __result = false;
//            return false;
//        }
//        if (PsycheHelper.PsychologyEnabled(inLovePawn) && PsycheHelper.PsychologyEnabled(lovedPawn))
//        {
//            Gender inLoveGender = inLovePawn.gender;
//            Gender lovedGender = lovedPawn.gender;
//            int pawnInLoveKinsey = PsycheHelper.Comp(inLovePawn).Sexuality.kinseyRating;
//            bool pawnInLoveCompat = inLoveGender == lovedGender ? pawnInLoveKinsey > 0 : pawnInLoveKinsey < 6;
//            if (!pawnInLoveCompat)
//            {
//                Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(inLovePawn, lovedPawn), inLovePawn, MessageTypeDefOf.RejectInput, historical: false);
//                __result = false;
//                return false;
//            }

//            int lovedKinsey = PsycheHelper.Comp(lovedPawn).Sexuality.kinseyRating;
//            bool pawnLovedCompat = inLoveGender == lovedGender ? lovedKinsey > 0 : lovedKinsey < 6;
//            if (!pawnLovedCompat)
//            {
//                Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(lovedPawn, inLovePawn), lovedPawn, MessageTypeDefOf.CautionInput, historical: false);
//            }
//        }
//        // NOTE: check that CompAbilityEffect_WithDest.ValidateTarget hasn't been changed from simply calling CompAbilityEffect_WithDest.CanHitTarget
//        __result = __instance.CanHitTarget(target);
//        return false;
//    }
//}

//[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.Valid))]
//public static class CompAbilityEffect_WordOfLove_Valid_Patch
//{
//    [HarmonyPostfix]
//    public static void Valid(ref bool __result, LocalTargetInfo target, bool throwMessages)
//    {
//        if (!PsychologySettings.enableKinsey)
//        {
//            return;
//        }
//        Pawn pawn = target.Pawn;
//        if (!PsycheHelper.PsychologyEnabled(pawn))
//        {
//            __result = false;
//            return;
//        }
//        if (PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive < 0.1f)
//        {
//            if (throwMessages)
//            {
//                Messages.Message("AbilityCantApplyOnAsexual".Translate(pawn.def.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
//            }
//            __result = false;
//        }
//    }
//}











