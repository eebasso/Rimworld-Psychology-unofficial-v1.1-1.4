using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Diagnostics;
using UnityEngine;

namespace Psychology;

public class Recipe_Treatment : Recipe_Surgery
{
    public TreatmentRecipeDef TreatmentRecipe => recipe as TreatmentRecipeDef;
    public List<Treatment> Treatments => TreatmentRecipe == null ? null : TreatmentRecipe.treatments;

    //public static SimpleCurve DiminishingReturnsCurve = new SimpleCurve
    //{
    //    new CurvePoint(0.00f, 0.00f),
    //    new CurvePoint(0.60f, 0.60f),
    //    new CurvePoint(1.00f, 0.90f),
    //    new CurvePoint(1.60f, 0.98f),
    //    //new CurvePoint(2.60f, 1.00f)
    //};

    public bool CheckTreatmentFail(Pawn surgeon, Pawn patient)
    {
        Treatment treatment = GetAppropriateTreatment(patient);
        if (treatment == null)
        {
            Log.Error("Psychology: CheckTreatmentFail, treatment was null");
            return true;
        }
        float num = 1f;
        float num2 = surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance, true);
        float num3 = surgeon.GetStatValue(StatDefOf.SocialImpact, true);
        float num4 = patient.needs.comfort.CurLevel;
        num *= Mathf.Min(2f * num2, 1f);
        num *= num3;
        num *= num4;
        num *= treatment.easeOfTreatment;
        //num = DiminishingReturnsCurve.Evaluate(num);
        num = 1f - Mathf.Exp(-num);
        return num < Rand.Value;
        //if (Rand.Value > num)
        //{
        //    return true;
        //}
        //return false;
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        Treatment treatment = GetAppropriateTreatment(pawn);
        if (treatment == null)
        {
            Log.Error("Recipe_Treatment.ApplyOnPawn, treatmentDef is null");
            return;
        }
        HediffDef hediffDef = TreatmentRecipe.hediff;
        string illnessName = TreatmentRecipe.illnessName;

