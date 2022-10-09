using System;
using Verse;
using RimWorld;

namespace Psychology;

public class ThoughtWorker_AlwaysActivePsychology : ThoughtWorker_AlwaysActive
{
    public override float MoodMultiplier(Pawn p)
    {
        float num = base.MoodMultiplier(p);
        Log.Message("ThoughtWorker_AlwaysActivePsychology, MoodMultiplier activated");
        if (p.health?.hediffSet?.HasHediff(HediffDefOfPsychology.Antidepressants) != true)
        {
            return num;
        }
        if (this.def.stages[0].baseOpinionOffset < 0f)
        {   
            num *= 0.5f;
        }
        return num;
    }
}

