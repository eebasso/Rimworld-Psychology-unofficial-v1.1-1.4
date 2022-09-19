using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(HealthCardUtility), nameof(HealthCardUtility.DrawPawnHealthCard))]
    public static class HealthCardUtility_DrawPawnHealthCardPatch
    {
        [HarmonyPrefix]
        public static void AddPsychologyRecipes(ref Pawn pawn)
        {
            List<RecipeDef> recipes = pawn.def.AllRecipes;
            if (!recipes.Contains(RecipeDefOfPsychology.TreatChemicalInterest))
            {
                recipes.Add(RecipeDefOfPsychology.TreatChemicalInterest);
            }
            if (!recipes.Contains(RecipeDefOfPsychology.TreatChemicalFascination))
            {
                recipes.Add(RecipeDefOfPsychology.TreatChemicalFascination);
            }
            if (!recipes.Contains(RecipeDefOfPsychology.TreatDepression))
            {
                recipes.Add(RecipeDefOfPsychology.TreatDepression);
            }
            if (!recipes.Contains(RecipeDefOfPsychology.TreatInsomnia))
            {
                recipes.Add(RecipeDefOfPsychology.TreatInsomnia);
            }
            if (!recipes.Contains(RecipeDefOfPsychology.CureAnxiety))
            {
                recipes.Add(RecipeDefOfPsychology.CureAnxiety);
            }
            if (!recipes.Contains(RecipeDefOfPsychology.TreatPyromania))
            {
                recipes.Add(RecipeDefOfPsychology.TreatPyromania);
            }

            pawn.def.recipes = recipes;

        }

    }

}
