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

        public PersonalityNode()
        {
        }

        public PersonalityNode(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Initialize(int inputSeed = 0)
        {
            string defName = this.def.defName;
            int defSeed = defName.GetHashCode();
            int pawnSeed = this.pawn.HashOffset();
            this.rawRating = Rand.ValueSeeded(2 * pawnSeed + defSeed + inputSeed);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref this.def, "def");
            Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
        }

        public float AdjustedRating
        {
            //[LogPerformance]
            get
            {
                PsycheHelper.Comp(pawn).Psyche.AdjustedRatingTicker--;
                if (cachedRating < 0f || PsycheHelper.Comp(pawn).Psyche.AdjustedRatingTicker < 0)
                {
                    PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
                }
                return cachedRating;
            }
        }

        //[LogPerformance]
        public float AdjustForCircumstance(float rating, bool applyingTwice = false)
        {
            float gM = (this.pawn.gender == Gender.Female) ? this.def.femaleModifier : -this.def.femaleModifier;
            gM *= PsychologySettings.enableKinsey ? 1f - this.pawn.GetComp<CompPsychology>().Sexuality.kinseyRating / 6f : this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
            if (Mathf.Abs(gM) > 0.001f)
            {
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
                    float skillWeight = Mathf.Lerp(-0.20f, 0.85f, this.pawn.skills.GetSkill(skillMod.skill).Level / 20f);
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
            if (this.def == PersonalityNodeDefOf.LaidBack)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def == HediffDefOfPsychology.Anxiety)
                    {
                        float anxietyWeight = Mathf.Lerp(-0.75f, -0.999f, hediff.Severity);
                        tM = RelativisticAddition(tM, anxietyWeight);
                        break;
                    }
                }
            }
            //tM = Mathf.Clamp(tM, -1f, 1f);
            if (applyingTwice)
            {
                tM = Mathf.Sign(tM) * (1f - Mathf.Sqrt(1f - Mathf.Abs(tM)));
            }
            return (1f - Mathf.Abs(tM)) * rating + Mathf.Max(0f, tM);
        }

        public float RelativisticAddition(float u, float v)
        {
            return (u + v) / (1f + u * v);
        }

        /* Hook for modding. */
        public float AdjustHook(float rating)
        {
            return rating;
        }

        public override int GetHashCode()
        {
            return this.def.defName.GetHashCode();
        }

        public bool HasPlatformIssue
        {
            get
            {
                //Log.Message("Defname = " + this.def.defName);
                //Log.Message("this.def.platformIssueHigh != null: " + (this.def.platformIssueHigh != null).ToString());
                return this.def.platformIssueHigh != null;
            }
        }

        public bool HasConvoTopics
        {
            get
            {
                //Log.Message("Defname = " + this.def.defName);
                //Log.Message("this.def.conversationTopics != null: " + (this.def.conversationTopics != null).ToString());
                if (this.def.conversationTopics != null)
                {
                    //Log.Message("this.def.conversationTopics.Any(): " + this.def.conversationTopics.Any().ToString());
                    return this.def.conversationTopics.Any();
                }
                return false;
            }
        }

        public string PlatformIssue
        {
            get
            {
                return this.AdjustedRating < 0.5f ? this.def.platformIssueLow : this.def.platformIssueHigh;
            }
        }

    }
}