using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.Experimental;

namespace Psychology;

public class Pawn_PsycheTracker : IExposable
{
    public int upbringing;
    public int lastDateTick = 0;
    private Pawn pawn;
    private HashSet<PersonalityNode> nodes;

    private Dictionary<PersonalityNodeDef, PersonalityNode> nodeDict = new Dictionary<PersonalityNodeDef, PersonalityNode>();
    private Dictionary<string, float> cachedOpinions = new Dictionary<string, float>();
    private Dictionary<string, bool> recalcCachedOpinions = new Dictionary<string, bool>();
    public Dictionary<Pair<string, string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
    public Dictionary<Pair<string, string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();
    public int AdjustedRatingTicker = -1;
    public Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>> certaintyFromMemesAndNodes = new Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>>();
    public Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>> certaintyFromPerceptsAndNodes = new Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>>();

    public HashSet<PersonalityNode> PersonalityNodes
    {
        get
        {
            return nodes;
        }
        set
        {
            nodes = value;
        }
    }

    public Dictionary<string, bool> OpinionCacheDirty
    {
        get
        {
            return recalcCachedOpinions;
        }
        set
        {
            recalcCachedOpinions = value;
        }
    }

    public Dictionary<Pair<string, string>, bool> DisagreementCacheDirty
    {
        get
        {
            return recalcNodeDisagreement;
        }
        set
        {
            recalcNodeDisagreement = value;
        }
    }

    public Pawn_PsycheTracker(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public void Initialize(int inputSeed = 0)
    {
        this.nodes = new HashSet<PersonalityNode>();
        foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        {
            nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
        }
        RandomizeRatings(inputSeed);
        foreach (PersonalityNode n in this.nodes)
        {
            nodeDict[n.def] = n;
        }
    }

    public void RandomizeRatings(int inputSeed = 0)
    {
        int pawnSeed = PsycheHelper.PawnSeed(this.pawn);
        int defSeed;
        foreach (PersonalityNode node in nodes)
        {
            defSeed = node.GetHashCode();
            node.rawRating = Rand.ValueSeeded(Gen.HashCombineInt(pawnSeed, defSeed, inputSeed, 37));
        }
        AdjustedRatingTicker = -1;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref this.upbringing, "upbringing", 0, false);
        Scribe_Values.Look(ref this.lastDateTick, "lastDateTick", 0, false);
        PsycheHelper.Look(ref this.nodes, "nodes", LookMode.Deep, new object[] { this.pawn });
        foreach (PersonalityNode n in this.nodes)
        {
            nodeDict[n.def] = n;
        }
    }

    public float GetPersonalityRating(PersonalityNodeDef def)
    {
        if (AdjustedRatingTicker < 0)
        {
            CalculateAdjustedRatings();
        }
        AdjustedRatingTicker--;
        return nodeDict[def].AdjustedRating;
    }

    public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
    {
        return nodeDict[def];
    }

    public float GetConversationTopicWeight(PersonalityNodeDef def, Pawn otherPawn)
    {
        /* Pawns will avoid controversial topics until they know someone better.
         * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
         */
        float totalOpinion = this.TotalThoughtOpinion(otherPawn, out IEnumerable<Thought_MemorySocialDynamic> convoMemories);
        float t = totalOpinion / 75f + GetPersonalityRating(PersonalityNodeDefOf.Outspoken);
        float c = def.controversiality;
        float weight = 1f / (c * c + 4f * t * t);
        //float weight = 10f / Mathf.Lerp(1f + 8f * def.controversiality, 1f + 0.5f * def.controversiality, t);

        /* Polite pawns will avoid topics they already know are contentious. */
        Pair<string, string> disagreementKey = new Pair<string, string>(otherPawn.ThingID, def.defName);
        if (cachedDisagreementWeights.ContainsKey(disagreementKey) && !recalcNodeDisagreement[disagreementKey])
        {
            weight *= cachedDisagreementWeights[disagreementKey];
        }
        else
        {
            float knownDisagreements = (from m in convoMemories
                                        where m.opinionOffset < 0f && m.CurStage.label == def.defName
                                        select Math.Abs(m.opinionOffset)).Sum();
            //float disagree = 1f;
            //if (knownDisagreements > 0)
            //{
            //    disagree = Mathf.Clamp01(1f / (knownDisagreements / 50f)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
            //}
            float disagree = Mathf.Lerp(1f, 0.25f, 0.1f * knownDisagreements * GetPersonalityRating(PersonalityNodeDefOf.Polite) - 0.25f);
            cachedDisagreementWeights[disagreementKey] = disagree;
            recalcNodeDisagreement[disagreementKey] = false;
            weight *= disagree;
        }
        return weight;
    }

