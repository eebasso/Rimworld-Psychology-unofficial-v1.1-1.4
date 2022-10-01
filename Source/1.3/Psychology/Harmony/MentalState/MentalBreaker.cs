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
    public static Dictionary<MentalBreakIntensity, Pair<float, float>> dict = new Dictionary<MentalBreakIntensity, Pair<float, float>>()
    {
        { MentalBreakIntensity.None   , new Pair<float, float>(0.0f, 0.0f) },
        { MentalBreakIntensity.Minor  , new Pair<float, float>(0.1f, 0.3f) },
        { MentalBreakIntensity.Major  , new Pair<float, float>(0.2f, 0.7f) },
        { MentalBreakIntensity.Extreme, new Pair<float, float>(0.4f, 1.1f) }
    };

    [HarmonyPostfix]
    public static void AddAnxiety(MentalBreaker __instance, ref bool __result, Pawn ___pawn)
    {
        if (__result != true || PsychologySettings.enableAnxiety != true)
        {
            return;
        }
        //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        Pair<float, float> pair = dict[Traverse.Create(__instance).Property("CurrentDesiredMoodBreakIntensity").GetValue<MentalBreakIntensity>()];
        float PTSDChance = pair.First;
        float severity = pair.Second;

        //int.TryParse("" + (byte)Traverse.Create(__instance).Property("CurrentDesiredMoodBreakIntensity").GetValue<MentalBreakIntensity>(), out intensity);
        if (PTSDChance == 0.0f)
        {
            return;
        }
        Hediff hediff = ___pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOfPsychology.Anxiety);
        if (hediff != null)
        {
            hediff.Severity += PTSDChance * severity;
            return;
        }
        if (___pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
        {
            PTSDChance *= 0.5f;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn))
        {
            //Laid-back pawns are less likely to get anxiety from mental breaks.
            //PTSDChance -= PsycheHelper.Comp(___pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack) / 10f;
            PTSDChance *= 1.5f - PsycheHelper.Comp(___pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.LaidBack);
        }
        if (Rand.Chance(PTSDChance))
        {
            Hediff newHediff = HediffMaker.MakeHediff(HediffDefOfPsychology.Anxiety, ___pawn, ___pawn.health.hediffSet.GetBrain());
            newHediff.Severity = severity;
            Letter newAnxiety = LetterMaker.MakeLetter("LetterLabelPTSD".Translate(), "LetterPTSD".Translate(___pawn), LetterDefOf.NegativeEvent, ___pawn);
            Find.LetterStack.ReceiveLetter(newAnxiety);
            ___pawn.health.AddHediff(newHediff, null, null);
        }
    }
}