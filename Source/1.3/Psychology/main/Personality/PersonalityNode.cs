using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology
{
    public class PersonalityNode : IExposable
    {
        public Pawn pawn;
        public PersonalityNodeDef def;
        public float rawRating;
        public float cachedRating = -1f;
        private HashSet<PersonalityNode> parents;
        private readonly static List<string> CoreDefNames = new List<string>() { "Extroverted", "Confident", "Emotional", "Experimental" };

        public PersonalityNode()
        {
        }

        public PersonalityNode(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize()
        {
            if (this.Core && this.def.defName != "Intelligent")
            {
                /* "Core" nodes are seeded based on a pawn's upbringing, separating pawns into 16 categories, similar to the Meyers-Brigg test.
                 * Two pawns with the same upbringing will always have the same core personality ratings.
                 * Pawns will never have conversations about core nodes, they exist only to influence child nodes.
                 */
                //int defSeed = this.def.defName.GetHashCode();
                //this.rawRating = Rand.ValueSeeded(this.pawn.GetComp<CompPsychology>().Psyche.upbringing + defSeed + Find.World.info.Seed);
                int upbringingInt = this.pawn.GetComp<CompPsychology>().Psyche.upbringing;
                int upbringingDefBit = PsycheHelper.Mod(Mathf.FloorToInt(Mathf.Pow(2f, -CoreDefNames.IndexOf(this.def.defName)) * upbringingInt), 2);
                int defSeed = this.def.defName.GetHashCode();
                float rating = 0.375f * Rand.ValueSeeded(this.pawn.HashOffset() + defSeed) + 0.125f * Rand.ValueSeeded(Find.World.info.Seed + defSeed);
                this.rawRating = Mathf.Clamp01(upbringingDefBit == 1 ? rating : 1f - rating);
            }
            else
            {
                this.rawRating = Rand.Value;
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.def, "def");
            Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
        }

        //[LogPerformance]
        //public float AdjustForParents(float rating)
        //{
        //    foreach (PersonalityNode parent in this.ParentNodes)
        //    {
        //        float parentRating = (def.GetModifier(parent.def) < 0 ? (1f - parent.AdjustedRating) : parent.AdjustedRating) * Mathf.Abs(def.GetModifier(parent.def));
        //        rating = (rating * (2f - Mathf.Abs(def.GetModifier(parent.def))) + parentRating) / 2f;
        //    }
        //    return Mathf.Clamp01(rating);
        //}

        [LogPerformance]
        public float AdjustForParents(float rating)
        {
            //Log.Message("Inside AdjustForParents");
            List<float> m = new List<float> { 1f };
            float num = m[0] * (rating - 0.5f);
            float den = Mathf.Abs(m[0]);
            foreach (PersonalityNode parent in this.ParentNodes)
            {
                //Log.Message("Inside foreach of " + this.def.label + " for parent = " + parent.def.label);
                float M = def.GetModifier(parent.def);
                //Log.Message("Getting adjusted rating");
                num += M * (parent.AdjustedRating - 0.5f);
                den += Mathf.Abs(M);
                m.Add(M);
            }
            //foreach (KeyValuePair<PersonalityNodeDef, PersonalityNodeParent> kvp in this.def.ParentNodes)
            //{
            //    PersonalityNode parent = kvp.Key;
            //    Log.Message("Inside foreach of " + this.def.label + "for parent = " + parent.def.label);
            //    float M = def.GetModifier(parent.def);
            //    Log.Message("Getting adjusted rating");
            //    num += M * (parent.AdjustedRating - 0.5f);
            //    den += Mathf.Abs(M);
            //    m.Add(M);
            //}
            //Log.Message(this.def.label + " has " + m.Count.ToString() + " parents.");
            float zi = num / den;
            float zf = zi;
            //Log.Message("Before if loops");
            if (m.Count - 1 == 0)
            {
                //Log.Message("m.Count == 0 start");
                return rating;
            }
            if (m.Count - 1 == 1)
            {
                //Log.Message("m.Count == 1 start");
                zf = AdjustForOneParent(zi, m[1] / m[0]);
            }
            if (m.Count - 1 == 2)
            {
                //Log.Message("m.Count == 2 start");
                zf = AdjustForTwoParents(zi, m[1] / m[0], m[2] / m[0]);
            }
            if (m.Count - 1 == 3)
            {
                //Log.Message("m.Count == 3 start");
                zf = AdjustForThreeParents(zi, m[1] / m[0], m[2] / m[0], m[3] / m[0]);
            }
            Log.Message("Reached end of AdjustForParents for " + this.def.label + "rating changed from " + rating.ToString() + " to " + (zf + 0.5f).ToString());
            return Mathf.Clamp01(zf + 0.5f);
        }

        [LogPerformance]
        public float AdjustForCircumstance(float rating)
        {
            if (this.def.traitModifiers != null && this.def.traitModifiers.Any())
            {
                foreach (PersonalityNodeTraitModifier traitMod in this.def.traitModifiers)
                {
                    if (this.pawn.story.traits.HasTrait(traitMod.trait) && this.pawn.story.traits.DegreeOfTrait(traitMod.trait) == traitMod.degree)
                    {
                        rating += traitMod.modifier;
                    }
                }
                //rating = Mathf.Clamp01(rating);
            }
            if (this.def.skillModifiers != null && this.def.skillModifiers.Any())
            {
                int totalLearning = 0;
                foreach (SkillRecord s in this.pawn.skills.skills)
                {
                    totalLearning += s.Level;
                }
                int skillWeight = 0;
                foreach (PersonalityNodeSkillModifier skillMod in this.def.skillModifiers)
                {
                    skillWeight += this.pawn.skills.GetSkill(skillMod.skill).Level;
                }
                if (totalLearning > 0)
                {
                    float totalWeight = skillWeight / totalLearning;
                    rating += Mathf.InverseLerp(.05f, .4f, totalWeight);
                    //rating = Mathf.Clamp01(rating);
                }
            }
            if (this.def.incapableModifiers != null && this.def.incapableModifiers.Any())
            {
                foreach (PersonalityNodeIncapableModifier incapableMod in this.def.incapableModifiers)
                {
                    if (this.pawn.WorkTypeIsDisabled(incapableMod.type))
                    {
                        rating += incapableMod.modifier;
                    }
                }
                //rating = Mathf.Clamp01(rating);
            }
            if (this.def == PersonalityNodeDefOf.Cool && RelationsUtility.IsDisfigured(this.pawn))
            {
                //rating = Mathf.Clamp01(rating - 0.1f);
                rating += -0.1f;
            }
            //return rating;
            return Mathf.Clamp01(rating);
        }

        [LogPerformance]
        public float AdjustGender(float rating)
        {
            if (this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female && PsychologyBase.ActivateKinsey())
            {
                rating = Rand.ValueSeeded(pawn.HashOffset()) < 0.8f ? rating * Mathf.Lerp(this.def.femaleModifier, 1f, this.pawn.GetComp<CompPsychology>().Sexuality.kinseyRating / 6) : rating;
            }
            else if (this.def.femaleModifier > 0f && this.pawn.gender == Gender.Female)
            {
                rating = this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? rating : rating * this.def.femaleModifier;
            }
            //return rating;
            return Mathf.Clamp01(rating);
        }

        public bool Core
        {
            get
            {
                return this.def.ParentNodes == null || !this.def.ParentNodes.Any();
            }
        }

        public HashSet<PersonalityNode> ParentNodes
        {
            [LogPerformance]
            get
            {
                //Log.Message("Inside PersonalityNode.ParentNodes for " + this.def.label);
                if (this.parents == null || this.pawn.IsHashIntervalTick(500))
                {
                    //Log.Message("Parents were null for node " + this.def.label);
                    this.parents = new HashSet<PersonalityNode>();
                    //Log.Message("Created new HashSet for parents for " + this.def.label);
                    if (this.def.ParentNodes != null && this.def.ParentNodes.Any())
                    {
                        //Log.Message("Actually populating parents for " + this.def.label);
                        this.parents.AddRange(this.pawn.GetComp<CompPsychology>().Psyche.PersonalityNodes.Where(p => this.def.ParentNodes.ContainsKey(p.def)));
                        //Log.Message("Finished populating parents for " + this.def.label);
                    }
                    //Log.Message("Past inner if loop of PersonalityNode.ParentNodes for " + this.def.label);
                }
                return this.parents;
            }
        }

        //public HashSet<PersonalityNode> ParentNodes
        //{
        //    [LogPerformance]
        //    get
        //    {
        //        Log.Message("Inside PersonalityNode.ParentNodes for " + this.def.label);
        //        if (this.parents == null || this.pawn.IsHashIntervalTick(500))
        //        {
        //            Log.Message("Parents were null for node " + this.def.label);
        //            this.parents = new HashSet<PersonalityNode>();
        //            Log.Message("Created new HashSet for parents for " + this.def.label);
        //            //bool bool1 = this.def.ParentNodes != null;
        //            //Log.Message("this.def.ParentNodes != null is " + bool1.ToString());
        //            //bool bool2 = this.def.ParentNodes.Any();
        //            //Log.Message("this.def.ParentNodes.Any() is " + bool2.ToString());
        //            if (this.def.ParentNodes != null && this.def.ParentNodes.Any())
        //            {
        //                Log.Message("Actually populating parents for " + this.def.label);
        //                this.parents = (from p in this.pawn.GetComp<CompPsychology>().Psyche.PersonalityNodes
        //                                where this.def.ParentNodes.ContainsKey(p.def)
        //                                select p) as HashSet<PersonalityNode>;
        //                Log.Message("Finished populating parents for " + this.def.label);
        //            }
        //            Log.Message("Past inner if loop of PersonalityNode.ParentNodes for " + this.def.label);
        //        }
        //        //Log.Message("Obtaining this.def.label");
        //        //string string1 = this.def.label;
        //        //Log.Message("Obtaining this.parents.Count.ToString()");
        //        //string string2 = this.parents.Count.ToString();
        //        //Log.Message("Finished PersonalityNode.ParentNodes for " + string1 + " with parent count = " + string2);
        //        return this.parents;
        //    }
        //}

        public string PlatformIssue
        {
            get
            {
                if (this.AdjustedRating >= 0.5f)
                {
                    return this.def.platformIssueHigh;
                }
                else
                {
                    return this.def.platformIssueLow;
                }
            }
        }

        /* Hook for modding. */
        public float AdjustHook(float rating)
        {
            return rating;
        }

        public float AdjustedRating
        {
            [LogPerformance]
            get
            {
                //Log.Message("Inside adjusted rating for " + this.def.label);
                //Log.Message("cachedRating < 0f is " + (cachedRating < 0f).ToString());
                //Log.Message("this.pawn.IsHashIntervalTick(100) is " + this.pawn.IsHashIntervalTick(100).ToString());
                if (cachedRating < 0f || this.pawn.IsHashIntervalTick(100))
                {
                    //float adjustedRating = AdjustForCircumstance(this.rawRating);
                    //adjustedRating = AdjustHook(adjustedRating);
                    //adjustedRating = AdjustGender(adjustedRating);
                    //cachedRating = AdjustForParents(adjustedRating);
                    //adjustedRating = AdjustForParents(adjustedRating);
                    //adjustedRating = ((3 * adjustedRating) + rawRating) / 4f; //Prevent it from being adjusted too strongly
                    //cachedRating = Mathf.Clamp01(adjustedRating);
                    //Log.Message(this.def.label + ": raw = " + this.rawRating.ToString() + ", adjusted = " + cachedRating.ToString());
                    float adjustedRating = AdjustForParents(this.rawRating);
                    adjustedRating = AdjustForCircumstance(adjustedRating);
                    adjustedRating = AdjustGender(adjustedRating);
                    cachedRating = AdjustHook(adjustedRating);
                }
                //Log.Message(this.def.label + ": raw = " + this.rawRating.ToString() + ", adjusted = " + cachedRating);
                return cachedRating;
            }
        }

        public override int GetHashCode()
        {
            return this.def.defName.GetHashCode();
        }

        public float AdjustForOneParent(float zi, float mi)
        {
            //Log.Message("Start AdjustForOneParent for " + this.def.label);
            float z = Mathf.Abs(zi);
            float m = Mathf.Abs(mi);
            float zm = 0.5f * (m - 1) / (m + 1);
            float Fout = z;
            Fout += (zm < z) ? -0.5f * (m + 1) * Mathf.Pow(z - zm, 2f) : 0f;
            Fout += (z < -zm) ? 0.5f * (m + 1) * Mathf.Pow(z + zm, 2f) : 0f;
            Fout *= (m + 1) / m;
            //Log.Message("End of AdjustForOneParent for " + this.def.label);
            //return Mathf.Clamp(Mathf.Sign(zi) * Fout, -0.5f, 0.5f);
            if ((Fout < 0f || 0.5f < Fout) && z <= 0.5f)
            {
                Log.Message("AdjustForOneParent has an error in its formulas for modifier " + m);
            }
            return Mathf.Sign(zi) * Fout;
        }

        public float AdjustForTwoParents(float zi, float m1, float m2)
        {
            float z = Mathf.Abs(zi);
            int m1inv0 = (m1 == 0f) ? 1 : Mathf.RoundToInt(1f / Mathf.Abs(m1));
            int m2inv0 = (m2 == 0f) ? 1 : Mathf.RoundToInt(1f / Mathf.Abs(m2));
            int m1inv = Mathf.Min(m1inv0, m2inv0);
            int m2inv = Mathf.Max(m1inv0, m2inv0);
            float Fout;
            if (m1inv == 1 && m2inv == 1)
            {
                Fout = 9f / 4f * z - 9f * Mathf.Pow(z, 3f);
                Fout += (z > 1f / 6f) ? 1f / 16f * Mathf.Pow(-1f + 6f * z, 3f) : 0f;
            }
            else if (m1inv == 1 && m2inv == 2)
            {
                Fout = 420f * z - 2000f * Mathf.Pow(z, 3f);
                Fout += (z > 3f / 10f) ? Mathf.Pow(-3f + 10f * z, 3f) : 0f;
                Fout += (z > 1f / 10f) ? 2f * Mathf.Pow(-1f + 10f * z, 3f) : 0f;
                Fout /= 192f;
            }
            else if (m1inv == 1 && m2inv == 3)
            {
                Fout = 28f * z * (33f - 196f * z * z);
                Fout += (z > 5f / 14f) ? Mathf.Pow(-5f + 14f * z, 3f) : 0f;
                Fout += (z > 1f / 14f) ? 2f * Mathf.Pow(-1f + 14f * z, 3f) : 0f;
                Fout /= 432f;
            }
            else if (m1inv == 2 && m2inv == 2)
            {
                Fout = 2f * z - 16f / 3f * Mathf.Pow(z, 3f);
                Fout += (z > 1f / 4f) ? 1f / 6f * Mathf.Pow(-1f + 4f * z, 3f) : 0f;
            }
            else if (m1inv == 2 && m2inv == 3)
            {
                Fout = 3168f * z;
                Fout += (z > 7f / 22f) ? Mathf.Pow(-7f + 22f * z, 3f) : 0f;
                Fout += (z > 5f / 22f) ? Mathf.Pow(-5f + 22f * z, 3f) : 0f;
                Fout += (z > 1f / 22f) ? -Mathf.Pow(-1f + 22f * z, 3f) : 0f;
                Fout /= 1728f;
            }
            else if (m1inv == 3 && m2inv == 3)
            {
                Fout = 240f * z;
                Fout += (z > 3f / 10f) ? 2f * Mathf.Pow(-3f + 10f * z, 3f) : 0f;
                Fout += (z > 1f / 10f) ? -Mathf.Pow(-1f + 10f * z, 3f) : 0f;
                Fout /= 144f;
            }
            else
            {
                Log.Message("Using AdjustForMoreParentsApprox");
                Fout = AdjustForMoreParentsApprox(z, new List<float>() { m1, m2 });
            }
            if ((Fout < 0f || 0.5f < Fout) && z <= 0.5f)
            {
                Log.Message("AdjustForTwoParents has an error in its formulas for modifiers " + m1inv + ", " + m2inv);
            }
            return Mathf.Sign(zi) * Fout;
        }

        public float AdjustForThreeParents(float zi, float m1, float m2, float m3)
        {
            float z = Mathf.Abs(zi);
            List<int> mvinv = new List<int>();
            mvinv.Add((m1 == 0f) ? 1 : Mathf.RoundToInt(1f / Mathf.Abs(m1)));
            mvinv.Add((m2 == 0f) ? 1 : Mathf.RoundToInt(1f / Mathf.Abs(m2)));
            mvinv.Add((m3 == 0f) ? 1 : Mathf.RoundToInt(1f / Mathf.Abs(m3)));
            mvinv.Sort();
            int m1inv = mvinv[0];
            int m2inv = mvinv[1];
            int m3inv = mvinv[2];
            float Fout;
            if (m1inv == 1 && m2inv == 1 && m3inv == 1)
            {
                Fout = 8f / 3f * (z - 8f * Mathf.Pow(z, 3f) + 12f * Mathf.Pow(z, 4f));
                Fout += (z > 1f / 4f) ? -1f / 6f * Mathf.Pow(1f - 4f * z, 4f) : 0f;
            }
            else if (m1inv == 1 && m2inv == 1 && m3inv == 2)
            {
                Fout = -1568f * z * (-5f + 28f * Mathf.Pow(z, 2f));
                Fout += (z > 5f / 14f) ? -Mathf.Pow(5f - 14f * z, 4f) : 0f;
                Fout += (z > 3f / 14f) ? -3f * Mathf.Pow(3f - 14f * z, 4f) : 0f;
                Fout += (z > 1f / 14f) ? 3f * Mathf.Pow(1f - 14f * z, 4f) : 0f;
                Fout /= 3072f;
            }
            else if (m1inv == 1 && m2inv == 1 && m3inv == 3)
            {
                Fout = -1600f * z * (-1f + 5f * Mathf.Pow(z, 2f));
                Fout += (z > 2f / 5f) ? -16f * Mathf.Pow(2f - 5f * z, 4f) : 0f;
                Fout += (z > 1f / 5f) ? -48f * Mathf.Pow(1f - 5f * z, 4f) : 0f;
                Fout += (z > 1f / 10f) ? 3f * Mathf.Pow(1f - 10f * z, 4f) : 0f;
                Fout /= 648f;
            }
            else if (m1inv == 1 && m2inv == 2 && m3inv == 2)
            {
                Fout = 5f / 2f * z - 18f * Mathf.Pow(z, 3f) + 27f * Mathf.Pow(z, 4f);
                Fout += (z > 1f / 3f) ? -1f / 3f * Mathf.Pow(1f - 3f * z, 4f) : 0f;
                Fout += (z > 1f / 6f) ? -1f / 96f * Mathf.Pow(1f - 6f * z, 4f) : 0f;
            }
            else if (m1inv == 1 && m2inv == 2 && m3inv == 3)
            {
                Fout = -1088f * z * (-185f + 1156f * Mathf.Pow(z, 2f));
                Fout += (z > 13f / 34f) ? -Mathf.Pow(13f - 34f * z, 4f) : 0f;
                Fout += (z > 11f / 34f) ? -Mathf.Pow(11f - 34f * z, 4f) : 0f;
                Fout += (z > 7f / 34f) ? Mathf.Pow(7f - 34f * z, 4f) : 0f;
                Fout += (z > 5f / 34f) ? -2f * Mathf.Pow(5f - 34f * z, 4f) : 0f;
                Fout += (z > 1f / 34f) ? 2f * Mathf.Pow(1f - 34f * z, 4f) : 0f;
                Fout /= 82944f;
            }
            else if (m1inv == 1 && m2inv == 3 && m3inv == 3)
            {
                Fout = 256f * (z - 8f * Mathf.Pow(z, 3f) + 16f * Mathf.Pow(z, 4f));
                Fout += (z > 3f / 8f) ? -Mathf.Pow(3f - 8f * z, 4f) : 0f;
                Fout += (z > 1f / 4f) ? 8f * Mathf.Pow(1f - 4f * z, 4f) : 0f;
                Fout += (z > 1f / 8f) ? -Mathf.Pow(1f - 8f * z, 4f) : 0f;
                Fout /= 108f;
            }
            else if (m1inv == 2 && m2inv == 2 && m3inv == 2)
            {
                Fout = -80f * z * (-23f + 100f * Mathf.Pow(z, 2f));
                Fout += (z > 3f / 10f) ? -3f * Mathf.Pow(3f - 10f * z, 4f) : 0f;
                Fout += (z > 1f / 10f) ? 2f * Mathf.Pow(1f - 10f * z, 4f) : 0f;
                Fout /= 768f;
            }
            else if (m1inv == 2 && m2inv == 2 && m3inv == 3)
            {
                Fout = -112f * z * (-53f + 196f * Mathf.Pow(z, 2f));
                Fout += (z > 5f / 14f) ? -Mathf.Pow(5f - 14f * z, 4f) : 0f;
                Fout += (z > 2f / 7f) ? -32f * Mathf.Pow(2f - 7f * z, 4f) : 0f;
                Fout += (z > 1f / 7f) ? 32f * Mathf.Pow(1f - 7f * z, 4f) : 0f;
                Fout /= 2592f;
            }
            else if (m1inv == 2 && m2inv == 3 && m3inv == 3)
            {
                Fout = -208f * z * (-287f + 676f * Mathf.Pow(z, 2f));
                Fout += (z > 9f / 26f) ? -2f * Mathf.Pow(9f - 26f * z, 4f) : 0f;
                Fout += (z > 7f / 26f) ? -Mathf.Pow(7f - 26f * z, 4f) : 0f;
                Fout += (z > 5f / 26f) ? Mathf.Pow(5f - 26f * z, 4f) : 0f;
                Fout += (z > 3f / 26f) ? 2f * Mathf.Pow(3f - 26f * z, 4f) : 0f;
                Fout += (z > 1f / 26f) ? -Mathf.Pow(1f - 26f * z, 4f) : 0f;
                Fout /= 27648f;
            }
            else if (m1inv == 3 && m2inv == 3 && m3inv == 3)
            {
                Fout = 2f * z - 18f * Mathf.Pow(z, 4f);
                Fout += (z > 1f / 3f) ? -2f / 3f * Mathf.Pow(1f - 3f * z, 4f) : 0f;
                Fout += (z > 1f / 6f) ? 1f / 24f * Mathf.Pow(1f - 6f * z, 4f) : 0f;
            }
            else
            {
                Log.Message("Using AdjustForMoreParentsApprox");
                Fout = AdjustForMoreParentsApprox(z, new List<float>() { m1, m2, m3 });
            }
            if ((Fout < 0f || 0.5f < Fout) && z <= 0.5f)
            {
                Log.Message("AdjustForThreeParents has an error in its formulas for modifiers " + m1inv + ", " + m2inv + ", " + m3inv);
            }
            //return Mathf.Clamp(Mathf.Sign(zi) * Fout, -0.5f, 0.5f);
            return Mathf.Sign(zi) * Fout;
        }

        public float AdjustForMoreParentsApprox(float z, List<float> mlist)
        {
            float top = 1f;
            float bot = 1f;
            foreach (float m in mlist)
            {
                top += m * m;
                bot += Mathf.Abs(m);
            }
            float sigma = Mathf.Sqrt(top / 6f) / bot;
            return 0.5f * ErfApprox(z / sigma) / ErfApprox(0.5f / sigma);
        }

        public static float ErfApprox(float x)
        {
            // constants
            float a1 = 0.254829592f;
            float a2 = -0.284496736f;
            float a3 = 1.421413741f;
            float a4 = -1.453152027f;
            float a5 = 1.061405429f;
            float p = 0.3275911f;
            // A&S formula 7.1.26
            float t = 1f / (1f + p * Mathf.Abs(x));
            float y = 1f - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(-x * x);
            return Mathf.Sign(x) * y;
        }
    }
}
