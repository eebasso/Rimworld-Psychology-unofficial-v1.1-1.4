﻿using UnityEngine;
public class Psychology_PsycheHelper_ManualPatches
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
    }