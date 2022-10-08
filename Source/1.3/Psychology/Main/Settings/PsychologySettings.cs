using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;
using HarmonyLib;

namespace Psychology;

public enum KinseyMode
{
    Realistic,
    Uniform,
    Invisible,
    Gaypocalypse,
    Custom
};

public class PsychologySettings : ModSettings
{
    public const bool enableKinseyDefault = true;
    public static bool enableKinsey = true;

    public const KinseyMode kinseyFormulaDefault = KinseyMode.Realistic;
    public static KinseyMode kinseyFormula = KinseyMode.Realistic;

    public static readonly List<float> kinseyWeightCustomDefault = new List<float>() { 100f, 0f, 50f, 100f, 50f, 0f, 100f };
    public static List<float> kinseyWeightCustom = new List<float>() { 100f, 0f, 50f, 100f, 50f, 0f, 100f };

    public const bool enableEmpathyDefault = true;
    public static bool enableEmpathy = true;

    public const bool enableIndividualityDefault = true;
    public static bool enableIndividuality = true;

    public const bool enableElectionsDefault = true;
    public static bool enableElections = true;

    public const bool enableDateLettersDefault = true;
    public static bool enableDateLetters = true;

    public const float mentalBreakAnxietyChanceDefault = 0.2f;
    public static float mentalBreakAnxietyChance = 0.2f;

    public const float imprisonedDebuffDefault = 40f; // v1.1
    public static float imprisonedDebuff = 40f; // v1.1

    public const float conversationDurationDefault = 60f;
    public static float conversationDuration = 60f;

    public const float romanceChanceMultiplierDefault = 1f; // v1.1
    public static float romanceChanceMultiplier = 1f; // v1.1

    public const float romanceOpinionThresholdDefault = 5f; // v1.1
    public static float romanceOpinionThreshold = 5f; // v1.1

    public const float mayorAgeDefault = 20f; // v1.1
    public static float mayorAge = 20f; // v1.1

    public const float visitMayorMtbHoursDefault = 0.1f;
    public static float visitMayorMtbHours = 0.1f;

    public const float traitOpinionMultiplierDefault = 0.25f; // v1.3
    public static float traitOpinionMultiplier = 0.25f; // v1.3

    public const float personalityExtremenessDefault = 0.33f; // v1.3
    public static float personalityExtremeness = 0.33f; // v1.3

    public const float ideoPsycheMultiplierDefault = 1f;
    public static float ideoPsycheMultiplier = 1f;

    public static Dictionary<string, SpeciesSettings> speciesDict = new Dictionary<string, SpeciesSettings>();

    // Hidden settings
    public const int displayOptionDefault = 4; // v1.3
    public static int displayOption = 4; // v1.3

    public const bool useColorsDefault = true; // v1.3
    public static bool useColors = true; // v1.3

    public const bool listAlphabeticalDefault = false; // v1.3
    public static bool listAlphabetical = false; // v1.3

    public const bool useAntonymsDefault = true; // v1.3
    public static bool useAntonyms = true; // v1.3

    /* DEPRECATED SETTINGS */
    private const bool enableAnxietyDefault = true; // v1.1
    private static bool enableAnxiety = true; // v1.1

    public override void ExposeData()
    {
        /* Options in settings winodw */
        Scribe_Values.Look(ref enableKinsey, "Psychology_EnableKinsey", enableKinseyDefault);
        Scribe_Values.Look(ref kinseyFormula, "Psychology_KinseyFormula", kinseyFormulaDefault);
        Scribe_Collections.Look(ref kinseyWeightCustom, "Psychology_KinseyWeightCustom", LookMode.Value);
        Scribe_Values.Look(ref enableEmpathy, "Psychology_EnableEmpathy", enableEmpathyDefault);
        Scribe_Values.Look(ref enableIndividuality, "Psychology_EnableIndividuality", enableIndividualityDefault);
        Scribe_Values.Look(ref enableElections, "Psychology_EnableElections", enableElectionsDefault);
        Scribe_Values.Look(ref enableDateLetters, "Psychology_EnableDateLetters", enableDateLettersDefault);
        Scribe_Values.Look(ref mentalBreakAnxietyChance, "Psychology_MentalBreakAnxietyChance", mentalBreakAnxietyChanceDefault);
        Scribe_Values.Look(ref imprisonedDebuff, "Psychology_ImprisonedDebuff", imprisonedDebuffDefault);
        Scribe_Values.Look(ref conversationDuration, "Psychology_ConversationDuration", conversationDurationDefault);
        Scribe_Values.Look(ref romanceChanceMultiplier, "Psychology_RomanceChanceMultiplier", romanceChanceMultiplierDefault);
        Scribe_Values.Look(ref romanceOpinionThreshold, "Psychology_RomanceOpinionThreshold", romanceOpinionThresholdDefault);
        Scribe_Values.Look(ref mayorAge, "Psychology_MayorAge", mayorAgeDefault);
        Scribe_Values.Look(ref visitMayorMtbHours, "Psychology_VisitMayorMtbHours", visitMayorMtbHoursDefault);
        Scribe_Values.Look(ref traitOpinionMultiplier, "Psychology_TraitOpinionMultiplier", traitOpinionMultiplierDefault);
        Scribe_Values.Look(ref personalityExtremeness, "Psychology_PersonalityExtremeness", personalityExtremenessDefault);
        Scribe_Values.Look(ref ideoPsycheMultiplier, "Psychology_IdeoPsycheMultiplier", ideoPsycheMultiplierDefault);
        Scribe_Collections.Look(ref speciesDict, "Psychology_SpeciesSettings", LookMode.Value, LookMode.Deep);

        /* Hidden settings */
        Scribe_Values.Look(ref displayOption, "Psychology_DisplayOption", displayOptionDefault);
        Scribe_Values.Look(ref useColors, "Psychology_UseColors", useColorsDefault);
        Scribe_Values.Look(ref listAlphabetical, "Psychology_ListAlphabetical", listAlphabeticalDefault);
        Scribe_Values.Look(ref useAntonyms, "Psychology_UseAntonyms", useAntonymsDefault);

        /* Hidden settings. Set each to default and use force save = false to essentially delete them from  */
        Scribe_Values.Look(ref enableAnxiety, "Psychology_EnableAnxiety", enableAnxietyDefault);
        enableAnxiety = enableAnxietyDefault;
    }

