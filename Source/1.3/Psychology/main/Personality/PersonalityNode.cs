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
        private byte AdjustedRatingTicker = 0;
        private HashSet<PersonalityNode> parents;
        private readonly static List<string> CoreDefNames = new List<string>() { "Experimental", "Thoughtful", "Extroverted", "Aggressive", "LaidBack" };

        public PersonalityNode()
        {
        }

        public PersonalityNode(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize()
        {
            int[] upbringingSigns = PsycheHelper.GetSignArray(this.pawn.GetComp<CompPsychology>().Psyche.upbringing, 5);
            Log.Message(pawn.LabelShortCap + ": Upbringing = " + this.pawn.GetComp<CompPsychology>().Psyche.upbringing + ": " + string.Join(", ", upbringingSigns));
            string defName = this.def.defName;
            int defSeed = defName.GetHashCode();
            int pawnSeed = this.pawn.HashOffset();
            if (CoreDefNames.Contains(defName))
            {
                /* "Core" nodes are seeded based on a pawn's upbringing, separating pawns into 32 categories based on the Big Five personality model. */
                /* Two pawns with the same upbringing will always have similar core personality ratings. */
                //int worldSeed = Find.World.info.Seed;
                //float displacement = 0.1f + 0.3f * Rand.ValueSeeded(pawnSeed + defSeed) + 0.1f * Rand.ValueSeeded(pawnSeed + defSeed + worldSeed);
                float displacement = 0.5f * Rand.ValueSeeded(2 * pawnSeed + defSeed);
                this.rawRating = 0.5f + upbringingSigns[CoreDefNames.IndexOf(defName)] * displacement;
            }
            else
            {
                //this.rawRating = Rand.Value;
                this.rawRating = Rand.ValueSeeded(2 * pawnSeed + defSeed);
            }
            Log.Message(pawn.LabelShortCap + ": Node = " + defName + " Raw = " + this.rawRating);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.def, "def");
            Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
        }

        public override int GetHashCode()
        {
            return this.def.defName.GetHashCode();
        }

        public bool HasPlatformIssue
        {
            get
            {
                Log.Message("Defname = " + this.def.defName);
                Log.Message("this.def.platformIssueHigh != null: " + (this.def.platformIssueHigh != null).ToString());
                return this.def.platformIssueHigh != null;
            }
        }

        public bool HasConvoTopics
        {
            get
            {
                Log.Message("Defname = " + this.def.defName);
                Log.Message("this.def.conversationTopics != null: " + (this.def.conversationTopics != null).ToString());
                if (this.def.conversationTopics != null)
                {
                    Log.Message("this.def.conversationTopics.Any(): " + this.def.conversationTopics.Any().ToString());
                    return this.def.conversationTopics.Any();
                }
                return false;
            }
        }

        public HashSet<PersonalityNode> ParentNodes
        {
            [LogPerformance]
            get
            {
                if (this.parents == null || this.pawn.IsHashIntervalTick(500))
                {
                    this.parents = new HashSet<PersonalityNode>();
                    if (this.def.ParentNodes != null && this.def.ParentNodes.Any())
                    {
                        this.parents.AddRange(this.pawn.GetComp<CompPsychology>().Psyche.PersonalityNodes.Where(p => this.def.ParentNodes.ContainsKey(p.def)));
                    }
                }
                return this.parents;
            }
        }

        public string PlatformIssue
        {
            get
            {
                return this.AdjustedRating < 0.5f ? this.def.platformIssueLow : this.def.platformIssueHigh;
            }
        }


        public float AdjustedRating
        {
            [LogPerformance]
            get
            {
                //if (cachedRating < 0f || this.pawn.IsHashIntervalTick(123))
                if (cachedRating < 0f || AdjustedRatingTicker == 0)
                {
                    AdjustedRatingTicker = 250;
                    float adjustedRating = AdjustForParents(this.rawRating);
                    adjustedRating = AdjustForCircumstance(adjustedRating);
                    cachedRating = AdjustHook(adjustedRating);
                }
                else
                {
                    AdjustedRatingTicker--;
                    //Log.Message(this.def.defName + " for " + this.pawn?.story?.birthLastName + ": AdjustedRatingTicker = " + AdjustedRatingTicker);
                }
                return cachedRating;
            }
        }

        [LogPerformance]
        public float AdjustForParents(float rating)
        {
            //Log.Message("Inside AdjustForParents");
            List<float> mList = new List<float> { 1f };
            float num = rating - 0.5f;
            float den = 1f;
            foreach (PersonalityNode parent in this.ParentNodes)
            {
                //Log.Message("Inside foreach of " + this.def.label + " for parent = " + parent.def.label);
                float M = def.GetModifier(parent.def);
                //Log.Message("Getting adjusted rating");
                num += M * (parent.AdjustedRating - 0.5f);
                den += Mathf.Abs(M);
                mList.Add(M);
            }
            float rating2 = mList.Count > 1 ? 0.5f + AdjustForNParentsExact(num / den, mList) : rating;
            //Log.Message(pawn.story.birthLastName + ": AdjustForParents for " + this.def.label + ". Rating changed from " + rating.ToString() + " to " + rating2.ToString());
            return rating2;
        }

        public static float AdjustForNParentsExact(float z, List<float> mList)
        {
            int n = mList.Count();
            float mTotal = mList.Sum(x => Mathf.Abs(x));
            float[] y = new float[n];
            float den = 2f;
            int count = 1;
            for (int k = 0; k < n; k++)
            {
                float frac = mList[k] / mTotal;
                y[k] = frac;
                den *= (k + 1) * frac;
                count *= 2;
            }
            float zf = 0f;
            //int num = (int)Mathf.Pow(2, n);
            for (int i = 0; i < count; i++)
            {
                int[] eta = PsycheHelper.GetSignArray(i, n);
                float arg = z;
                float fAddition = 1f;
                for (int k = 0; k < n; k++)
                {
                    fAddition *= eta[k];
                    arg += -0.5f * eta[k] * y[k];
                }
                fAddition *= Mathf.Pow(-arg, n) * Mathf.Sign(arg);
                zf += fAddition;
            }
            zf /= den;
            if (Mathf.Abs(zf) > 0.5f)
            {
                Log.Warning("AdjustForNParentsExact has an error for n = " + n.ToString());
                return 0.5f * Mathf.Sign(zf);
            }
            return zf;
        }

        [LogPerformance]
        public float AdjustForCircumstance(float rating)
        {
            float gM = (this.pawn.gender == Gender.Female) ? this.def.femaleModifier : -this.def.femaleModifier;
            gM *= PsychologyBase.ActivateKinsey() ? 1f - this.pawn.GetComp<CompPsychology>().Sexuality.kinseyRating / 6f : this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
            if (Mathf.Abs(gM) > 0.001f)
            {
                //float kinseyFactor = PsychologyBase.ActivateKinsey() ? 1f - this.pawn.GetComp<CompPsychology>().Sexuality.kinseyRating / 6f : this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
                //float genderWeight = Mathf.Lerp(-0.33f, 1f, Rand.ValueSeeded(5 * pawn.HashOffset() + this.def.GetHashCode())) * kinseyFactor * genderModifier;
                float gMm1 = gM - 1f;
                rating = (gMm1 + Mathf.Sqrt(gMm1 * gMm1 + 4f * gM * rating)) / (2f * gM);
            }

            float tM = 0f;
            if (this.def.traitModifiers != null && this.def.traitModifiers.Any())
            {
                foreach (PersonalityNodeTraitModifier traitMod in this.def.traitModifiers)
                {
                    if (this.pawn.story.traits.HasTrait(traitMod.trait) && this.pawn.story.traits.DegreeOfTrait(traitMod.trait) == traitMod.degree)
                    {
                        tM = RelativisticAddition(tM, traitMod.modifier);
                    }
                }
            }
            if (this.def.skillModifiers != null && this.def.skillModifiers.Any())
            {
                foreach (PersonalityNodeSkillModifier skillMod in this.def.skillModifiers)
                {
                    float skillWeight = Mathf.Lerp(-0.25f, 0.75f, Mathf.Pow(this.pawn.skills.GetSkill(skillMod.skill).Level / 20f, 1.5f));
                    tM = RelativisticAddition(tM, skillWeight);
                }
            }
            if (this.def.incapableModifiers != null && this.def.incapableModifiers.Any())
            {
                foreach (PersonalityNodeIncapableModifier incapableMod in this.def.incapableModifiers)
                {
                    if (this.pawn.WorkTypeIsDisabled(incapableMod.type))
                    {
                        tM = RelativisticAddition(tM, incapableMod.modifier);
                    }
                }
            }
            if (this.def == PersonalityNodeDefOf.Cool && RelationsUtility.IsDisfigured(this.pawn))
            {
                tM = RelativisticAddition(tM, -0.1f);
            }
            //tM = Mathf.Clamp(tM, -1f, 1f);
            return (1f - Mathf.Abs(tM)) * rating + Mathf.Max(0f, tM);
        }

        public float RelativisticAddition(float u, float v)
        {
            return (u + v) / (1f + u * v);
        }

        public bool HasParents
        {
            get
            {
                return this.def.ParentNodes != null && this.def.ParentNodes.Any();
            }
        }

        /* Hook for modding. */
        public float AdjustHook(float rating)
        {
            return rating;
        }

    }
}