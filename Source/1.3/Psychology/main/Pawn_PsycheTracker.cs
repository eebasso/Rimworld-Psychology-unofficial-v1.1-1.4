using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
//using Accord.Math;
//using Accord.Math.Decompositions;
//using MathNet.Numerics.LinearAlgebra;

namespace Psychology
{
    public class Pawn_PsycheTracker : IExposable
    {
        public int upbringing;
        public int lastDateTick = 0;
        private Pawn pawn;
        private HashSet<PersonalityNode> nodes;
        public int AdjustedRatingTicker = 0;
        private Dictionary<PersonalityNodeDef, PersonalityNode> nodeDict = new Dictionary<PersonalityNodeDef, PersonalityNode>();
        private Dictionary<string, float> cachedOpinions = new Dictionary<string, float>();
        private Dictionary<string, bool> recalcCachedOpinions = new Dictionary<string, bool>();
        private Dictionary<Pair<string, string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
        private Dictionary<Pair<string, string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();
        public const int PersonalityCategories = 32;
        //public float[] rawNormalDisplacementList;
        //public Vector<float> rawNormalDisplacementList;
        //public Dictionary<PersonalityNodeDef, float> parentAdjRatingDict = new Dictionary<PersonalityNodeDef, float>();
        public float[] rawNormalDisplacementList;

        public Pawn_PsycheTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        [LogPerformance]
        public void Initialize()
        {
            Log.Message("Initialize() Pawn_PsycheTracker for " + pawn.LabelShortCap);
            InitializeUpbringing();
            this.nodes = new HashSet<PersonalityNode>();
            foreach (PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
            {
                nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
            }
            foreach (PersonalityNode n in this.nodes)
            {
                nodeDict[n.def] = n;
            }
        }

        public void InitializeUpbringing(int inputSeed = 0)
        {
            this.upbringing = Mathf.CeilToInt(Rand.ValueSeeded(this.pawn.HashOffset() + inputSeed) * PersonalityCategories);
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

        [LogPerformance]
        public float GetPersonalityRating(PersonalityNodeDef def)
        {
            return nodeDict[def].AdjustedRating;
        }

        public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def)
        {
            return nodeDict[def];
        }

        [LogPerformance]
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
                //    disagree = Mathf.Clamp01(1f / (knownDisagreements / 50)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
                //}
                float disagree = Mathf.Lerp(1f, 0.25f, 0.1f * knownDisagreements * this.GetPersonalityRating(PersonalityNodeDefOf.Polite) - 0.25f);
                cachedDisagreementWeights[disagreementKey] = disagree;
                recalcNodeDisagreement[disagreementKey] = false;
                weight *= disagree;
            }
            return weight;
        }

        [LogPerformance]
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

        public void ConstructRawDisplacementList()
        {
            //rawNormalDisplacementList = new float[PersonalityNodeParentMatrix.defList.Count()];
            rawNormalDisplacementList = new float[37];

            foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
            {
                int index = PersonalityNodeParentMatrix.indexDict[def];
                float rawRating = nodeDict[def].rawRating;
                rawRating = nodeDict[def].AdjustForCircumstance(rawRating, true);
                //float displacement = Mathf.Clamp(2f * rawRating - 1f, -0.99999f, 0.99999f);
                //rawNormalDisplacementList[index] = (float)Special.Ierf(displacement);
                rawNormalDisplacementList[index] = PsycheHelper.NormalCDFInv(rawRating);
            }
        }

        [LogPerformance]
        public void CalculateAdjustedRatings(bool calcRaw = true)
        {
            if (calcRaw || rawNormalDisplacementList == null)
            {
                ConstructRawDisplacementList();
            }
            //float[] adjNormalDisplacementList = PersonalityNodeParentMatrix.parentTransformMatrix.Dot(rawNormalDisplacementList);
            float[] adjNormalDisplacementList = PersonalityNodeParentMatrix.MatrixVectorProduct(PersonalityNodeParentMatrix.parentTransformMatrix, rawNormalDisplacementList, 37);
            //parentAdjRatingDict.Clear();
            foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
            {
                int index = PersonalityNodeParentMatrix.indexDict[def];
                float adjustedRating = PsycheHelper.NormalCDF(adjNormalDisplacementList[index]);
                adjustedRating = nodeDict[def].AdjustForCircumstance(adjustedRating, true);
                adjustedRating = nodeDict[def].AdjustHook(adjustedRating);
                //parentAdjRatingDict.Add(def, adjustedRating);
                nodeDict[def].cachedRating = adjustedRating;
            }
        }
    }
}
