﻿using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;


namespace Psychology.Harm
{
    [HarmonyPatch(typeof(TooltipHandler), nameof(TooltipHandler.DoTooltipGUI))]
    public class TooltipHandler_DoTooltipGUI_PsychologyPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void DoTooltipGUI()
        {
            PolygonTooltipHandler.DoTooltipGUI();
        }
    }
}