    public float TotalThoughtOpinion(Pawn other, out IEnumerable<Thought_MemorySocialDynamic> convoMemories)
    {
        convoMemories = from m in this.pawn.needs.mood.thoughts.memories.Memories.OfType<Thought_MemorySocialDynamic>()
                        where m.def.defName.Contains("Conversation") && m.otherPawn == other
                        select m;
        if (cachedOpinions.ContainsKey(other.ThingID) && !recalcCachedOpinions[other.ThingID])
        {
            return cachedOpinions[other.ThingID];
        }
        float knownThoughtOpinion = 1f;
        convoMemories.Do(m => knownThoughtOpinion += Math.Abs(m.opinionOffset));
        cachedOpinions[other.ThingID] = knownThoughtOpinion;
        recalcCachedOpinions[other.ThingID] = false;
        return knownThoughtOpinion;
    }

    public void CalculateAdjustedRatings()
    {
        //Stopwatch stopwatch = new Stopwatch();
        //Stopwatch stopwatch2 = new Stopwatch();

        int index;
        float adjustedRating;
        float[] adjustedRatingList = new float[PersonalityNodeParentMatrix.order];
        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            //stopwatch.Start();
            adjustedRating = node.AdjustForCircumstance(node.rawRating, true);
            //stopwatch.Stop();
            //stopwatch2.Start();
            adjustedRatingList[index] = PsycheHelper.NormalCDFInv(adjustedRating);
            //stopwatch2.Stop();
        }
        //Log.Message("CalculateAdjustedRatings, AdjustForCircumstance took " + stopwatch.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.27 ms
        //Log.Message("CalculateAdjustedRatings, NormalCDFInv took " + stopwatch2.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.015 ms

        //stopwatch.Reset();
        //stopwatch.Start();
        adjustedRatingList = PersonalityNodeParentMatrix.MatrixVectorProduct(PersonalityNodeParentMatrix.parentTransformMatrix, adjustedRatingList);
        //stopwatch.Stop();
        //Log.Message("CalculateAdjustedRatings, MatrixVectorProduct took " + stopwatch.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.013 ms

