//using Verse;
//using RimWorld;
//using System;

//namespace Psychology;

//public class ThoughtWorker_PanicAttack : ThoughtWorker
//{
//    protected override ThoughtState CurrentStateInternal(Pawn p)
//    {
//        if (!p.Spawned || !p.Awake() || p.Dead)
//        {
//            return ThoughtState.Inactive;
//        }
//        //if (!p.RaceProps.Humanlike)
//        //{
//        //    return ThoughtState.Inactive;
//        //}


//        Hediff anxiety = p.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
//        if (anxiety == null)
//        {
//            return ThoughtState.Inactive;
//        }
//        if (!anxiety.)
//        {
//            return ThoughtState.Inactive;
//        }
//        return ThoughtState.ActiveAtStage(0);
//    }
//}
