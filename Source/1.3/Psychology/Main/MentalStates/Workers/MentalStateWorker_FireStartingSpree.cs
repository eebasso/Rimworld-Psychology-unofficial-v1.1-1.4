using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology;

public class MentalStateWorker_FireStartingSpree : MentalStateWorker
{
    public override bool StateCanOccur(Pawn pawn)
    {
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.RecoveringPyromaniac);
        //if ((hediff is Hediff_RecoveringPyromaniac hediff_RecoveringPyromaniac) != true)
        //{
        //    return true;
        //}
        if (hediff == null)
        {
            return true;
        }
        float severity = hediff.Severity;
        float maxSeverity = hediff.def.maxSeverity;
        return Rand.Chance(1f - severity / maxSeverity);
    }
}
