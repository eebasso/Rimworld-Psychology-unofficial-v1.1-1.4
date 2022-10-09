using Verse;
using RimWorld;
using System;

namespace Psychology
{
    public class ThoughtWorker_Codependency : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p?.Spawned != true)
            {
                return ThoughtState.Inactive;
            }
            //if (p.RaceProps?.Humanlike != true)
            //{
            //    return ThoughtState.Inactive;
            //}
            if (p.story?.traits?.HasTrait(TraitDefOfPsychology.Codependent) != true)
            {
                return ThoughtState.Inactive;
            }
            if (PsycheHelper.PsychologyEnabled(p) != true)
            {
                return ThoughtState.Inactive;
            }
            Pawn lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
            if (lover != null)
            {
                return lover.Dead ? ThoughtState.ActiveAtStage(3) : ThoughtState.ActiveAtStage(1);
            }
            lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
            if (lover != null)
            {
                return lover.Dead ? ThoughtState.ActiveAtStage(3) : ThoughtState.ActiveAtStage(1);
            }
            lover = p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
            if (lover != null)
            {
                return lover.Dead ? ThoughtState.ActiveAtStage(3) : ThoughtState.ActiveAtStage(2);
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
