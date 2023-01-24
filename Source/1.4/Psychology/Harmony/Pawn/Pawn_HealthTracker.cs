using Verse;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.SetDead))]
public static class Pawn_HealthTracker_SetDead_Patch
{
  [HarmonyPostfix]
  public static void SetDead(Pawn ___pawn)
  {
    PsycheHelper.GameComp.RemoveAllMayorshipsFromPawn(___pawn);
  }
}

