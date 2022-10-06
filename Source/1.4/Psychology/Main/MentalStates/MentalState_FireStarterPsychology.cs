using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology;

public class MentalState_FireStartingSpreePsychology : MentalState_FireStartingSpree
{
    //Hediff_RecoveringPyromaniac hediff_RecoveringPyromaniac = null;
    Hediff hediff = null;

    public override void PreStart()
    {
        hediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.RecoveringPyromaniac);
        base.PreStart();
    }

    //public override void PostEnd()
    //{
    //    base.PostEnd();
    //    hediff_RecoveringPyromaniac = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.RecoveringPyromaniac);
    //    CheckRecoveringPyromaniac(false);
    //}

    //public void CheckRecoveringPyromaniac(bool starting)
    //{
    //    Hediff hediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.RecoveringPyromaniac);
    //    hediff_RecoveringPyromaniac = hediff as Hediff_RecoveringPyromaniac;
    //    if (hediff_RecoveringPyromaniac != null)
    //    {
    //        hediff_RecoveringPyromaniac.inFireStarterMentalState = starting;
    //    }
    //}

    public override void MentalStateTick()
    {
        base.MentalStateTick();
        if (hediff == null || this.pawn.IsHashIntervalTick(30) != true)
        {
            return;
        }
        float severity = hediff.Severity;
        if (severity == 0f)
        {
            return;
        }
        float maxSeverity = hediff.def.maxSeverity;
        // This ensures that the effective overall mtbDays is recoveryMtbDays * (1 - severity / maxSeverity)
        float mtbDays = this.def.recoveryMtbDays * (maxSeverity / severity - 1f);
        if (mtbDays == 0f || Rand.MTBEventOccurs(mtbDays, 60000, 30))
        {
            base.RecoverFromState();
        }
    }
}

