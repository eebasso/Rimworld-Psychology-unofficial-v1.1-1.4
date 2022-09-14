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
    private Dictionary<Pair<string, string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
    private Dictionary<Pair<string, string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();
    public int AdjustedRatingTicker = -1;

    public Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>> certaintyFromMemesAndNodes = new Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>>();
    public Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>> certaintyFromPerceptsAndNodes = new Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>>();

    public Pawn_PsycheTracker(Pawn pawn)
    {
        this.pawn = pawn;
        //Initialize();
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
        IEnumerable<Thought_MemorySocialDynamic> convoMemories;
        float weight = 10f / Mathf.Lerp(1f + 8f * def.controversiality, 1f + 0.5f * def.controversiality, this.TotalThoughtOpinion(otherPawn, out convoMemories) / 75f + this.GetPersonalityRating(PersonalityNodeDefOf.Outspoken));
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
            float disagree = Mathf.Lerp(1f, 0.25f, 0.1f * knownDisagreements * this.GetPersonalityRating(PersonalityNodeDefOf.Polite) - 0.25f);
            cachedDisagreementWeights[disagreementKey] = disagree;
            recalcNodeDisagreement[disagreementKey] = false;
            weight *= disagree;
        }
        return weight;
    }

    public float TotalThoughtOpinion(Pawn other, out IEnumerable<Thought_MemorySocialDynamic> convoMemories)
    {
        convoMemories = (from m in this.pawn.needs.mood.thoughts.memories.Memories.OfType<Thought_MemorySocialDynamic>()
                         where m.def.defName.Contains("Conversation") && m.otherPawn == other
                         select m);
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

    public void CalculateAdjustedRatings()
    {
        TimeSpan ts;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        int index;
        float adjustedRating;
        float[] adjustedRatingList = new float[PersonalityNodeParentMatrix.order];
        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            adjustedRating = node.AdjustForCircumstance(node.rawRating, true);
            adjustedRatingList[index] = PsycheHelper.NormalCDFInv(adjustedRating);
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings, AdjustForCircumstance + NormalCDFInv took " + ts.TotalMilliseconds + " ms.");

        stopwatch.Start();
        adjustedRatingList = PersonalityNodeParentMatrix.MatrixVectorProduct(PersonalityNodeParentMatrix.parentTransformMatrix, adjustedRatingList);
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings, MatrixVectorProduct took " + ts.TotalMilliseconds + " ms.");

        stopwatch.Start();
        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            adjustedRating = PsycheHelper.NormalCDF(adjustedRatingList[index]);
            adjustedRating = node.AdjustForCircumstance(adjustedRating, true);
            adjustedRating = node.AdjustHook(adjustedRating);
            adjustedRatingList[index] = adjustedRating;
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings, NormalCDF + AdjustForCircumstance took " + ts.TotalMilliseconds + " ms.");

        stopwatch.Start();
        adjustedRatingList = PersonalityNodeParentMatrix.ApplyBigFiveProjections(adjustedRatingList);
        ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings, ApplyBigFiveProjections took " + ts.TotalMilliseconds + " ms.");

        foreach (PersonalityNode node in this.nodes)
        {
            index = PersonalityNodeParentMatrix.indexDict[node.def];
            node.cachedRating = adjustedRatingList[index];
        }
        //stopwatch.Stop();
        //TimeSpan ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings took " + ts.TotalMilliseconds + " ms.");
        AdjustedRatingTicker = 500;
    }

    public float CalculateCertaintyChangePerDay()
    {
        CalculateAdjustedRatings();
        return 0.0015f * CompatibilityWithIdeo(pawn.Ideo); // ToDo: make how much personality affects certainty into a setting
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
        foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp in PersonalityNodeIdeoUtility.memesAffectedByNodes)
        {
            memeDef = kvp.Key;
            if (ideo.HasMeme(memeDef))
            {
                foreach (KeyValuePair<PersonalityNodeDef, float> pair in kvp.Value)
                {
                    nodeDef = pair.Key;
                    rating = GetPersonalityRating(nodeDef);
                    adjustment = (2f * rating - 1f) * pair.Value;
                    compatibility += adjustment;
                    if (addToDicts != true)
                    {
                        continue;
                    }
                    if (!certaintyFromMemesAndNodes.ContainsKey(memeDef))
                    {
                        certaintyFromMemesAndNodes.Add(memeDef, new Dictionary<PersonalityNodeDef, float>());
                    }
                    certaintyFromMemesAndNodes[memeDef][nodeDef] = 0.0015f * adjustment;
                }
            }
        }
        foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp in PersonalityNodeIdeoUtility.preceptsAffectedByNodes)
        {
            preceptDef = kvp.Key;
            if (ideo.HasPrecept(preceptDef))
            {
                foreach (KeyValuePair<PersonalityNodeDef, float> pair in kvp.Value)
                {
                    nodeDef = pair.Key;
                    rating = GetPersonalityRating(nodeDef);
                    adjustment = (2f * rating - 1f) * pair.Value;
                    compatibility += adjustment;
                    if (addToDicts != true)
                    {
                        continue;
                    }
                    if (!certaintyFromPerceptsAndNodes.ContainsKey(preceptDef))
                    {
                        certaintyFromPerceptsAndNodes.Add(preceptDef, new Dictionary<PersonalityNodeDef, float>());
                    }
                    certaintyFromPerceptsAndNodes[preceptDef][nodeDef] = 0.0015f * adjustment;
                }
            }
        }
        return compatibility;
    }
}