    public static void ResetAllSettings()
    {
        ResetEnableKinsey();
        ResetKinseyFormula();
        ResetKinseyWeightCustom();
        ResetEnableEmpathy();
        ResetEnableIndividuality();
        ResetEnableElections();
        ResetEnableDateLetters();
        ResetMentalBreakAnxietyChance(); // v1.1
        ResetImprisonedDebuff(); // v1.1
        ResetConversationDuration();
        ResetRomanceChanceMultiplier(); // v1.1
        ResetRomanceOpinionThreshold(); // v1.1
        ResetMayorAge(); // v1.1
        ResetVisitMayorMbtHours();
        ResetTraitOpinionMultiplier(); // v1.2
        ResetPersonalityExtremeness();
        ResetIdeoPsycheMultiplier();
        ResetSpeciesSettings();
        ResetDisplayOption(); // v1.3
        ResetUseColors(); // v1.3
        ResetListAlphabetical(); // v1.3
        ResetUseAntonyms(); // v1.3
    }

    public static void ResetEnableKinsey()
    {
        //kinseySettingChanged = enableKinsey != enableKinseyDefault;
        enableKinsey = enableKinseyDefault;
    }

    public static void ResetKinseyFormula()
    {
        kinseyFormula = kinseyFormulaDefault;
    }

    public static void ResetKinseyWeightCustom()
    {
        kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();
    }

    public static void ResetEnableEmpathy()
    {
        enableEmpathy = enableEmpathyDefault;
    }

    public static void ResetEnableIndividuality()
    {
        enableIndividuality = enableIndividualityDefault;
    }

    public static void ResetEnableElections()
    {
        enableElections = enableElectionsDefault;
    }

    public static void ResetEnableDateLetters()
    {
        enableDateLetters = enableDateLettersDefault;
    }

    public static void ResetMentalBreakAnxietyChance()
    {
        mentalBreakAnxietyChance = mentalBreakAnxietyChanceDefault; // v1.1
    }

    public static void ResetImprisonedDebuff()
    {
        imprisonedDebuff = imprisonedDebuffDefault; // v1.1
    }

    public static void ResetConversationDuration()
    {
        conversationDuration = conversationDurationDefault;
    }

    public static void ResetRomanceChanceMultiplier()
    {
        romanceChanceMultiplier = romanceChanceMultiplierDefault; // v1.1
    }

    public static void ResetRomanceOpinionThreshold()
    {
        romanceOpinionThreshold = romanceOpinionThresholdDefault; // v1.1
    }

    public static void ResetMayorAge()
    {
        mayorAge = mayorAgeDefault; // v1.1
    }

    public static void ResetVisitMayorMbtHours()
    {
        visitMayorMtbHours = visitMayorMtbHoursDefault;
    }

    public static void ResetTraitOpinionMultiplier()
    {
        traitOpinionMultiplier = traitOpinionMultiplierDefault; // v1.2
    }

    public static void ResetPersonalityExtremeness()
    {
        personalityExtremeness = personalityExtremenessDefault;
    }

    public static void ResetDisplayOption()
    {
        displayOption = displayOptionDefault; // v1.3
    }

    public static void ResetUseColors()
    {
        useColors = useColorsDefault; // v1.3
    }

    public static void ResetListAlphabetical()
    {
        listAlphabetical = listAlphabeticalDefault; // v1.3
    }

    public static void ResetUseAntonyms()
    {
        useAntonyms = useAntonymsDefault; // v1.3
    }

    public static void ResetIdeoPsycheMultiplier()
    {
        ideoPsycheMultiplier = ideoPsycheMultiplierDefault;
    }

    public static void ResetSpeciesSettings()
    {
        SpeciesHelper.ResetSpeciesDict(speciesDict);
    }

    public static void ResetVisitMayorMtbHours()
    {
        visitMayorMtbHours = visitMayorMtbHoursDefault;
    }

}

