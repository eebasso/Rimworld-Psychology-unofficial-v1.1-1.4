using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class LordToil_WardenTour : LordToil
    {
        public LordToil_WardenTour(Pawn[] pawns)
        {
            this.tourPawns = pawns;
        }

        public override void UpdateAllDuties()
        {
            /*if((Find.TickManager.TicksGame % (GenDate.TicksPerHour/4) == 0)
                && Rand.MTBEventOccurs(2f, GenDate.TicksPerHour, GenDate.TicksPerHour/4))
            {

            }*/
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOfPsychology.HangOut, this.tourPawns[0].Position, this.tourPawns[1].Position, -1f);
            }
        }
        
        public Pawn[] tourPawns;
        public Job hangOut;
        public int lastVisitChangeTick = 0;
    }
}
