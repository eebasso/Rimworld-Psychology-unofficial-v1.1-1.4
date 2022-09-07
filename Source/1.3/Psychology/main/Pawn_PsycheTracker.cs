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

namespace Psychology
{
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
        //public const int PersonalityCategories = 32;
        public int AdjustedRatingTicker = -1;
        public float[] ProjectedRawRatings;
        public bool needToCalcProjectedRaw = true;

        public Pawn_PsycheTracker(Pawn pawn)
        {
            this.pawn = pawn;
            Initialize();
        }

        //[LogPerformance]
        public void Initialize(int inputSeed = 0)
        {
            this.nodes = new HashSet<PersonalityNode>();
            foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
            {
                nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
            }
            RandomizeUpbringingAndRatings(inputSeed);
            foreach (PersonalityNode n in this.nodes)
            {
                nodeDict[n.def] = n;
            }
        }

        //public void RandomizeUpbringingAndRatings(int inputSeed = 0)
        //{
        //    int pawnSeed = this.pawn.story.childhood.GetHashCode() + this.pawn.story.birthLastName.GetHashCode();
        //    this.upbringing = Mathf.CeilToInt(Rand.ValueSeeded(pawnSeed + inputSeed) * PersonalityCategories);
        //    float[] ratingList = new float[PersonalityNodeParentMatrix.defList.Count()];
        //    foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        //    {
        //        int defSeed = def.GetHashCode();
        //        int index = PersonalityNodeParentMatrix.indexDict[def];
        //        ratingList[index] = Rand.ValueSeeded((2 + index) * pawnSeed + defSeed + (3 + index) * inputSeed);
        //    }

        //    // ratingList = PersonalityNodeParentMatrix.ApplyUpbringingProjection(ratingList, upbringing);
        //    foreach (PersonalityNode node in nodes)
        //    {
        //        int index = PersonalityNodeParentMatrix.indexDict[node.def];
        //        node.rawRating = ratingList[index];
        //    }
        //    AdjustedRatingTicker = -1;
        //}

        public void RandomizeUpbringingAndRatings(int inputSeed = 0)
        {
            int pawnSeed = PsycheHelper.PawnSeed(this.pawn);
            //this.upbringing = Mathf.CeilToInt(Rand.ValueSeeded(pawnSeed + inputSeed) * PersonalityCategories);
            float[] ratingList = new float[PersonalityNodeParentMatrix.defList.Count()];
            foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
            {
                int defSeed = def.GetHashCode();
                int index = PersonalityNodeParentMatrix.indexDict[def];
                ratingList[index] = Rand.ValueSeeded((2 + index) * pawnSeed + defSeed + (3 + index) * inputSeed);
            }
            foreach (PersonalityNode node in nodes)
            {
                int index = PersonalityNodeParentMatrix.indexDict[node.def];
                node.rawRating = ratingList[index];
            }
            needToCalcProjectedRaw = true;
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

        //[LogPerformance]
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
                //    disagree = Mathf.Clamp01(1f / (knownDisagreements / 50)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
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

        //public void ConstructRawDisplacementList()
        //{
        //    //rawNormalDisplacementList = new float[PersonalityNodeParentMatrix.defList.Count()];
        //    foreach (PersonalityNodeDef def in PersonalityNodeParentMatrix.defList)
        //    {
        //        int index = PersonalityNodeParentMatrix.indexDict[def];
        //        float rawRating = nodeDict[def].rawRating;
        //        rawRating = nodeDict[def].AdjustForCircumstance(rawRating, true);
        //        rawNormalDisplacementList[index] = PsycheHelper.NormalCDFInv(rawRating);
        //    }
        //}

        //[LogPerformance]
        public void CalculateAdjustedRatings()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (needToCalcProjectedRaw)
            {
                float[] rawRatingList = new float[PersonalityNodeParentMatrix.order];
                foreach (PersonalityNode node in this.nodes)
                {
                    rawRatingList[PersonalityNodeParentMatrix.indexDict[node.def]] = node.rawRating;
                }
                /* Pawns are separated into 2^5 = 32 categories based on the five factor model. */
                /* Two pawns with the same upbringing should always have similar personality ratings. */
                ProjectedRawRatings = PersonalityNodeParentMatrix.ApplyUpbringingProjection(rawRatingList, upbringing);
                needToCalcProjectedRaw = false;
            }
            int index;
            float adjustedRating;
            float[] adjustedRatingList = new float[PersonalityNodeParentMatrix.order];
            foreach (PersonalityNode node in this.nodes)
            {
                index = PersonalityNodeParentMatrix.indexDict[node.def];
                adjustedRating = node.AdjustForCircumstance(ProjectedRawRatings[index], true);
                adjustedRatingList[index] = PsycheHelper.NormalCDFInv(adjustedRating);
            }
            adjustedRatingList = PersonalityNodeParentMatrix.MatrixVectorProduct(PersonalityNodeParentMatrix.parentTransformMatrix, adjustedRatingList);
            foreach (PersonalityNode node in this.nodes)
            {
                index = PersonalityNodeParentMatrix.indexDict[node.def];
                adjustedRating = PsycheHelper.NormalCDF(adjustedRatingList[index]);
                adjustedRating = node.AdjustForCircumstance(adjustedRating, true);
                adjustedRating = node.AdjustHook(adjustedRating);
                nodeDict[node.def].cachedRating = adjustedRating;
            }
            //stopwatch.Stop();
            //TimeSpan ts = stopwatch.Elapsed;
            //Log.Message("CalculateAdjustedRatings took " + ts.TotalMilliseconds + " ms.");
            AdjustedRatingTicker = 100;
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
}


