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
    public static bool enableKinseyDefault = true;
    public static bool enableKinsey = true;

    public static KinseyMode kinseyFormulaDefault = KinseyMode.Realistic;
    public static KinseyMode kinseyFormula = KinseyMode.Realistic;

    public static List<float> kinseyWeightCustomDefault = new List<float>() { 62.5f, 11.3f, 9.3f, 6.8f, 4.5f, 2.8f, 2.8f };
    public static List<float> kinseyWeightCustom = new List<float>() { 62.5f, 11.3f, 9.3f, 6.8f, 4.5f, 2.8f, 2.8f };

    public static bool enableEmpathyDefault = true;
    public static bool enableEmpathy = true;

    public static bool enableIndividualityDefault = true;
    public static bool enableIndividuality = true;

    public static bool enableElectionsDefault = true;
    public static bool enableElections = true;

    public static bool enableDateLettersDefault = true;
    public static bool enableDateLetters = true;

    public static bool enableImprisonedDebuffDefault = true; // v1.1
    public static bool enableImprisonedDebuff = true; // v1.1

    public static bool enableAnxietyDefault = true; // v1.1
    public static bool enableAnxiety = true; // v1.1

    public static float conversationDurationDefault = 60f;
    public static float conversationDuration = 60f;

    public static float romanceChanceMultiplierDefault = 1f; // v1.1
    public static float romanceChanceMultiplier = 1f; // v1.1

    public static float romanceOpinionThresholdDefault = 5f; // v1.1
    public static float romanceOpinionThreshold = 5f; // v1.1

    public static float mayorAgeDefault = 20f; // v1.1
    public static float mayorAge = 20f; // v1.1

    public static float traitOpinionMultiplierDefault = 0.25f; // v1.2
    public static float traitOpinionMultiplier = 0.25f; // v1.2

    public static int displayOptionDefault = 4; // v1.3
    public static int displayOption = 4; // v1.3

    public static bool useColorsDefault = true; // v1.3
    public static bool useColors = true; // v1.3

    public static bool listAlphabeticalDefault = false; // v1.3
    public static bool listAlphabetical = false; // v1.3

    public static bool useAntonymsDefault = true; // v1.3
    public static bool useAntonyms = true; // v1.3

    public static Dictionary<string, SpeciesSettings> speciesDict = new Dictionary<string, SpeciesSettings>();

    // Hidden settings
    public static bool taraiSiblingsGenerated = false;
    //public static bool firstTimeLoadingNewPsychology = true;
    public static bool kinseySettingChanged = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableKinsey, "Psychology_EnableKinsey", enableKinseyDefault);
        Scribe_Values.Look(ref kinseyFormula, "Psychology_KinseyFormula", kinseyFormulaDefault);
        Scribe_Collections.Look(ref kinseyWeightCustom, "Psychology_KinseyWeightCustom", LookMode.Value);
        Scribe_Values.Look(ref enableEmpathy, "Psychology_EnableEmpathy", enableEmpathyDefault);
        Scribe_Values.Look(ref enableIndividuality, "Psychology_EnableIndividuality", enableIndividualityDefault);
        Scribe_Values.Look(ref enableElections, "Psychology_EnableElections", enableElectionsDefault);
        Scribe_Values.Look(ref enableDateLetters, "Psychology_EnableDateLetters", enableDateLettersDefault);
        Scribe_Values.Look(ref enableImprisonedDebuff, "Psychology_EnableImprisonedDebuff", enableImprisonedDebuffDefault);
        Scribe_Values.Look(ref enableAnxiety, "Psychology_EnableAnxiety", enableAnxietyDefault);
        Scribe_Values.Look(ref conversationDuration, "Psychology_ConversationDuration", conversationDurationDefault);
        Scribe_Values.Look(ref romanceChanceMultiplier, "Psychology_RomanceChanceMultiplier", romanceChanceMultiplierDefault);
        Scribe_Values.Look(ref romanceOpinionThreshold, "Psychology_RomanceOpinionThreshold", romanceOpinionThresholdDefault);
        Scribe_Values.Look(ref mayorAge, "Psychology_MayorAge", mayorAgeDefault);
        Scribe_Values.Look(ref traitOpinionMultiplier, "Psychology_TraitOpinionMultiplier", traitOpinionMultiplierDefault);
        Scribe_Values.Look(ref displayOption, "Psychology_DisplayOption", displayOptionDefault);
        Scribe_Values.Look(ref useColors, "Psychology_UseColors", useColorsDefault);
        Scribe_Values.Look(ref listAlphabetical, "Psychology_ListAlphabetical", listAlphabeticalDefault);
        Scribe_Values.Look(ref useAntonyms, "Psychology_UseAntonyms", useAntonymsDefault);
        Scribe_Collections.Look(ref speciesDict, "Psychology_SpeciesSettings", LookMode.Value, LookMode.Deep);
        Scribe_Values.Look(ref taraiSiblingsGenerated, "Psychology_TaraiSiblingsGenerated", false);
        //Scribe_Values.Look(ref firstTimeLoadingNewPsychology, "FirstTimeLoadingNewPsychology", false);
        Scribe_Values.Look(ref kinseySettingChanged, "KinseySettingChanged", false);
    }

    public void ResetAllSettings()
    {
        ResetEnableKinsey();
        ResetKinseyFormula();
        ResetKinseyWeightCustom();
        ResetEnableEmpathy();
        ResetEnableIndividuality();
        ResetEnableElections();
        ResetEnableDateLetters();
        ResetEnableImprisonedDebuff(); // v1.1
        ResetEnableAnxiety(); // v1.1
        ResetConversationDuration();
        ResetRomanceChanceMultiplier(); // v1.1
        ResetRomanceOpinionThreshold(); // v1.1
        ResetMayorAge(); // v1.1
        ResetTraitOpinionMultiplier(); // v1.2
        ResetDisplayOption(); // v1.3
        ResetUseColors(); // v1.3
        ResetListAlphabetical(); // v1.3
        ResetUseAntonyms(); // v1.3
        ResetSpeciesSettings();
    }

    public void ResetEnableKinsey()
    {
        kinseySettingChanged = enableKinsey != enableKinseyDefault;
        enableKinsey = enableKinseyDefault;
    }

    public void ResetKinseyFormula()
    {
        kinseyFormula = kinseyFormulaDefault;
    }

    public void ResetKinseyWeightCustom()
    {
        kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();
    }

    public void ResetEnableEmpathy()
    {
        enableEmpathy = enableEmpathyDefault;
    }

    public void ResetEnableIndividuality()
    {
        enableIndividuality = enableIndividualityDefault;
    }

    public void ResetEnableElections()
    {
        enableElections = enableElectionsDefault;
    }

    public void ResetEnableDateLetters()
    {
        enableDateLetters = enableDateLettersDefault;
    }

    public void ResetEnableImprisonedDebuff()
    {
        enableImprisonedDebuff = enableImprisonedDebuffDefault; // v1.1
    }

    public void ResetEnableAnxiety()
    {
        enableAnxiety = enableAnxietyDefault; // v1.1
    }

    public void ResetConversationDuration()
    {
        conversationDuration = conversationDurationDefault;
    }

    public void ResetRomanceChanceMultiplier()
    {
        romanceChanceMultiplier = romanceChanceMultiplierDefault; // v1.1
    }

    public void ResetRomanceOpinionThreshold()
    {
        romanceOpinionThreshold = romanceOpinionThresholdDefault; // v1.1
    }

    public void ResetMayorAge()
    {
        mayorAge = mayorAgeDefault; // v1.1
    }

    public void ResetTraitOpinionMultiplier()
    {
        traitOpinionMultiplier = traitOpinionMultiplierDefault; // v1.2
    }

    public void ResetDisplayOption()
    {
        displayOption = displayOptionDefault; // v1.3
    }

    public void ResetUseColors()
    {
        useColors = useColorsDefault; // v1.3
    }

    public void ResetListAlphabetical()
    {
        listAlphabetical = listAlphabeticalDefault; // v1.3
    }

    public void ResetUseAntonyms()
    {
        useAntonyms = useAntonymsDefault; // v1.3
    }

    public void ResetSpeciesSettings()
    {
        SpeciesHelper.ResetSpeciesDict(speciesDict);
    }

}

