using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;
using System.Xml;
using System.Security.Cryptography;

namespace Psychology.Harmony;

[StaticConstructorOnStartup]
public static class RomanceHelperMethods
{
  public static ThoughtDef BrokeUpWithMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("BrokeUpWithMeMood");
  public static ThoughtDef DivorcedMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("DivorcedMeMood");
  public static ThoughtDef CheatedOnMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("CheatedOnMeMood");

  public static Dictionary<ThoughtDef, string[]> PsychologyMethodNameDict = new Dictionary<ThoughtDef, string[]>()
  {
    { ThoughtDefOf.BrokeUpWithMe         , new string[] { nameof(GainBrokeUpWithMe)         , nameof(RemoveBrokeUpWithMe) } },
    { ThoughtDefOf.DivorcedMe            , new string[] { nameof(GainDivorcedMe)            , nameof(RemoveDivorcedMe) } },
    { ThoughtDefOf.RejectedMyProposal    , new string[] { nameof(GainRejectedMyProposal)    , nameof(RemoveRejectedMyProposal) } },
    { ThoughtDefOf.CheatedOnMe           , new string[] { nameof(GainCheatedOnMe)           , nameof(RemoveCheatedOnMe) } },
    { ThoughtDefOf.IRejectedTheirProposal, new string[] { nameof(GainIRejectedTheirProposal), nameof(RemoveIRejectedTheirProposal) } }
  };

  public static List<ThoughtDef> OpinionThoughtDefs;
  public static Dictionary<ThoughtDef, Pair<float, float>> MinMaxOpinionDict = new Dictionary<ThoughtDef, Pair<float, float>>();

  public static List<ThoughtDef> MoodThoughtDefs = new List<ThoughtDef> { BrokeUpWithMeMoodDef, DivorcedMeMoodDef, ThoughtDefOf.RejectedMyProposalMood, CheatedOnMeMoodDef, ThoughtDefOfPsychology.IBrokeUpWithThemMood, ThoughtDefOfPsychology.IDivorcedThemMood, ThoughtDefOfPsychology.IRejectedTheirProposalMood };

  public static Dictionary<ThoughtDef, Pair<float, float>> MinMaxMoodDict = new Dictionary<ThoughtDef, Pair<float, float>>();

  public static Dictionary<ThoughtDef, ThoughtDef> OpinionDefToMoodDefDict = new Dictionary<ThoughtDef, ThoughtDef>() { { ThoughtDefOf.BrokeUpWithMe, BrokeUpWithMeMoodDef }, { ThoughtDefOf.DivorcedMe, DivorcedMeMoodDef }, { ThoughtDefOf.RejectedMyProposal, ThoughtDefOf.RejectedMyProposalMood }, { ThoughtDefOf.CheatedOnMe, CheatedOnMeMoodDef }, { ThoughtDefOf.IRejectedTheirProposal, ThoughtDefOfPsychology.IRejectedTheirProposalMood } };

  static RomanceHelperMethods()
  {
    OpinionThoughtDefs = PsychologyMethodNameDict.Keys.ToList();
    List<float> list;
    foreach (ThoughtDef thoughtDef in OpinionThoughtDefs)
    {
      list = (from ThoughtStage s in thoughtDef.stages
              select s.baseOpinionOffset).ToList();
      MinMaxOpinionDict[thoughtDef] = GetMinMaxPair(list);
    }
    foreach (ThoughtDef thoughtDef in MoodThoughtDefs)
    {
      list = (from ThoughtStage s in thoughtDef.stages
              select s.baseMoodEffect).ToList();
      MinMaxMoodDict[thoughtDef] = GetMinMaxPair(list);
    }
  }

  public static Pair<float, float> GetMinMaxPair(List<float> list)
  {
    list.Add(0f);
    list.Sort();
    float minDiff = Mathf.Abs(list[1] - list[0]);
    for (int i = 2; i < list.Count(); i++)
    {
      float diff = Mathf.Abs(list[i] - list[i - 1]);
      minDiff = diff > 0f ? Mathf.Min(diff, minDiff) : minDiff;
    }
    float min = list.Min();
    float max = list.Max();
    min = min == 0f ? 0f : min - minDiff;
    max = max == 0f ? 0f : max + minDiff;
    return new Pair<float, float>(min, max);
  }

