using RimWorld;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony
{
    //[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget))]
    //public static class CompAbilityEffect_WordOfLovePatch
    //{
    //    [HarmonyPostfix]
    //    public static void ValidateTarget(CompAbilityEffect_WordOfLove __instance, ref bool __result, LocalTargetInfo target)
    //    {
    //        if (PsychologyBase.ActivateKinsey())
    //        {
    //            /* Until the problem with protected selectedTarget is resolved */
    //            __result = true;
    //        }
    //    }
    //}
    [HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget))]
    public static class CompAbilityEffect_WordOfLovePatch
    {
        [HarmonyPostfix]
        public static void ValidateTarget(CompAbilityEffect_WordOfLove __instance, ref bool __result, LocalTargetInfo target)
        {
            if (!PsychologyBase.ActivateKinsey())
            {
                return;
            }
            Pawn pawnLoved = __instance.selectedTarget.Pawn;
            Pawn pawnInLove = target.Pawn;
            if (pawnLoved == pawnInLove)
            {
                __result = false;
                return;
            }
            if (pawnLoved != null && pawnInLove != null)
            {
                Gender genderLoved = pawnLoved.gender;
                Gender genderInLove = pawnInLove.gender;
                int kinseyLoved = PsycheHelper.Comp(pawnLoved).Sexuality.kinseyRating;
                int kinseyInLove = PsycheHelper.Comp(pawnInLove).Sexuality.kinseyRating;
                if (genderLoved == genderInLove)
                {
                    // A potential homosexual relationship needs both parties to be at least a little gay
                    __result = kinseyLoved > 0 && kinseyInLove > 0;
                    return;
                }
                else
                {
                    // A potential heurtosexual relationship needs both parties to be at least a little straight
                    __result = kinseyLoved < 6 && kinseyInLove < 6;
                }
            }
        }
    }
}

/*

 [Psychology][ERR] Failed to apply Harmony patches for Community.Psychology.UnofficialUpdate. Exception was: HarmonyLib.HarmonyException: Patching exception in method virtual System.Boolean RimWorld.CompAbilityEffect_WordOfLove::ValidateTarget(Verse.LocalTargetInfo target) ---> System.Exception: Return type of pass through postfix Boolean ValidateTarget(RimWorld.CompAbilityEffect_WordOfLove, Boolean ByRef, Verse.LocalTargetInfo) does not match type of its first parameter

*/


 
        
 
     

