using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Diagnostics;

namespace Psychology;

public class PersonalityNode : IExposable
{
    public Pawn pawn;
    public PersonalityNodeDef def;
    public float rawRating;
    private float cachedRating = -1f;

    public bool HasConvoTopics => this.def.conversationTopics.NullOrEmpty() != true;
    public bool HasPlatformIssue => this.def.platformIssueHigh != null && this.def.platformIssueLow != null;
    public string PlatformIssue => this.AdjustedRating < 0.5f ? this.def.platformIssueLow : this.def.platformIssueHigh;
    public float AdjustedRating
    {
        get
        {
            return cachedRating;
        }
        set
        {
            cachedRating = value;
        }
    }

    public PersonalityNode()
    {
    }

    public PersonalityNode(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref this.def, "def");
        Scribe_Values.Look(ref this.rawRating, "rawRating", -1f, false);
    }

    public override int GetHashCode()
    {
        return this.pawn.GetHashCode() + GenText.StableStringHash(this.def.defName);
    }

    //public float AdjustForCircumstance(float rating, bool applyingTwice = false)
    //{
    //    Stopwatch stopwatch = new Stopwatch();

    //    stopwatch.Start();
    //    float gM = (this.pawn.gender == Gender.Female) ? this.def.femaleModifier : -this.def.femaleModifier;
    //    gM *= PsychologySettings.enableKinsey ? 1f - PsycheHelper.Comp(this.pawn).Sexuality.kinseyRating / 6f : this.pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
    //    if (Mathf.Abs(gM) > 0.001f)
    //    {
    //        float gMm1 = gM - 1f;
    //        rating = (gMm1 + Mathf.Sqrt(gMm1 * gMm1 + 4f * gM * rating)) / (2f * gM);
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[0] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    stopwatch.Start();
    //    float tM = 0f;
    //    if (this.def.traitModifiers != null && this.def.traitModifiers.Any())
    //    {
    //        foreach (PersonalityNodeTraitModifier traitMod in this.def.traitModifiers)
    //        {
    //            //if (this.pawn.story.traits.HasTrait(traitMod.trait) && this.pawn.story.traits.DegreeOfTrait(traitMod.trait) == traitMod.degree)
    //            //{
    //            //    tM = PsycheHelper.RelativisticAddition(tM, traitMod.modifier);
    //            //}
    //            if (this.pawn.story.traits.HasTrait(traitMod.trait, traitMod.degree))
    //            {
    //                tM = PsycheHelper.RelativisticAddition(tM, traitMod.modifier);
    //            }
    //        }
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[1] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    stopwatch.Start();
    //    if (this.def.skillModifiers != null && this.def.skillModifiers.Any())
    //    {
    //        foreach (PersonalityNodeSkillModifier skillMod in this.def.skillModifiers)
    //        {
    //            float skillWeight = Mathf.Lerp(-0.20f, 0.85f, this.pawn.skills.GetSkill(skillMod.skill).Level / 20f);
    //            tM = PsycheHelper.RelativisticAddition(tM, skillWeight);
    //        }
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[2] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    stopwatch.Start();
    //    if (this.def.incapableModifiers != null && this.def.incapableModifiers.Any())
    //    {
    //        stopwatch.Stop();
    //        PsycheHelper.CircumstanceTimings[3] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //        stopwatch.Reset();

    //        foreach (PersonalityNodeIncapableModifier incapableMod in this.def.incapableModifiers)
    //        {
    //            stopwatch.Start();
    //            if (this.pawn.WorkTypeIsDisabled(incapableMod.type))
    //            {
    //                stopwatch.Stop();
    //                PsycheHelper.CircumstanceTimings[4] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //                stopwatch.Reset();

    //                stopwatch.Start();
    //                tM = PsycheHelper.RelativisticAddition(tM, incapableMod.modifier);
    //                stopwatch.Stop();
    //                PsycheHelper.CircumstanceTimings[5] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //                stopwatch.Reset();
    //            }
    //            else
    //            {
    //                stopwatch.Stop();
    //                PsycheHelper.CircumstanceTimings[4] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //                stopwatch.Reset();
    //            }
    //        }
    //    }
    //    else
    //    {
    //        stopwatch.Stop();
    //        PsycheHelper.CircumstanceTimings[3] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //        stopwatch.Reset();
    //    }
        
    //    stopwatch.Start();
    //    if (this.def == PersonalityNodeDefOf.Cool)
    //    {
    //        tM = RelationsUtility.IsDisfigured(this.pawn) ? PsycheHelper.RelativisticAddition(tM, -0.1f) : tM;
    //        tM = PsycheHelper.RelativisticAddition(tM, 0.3f * this.pawn.GetStatValue(StatDefOf.PawnBeauty));
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[6] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    stopwatch.Start();
    //    if (this.def == PersonalityNodeDefOf.LaidBack)
    //    {
    //        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
    //        {
    //            if (hediff.def == HediffDefOfPsychology.Anxiety)
    //            {
    //                float anxietyWeight = Mathf.Lerp(-0.75f, -0.999f, hediff.Severity);
    //                tM = PsycheHelper.RelativisticAddition(tM, anxietyWeight);
    //                break;
    //            }
    //        }
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[7] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    //tM = Mathf.Clamp(tM, -1f, 1f);
    //    stopwatch.Start();
    //    if (applyingTwice)
    //    {
    //        tM = Mathf.Sign(tM) * (1f - Mathf.Sqrt(1f - Mathf.Abs(tM)));
    //    }
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[8] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    stopwatch.Start();
    //    float newRating = (1f - Mathf.Abs(tM)) * rating + Mathf.Max(0f, tM);
    //    stopwatch.Stop();
    //    PsycheHelper.CircumstanceTimings[9] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //    stopwatch.Reset();

    //    return newRating;
    //}


    
}
