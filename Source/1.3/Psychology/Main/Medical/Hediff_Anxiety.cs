using System;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace Psychology;

public class Hediff_Anxiety : HediffWithComps
{
    public const int intervalsPerDay = 20;
    public const int ticksPerInterval = GenDate.TicksPerDay / intervalsPerDay;
    public int ticksPerIntervalTracker = Mathf.CeilToInt(ticksPerInterval * Rand.Value);
    public int cooldownIntervalTracker = 0;

    public override void Tick()
    {
        base.Tick();
        if (ticksPerIntervalTracker > 0)
        {
            ticksPerIntervalTracker--;
            return;
        }
        ticksPerIntervalTracker = ticksPerInterval;
        if (cooldownIntervalTracker > 0)
        {
            cooldownIntervalTracker--;
            return;
        }
        if (pawn.InMentalState == true)
        {
            return;
        }
        //int x = pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391;
        //int modBase = 50 * (11 - 2 * this.CurStageIndex);
        //if (x % modBase != 0)
        //{
        //    return;
        //}
        float mtbDays = CurStage.mentalBreakMtbDays;
        if (mtbDays <= 0f)
        {
            return;
        }
        if (Rand.MTBEventOccurs(mtbDays, GenDate.TicksPerDay, ticksPerInterval) != true)
        {
            return;
        }
        if (pawn.jobs.curDriver.asleep)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamNightmare);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOfPsychology.PanicAttack, forceWake: true);
        }
    }
}

//public class Hediff_Anxiety : HediffWithComps
//{
//    public bool panic = false;
//    public override void Tick()
//    {
//        base.Tick();
//        if (!pawn.Downed)
//        {
//            switch ((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % (50 * (13 - ((this.CurStageIndex + 1) * 2))))
//            {
//                case 0:
//                    panic = true;
//                    this.Severity += 0.00000002f;
//                    if (pawn.Spawned && pawn.RaceProps.Humanlike)
//                    {
//                        if (pawn.jobs.curJob.def != JobDefOf.FleeAndCower && !pawn.jobs.curDriver.asleep)
//                        {
//                            pawn.jobs.StartJob(new Job(JobDefOf.FleeAndCower, pawn.Position), JobCondition.InterruptForced, null, false, true, null);
//                        }
//                        else if (pawn.jobs.curDriver.asleep)
//                        {
//                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamNightmare);
//                        }
//                    }
//                    break;
//                default:
//                    panic = false;
//                    break;
//            }
//        }
//        else
//        {
//            panic = false;
//        }
//    }
//}