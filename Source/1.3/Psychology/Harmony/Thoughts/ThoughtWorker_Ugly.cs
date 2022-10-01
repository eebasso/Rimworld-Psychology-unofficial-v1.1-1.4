using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(ThoughtWorker_Ugly), "CurrentSocialStateInternal")]
public static class ThoughtWorker_UglyPatch
{
    
    [HarmonyPostfix]
    public static void CurrentSocialStateInternal(ref ThoughtState __result, Pawn pawn, Pawn other)
    {
        if (pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
        {
            __result = false;
        }
    }
}
