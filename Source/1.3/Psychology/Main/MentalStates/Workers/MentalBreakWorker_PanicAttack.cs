//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using Verse.AI;
//using UnityEngine;

//namespace Psychology;

//public class MentalBreakWorker_PanicAttack : MentalBreakWorker
//{
//    public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
//    {
//        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
//        if (hediff == null)
//        {
//            return 0f;
//        }
//        float num = base.CommonalityFor(pawn, moodCaused);
//        num *= 9f / Mathf.Max(1f, 11f - 2f * hediff.CurStageIndex);
//        if (hediff.IsTended() != true)
//        {
//            return num;
//        }
//        HediffComp_TendDuration tendDuration = hediff.TryGetComp<HediffComp_TendDuration>();
//        float tendQuality = tendDuration.tendQuality;
//        num *= Mathf.InverseLerp(1f, 0.01f, 0.8f * tendQuality);
//        return num;
//    }

//    public override bool BreakCanOccur(Pawn pawn)
//    {
//        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
//        if (hediff == null)
//        {
//            return false;
//        }
//        if (hediff.IsTended() != true)
//        {
//            return base.BreakCanOccur(pawn);
//        }
//        HediffComp_TendDuration tendDuration = hediff.TryGetComp<HediffComp_TendDuration>();
//        float tendQuality = 1.5f * tendDuration.tendQuality;
//        if (Rand.Value > Mathf.Exp(-tendQuality))
//        {
//            return false;
//        }
//        return base.BreakCanOccur(pawn);
//    }
//}

