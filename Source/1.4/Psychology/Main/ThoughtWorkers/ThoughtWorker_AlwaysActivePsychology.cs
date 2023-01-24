using System;
using Verse;
using RimWorld;

namespace Psychology;

public class ThoughtWorker_AlwaysActivePsychology : ThoughtWorker_AlwaysActive
{
    public override float MoodMultiplier(Pawn p)
    {
        float num = base.MoodMultiplier(p);
        if (p.health?.hediffSet?.HasHediff(HediffDefOfPsychology.Antidepressants) == true && this.def.stages[0].baseMoodEffect < 0f)
        {
            num *= 0.5f;
        }
        return num;
    }
}

