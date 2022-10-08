using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class IncidentWorker_Election : IncidentWorker_MakeGameCondition
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Settlement settlementBase = Find.WorldObjects.ObjectsAt(map.Tile).OfType<Settlement>().First();
            int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * GenDate.TicksPerDay);
            GameCondition cond = GameConditionMaker.MakeCondition(this.def.gameCondition, duration);
            map.gameConditionManager.RegisterCondition(cond);
            Find.LetterStack.ReceiveLetter("LetterLabelNewElection".Translate(settlementBase.Label), "LetterNewElection".Translate(settlementBase.Label), LetterDefOf.PositiveEvent, new TargetInfo(map.Center, map, false), null);
            return true;
        }
    }
}
