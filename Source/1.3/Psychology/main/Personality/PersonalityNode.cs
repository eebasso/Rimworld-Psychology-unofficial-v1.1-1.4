using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology;

public class PersonalityNode : IExposable
{
    public Pawn pawn;
    public PersonalityNodeDef def;
    public float rawRating = -1f;
    public float AdjustedRating = -1f;

    public PersonalityNode()
    {
    }

    public PersonalityNode(Pawn pawn, PersonalityNodeDef def)
    {
        this.pawn = pawn;
        this.def = def;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref this.def, "def");
        Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
    }

    public float AdjustForCircumstance(float rating, bool applyingTwice = false)
    {
        // gM = gender modifier
        float gM = this.pawn.gender == Gender.Female ? this.def.femaleModifier : -this.def.femaleModifier;
        // Homosexual pawns are more likely to ignore gender norms
        gM *= PsychologySettings.enableKinsey ? 1f - PsycheHelper.Comp(this.pawn).Sexuality.kinseyRating / 6f : this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
        // This is a rough approximation but it works for the gM of interest
        gM *= applyingTwice ? 0.5f : 1f;
        if (Mathf.Abs(gM) > 0.001f)
        {
            float gMm1 = gM - 1f;
            rating = (gMm1 + Mathf.Sqrt(gMm1 * gMm1 + 4f * gM * rating)) / (2f * gM);
        }
        // When gM < 0.001, it is always 0 in practice, so this can be ignored
        //else
        //{
        //    rating += rating * (1f - rating) * gM; // + O(gm^2) corrections that are negligible
        //}

        // tM = total modifier
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
        return this.pawn.GetHashCode() + this.def.defName.GetHashCode();
    }

    public bool HasPlatformIssue
    {
        get
        {
            return this.def.platformIssueHigh != null;
        }
    }

    public bool HasConvoTopics
    {
        get
        {
            if (this.def.conversationTopics != null)
            {
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
