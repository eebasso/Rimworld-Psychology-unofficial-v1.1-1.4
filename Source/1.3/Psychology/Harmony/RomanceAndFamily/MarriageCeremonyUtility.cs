using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace Psychology.Harmony;

public static class MarriageCeremonyUtility_MarriedPatch
{

    public static void Marriage(Pawn firstPawn, Pawn secondPawn)
    {
        if (firstPawn.needs.mood != null)
        {
            firstPawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.DivorcedMeCodependent, secondPawn);
        }
        if (secondPawn.needs.mood != null)
        {
            secondPawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.DivorcedMeCodependent, firstPawn);
        }
    }
}

