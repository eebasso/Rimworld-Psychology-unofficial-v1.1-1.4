//using System;
//using System.Text;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;
//using RimWorld;
//using HarmonyLib;
//using System.Reflection;

//namespace Psychology.Harmony;

//[HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
//public static class MemoryThoughtHandler_TryGainMemoryPatch
//{
//    [HarmonyPrefix]
//    public static void TryGainMemoryPrefix(ref Thought_Memory newThought, Pawn otherPawn, Pawn ___pawn)
//    {
//        ThoughtDef def = newThought.def;
//        if (def == ThoughtDefOf.BrokeUpWithMe)
//        {
//            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
//            {
//                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, newThought.sourcePrecept);
//                return;
//            }
//        }
//        if (def == ThoughtDefOf.BrokeUpWithMe)
//        {

//        }

//        newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
//        return;


//        if (def == ThoughtDefOf.CheatedOnMe)
//        {
//            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
//            {
//                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.CheatedOnMeCodependent, newThought.sourcePrecept);
//                return;
//            }
//            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
//            return;
//        }
//        if (def == ThoughtDefOf.DivorcedMe)
//        {
//            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
//            {
//                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.DivorcedMeCodependent, newThought.sourcePrecept);
//                return;
//            }
//            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
//            return;
//        }
//        if (def == ThoughtDefOf.RejectedMyProposal)
//        {
//            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
//            {
//                //Thought_MemorySocial newThoughtSocial = newThought as Thought_MemorySocial;
//                //if (newThoughtSocial != null)
//                //{
//                //    (newThought as Thought_MemorySocial).opinionOffset = 15f;
//                //}
//                //else
//                //{
//                //    Log.Warning("TryGainMemoryPrefix, could not cast newThought as Thought_MemorySocial for def = " + TraitDefOfPsychology.Codependent);
//                //}
//                //newThought.durationTicksOverride = 2 * newThought.def.DurationTicks;
//                newThought = ThoughtMaker.MakeThought(ThoughtDefOfPsychology.RejectedMyProposalCodependent, newThought.sourcePrecept);
//                return;
//            }
//            newThought.moodOffset = CalcMoodOffset(___pawn, otherPawn, newThought);
//            return;
//        }
//    }

//}

