using RimWorld;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget))]
    public static class CompAbilityEffect_WordOfLovePatch
    {

        [HarmonyPostfix]
        public static void ValidateTarget(CompAbilityEffect_WordOfLove __instance, ref bool __result, LocalTargetInfo target)
        {

            if (PsychologyBase.ActivateKinsey())
            {
                /* Until the problem with protected selectedTarget is resolved */
                __result = true;
            }

        }

    }

}

/*

 [Psychology][ERR] Failed to apply Harmony patches for Community.Psychology.UnofficialUpdate. Exception was: HarmonyLib.HarmonyException: Patching exception in method virtual System.Boolean RimWorld.CompAbilityEffect_WordOfLove::ValidateTarget(Verse.LocalTargetInfo target) ---> System.Exception: Return type of pass through postfix Boolean ValidateTarget(RimWorld.CompAbilityEffect_WordOfLove, Boolean ByRef, Verse.LocalTargetInfo) does not match type of its first parameter

*/


/*
 
        {
            if (PsychologyBase.ActivateKinsey())
            {
                Pawn pawn = __instance.selectedTarget.Pawn;
                Pawn pawn2 = target.Pawn;
                if (pawn == pawn2)
                {
                    return false;
                }
                if (pawn != null && pawn2 != null && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                {
                    Gender gender = pawn.gender;
                    Gender gender2 = pawn.story.traits.HasTrait(TraitDefOf.Gay) ? gender : gender.Opposite();
                    if (pawn2.gender != gender2)
                    {
                        Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(pawn, pawn2), pawn, MessageTypeDefOf.RejectInput, false);
                        return false;
                    }
                }
                return __instance.parent.ValidateTarget(target);
            }

            return __result;
 
     
 */
