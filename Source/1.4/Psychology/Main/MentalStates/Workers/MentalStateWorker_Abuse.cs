using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
  public class MentalStateWorker_Abuse : MentalStateWorker
  {
    public override bool StateCanOccur(Pawn pawn)
    {
      if (pawn.Map == null)
        return false;
      return base.StateCanOccur(pawn) && (pawn.Map.mapPawns.FreeColonistsSpawnedCount) > 1;
    }
  }
}
