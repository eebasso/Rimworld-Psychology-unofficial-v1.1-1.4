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
public static class BreakupHelperMethods
{
    public static ThoughtDef BrokeUpWithMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("BrokeUpWithMeMood");
    public static ThoughtDef DivorcedMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("DivorcedMeMood");
    public static ThoughtDef CheatedOnMeMoodDef = DefDatabase<ThoughtDef>.GetNamed("CheatedOnMeMood");

    public static Dictionary<ThoughtDef, string[]> PsychologyMethodNameDict = new Dictionary<ThoughtDef, string[]>()
    {
        { ThoughtDefOf.BrokeUpWithMe          , new string[] { nameof(GainBrokeUpWithMe) , nameof(RemoveBrokeUpWithMe) } },
        { ThoughtDefOf.DivorcedMe             , new string[] { nameof(GainDivorcedMe) , nameof(RemoveDivorcedMe) } },
        { ThoughtDefOf.RejectedMyProposal     , new string[] { nameof(GainRejectedMyProposal) , nameof(RemoveRejectedMyProposal) } },
        { ThoughtDefOf.CheatedOnMe            , new string[] { nameof(GainCheatedOnMe) , nameof(RemoveCheatedOnMe) } },
        { ThoughtDefOf.IRejectedTheirProposal , new string[] { nameof(GainIRejectedTheirProposal) , nameof(RemoveIRejectedTheirProposal) } }
    };

    public static List<ThoughtDef> OpinionThoughtDefs;
    public static Dictionary<ThoughtDef, Pair<float, float>> MinMaxOpinionDict = new Dictionary<ThoughtDef, Pair<float, float>>();

    public static List<ThoughtDef> MoodThoughtDefs = new List<ThoughtDef> { BrokeUpWithMeMoodDef, DivorcedMeMoodDef, ThoughtDefOf.RejectedMyProposalMood, CheatedOnMeMoodDef, ThoughtDefOfPsychology.IBrokeUpWithThemMood, ThoughtDefOfPsychology.IDivorcedThemMood, ThoughtDefOfPsychology.IRejectedTheirProposalMood };
    public static Dictionary<ThoughtDef, Pair<float, float>> MinMaxMoodDict = new Dictionary<ThoughtDef, Pair<float, float>>();

