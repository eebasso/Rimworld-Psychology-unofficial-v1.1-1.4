using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Psychology;

public class MentalState_PanicAttack : MentalState
{
  public int mentalStateTicker = 0;
  public const int mentalStateTicksPerInterval = 250;

  public override void PreStart()
  {
    if (pawn.jobs.curDriver.asleep)
    {
      pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamNightmare);
    }
    base.PreStart();
  }

  public override void PostStart(string reason)
  {
    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.PanicAttack);
    base.PostStart(reason);
  }

  public override void PostEnd()
  {
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOfPsychology.PanicAttack);
    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOfPsychology.PanicAttack);
    if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety) is Hediff_Anxiety anxiety)
    {
      // Give at least one day between anxiety attacks
      anxiety.cooldownIntervalTracker = Hediff_Anxiety.intervalsPerDay;
    }
    base.PostEnd();
  }

  public override void MentalStateTick()
  {
    base.MentalStateTick();
    if (mentalStateTicker > 0)
    {
      mentalStateTicker--;
      return;
    }
    mentalStateTicker = mentalStateTicksPerInterval;
    if (!PsycheHelper.PsychologyEnabled(pawn))
    {
      //Log.WarningOnce("MentalStateTick, Psychology not enabled for pawn " + (pawn?.Label ?? "null"), 20394823);
      return;
    }
    if (!PsychologySettings.enableAnxiety)
    {
      RecoverFromState();
      return;
    }
    Hediff hediff = pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
    if (hediff == null || hediff is not Hediff_Anxiety anxiety)
    {
      return;
    }
    anxiety.Severity += 0.00000002f * mentalStateTicksPerInterval;
    //if (anxiety.IsTended())
    //{
    //  HediffComp_TendDuration tendDuration = anxiety.TryGetComp<HediffComp_TendDuration>();
    //  if (tendDuration == null)
    //  {
    //    Log.WarningOnce("MentalStateTick, tendDuration is null", 9230492);
    //  }
    //  else if (tendDuration?.tendQuality is float tendQuality && tendQuality > 0f)
    //  {
    //    float recoveryMTBDays = 1f / Mathf.Max(0.01f, Mathf.Exp(tendQuality) - 1f) * def.recoveryMtbDays;
    //    if (Rand.MTBEventOccurs(recoveryMTBDays, GenDate.TicksPerDay, mentalStateTicksPerInterval))
    //    {
    //      RecoverFromState();
    //      return;
    //    }
    //  }
    //}
    if (!pawn.Spawned || pawn.Downed) // || !pawn.RaceProps.Humanlike
    {
      return;
    }
    if (pawn.jobs != null && pawn.jobs.curJob?.def != JobDefOf.FleeAndCower)
    {
      pawn.jobs.StartJob(new Job(JobDefOf.FleeAndCower, pawn.Position), JobCondition.InterruptForced, null, false, true, null);
    }
  }
}






