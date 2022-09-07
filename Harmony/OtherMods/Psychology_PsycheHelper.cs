using UnityEngine;using RimWorld;using Verse;using Verse.Sound;using System;using System.Runtime.Remoting.Contexts;using HarmonyLib;using UnityEngine.UIElements.Experimental;using System.Reflection;using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;namespace Psychology.Harmony;
public class Psychology_PsycheHelper_ManualPatches{    public static bool PsychologyEnabled_PawnmorpherPrefix(ref bool __result, Pawn pawn)
    {
        if (pawn == null)
        {
            __result = false;
            return false;
        }
        if(!Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn))
        {
            __result = false;
            return false;
        }
        if (PsychologySettings.speciesDict.ContainsKey(pawn.def.defName))
        {
            return true;
        }
        PsychologySettings.speciesDict.Add(pawn.def.defName, new SpeciesSettings());
        SpeciesHelper.AddCompsToPsycheEnabledDef(pawn.def);
        return true;
    }}