        if (CheckTreatmentFail(billDoer, pawn) == false)
        {
            TaleRecorder.RecordTale(TreatmentRecipe.tale, new object[]
            {
                billDoer,
                pawn
            });
            if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
            {
                Messages.Message("TreatedTrait".Translate(pawn, illnessName), pawn, MessageTypeDefOf.PositiveEvent);
            }
            Hediff recover = HediffMaker.MakeHediff(hediffDef, pawn, pawn.health.hediffSet.GetBrain());
            recover.Tended(1f, 1f); // TEST IN 1.3
            pawn.health.AddHediff(recover);
            return;
        }
        ThoughtDef failure = ThoughtDefOfPsychology.TreatmentFailed;
        // Added this
        // failure.stages.First().label = string.Format(failure.stages.First().label, illnessName);
        Thought_TreatmentFailed mem = (Thought_TreatmentFailed)ThoughtMaker.MakeThought(failure, null);
        mem.treatmentRecipe = TreatmentRecipe;
        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);

        //IEnumerable<Thought_Memory> failureThoughts = (from memory in pawn.needs.mood.thoughts.memories.Memories
        //                                               where memory.def.workerClass != null && memory.def.workerClass.Name == "Thought_TreatmentFailed"
        //                                               orderby memory.age ascending
        //                                               select memory);
        //foreach (Thought_TreatmentFailed failureThought in failureThoughts)
        //{
        //    if (failureThought.traitName == null)
        //    {
        //        failureThought.traitName = illnessName;
        //    }
        //}
    }

    [DebuggerHidden]
    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        List<BodyPartRecord> brain = new List<BodyPartRecord>();
        Treatment treatment = GetAppropriateTreatment(pawn);
        if (treatment == null)
        {
            return brain;
        }
        if (pawn.health.hediffSet.HasHediff(TreatmentRecipe.hediff) == true)
        {
            return brain;
        }
        
        foreach (Thought_Memory m in pawn.needs.mood.thoughts.memories.Memories)
        {
            if (m.def != ThoughtDefOfPsychology.TreatmentFailed)
            {
                continue;
            }
            if ((m is Thought_TreatmentFailed failed) != true)
            {
                continue;
            }
            if (failed.treatmentRecipe == TreatmentRecipe)
            {
                return brain;
            }
            if (failed.treatmentRecipe == null)
            {
                return brain;
            }
        }
        if (pawn.story.traits.HasTrait(treatment.trait, treatment.degree))
        {
            brain.Add(pawn.health.hediffSet.GetBrain());
            return brain;
        }
        return brain;
    }

    public Treatment GetAppropriateTreatment(Pawn pawn)
    {
        if (Treatments.NullOrEmpty() || pawn?.story?.traits == null)
        {
            return null;
        }
        List<Treatment> list = new List<Treatment>();
        bool foundOne = false;
        Treatment leastEasyTreatment = null;
        foreach (Treatment def in Treatments)
        {
            if (pawn.story.traits.HasTrait(def.trait, def.degree) != true)
            {
                continue;
            }
            if (foundOne == false)
            {
                leastEasyTreatment = def;
                foundOne = true;
                continue;
            }
            if (def.easeOfTreatment < leastEasyTreatment.easeOfTreatment)
            {
                leastEasyTreatment = def;
            }
        }
        return leastEasyTreatment;

        //if (foundOne != true)
        //{
        //    return null;
        //}
        //return list.OrderBy(x => x.easeOfTreatment).First();
    }

    //protected string traitName;
    //protected int traitDegree;
    //protected float easeFactor;
    //protected TaleDef taleDef;
    //protected HediffDef hediffDef;
    //protected TraitDef traitDef;

    //public Recipe_Treatment(TraitDef treatedTrait, HediffDef treatedHediff, string treatedTraitName, float easeOfTreatment, TaleDef treatedTale, int treatedDegree = 0) : base()
    //{
    //    traitDef = treatedTrait;
    //    traitDegree = treatedDegree;
    //    hediffDef = treatedHediff;
    //    traitName = treatedTraitName;
    //    easeFactor = easeOfTreatment;
    //    taleDef = treatedTale;
    //}

    //public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    //{
    //    if (!CheckTreatmentFail(billDoer, pawn))
    //    {
    //        TaleRecorder.RecordTale(taleDef, new object[]
    //        {
    //            billDoer,
    //            pawn
    //        });
    //        if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
    //        {
    //            Messages.Message("TreatedTrait".Translate(pawn, traitName), pawn, MessageTypeDefOf.PositiveEvent);
    //        }
    //        Hediff recover = HediffMaker.MakeHediff(hediffDef, pawn, pawn.health.hediffSet.GetBrain());
    //        recover.Tended(1f, 1f); // TEST IN 1.3
    //        pawn.health.AddHediff(recover);
    //        return;
    //    }
    //    ThoughtDef failure = ThoughtDefOfPsychology.TreatmentFailed;
    //    pawn.needs.mood.thoughts.memories.TryGainMemory(failure);
    //    IEnumerable<Thought_Memory> failureThoughts = (from memory in pawn.needs.mood.thoughts.memories.Memories
    //                                                   where memory.def.workerClass != null && memory.def.workerClass.Name == "Thought_TreatmentFailed"
    //                                                   orderby memory.age ascending
    //                                                   select memory);
    //    foreach (Thought_TreatmentFailed failureThought in failureThoughts)
    //    {
    //        if (failureThought.traitName == null)
    //        {
    //            failureThought.traitName = this.traitName;
    //        }
    //    }
    //}

    //[DebuggerHidden]
    //public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    //{
    //    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(traitDef) && pawn.story.traits.GetTrait(traitDef).Degree == traitDegree && !pawn.health.hediffSet.HasHediff(hediffDef))
    //    {
    //        List<BodyPartRecord> brain = new List<BodyPartRecord>();
    //        brain.Add(pawn.health.hediffSet.GetBrain());
    //        return brain;
    //    }
    //    return new List<BodyPartRecord>();
    //}

    //public TreatmentDef GetAppropriateTreatmentDef(Pawn pawn)
    //{
    //    if (Treatments.NullOrEmpty())
    //    {
    //        return null;
    //    }
    //    foreach (TreatmentDef def in Treatments)
    //    {
    //        if (pawn.story.traits.HasTrait(def.trait, def.degree))
    //        {
    //            return def;
    //        }
    //    }
    //    return null;
    //}

}


