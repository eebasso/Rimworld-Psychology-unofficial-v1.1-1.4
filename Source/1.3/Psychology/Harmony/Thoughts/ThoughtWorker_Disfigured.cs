using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(ThoughtWorker_Disfigured), "CurrentSocialStateInternal")]
public static class ThoughtWorker_DisfiguredPatch
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
