using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.CertaintyChangePerDay), MethodType.Getter)]
public static class Pawn_IdeoTracker_CertaintyChangePerDay_Patch
{
    [HarmonyPostfix]
    public static void CertaintyChangePerDay(ref float __result, Pawn ___pawn)
    {
        if (!PsycheHelper.PsychologyEnabled(___pawn))
        {
            return;
        }
        __result += Current.Game.GetComponent<PsychologyGameComponent>().CertaintyChange(___pawn, false);
    }
}

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
public static class Pawn_IdeoTracker_IdeoTrackerTick_Patches
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return GenTicks.TicksGame % 250 == 137;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        foreach (CodeInstruction c in codes)
        {
            if (c.operand is float floatValue && floatValue == 60000f)
            {
                c.operand = 240f;
            }
            yield return c;
        }
    }
}

//[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
//public class Pawn_IdeoTracker_IdeoTrackerTick_Patch
//{
//    [HarmonyPrefix]
//    public void IdeoTrackerTick(ref float ___certainty, Pawn ___pawn)
//    {
//        if (!___pawn.Destroyed && !___pawn.InMentalState && !Find.IdeoManager.classicMode && PsycheHelper.PsychologyEnabled(___pawn))
//        {
//            ___certainty += PsycheHelper.Comp(___pawn).Psyche.CertaintyChangePerTick();
//        }
//    }
//}

//[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
//public static class Pawn_IdeoTracker_IdeoTrackerTick_Patches
//{
//    [HarmonyPrefix]
//    public static bool Prefix()
//    {
//        return GenTicks.TicksGame % 250 == 0;
//    }

//    [HarmonyTranspiler]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
//    {
//        foreach (CodeInstruction c in codes)
//        {
//            yield return c;
//            float floatOperand;
//            if (float.TryParse("" + c.operand, out floatOperand) && floatOperand == 60000f)
//            {
//                c.operand = 240f;
//                yield return new CodeInstruction(OpCodes.Ldc_R4, 240f);
//            }
//        }
//    }
//}

//if (PsycheHelper.PsychologyEnabled(___pawn))
//{
//    return;
//}
//Pawn_PsycheTracker psyche = PsycheHelper.Comp(___pawn).Psyche;
//psyche.certaintyChangeTick -= 1;
//if (psyche.certaintyChangeTick < 0)
//{
//    psyche.CalculateAdjustedRatings();
//    psyche = 
//    __result +=
//    PsycheHelper.Comp(___pawn).Psyche.certaintyChangeTick = GenDate.TicksPerDay;
//}

//if (___pawn.IsHashIntervalTick(GenDate.TicksPerHour))
//{
//}