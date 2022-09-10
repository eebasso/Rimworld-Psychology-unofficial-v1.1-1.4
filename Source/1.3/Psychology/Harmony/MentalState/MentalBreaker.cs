using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak))]
public static class MentalBreaker_AnxietyPatch
{
    [HarmonyPostfix]
    public static void AddAnxiety(MentalBreaker __instance, ref bool __result, Pawn ___pawn)
    {
        if (__result && PsychologySettings.enableAnxiety)
        {
            //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            int intensity;
            int.TryParse("" + (byte)Traverse.Create(__instance).Property("CurrentDesiredMoodBreakIntensity").GetValue<MentalBreakIntensity>(), out intensity);

            Hediff hediff = ___pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
            float PTSDChance = (0.15f - (0.075f * intensity)); //0.25f
            if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
            {
                PTSDChance *= 0.75f;
            }
            if (PsycheHelper.PsychologyEnabled(___pawn))
            {
                //Laid-back pawns are less likely to get anxiety from mental breaks.
                PTSDChance -= PsycheHelper.Comp(___pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack) / 10f;
            }
            if (hediff != null)
            {
                hediff.Severity += 0.15f - (intensity * 0.5f);
            }
            else if (Rand.Chance(PTSDChance))
            {
                Hediff newHediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Anxiety, ___pawn, ___pawn.health.hediffSet.GetBrain());
                newHediff.Severity = 0.75f - (intensity * 0.25f);
                Letter newAnxiety = LetterMaker.MakeLetter("LetterLabelPTSD".Translate(), "LetterPTSD".Translate(___pawn), LetterDefOf.NegativeEvent, ___pawn);
                Find.LetterStack.ReceiveLetter(newAnxiety);
                ___pawn.health.AddHediff(newHediff, null, null);
            }
        }
    }
}