using UnityEngine;
using RimWorld;
using Verse;
using System;
using Verse.Sound;
using System.Runtime.Remoting.Contexts;
using HarmonyLib;
using UnityEngine.UIElements.Experimental;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace Psychology;

[StaticConstructorOnStartup]
public class AlienRace_Patches
{
    public AlienRace_Patches()
    {
        MethodInfo originalInfo;
        HarmonyMethod harmonyMethod;
        HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate.AlienRace");

        originalInfo = AccessTools.Method(typeof(SpeciesHelper), nameof(SpeciesHelper.AlienRaceHeuristicSettingsHook));
        harmonyMethod = new HarmonyMethod(typeof(AlienRace_Patches), nameof(AlienRace_Patches.AlienRaceHeuristicSettingsPostfix));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        originalInfo = AccessTools.Method(typeof(RomanceUtility), nameof(RomanceUtility.AlienRaceAgeCurveHook));
        harmonyMethod = new HarmonyMethod(typeof(AlienRace_Patches), nameof(AlienRace_Patches.AlienRaceAgeCurvePostfix));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
    }

    public static void AlienRaceHeuristicSettingsPostfix(SpeciesSettings settings, ThingDef def)
    {
        if (def is AlienRace.ThingDef_AlienRace alienDef)
        {
            if (alienDef?.alienRace?.compatibility?.IsSentient is bool isSentient)
            {
                settings.enablePsyche = isSentient;
            }

            bool? immuneToAge = alienDef?.alienRace?.generalSettings?.immuneToAge;
            bool? notFlesh = !alienDef?.alienRace?.compatibility?.IsFlesh;
            if (immuneToAge == true || notFlesh == true)
            {
                settings.enableAgeGap = false;
            }
        }
    }

    public static void AlienRaceAgeCurvePostfix(SimpleCurve __result, Pawn pawn)
    {
        if (pawn?.def is AlienRace.ThingDef_AlienRace alienDef && alienDef?.alienRace?.generalSettings?.lovinIntervalHoursFromAge is SimpleCurve curve)
        {
            __result = curve;
        }
        else
        {
            __result = null;
        }
    }

}

