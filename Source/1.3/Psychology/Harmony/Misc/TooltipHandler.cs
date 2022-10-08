using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;


namespace Psychology.Harmony;

[HarmonyPatch(typeof(TooltipHandler), nameof(TooltipHandler.DoTooltipGUI))]
public class TooltipHandler_DoTooltipGUI_PsychologyPatch
{
    
    [HarmonyPostfix]
    public static void DoTooltipGUI()
    {
        PolygonTooltipHandler.DoTooltipGUI();
    }
}

