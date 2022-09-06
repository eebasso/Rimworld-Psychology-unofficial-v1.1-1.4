using System;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace Psychology;

public class Hediff_Anxiety : HediffWithComps
{
    public bool panic = false;
    public bool needToFire = true;
    public const int intervalsPerDay = 500;
    public const int ticksPerInterval = GenDate.TicksPerDay / 500;
    public int ticksPerIntervalTracker = 0;
    public int cooldownIntervals = 0;

    //[LogPerformance]
    public override void Tick()
    {
        base.Tick();
        if (ticksPerIntervalTracker > 0)
        {
            ticksPerIntervalTracker--;
            return;
        }
        ticksPerIntervalTracker = ticksPerInterval;

        if (cooldownTicks > 0)
        {
            cooldownTicks--;
            panic = false;
            return;
        }

        if (!pawn.Spawned || !pawn.RaceProps.Humanlike)
        {
            panic = false;
            return;
        }

        

        //float chance = 0.02f / Math.Max(1, 11 - 2 * this.CurStageIndex);
        float chance = 0.02f / Math.Max(1, 12 - 10 * this.Severity);
        int seed = pawn.GetHashCode() + 391 * GenDate.DaysPassed + 31 * Mathf.FloorToInt(5 * GenLocalDate.DayPercent(pawn));

        if (Rand.ChanceSeeded(chance, seed))
        {
            this.Severity += 0.00000002f * ticksPerInterval;
            // Give at least one day between anxiety attacks
            cooldownIntervals = GenDate.TicksPerDay / ticksPerInterval;

            panic = true;
            if (pawn.jobs.curDriver.asleep)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.DreamNightmare);
            }
            else if (pawn.jobs.curJob.def != JobDefOf.FleeAndCower && !pawn.Downed)
            {
                //this.Severity += 0.01f;
                pawn.jobs.StartJob(new Job(JobDefOf.FleeAndCower, pawn.Position), JobCondition.InterruptForced, null, false, true, null);
            }
        }
        else
        {
            panic = false;
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