        //stopwatch.Reset();
        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            //stopwatch.Start();
            adjustedRating = PsycheHelper.NormalCDF(adjustedRatingList[index]);
            //stopwatch.Stop();
            //stopwatch2.Start();
            adjustedRating = node.AdjustForCircumstance(adjustedRating, true);
            //stopwatch2.Stop();
            adjustedRating = node.AdjustHook(adjustedRating);
            adjustedRatingList[index] = adjustedRating;
        }
        //Log.Message("CalculateAdjustedRatings, NormalCDF took " + stopwatch.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.015 ms
        //Log.Message("CalculateAdjustedRatings, AdjustForCircumstance took " + stopwatch2.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.27 ms

        //stopwatch.Reset();
        //stopwatch.Start();
        adjustedRatingList = PersonalityNodeParentMatrix.ApplyBigFiveProjections(adjustedRatingList);
        //stopwatch.Stop();
        //Log.Message("CalculateAdjustedRatings, ApplyBigFiveProjections took " + stopwatch.Elapsed.TotalMilliseconds + " ms."); // Takes around 0.013 ms

        //stopwatch.Reset();
        //stopwatch.Start();
        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            node.cachedRating = adjustedRatingList[index];
        }
        //stopwatch.Stop();
        //Log.Message("CalculateAdjustedRatings, saving cachedRatings took " + stopwatch.Elapsed.TotalMilliseconds + " ms."); // Take around 0.01 ms

        AdjustedRatingTicker = 500;
    }

    public float CalculateCertaintyChangePerDay(bool addToDicts = false)
    {
        CalculateAdjustedRatings();
        return CompatibilityWithIdeo(pawn.Ideo, addToDicts);
    }

    public float CompatibilityWithIdeo(Ideo ideo, bool addToDicts = false)
    {
        if (ideo == null)
        {
            return 0f;
        }
        float compatibility = 0f;
        float rating;
        float adjustment;
        MemeDef memeDef;
        PreceptDef preceptDef;
        PersonalityNodeDef nodeDef;
        foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in PersonalityNodeIdeoUtility.memesAffectedByNodes)
        {
            memeDef = kvp0.Key;
            if (ideo.HasMeme(memeDef) != true)
            {
                continue;
            }
            if (addToDicts)
            {
                if (certaintyFromMemesAndNodes.ContainsKey(memeDef) != true)
                {
                    certaintyFromMemesAndNodes[memeDef] = new Dictionary<PersonalityNodeDef, float>();
                }
            }
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                nodeDef = kvp1.Key;
                rating = GetPersonalityRating(nodeDef);
                adjustment = PsycheHelper.baseIdeoCompatScale * (2f * rating - 1f) * kvp1.Value;
                compatibility += adjustment;
                if (addToDicts)
                {
                    certaintyFromMemesAndNodes[memeDef][nodeDef] = adjustment;
                }
            }
        }
        foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp0 in PersonalityNodeIdeoUtility.preceptsAffectedByNodes)
        {
            preceptDef = kvp0.Key;
            if (ideo.HasPrecept(preceptDef) != true)
            {
                continue;
            }
            if (addToDicts)
            {
                if (certaintyFromPerceptsAndNodes.ContainsKey(preceptDef) != true)
                {
                    certaintyFromPerceptsAndNodes[preceptDef] = new Dictionary<PersonalityNodeDef, float>();
                }
            }
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                nodeDef = kvp1.Key;
                rating = GetPersonalityRating(nodeDef);
                adjustment = PsycheHelper.baseIdeoCompatScale * (2f * rating - 1f) * kvp1.Value;
                compatibility += adjustment;
                if (addToDicts)
                {
                    certaintyFromPerceptsAndNodes[preceptDef][nodeDef] = adjustment;
                }
            }
        }
        return compatibility;
    }

    public void DeepCopyFromOtherTracker(Pawn_PsycheTracker otherTracker)
    {
        this.upbringing = otherTracker.upbringing;
        this.lastDateTick = otherTracker.lastDateTick;

        this.nodes = new HashSet<PersonalityNode>();
        foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        {
            this.nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
        }
        foreach (PersonalityNode node in this.nodes)
        {
            node.rawRating = otherTracker.GetPersonalityNodeOfDef(node.def).rawRating;
        }
        foreach (PersonalityNode n in this.nodes)
        {
            this.nodeDict[n.def] = n;
        }
        foreach (KeyValuePair<string, float> kvp in otherTracker.cachedOpinions)
        {
            this.cachedOpinions[kvp.Key] = kvp.Value;
        }
        foreach (KeyValuePair<string, bool> kvp in otherTracker.recalcCachedOpinions)
        {
            this.recalcCachedOpinions[kvp.Key] = kvp.Value;
        }
        foreach (KeyValuePair<Pair<string, string>, float> kvp in otherTracker.cachedDisagreementWeights)
        {
            this.cachedDisagreementWeights[new Pair<string, string>(kvp.Key.First, kvp.Key.Second)] = kvp.Value;
        }
        foreach (KeyValuePair<Pair<string, string>, bool> kvp in otherTracker.recalcNodeDisagreement)
        {
            this.recalcNodeDisagreement[new Pair<string, string>(kvp.Key.First, kvp.Key.Second)] = kvp.Value;
        }
        foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in otherTracker.certaintyFromMemesAndNodes)
        {
            this.certaintyFromMemesAndNodes[kvp0.Key] = new Dictionary<PersonalityNodeDef, float>();
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                this.certaintyFromMemesAndNodes[kvp0.Key][kvp1.Key] = kvp1.Value;
            }
        }
        foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp0 in otherTracker.certaintyFromPerceptsAndNodes)
        {
            this.certaintyFromPerceptsAndNodes[kvp0.Key] = new Dictionary<PersonalityNodeDef, float>();
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                this.certaintyFromPerceptsAndNodes[kvp0.Key][kvp1.Key] = kvp1.Value;
            }
        }
    }
}


