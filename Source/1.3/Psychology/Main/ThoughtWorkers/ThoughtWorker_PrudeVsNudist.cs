using System;using System.Collections.Generic;using Verse;using RimWorld;namespace Psychology;
public class ThoughtWorker_PrudeVsNudist : ThoughtWorker{    public override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
    {
        if (p?.story?.traits?.HasTrait(TraitDefOfPsychology.Prude) != true)
        {
            return false;
        }
        if (other?.story?.traits?.HasTrait(TraitDefOf.Nudist) != true)
        {
            return false;
        }
        if (!RelationsUtility.PawnsKnowEachOther(p, other))
        {
            return false;
        }
        return true;
    }}