  // ToDo: might need to change lerpOpinionToZeroAfterAgeDuration
  // ToDo: look for TryGainThought of things like BrokeUpWithMe, DivorcedMe, etc
  // TryGainMemory and RemoveMemoriesOfDefWhereOtherPawnIs occur in the following methods:
  // - InteractionWorker_Breakup.Interacted                                     for TryGain of BrokeUpWithMe
  // - InteractionWorker_MarriageProposal.Interacted                            for TryGain and RemoveMemories of RejectedMyProposal
  // - InteractionWorker_RomanceAttempt.RemoveBrokeUpAndFailedRomanceThoughts   for RemoveMemories of BrokeUpWithMe
  // - InteractionWorker_RomanceAttempt.TryAddCheaterThought                    for TryGain of CheatedOnMe
  // - MarriageCeremonyUtility.Married                                          for RemoveMemories of DivorcedMe
  // - SpouseRelationUtility.RemoveGotMarriedThoughts                           for TryGain of DivorcedMe

  public static IEnumerable<CodeInstruction> InterdictTryGainAndRemoveMemories(IEnumerable<CodeInstruction> codes)
  {
    ////Log.Message("InterdictTryGainAndRemoveMemories, start");
    List<CodeInstruction> clist = codes.ToList();
    //bool success = false;


    FieldInfo fieldInfoMemories = AccessTools.Field(typeof(ThoughtHandler), nameof(ThoughtHandler.memories));
    MethodInfo methodInfoTryGainMemory = AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) });
    MethodInfo methodInfoRemoveMemories = AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemoriesOfDefWhereOtherPawnIs));
    ////Log.Message("InterdictTryGainAndRemoveMemories, test");
    //FieldInfo fieldInfo = AccessTools.Field(typeof(ThoughtDefOf), ThoughtDefOf.BrokeUpWithMe.defName);
    ////Log.Message("InterdictTryGainAndRemoveMemories, OpinionThoughtDefs NullOrEmpty: " + OpinionThoughtDefs.NullOrEmpty());
    ////Log.Message("InterdictTryGainAndRemoveMemories, OpinionThoughtDefs count: " + OpinionThoughtDefs.Count());
    List<Pair<int, FieldInfo>> fieldInfos = (from ThoughtDef def in OpinionThoughtDefs
                                             select new Pair<int, FieldInfo>(OpinionThoughtDefs.IndexOf(def), AccessTools.Field(typeof(ThoughtDefOf), def.defName))).ToList();

    //int memoriesIndexInClist = -1;
    //int thoughtDefIndexInClist = -1;
    //int thoughtDefIndexInOpinionThoughtDefs = -1;
    //int tryGainOrRemoveIndexInClist = -1;
    //int gainVsRemoveBit = -1;

    List<int> listTryGainOrRemoveIndexInClist = new List<int>();
    List<int> listGainVsRemoveBit = new List<int>();
    List<int> listOtherPawnIndexInClist = new List<int>();
    List<int> listThoughtDefIndexInClist = new List<int>();
    List<int> listThoughtDefIndexInOpinionThoughtDefs = new List<int>();
    List<int> listMemoriesIndexInClist = new List<int>();
    List<int> listPawnIndexInClist = new List<int>();

    //List<CodeInstruction> listPawnCode = new List<CodeInstruction>();
    //List<CodeInstruction> listOtherPawnCode = new List<CodeInstruction>();
    ////Log.Message("InterdictTryGainAndRemoveMemories, search clist for matches");
    int searchMode = 0;
    for (int i = clist.Count() - 1; i >= 0; i--)
    {
      switch (searchMode)
      {
        case 0:
          int gainVsRemoveBit = clist[i].Calls(methodInfoTryGainMemory) ? 0 : clist[i].Calls(methodInfoRemoveMemories) ? 1 : -1;
          if (gainVsRemoveBit != -1)
          {
            searchMode++;
            listTryGainOrRemoveIndexInClist.Add(i);
            listGainVsRemoveBit.Add(gainVsRemoveBit);
            ////Log.Message("InterdictTryGainAndRemoveMemories, found TryGainOrRemove");
          }
          break;
        case 1:
          if (clist[i].LoadsField(fieldInfoMemories))
          {
            searchMode++;
            listMemoriesIndexInClist.Add(i);
            listPawnIndexInClist.Add(i - 4);
            ////Log.Message("InterdictTryGainAndRemoveMemories, found memories");
          }
          break;
        default:
          bool foundDef = false;
          for (int j = i; j < listTryGainOrRemoveIndexInClist.Last(); j++)
          {
            foreach (Pair<int, FieldInfo> pair in fieldInfos)
            {
              if (clist[j].LoadsField(pair.Second))
              {
                foundDef = true;
                listOtherPawnIndexInClist.Add(j + 1);
                listThoughtDefIndexInClist.Add(j);
                listThoughtDefIndexInOpinionThoughtDefs.Add(pair.First);
                ////Log.Message("InterdictTryGainAndRemoveMemories, found ThoughtDef");
                break;
              }
            }
            if (foundDef)
            {
              break;
            }
          }
          if (!foundDef)
          {
            listTryGainOrRemoveIndexInClist.RemoveLast();
            listGainVsRemoveBit.RemoveLast();
            listMemoriesIndexInClist.RemoveLast();
            listPawnIndexInClist.RemoveLast();
          }
          searchMode = 0;
          break;
      }
    }

    if (searchMode != 0)
    {
      Log.Error("InterdictTryGainThoughtAndRemoveMemories failed, stuck at searchMode = " + searchMode);
      foreach (CodeInstruction c in codes)
      {
        yield return c;
      }
      yield break;
    }
    ////Log.Message("InterdictTryGainThoughtAndRemoveMemories, found this number of matches: " + listPawnIndexInClist.Count());
    ////Log.Message("InterdictTryGainAndRemoveMemories, start yield return");
    for (int i = 0; i < clist.Count(); i++)
    {
      yield return clist[i];
      int matchNum = listPawnIndexInClist.IndexOf(i);
      if (matchNum >= 0)
      {
        yield return clist[listOtherPawnIndexInClist[matchNum]];
        ThoughtDef thoughtDef = OpinionThoughtDefs[listThoughtDefIndexInOpinionThoughtDefs[matchNum]];
        yield return CodeInstruction.Call(typeof(RomanceHelperMethods), PsychologyMethodNameDict[thoughtDef][listGainVsRemoveBit[matchNum]]);
        i = listTryGainOrRemoveIndexInClist[matchNum];
      }
    }

  }




  /* Gain memories */
  public static void GainBrokeUpWithMe(Pawn pawn, Pawn otherPawn)
  {
    TryGainOpinionAndAdjustMood(pawn, otherPawn, ThoughtDefOf.BrokeUpWithMe);
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, otherPawn);
    TryGainMood(otherPawn, pawn, ThoughtDefOfPsychology.IBrokeUpWithThemMood);
  }

  public static void GainDivorcedMe(Pawn pawn, Pawn otherPawn)
  {
    TryGainOpinionAndAdjustMood(pawn, otherPawn, ThoughtDefOf.DivorcedMe);
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DivorcedMeCodependent, otherPawn);
    TryGainMood(otherPawn, pawn, ThoughtDefOfPsychology.IDivorcedThemMood);
  }

  public static void GainCheatedOnMe(Pawn pawn, Pawn otherPawn)
  {
    TryGainOpinion(pawn, otherPawn, ThoughtDefOf.CheatedOnMe);
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CheatedOnMeCodependent, otherPawn);
  }

  public static void GainRejectedMyProposal(Pawn pawn, Pawn otherPawn)
  {
    TryGainOpinionAndAdjustMood(pawn, otherPawn, ThoughtDefOf.RejectedMyProposal);
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RejectedMyProposalCodependent, otherPawn);
  }

  public static void GainIRejectedTheirProposal(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, otherPawn);
    TryGainMood(pawn, otherPawn, ThoughtDefOfPsychology.IRejectedTheirProposalMood);
  }

  public static void TryGainOpinionAndAdjustMood(Pawn pawn, Pawn otherPawn, ThoughtDef opinionDef)
  {
    TryGainOpinion(pawn, otherPawn, opinionDef);
    if (OpinionDefToMoodDefDict.TryGetValue(opinionDef, out ThoughtDef moodDef))
    {
      foreach (Thought_Memory thought_Memory in pawn.needs.mood.thoughts.memories.Memories)
      {
        if (thought_Memory.otherPawn == otherPawn && thought_Memory.def == moodDef)
        {
          AdjustMood(pawn, otherPawn, thought_Memory);
        }
      }
    }
  }

  public static void TryGainOpinion(Pawn pawn, Pawn otherPawn, ThoughtDef opinionDef)
  {
    float opinionOffset = GetOffsetFromMinMax(pawn, otherPawn, MinMaxOpinionDict[opinionDef]);
    List<Pair<int, float>> baseOpinionOffsetList = (from int k in Enumerable.Range(0, opinionDef.stages.Count())
                                                    select new Pair<int, float>(k, opinionDef.stages[k].baseOpinionOffset)).ToList();
    if (GetStageIndex(opinionOffset, baseOpinionOffsetList, out int index))
    {
      if (ThoughtMaker.MakeThought(opinionDef, index) is Thought_MemorySocial thought)
      {
        thought.opinionOffset = opinionOffset;
        pawn.needs.mood.thoughts.memories.TryGainMemory(thought, otherPawn);
      }
      else
      {
        Log.Error("Thought as Thought_MemorySocial was null");
        pawn.needs.mood.thoughts.memories.TryGainMemory(opinionDef, otherPawn);
      }
    }
  }

  public static void TryGainMood(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
  {
    if (AdjustMoodIntro(pawn, otherPawn, thoughtDef, out float MoodOffset, out int index))
    {
      Thought_Memory thought = ThoughtMaker.MakeThought(thoughtDef, index);
      thought.moodOffset = Mathf.RoundToInt(MoodOffset - thought.CurStage.baseMoodEffect);
      pawn.needs.mood.thoughts.memories.TryGainMemory(thought, otherPawn);
    }
  }

  public static void AdjustMood(Pawn pawn, Pawn otherPawn, Thought_Memory thought)
  {
    if (AdjustMoodIntro(pawn, otherPawn, thought.def, out float MoodOffset, out int index))
    {
      thought.SetForcedStage(index);
      thought.moodOffset = Mathf.RoundToInt(MoodOffset - thought.CurStage.baseMoodEffect);
    }
  }

  public static bool AdjustMoodIntro(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef, out float moodOffset, out int index)
  {
    moodOffset = GetOffsetFromMinMax(pawn, otherPawn, MinMaxMoodDict[thoughtDef]);
    List<Pair<int, float>> baseMoodEffectList = (from int k in Enumerable.Range(0, thoughtDef.stages.Count())
                                                 select new Pair<int, float>(k, thoughtDef.stages[k].baseMoodEffect)).ToList();
    bool result = GetStageIndex(moodOffset, baseMoodEffectList, out index);
    return result;
  }

  public static float GetOffsetFromMinMax(Pawn pawn, Pawn otherPawn, Pair<float, float> pair)
  {
    float min = pair.First;
    float max = pair.Second;
    if (max - min < 0.25f)
    {
      return 0.5f * (max + min);
    }
    float r = 0.75f * (PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) - 0.5f);
    float o = 1.25f * (Mathf.InverseLerp(-5f, 100f, pawn.relations.OpinionOf(otherPawn)) - 0.5f);
    float moodOffset = Mathf.Lerp(min, max, 0.5f - r - o);
    return Mathf.Sign(moodOffset) * Mathf.Ceil(1e-5f + Mathf.Abs(moodOffset));
  }

  public static bool GetStageIndex(float offset, List<Pair<int, float>> baseEffectList, out int index)
  {
    List<int> allowedStageIndices = (from pair in baseEffectList
                                     where Math.Sign(offset) == Math.Sign(pair.Second)
                                     select pair.First).ToList();
    if (allowedStageIndices.NullOrEmpty())
    {
      Log.Error("allowedStageIndices is null or empty.");
      index = 0;
      return false;
    }
    index = allowedStageIndices.MinBy(k => Mathf.Abs(offset - baseEffectList[k].Second));
    return true;
  }

  /* Remove memories */
  public static void RemoveBrokeUpWithMe(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(BrokeUpWithMeMoodDef, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependentMood, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.IBrokeUpWithThemMood, otherPawn);
  }

  public static void RemoveDivorcedMe(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.DivorcedMe, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(DivorcedMeMoodDef, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.DivorcedMeCodependent, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.DivorcedMeCodependentMood, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.IDivorcedThemMood, otherPawn);
  }

  public static void RemoveRejectedMyProposal(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, otherPawn);
    // This one is redundant in practice, but it's good to be safe
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposalMood, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.RejectedMyProposalCodependent, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.RejectedMyProposalCodependentMood, otherPawn);
  }

  public static void RemoveCheatedOnMe(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.CheatedOnMe, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(CheatedOnMeMoodDef, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.CheatedOnMeCodependent, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.CheatedOnMeCodependentMood, otherPawn);
    //pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.ICheatedOnThemMood, otherPawn);
  }

  public static void RemoveIRejectedTheirProposal(Pawn pawn, Pawn otherPawn)
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, otherPawn);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.IRejectedTheirProposalMood, otherPawn);
  }

}

