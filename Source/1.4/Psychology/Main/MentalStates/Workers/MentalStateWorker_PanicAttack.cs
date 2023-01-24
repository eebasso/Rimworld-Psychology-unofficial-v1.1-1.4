using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology;

public class MentalStateWorker_PanicAttack : MentalStateWorker
{
  public override bool StateCanOccur(Pawn pawn)
  {
    Hediff_Anxiety hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety) as Hediff_Anxiety;
    if (hediff == null)
    {
      return false;
    }
    return hediff.PanicAttackCanOccur && base.StateCanOccur(pawn);
  }
}

