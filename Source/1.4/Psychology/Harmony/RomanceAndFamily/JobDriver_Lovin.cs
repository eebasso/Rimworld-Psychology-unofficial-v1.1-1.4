using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(JobDriver_Lovin), "GenerateRandomMinTicksToNextLovin")]
    public static class JobDriver_Lovin_GenerateRandomMinTicksToNextLovin_Patch
    {
        [HarmonyPostfix]
        public static void GenerateRandomMinTicksToNextLovinPostfix(ref int __result, Pawn pawn)
        {
            if (DebugSettings.alwaysDoLovin)
            {
                __result = 100;
                return;
            }
            RomanceUtility.LovinMtbSinglePawnFactorPsychology(pawn, out float yMean);
            float num = 0.5f * yMean;

            num = Rand.Gaussian(num, 0.3f);
            if (num < 0.5f)
            {
                num = 0.5f;
            }

            __result = (int)(num * GenDate.TicksPerHour);
        }
    }
}

