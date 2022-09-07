using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Psychology
{
    public class JobGiver_Vote : ThinkNode_JobGiver
    {
        public const float ReachDestDist = 50f;

        //[LogPerformance]
        public override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 result;

            if (!GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, checkCell, out result))
            {
                return null;
            }
            if (result.IsValid && result.DistanceToSquared(pawn.Position) < ReachDestDist && result.GetRoom(pawn.Map) == pawn.GetRoom())
            {
                pawn.GetLord().Notify_ReachedDutyLocation(pawn);
                return null;
            }
            return new Job(JobDefOf.Goto, result, 500, true);
        }

        public static bool checkCell(IntVec3 cell) // TEST IN 1.3
        {
            return (cell.x - cell.y) > cell.z;
        }

    }

}
