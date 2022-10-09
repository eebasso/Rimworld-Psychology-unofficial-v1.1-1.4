//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using Verse.AI;


//namespace Psychology;

//public class Hediff_RecoveringPyromaniac : Hediff
//{
//    public bool inFireStarterMentalState = false;

//    public override void PostTick()
//    {
//        base.PostTick();
//        if (inFireStarterMentalState != true)
//        {
//            return;
//        }
//        if (pawn.IsHashIntervalTick(30) != true)
//        {
//            return;
//        }
//        if (pawn.InMentalState != true)
//        {
//            inFireStarterMentalState = false;
//            return;
//        }
//        MentalStateDef fireStartDef = DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree");
//        if (pawn.MentalState.def != fireStartDef)
//        {
//            inFireStarterMentalState = false;
//            return;
//        }
//        inFireStarterMentalState = true;

//        fireStartDef.recoveryMtbDays


        
//        if (pawn.InMentalState && pawn.MentalState.def == DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree"))
//        {
//            pawn.MentalState.PostEnd();
//        }
//    }
//}
