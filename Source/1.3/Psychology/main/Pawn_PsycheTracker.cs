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
namespace Psychology;

public class Pawn_PsycheTracker : IExposable
{
    public int upbringing;
    public int lastDateTick = 0;
    public Pawn pawn;
    public int adjustedRatingTicker = -1;

    private HashSet<PersonalityNode> nodes;
    public Dictionary<string, PersonalityNode> nodeDict = new Dictionary<string, PersonalityNode>();

    public Dictionary<string, float> cachedOpinions = new Dictionary<string, float>();
    public Dictionary<string, bool> recalcCachedOpinions = new Dictionary<string, bool>();
    public Dictionary<Pair<string, string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
    public Dictionary<Pair<string, string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();

    public float[] rawNormalDisplacementList = new float[PersonalityNodeParentMatrix.order];
    public float[] adjNormalDisplacementList = new float[PersonalityNodeParentMatrix.order];
    public const int PersonalityCategories = 32;

    public Pawn_PsycheTracker(Pawn pawn)
    {
        this.pawn = pawn;
        Initialize();
    }

    public void Initialize(int inputSeed = 0)
    {
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 0");
        this.nodes = new HashSet<PersonalityNode>();
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 1");
        PersonalityNode node;
        foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        {
            node = new PersonalityNode(pawn, def);
            nodes.Add(node);
            nodeDict[def.defName] = node;
            //nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
        }
        foreach (PersonalityNode n in this.nodes)
        {
            nodeDict[n.def.defName] = n;
        }
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 2");
        RandomizeUpbringingAndRatings(inputSeed);
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 3");
        
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 4");
        IEnumerable<float> rlist = from n in nodes
                                   select n.rawRating;
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": raw ratings = " + String.Join(", ", rlist));
        CalculateAdjustedRatings();
        Log.Message("Pawn_PsycheTracker.Initialize for " + pawn.LabelShort + ": step 5");
        //Log.Message("raw displacement = " + string.Join(", ", rawNormalDisplacementList));
    }

    public void RandomizeUpbringingAndRatings(int inputSeed = 0)
    {
        int pawnSeed = this.pawn.HashOffset();
        this.upbringing = Mathf.CeilToInt(Rand.ValueSeeded(pawnSeed + inputSeed) * PersonalityCategories);
        float[] ratingList = new float[PersonalityNodeParentMatrix.order];
        foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        {
            int defSeed = def.defName.GetHashCode();
            int index = PersonalityNodeParentMatrix.indexDict[def];
            ratingList[index] = Rand.ValueSeeded((2 + index) * pawnSeed + defSeed + (3 + index) * inputSeed);
        }
        /* Pawns are separated into 2^5 = 32 categories based on the five factor model. */
        /* Two pawns with the same upbringing should always have similar personality ratings, unless there are overriding traits. */
        ratingList = PersonalityNodeParentMatrix.ApplyUpbringingProjection(ratingList, upbringing);
        foreach (PersonalityNode node in nodes)
        {
            int index = PersonalityNodeParentMatrix.indexDict[node.def];
            node.rawRating = ratingList[index];
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref this.upbringing, "upbringing", 0, false);
        Scribe_Values.Look(ref this.lastDateTick, "lastDateTick", 0, false);
        PsycheHelper.Look(ref this.nodes, "nodes", LookMode.Deep, new object[] { this.pawn });
        foreach (PersonalityNode n in this.nodes)
        {
            nodeDict[n.def.defName] = n;
        }
        CalculateAdjustedRatings();
    }

    public float GetPersonalityRating(PersonalityNodeDef def)
    {
        Log.Message("Pawn_PsycheTracker.GetPersonalityRating");
        if (adjustedRatingTicker < 0)
        {
            CalculateAdjustedRatings();
        }
        adjustedRatingTicker--;
        return nodeDict[def.defName].AdjustedRating;
    }

    //public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
    //{
    //    return nodeDict[def.defName];
    //}

    public float GetConversationTopicWeight(PersonalityNodeDef def, Pawn otherPawn)
    {
        /* Pawns will avoid controversial topics until they know someone better.
         * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
         */
        IEnumerable<Thought_MemorySocialDynamic> convoMemories;
        float weight = 10f / Mathf.Lerp(1f + 8f * def.controversiality, 1f + 0.5f * def.controversiality, this.TotalThoughtOpinion(otherPawn, out convoMemories) / 75f + GetPersonalityRating(PersonalityNodeDefOf.Outspoken));
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
        //Stopwatch stopwatch = new Stopwatch();
        //stopwatch.Start();
        Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 0");
        float rawRating;
        foreach (PersonalityNode node in nodes)
        {
            rawRating = node.AdjustForCircumstance(node.rawRating, true);
            rawNormalDisplacementList[PersonalityNodeParentMatrix.indexDict[node.def]] = PsycheHelper.NormalCDFInv(rawRating);
        }

        Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 1");
        adjNormalDisplacementList = PersonalityNodeParentMatrix.MatrixVectorProduct(PersonalityNodeParentMatrix.parentTransformMatrix, rawNormalDisplacementList);
        //parentAdjRatingDict.Clear();
        Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2");
        foreach (PersonalityNode node in nodes)
        {
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2a");
            int index = PersonalityNodeParentMatrix.indexDict[node.def];
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2b");
            float adjustedRating = PsycheHelper.NormalCDF(adjNormalDisplacementList[index]);
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2c");
            adjustedRating = node.AdjustForCircumstance(adjustedRating, true);
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2d");
            adjustedRating = node.AdjustHook(adjustedRating);
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2e");
            node.AdjustedRating = adjustedRating;
            Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 2f");
        }
        this.adjustedRatingTicker = 100;
        Log.Message("Pawn_PsycheTracker.CalculateAdjustedRatings for " + pawn.LabelShort + ": step 3");
        var arlist = from PersonalityNodeDef def in PersonalityNodeParentMatrix.defList select GetPersonalityRating(def);
        Log.Message("CalculateAdjustedRatings(): Adjusted ratings for " + pawn.LabelShort + ": " + string.Join(", ", arlist));
        //stopwatch.Stop();
        //TimeSpan ts = stopwatch.Elapsed;
        //Log.Message("CalculateAdjustedRatings took " + ts.TotalMilliseconds + " ms.");
    }

    public float CalculateCertaintyChangePerDay()
    {
        CalculateAdjustedRatings();
        return 0.0015f * CompatibilityWithIdeo(pawn.Ideo);
    }

    public float CompatibilityWithIdeo(Ideo ideo)
    {
        if (ideo == null)
        {
            return 0f;
        }
        float compatibility = 0f;
        foreach (KeyValuePair<MemeDef, List<Pair<PersonalityNodeDef, float>>> kvp in PersonalityNodeIdeoUtility.memesAffectedByNodes)
        {
            if (ideo.HasMeme(kvp.Key))
            {
                foreach (Pair<PersonalityNodeDef, float> pair in kvp.Value)
                {
                    float rating = GetPersonalityRating(pair.First);
                    compatibility += (2f * rating - 1f) * pair.Second;
                }
            }
        }
        foreach (KeyValuePair<PreceptDef, List<Pair<PersonalityNodeDef, float>>> kvp in PersonalityNodeIdeoUtility.preceptsAffectedByNodes)
        {
            if (ideo.HasPrecept(kvp.Key))
            {
                foreach (Pair<PersonalityNodeDef, float> pair in kvp.Value)
                {
                    float rating = GetPersonalityRating(pair.First);
                    compatibility += (2f * rating - 1f) * pair.Second;
                }
            }
        }
        return compatibility;
    }
}