    static BreakupHelperMethods()
    {
        OpinionThoughtDefs = (from kvp in PsychologyMethodNameDict
                              select kvp.Key).ToList();
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
        //Log.Message("InterdictTryGainAndRemoveMemories, start");
        List<CodeInstruction> clist = codes.ToList();
        //bool success = false;


        FieldInfo fieldInfoMemories = AccessTools.Field(typeof(ThoughtHandler), nameof(ThoughtHandler.memories));
        MethodInfo methodInfoTryGainMemory = AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) });
        MethodInfo methodInfoRemoveMemories = AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemoriesOfDefWhereOtherPawnIs));
        //Log.Message("InterdictTryGainAndRemoveMemories, test");
        //FieldInfo fieldInfo = AccessTools.Field(typeof(ThoughtDefOf), ThoughtDefOf.BrokeUpWithMe.defName);
        //Log.Message("InterdictTryGainAndRemoveMemories, OpinionThoughtDefs NullOrEmpty: " + OpinionThoughtDefs.NullOrEmpty());
        //Log.Message("InterdictTryGainAndRemoveMemories, OpinionThoughtDefs count: " + OpinionThoughtDefs.Count());
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
        //Log.Message("InterdictTryGainAndRemoveMemories, search clist for matches");
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
                        //Log.Message("InterdictTryGainAndRemoveMemories, found TryGainOrRemove");
                    }
                    break;
                case 1:
                    if (clist[i].LoadsField(fieldInfoMemories))
                    {
                        searchMode++;
                        listMemoriesIndexInClist.Add(i);
                        listPawnIndexInClist.Add(i - 4);
                        //Log.Message("InterdictTryGainAndRemoveMemories, found memories");
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
                                //Log.Message("InterdictTryGainAndRemoveMemories, found ThoughtDef");
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
        //Log.Message("InterdictTryGainThoughtAndRemoveMemories, found this number of matches: " + listPawnIndexInClist.Count());
        //Log.Message("InterdictTryGainAndRemoveMemories, start yield return");
        for (int i = 0; i < clist.Count(); i++)
        {
            yield return clist[i];
            int matchNum = listPawnIndexInClist.IndexOf(i);
            if (matchNum >= 0)
            {
                yield return clist[listOtherPawnIndexInClist[matchNum]];
                ThoughtDef thoughtDef = OpinionThoughtDefs[listThoughtDefIndexInOpinionThoughtDefs[matchNum]];
                yield return CodeInstruction.Call(typeof(BreakupHelperMethods), PsychologyMethodNameDict[thoughtDef][listGainVsRemoveBit[matchNum]]);
                i = listTryGainOrRemoveIndexInClist[matchNum];

                //Log.Message("InterdictTryGainAndRemoveMemories, matchNum: " + matchNum + (listGainVsRemoveBit[matchNum] == 0 ? ", TryGain" : ", Remove") + " ThoughtDef: " + OpinionThoughtDefs[listThoughtDefIndexInOpinionThoughtDefs[matchNum]] + ", implement " + PsychologyMethodNameDict[thoughtDef] +
                //            "\nindexOfPawnInClist: " + listPawnIndexInClist[matchNum] +
                //            "\nindexOfMemoriesInClist: " + listMemoriesIndexInClist[matchNum] +
                //            "\nindexOfThoughtDefInClist: " + listThoughtDefIndexInClist[matchNum] +
                //            "\nindexOfOtherPawnInClist: " + listOtherPawnIndexInClist[matchNum] +
                //            "\nindexOfTryGainOrRemoveMemoriesInClist: " + listTryGainOrRemoveIndexInClist[matchNum] +
                //            "\nindexOfThoughtDefInOpinionThoughtDefs: " + listThoughtDefIndexInOpinionThoughtDefs[matchNum]
                //            );
            }
        }
        //Log.Message("InterdictTryGainAndRemoveMemories, end");


        //    switch (searchMode)
        //    {
        //        case 0:

        //        //if (clist[i].LoadsField(fieldInfoMemories))
        //        //{
        //        //    searchMode = 1;
        //        //    indexOfMemoriesInClist = i;
        //        //}
        //        //break;
        //        case 1:
        //            foreach (Pair<int, FieldInfo> pair in fieldInfos)
        //            {
        //                if (clist[i].LoadsField(pair.Second))
        //                {
        //                    searchMode = 2;
        //                    indexOfThoughtDefInClist = i;
        //                    indexOfThoughtDefInOpinionThoughtDefs = pair.First;
        //                    continue;
        //                }
        //            }
        //            break;
        //        default:
        //            gainVsRemoveBit = clist[i].Calls(methodInfoTryGainMemory) ? 0 : clist[i].Calls(methodInfoRemoveMemories) ? 1 : -1;
        //            if (gainVsRemoveBit != -1)
        //            {
        //                searchMode = 0;
        //                //listPawnCode.Add(clist[indexOfMemoriesInClist - 4]);
        //                listMemoriesIndexInClist.Add(indexOfMemoriesInClist);
        //                listThoughtDefIndexInClist.Add(indexOfThoughtDefInClist);
        //                listThoughtDefIndexInOpinionThoughtDefs.Add(indexOfThoughtDefInOpinionThoughtDefs);
        //                //listOtherPawnCode.Add(clist[indexOfThoughtDefInClist + 1]);
        //                listTryGainOrRemoveMemoriesIndexInClist.Add(i);
        //                listGainVsRemoveBit.Add(gainVsRemoveBit);
        //                success = true;
        //                Log.Message("InterdictTryGainAndRemoveMemories, matchNum: " + (listMemoriesIndexInClist.Count() - 1) + (gainVsRemoveBit == 0 ? ", TryGain" : ", Remove") + " ThoughtDef: " + OpinionThoughtDefs[indexOfThoughtDefInOpinionThoughtDefs] +
        //                    "\nindexOfPawnInClist: " + (indexOfMemoriesInClist - 4) +
        //                    "\nindexOfMemoriesInClist: " + indexOfMemoriesInClist +
        //                    "\nindexOfThoughtDefInClist: " + indexOfThoughtDefInClist +
        //                    "\nindexOfOtherPawnInClist: " + (indexOfThoughtDefInClist + 1) +
        //                    "\nindexOfTryGainOrRemoveMemoriesInClist: " + i +
        //                    "\nindexOfThoughtDefInOpinionThoughtDefs: " + indexOfThoughtDefInOpinionThoughtDefs
        //                    );
        //            }
        //            break;
        //    }
        //}

        //Log.Message("InterdictTryGainAndRemoveMemories, verify each match");
        //for (int matchNum = 0; matchNum < listTryGainOrRemoveIndexInClist.Count(); matchNum++)
        //{
        //    success = false;
        //    searchMode = 0;
        //    for (int k = listTryGainOrRemoveIndexInClist[matchNum]; k >= 0; k--)
        //    {
        //        //if (searchMode == 0)
        //        //{
        //        //    Log.Message("InterdictTryGainAndRemoveMemories, clist[k]: " + clist[k] + ", clist[k]?.operand: " + clist[k]?.operand + ", clist[k]?.operand is Pawn" + (clist[k]?.operand is Pawn) + ", typeof(Pawn).IsInstanceOfType(clist[k]?.operand): " + typeof(Pawn).IsInstanceOfType(clist[k]?.operand));
        //        //    if (typeof(Pawn).IsInstanceOfType(clist[k]?.operand))
        //        //    {
        //        //        searchMode++;
        //        //        listOtherPawnIndexInClist.Add(k);
        //        //        Log.Message("InterdictTryGainAndRemoveMemories, found otherPawn");
        //        //    }
        //        //    continue;
        //        //}
        //        if (searchMode == 0)
        //        {
        //            foreach (Pair<int, FieldInfo> pair in fieldInfos)
        //            {
        //                if (clist[k].LoadsField(pair.Second))
        //                {
        //                    searchMode++;
        //                    listOtherPawnIndexInClist.Add(k + 1);
        //                    listThoughtDefIndexInClist.Add(k);
        //                    listThoughtDefIndexInOpinionThoughtDefs.Add(pair.First);
        //                    Log.Message("InterdictTryGainAndRemoveMemories, found ThoughtDef");
        //                    break;
        //                }
        //            }
        //            continue;
        //        }
        //        if (searchMode == 1)
        //        {
        //            if (clist[k].LoadsField(fieldInfoMemories))
        //            {
        //                success = true;
        //                listMemoriesIndexInClist.Add(k);
        //                listPawnIndexInClist.Add(k - 4);
        //                Log.Message("InterdictTryGainAndRemoveMemories, found memories");
        //                break;
        //            }
        //            continue;
        //        }
        //        //if (searchMode == 3)
        //        //{
        //        //    if (clist[k]?.operand is Pawn)
        //        //    {
        //        //        success = true;
        //        //        listPawnIndexInClist.Add(k);
        //        //        Log.Message("InterdictTryGainAndRemoveMemories, found pawn");
        //        //        break;
        //        //    }
        //        //    continue;
        //        //}
        //    }
        //    if (success != true)
        //    {
        //        Log.Error("InterdictTryGainThoughtAndRemoveMemories failed, stuck at searchMode = " + searchMode);
        //        foreach (CodeInstruction c in codes)
        //        {
        //            yield return c;
        //        }
        //        yield break;
        //    }
        //    Log.Message("InterdictTryGainAndRemoveMemories, matchNum: " + matchNum + (listGainVsRemoveBit[matchNum] == 0 ? ", TryGain" : ", Remove") + " ThoughtDef: " + OpinionThoughtDefs[listThoughtDefIndexInOpinionThoughtDefs[matchNum]] +
        //                    "\nindexOfPawnInClist: " + listPawnIndexInClist[matchNum] +
        //                    "\nindexOfMemoriesInClist: " + listMemoriesIndexInClist[matchNum] +
        //                    "\nindexOfThoughtDefInClist: " + listThoughtDefIndexInClist[matchNum] +
        //                    "\nindexOfOtherPawnInClist: " + listOtherPawnIndexInClist[matchNum] +
        //                    "\nindexOfTryGainOrRemoveMemoriesInClist: " + listTryGainOrRemoveIndexInClist[matchNum] +
        //                    "\nindexOfThoughtDefInOpinionThoughtDefs: " + listThoughtDefIndexInOpinionThoughtDefs[matchNum]
        //                    );
        //}
    }

    /* Gain memories */
    public static void GainBrokeUpWithMe(Pawn pawn, Pawn otherPawn)
    {
        TryGainOpinion(pawn, otherPawn, ThoughtDefOf.BrokeUpWithMe);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, otherPawn);
        TryGainMood(pawn, otherPawn, BrokeUpWithMeMoodDef);
        TryGainMood(otherPawn, pawn, ThoughtDefOfPsychology.IBrokeUpWithThemMood);
    }

    public static void GainDivorcedMe(Pawn pawn, Pawn otherPawn)
    {
        TryGainOpinion(pawn, otherPawn, ThoughtDefOf.DivorcedMe);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DivorcedMeCodependent, otherPawn);
        TryGainMood(pawn, otherPawn, DivorcedMeMoodDef);
        TryGainMood(otherPawn, pawn, ThoughtDefOfPsychology.IDivorcedThemMood);
    }

    public static void GainRejectedMyProposal(Pawn pawn, Pawn otherPawn)
    {
        TryGainOpinion(pawn, otherPawn, ThoughtDefOf.RejectedMyProposal);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RejectedMyProposalCodependent, otherPawn);
        TryGainMood(otherPawn, pawn, ThoughtDefOf.RejectedMyProposalMood);
        TryGainMood(otherPawn, pawn, ThoughtDefOfPsychology.IRejectedTheirProposalMood);
    }

    public static void GainCheatedOnMe(Pawn pawn, Pawn otherPawn)
    {
        //TryGainOpinion(pawn, otherPawn, ThoughtDefOf.CheatedOnMe);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, otherPawn);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CheatedOnMeCodependent, otherPawn);
        TryGainMood(pawn, otherPawn, CheatedOnMeMoodDef);
        //ToDo: make ICheatedOnThemMood ?
        //TryGainMood(pawn, otherPawn, ThoughtDefOfPsychology.ICheatedOnThemMood);
    }

    public static void GainIRejectedTheirProposal(Pawn pawn, Pawn otherPawn)
    {
        //TryGainOpinion(pawn, otherPawn, ThoughtDefOf.CheatedOnMe);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, otherPawn);
        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CheatedOnMeCodependent, otherPawn);
        TryGainMood(pawn, otherPawn, CheatedOnMeMoodDef);
        //ToDo: make ICheatedOnThemMood ?
        //TryGainMood(pawn, otherPawn, ThoughtDefOfPsychology.ICheatedOnThemMood);
    }

    public static void TryGainOpinion(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
    {
        float opinionOffset = GetOffsetFromMinMax(pawn, otherPawn, MinMaxOpinionDict[thoughtDef]);
        List<Pair<int, float>> baseOpinionOffsetList = (from int k in Enumerable.Range(0, thoughtDef.stages.Count())
                                                        select new Pair<int, float>(k, thoughtDef.stages[k].baseOpinionOffset)).ToList();
        int index = GetStageIndex(opinionOffset, baseOpinionOffsetList);
        if (ThoughtMaker.MakeThought(thoughtDef, index) is Thought_MemorySocial thought)
        {
            thought.opinionOffset = opinionOffset;
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought, otherPawn);
            return;
        }
        Log.Error("Thought as Thought_MemorySocial was null");
        pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, otherPawn);
    }

    public static void TryGainMood(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
    {
        float moodOffset = GetOffsetFromMinMax(pawn, otherPawn, MinMaxMoodDict[thoughtDef]);
        List<Pair<int, float>> baseMoodEffectList = (from int k in Enumerable.Range(0, thoughtDef.stages.Count())
                                                     select new Pair<int, float>(k, thoughtDef.stages[k].baseMoodEffect)).ToList();
        int index = GetStageIndex(moodOffset, baseMoodEffectList);
        if (index == -1)
        {
            return;
        }
        Thought_Memory thought = ThoughtMaker.MakeThought(thoughtDef, index);
        thought.moodOffset = Mathf.RoundToInt(moodOffset);
        pawn.needs.mood.thoughts.memories.TryGainMemory(thought, otherPawn);
    }

    public static float GetOffsetFromMinMax(Pawn pawn, Pawn otherPawn, Pair<float, float> pair)
    {
        float r = 0.75f * (PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) - 0.5f);
        float o = 1.25f * (Mathf.InverseLerp(-5f, 100f, pawn.relations.OpinionOf(otherPawn)) - 0.5f);
        float moodOffset = Mathf.Lerp(pair.First, pair.Second, 0.5f - r - o);
        return Mathf.Sign(moodOffset) * Mathf.Ceil(1e-5f + Mathf.Abs(moodOffset));
    }

    public static int GetStageIndex(float offset, List<Pair<int, float>> baseEffectList)
    {
        List<int> allowedStageIndices = (from pair in baseEffectList
                                         where Math.Sign(offset) == Math.Sign(pair.Second)
                                         select pair.First).ToList();
        if (allowedStageIndices.NullOrEmpty())
        {
            Log.Error("allowedStageIndices is null or empty.");
            return -1;
        }
        return allowedStageIndices.MinBy(k => Mathf.Abs(offset - baseEffectList[k].Second));
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

    //public static float AdjustedOpinionOffset(Pawn pawn, Pawn otherPawn, float opinionOffset)
    //{
    //    opinionOffset *= PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(5f, 100f, pawn.relations.OpinionOf(otherPawn));
    //    return Mathf.Ceil(opinionOffset);
    //}

    //public static void AddExLover(Pawn lover, Pawn ex)
    //{
    //  lover.relations.AddDirectRelation(PawnRelationDefOf.ExLover, ex);
    //}

    //public static void AddBrokeUpMood(Pawn lover, Pawn ex)
    //{
    //    //Log.Message("AddBrokeUpMood for lover = " + lover + ", and ex = " + ex);
    //    ThoughtDef brokeUpMoodDef = new ThoughtDef();
    //    brokeUpMoodDef.defName = "BrokeUpWithMeMood" + lover.LabelShort + Find.TickManager.TicksGame;
    //    brokeUpMoodDef.durationDays = 25f;
    //    brokeUpMoodDef.thoughtClass = typeof(Thought_MemoryDynamic);
    //    brokeUpMoodDef.stackedEffectMultiplier = 1f;
    //    brokeUpMoodDef.stackLimit = 999;
    //    ThoughtStage brokeUpStage = new ThoughtStage();
    //    brokeUpStage.label = "Broke up with {0}";
    //    int absoluteMoodEffect = Mathf.CeilToInt(20f * Mathf.InverseLerp(0.25f, 0.75f, PsycheHelper.Comp(lover).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic)) * Mathf.InverseLerp(-20f, 100f, lover.relations.OpinionOf(ex)));

    //    if (absoluteMoodEffect < 5f)
    //    {
    //        brokeUpStage.description = "My lover and I parted ways amicably, but it's still a little sad.";
    //    }
    //    else
    //    {
    //        brokeUpStage.description = "I'm going through a bad break-up right now.";
    //    }
    //    brokeUpMoodDef.stages.Add(brokeUpStage);
    //    brokeUpStage.baseMoodEffect = -absoluteMoodEffect;
    //    if (lover.story.traits.HasTrait(TraitDefOf.Psychopath) != true)
    //    {
    //        lover.needs.mood.thoughts.memories.TryGainMemory(brokeUpMoodDef, ex);
    //    }
    //}

    //public static void AddBrokeUpOpinion(Pawn lover, Pawn ex)
    //{
    //    ThoughtDef brokeUpDef = new ThoughtDef();
    //    brokeUpDef.defName = "BrokeUpWithMe" + lover.LabelShort + Find.TickManager.TicksGame;
    //    brokeUpDef.durationDays = 40f;
    //    brokeUpDef.thoughtClass = typeof(Thought_MemorySocialDynamic);
    //    ThoughtStage brokeUpStage = new ThoughtStage();
    //    brokeUpStage.label = "broke up with me";
    //    brokeUpStage.baseOpinionOffset = Mathf.RoundToInt(-50f * PsycheHelper.Comp(lover).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(5f, 100f, lover.relations.OpinionOf(ex)));
    //    brokeUpDef.stages.Add(brokeUpStage);
    //    lover.needs.mood.thoughts.memories.TryGainMemory(brokeUpDef, ex);
    //}

}

