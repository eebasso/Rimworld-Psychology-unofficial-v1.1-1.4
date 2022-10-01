using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
public static class MemoryThoughtHandler_TryGainMemoryPatch
{
    [HarmonyPrefix]
    public static void TryGainMemoryPrefix(Pawn ___pawn, ref Thought_Memory newThought, Pawn otherPawn)
    {
        ThoughtDef def = newThought.def;
        if (def == ThoughtDefOf.BrokeUpWithMe)
        {
            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, newThought.sourcePrecept);
                return;
            }
            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
            return;
        }
        if (def == ThoughtDefOf.CheatedOnMe)
        {
            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.CheatedOnMeCodependent, newThought.sourcePrecept);
                return;
            }
            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
            return;
        }
        if (def == ThoughtDefOf.DivorcedMe)
        {
            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.DivorcedMeCodependent, newThought.sourcePrecept);
                return;
            }
            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
            return;
        }
        if (def == ThoughtDefOf.RejectedMyProposal)
        {
            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                //Thought_MemorySocial newThoughtSocial = newThought as Thought_MemorySocial;
                //if (newThoughtSocial != null)
                //{
                //    (newThought as Thought_MemorySocial).opinionOffset = 15f;
                //}
                //else
                //{
                //    Log.Warning("TryGainMemoryPrefix, could not cast newThought as Thought_MemorySocial for def = " + TraitDefOfPsychology.Codependent);
                //}
                //newThought.durationTicksOverride = 2 * newThought.def.DurationTicks;
                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.RejectedMyProposalCodependent, newThought.sourcePrecept);
                return;
            }
            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
            return;
        }
    }

    public static int CalcMoodOffset(Pawn pawn, Pawn otherPawn, Thought_Memory mem)
    {
        float x = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) - 0.5f;
        x += GenMath.LerpDoubleClamped(-5f, 100f, -0.5f, 0.5f, pawn.relations.OpinionOf(otherPawn));
        return Mathf.RoundToInt(mem.CurStage.baseMoodEffect * x);
    }

}

