using RimWorld;
using Verse;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget))]
public static class CompAbilityEffect_WordOfLove_ValidateTarget_Patch
{
    // Used a prefix to prevent incorrect messages being sent to the player
    [HarmonyPrefix]
    public static bool ValidateTarget(ref bool __result, LocalTargetInfo target, CompAbilityEffect_WordOfLove __instance, LocalTargetInfo ___selectedTarget)
    {
        if (!PsychologySettings.enableKinsey)
        {
            return true;
        }
        //Traverse.Create(__instance).Field("selectedTarget").GetValue<Pawn>()
        //Pawn inLovePawn = __instance.selectedTarget.Pawn;
        Pawn inLovePawn = ___selectedTarget.Pawn;
        Pawn lovedPawn = target.Pawn;
        if (inLovePawn == lovedPawn)
        {
            __result = false;
            return false;
        }
        if (PsycheHelper.PsychologyEnabled(inLovePawn) && PsycheHelper.PsychologyEnabled(lovedPawn))
        {
            Gender inLoveGender = inLovePawn.gender;
            Gender lovedGender = lovedPawn.gender;
            int pawnInLoveKinsey = PsycheHelper.Comp(inLovePawn).Sexuality.kinseyRating;
            bool pawnInLoveCompat = inLoveGender == lovedGender ? pawnInLoveKinsey > 0 : pawnInLoveKinsey < 6;
            if (!pawnInLoveCompat)
            {
                Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(inLovePawn, lovedPawn), inLovePawn, MessageTypeDefOf.RejectInput, historical: false);
                __result = false;
                return false;
            }

            int lovedKinsey = PsycheHelper.Comp(lovedPawn).Sexuality.kinseyRating;
            bool pawnLovedCompat = inLoveGender == lovedGender ? lovedKinsey > 0 : lovedKinsey < 6;
            if (!pawnLovedCompat)
            {
                Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(lovedPawn, inLovePawn), lovedPawn, MessageTypeDefOf.CautionInput, historical: false);
            }
        }
        // NOTE: check that CompAbilityEffect_WithDest.ValidateTarget hasn't been changed from simply calling CompAbilityEffect_WithDest.CanHitTarget
        __result = __instance.CanHitTarget(target);
        return false;
    }
}

[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.Valid))]
public static class CompAbilityEffect_WordOfLove_Valid_Patch
{
    [HarmonyPostfix]
    public static void Valid(ref bool __result, LocalTargetInfo target, bool throwMessages)
    {
        if (!PsychologySettings.enableKinsey)
        {
            return;
        }
        Pawn pawn = target.Pawn;
        if (!PsycheHelper.PsychologyEnabled(pawn))
        {
            __result = false;
            return;
        }
        if (PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive < 0.1f)
        {
            if (throwMessages)
            {
                Messages.Message("AbilityCantApplyOnAsexual".Translate(pawn.def.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
            }
            __result = false;
        }
    }
}


/*

[Psychology][ERR] Failed to apply Harmony patches for Community.Psychology.UnofficialUpdate. Exception was: HarmonyLib.HarmonyException: Patching exception in method virtual System.Boolean RimWorld.CompAbilityEffect_WordOfLove::ValidateTarget(Verse.LocalTargetInfo target) ---> System.Exception: Return type of pass through postfix Boolean ValidateTarget(RimWorld.CompAbilityEffect_WordOfLove, Boolean ByRef, Verse.LocalTargetInfo) does not match type of its first parameter

*/



    

 

