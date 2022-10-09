using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse.AI;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(MentalStateWorker_BingingDrug), nameof(MentalStateWorker_BingingDrug.StateCanOccur), new Type[] { typeof(Pawn) })]
public static class MentalStateWorker_BingingDrugPatch
{
    [HarmonyPostfix]
    public static void DrugFreeDisable(ref bool __result, Pawn pawn)
    {
        if (pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.DrugFree) == true)
        {
            __result = false;
        }
    }
}
