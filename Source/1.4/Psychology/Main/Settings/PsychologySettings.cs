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

// Things that a setting needs to do:
// Define a field in PsychologySettings
// Ref that field in ExposeData
// Define a default
// Define a backup
// Define a string buffer if the setting is a float
// Have an ability to iterate over all settings, at least those of a particular type
// Set cached and buffer from field
// Set backup from field
// Reset the field to the default
// Reset the field/cached to the backup
// Define a title and tooltip from a string name
public class PsychologySettings : ModSettings
{
  public static HashSet<string> BoolSettingNameList = new HashSet<string>();
  public static HashSet<string> FloatSettingNameList = new HashSet<string>();
  public static HashSet<string> CombinedSettingNameList = new HashSet<string>();

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

  public const float convoOpinionMultiplierDefault = 1f;
  public static float convoOpinionMultiplier = 1f;

  public const float convoMeanHoursDefault = 1f;
  public static float convoMeanHours = 1f;

  public const float convoTimeScaleHoursDefault = 1f;
  public static float convoTimeScaleHours = 1f;

  public const float convoPersonalityEffectMultiplierDefault = 1f;
  public static float convoPersonalityEffectMultiplier = 1f;

  public const float convoMaxOpinionChangeDefault = 40f;
  public static float convoMaxOpinionChange = 40f;

  public const float romanceChanceMultiplierDefault = 1f; // v1.1
  public static float romanceChanceMultiplier = 1f; // v1.1

  public const float romanceOpinionThresholdDefault = 5f; // v1.1
  public static float romanceOpinionThreshold = 5f; // v1.1

  public const float mayorAgeDefault = 20f; // v1.1
  public static float mayorAge = 20f; // v1.1

  public const float visitMayorMtbHoursDefault = 10f;
  public static float visitMayorMtbHours = 10f;

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

  // Hookup settings
  public const float hookupRateMultiplierDefault = 1f;
  public static float hookupRateMultiplier = 1f;

  public const float minOpinionForHookupDefault = 5f;
  public static float minOpinionForHookup = 5f;

  public const float hookupCheatChanceDefault = 0.05f;
  public static float hookupCheatChance = 0.05f;



  /* DEPRECATED SETTINGS */
  private const bool enableAnxietyDefault = true; // v1.1
  private static bool enableAnxiety = true; // v1.1

  public override void ExposeData()
  {
    //Log.Message("PsychologySettings, ExposeData start");
    /* Options in settings window */
    ScribeValueAndAddToNameList(nameof(enableKinsey));
    ScribeValueAndAddToNameList(nameof(enableEmpathy));
    ScribeValueAndAddToNameList(nameof(enableIndividuality));
    ScribeValueAndAddToNameList(nameof(enableDateLetters));
    ScribeValueAndAddToNameList(nameof(enableElections));
    ScribeValueAndAddToNameList(nameof(mayorAge));
    ScribeValueAndAddToNameList(nameof(visitMayorMtbHours));
    ScribeValueAndAddToNameList(nameof(mentalBreakAnxietyChance));
    ScribeValueAndAddToNameList(nameof(imprisonedDebuff));
    ScribeValueAndAddToNameList(nameof(conversationDuration));
    ScribeValueAndAddToNameList(nameof(convoOpinionMultiplier));
    ScribeValueAndAddToNameList(nameof(convoMaxOpinionChange));
    ScribeValueAndAddToNameList(nameof(convoMeanHours));
    ScribeValueAndAddToNameList(nameof(convoTimeScaleHours));
    ScribeValueAndAddToNameList(nameof(convoPersonalityEffectMultiplier));
    ScribeValueAndAddToNameList(nameof(romanceChanceMultiplier));
    ScribeValueAndAddToNameList(nameof(romanceOpinionThreshold));
    ScribeValueAndAddToNameList(nameof(traitOpinionMultiplier));
    ScribeValueAndAddToNameList(nameof(personalityExtremeness));
    ScribeValueAndAddToNameList(nameof(ideoPsycheMultiplier));

    Scribe_Values.Look(ref kinseyFormula, "Psychology_KinseyFormula", kinseyFormulaDefault);
    Scribe_Collections.Look(ref kinseyWeightCustom, "Psychology_KinseyWeightCustom", LookMode.Value);
    Scribe_Collections.Look(ref speciesDict, "Psychology_SpeciesSettings", LookMode.Value, LookMode.Deep);

    /* Hidden settings */
    Scribe_Values.Look(ref displayOption, "Psychology_DisplayOption", displayOptionDefault);
    Scribe_Values.Look(ref useColors, "Psychology_UseColors", useColorsDefault);
    Scribe_Values.Look(ref listAlphabetical, "Psychology_ListAlphabetical", listAlphabeticalDefault);
    Scribe_Values.Look(ref useAntonyms, "Psychology_UseAntonyms", useAntonymsDefault);

    /* Deprecated settings. Set each to default and use forceSave = false to essentially delete them from savefile */
    Scribe_Values.Look(ref enableAnxiety, "Psychology_EnableAnxiety", enableAnxietyDefault);
    enableAnxiety = enableAnxietyDefault;
    //Log.Message("PsychologySettings, ExposeData end");
  }

  public static void ScribeValueAndAddToNameList(string settingName)
  {
    FieldInfo fieldInfo = AccessTools.Field(typeof(PsychologySettings), settingName);
    FieldInfo fieldInfoDefault = AccessTools.Field(typeof(PsychologySettings), settingName + "Default");
    object setting = fieldInfo.GetValue(null);
    object settingDefault = fieldInfoDefault.GetValue(null);
    string scribeName = "Psychology_" + settingName.CapitalizeFirst();
    if (setting is bool boolSetting && settingDefault is bool boolSettingDefault)
    {
      Scribe_Values.Look(ref boolSetting, scribeName, boolSettingDefault);
      fieldInfo.SetValue(null, boolSetting);
      if (BoolSettingNameList.Add(settingName))
      {
        CombinedSettingNameList.Add(settingName);
        //Log.Message("Psychology: added " + settingName + " as a bool setting");
      }
      return;
    }
    if (setting is float floatSetting && settingDefault is float floatSettingDefault)
    {
      Scribe_Values.Look(ref floatSetting, scribeName, floatSettingDefault);
      fieldInfo.SetValue(null, floatSetting);
      if (FloatSettingNameList.Add(settingName))
      {
        CombinedSettingNameList.Add(settingName);
        //Log.Message("Psycholog: added " + settingName + " as a float setting");
      }
      return;
    }
    Log.Error("Could not add setting " + settingName + " correctly");
  }

  public static object GetSettingFromName(string settingName)
  {
    FieldInfo field = AccessTools.Field(typeof(PsychologySettings), settingName);
    object obj = field.GetValue(null);
    if (obj == null)
    {
      Log.Error("GetSettingFromName, object was null");
    }
    return obj;
  }

  public static void SetSettingFromName(string settingName, object value)
  {
    AccessTools.Field(typeof(PsychologySettings), settingName).SetValue(null, value);
  }

  public static void ResetAllSettings()
  {
    foreach (string settingName in CombinedSettingNameList)
    {
      ResetSettingToDefault(settingName);
    }
    ResetKinseyFormula();
    ResetKinseyWeightCustom();
    ResetSpeciesSettings();
  }

  public static void ResetSettingToDefault(string settingName)
  {
    FieldInfo fieldInfoSetting = AccessTools.Field(typeof(PsychologySettings), settingName);
    FieldInfo fieldInfoDefault = AccessTools.Field(typeof(PsychologySettings), settingName + "Default");
    fieldInfoSetting.SetValue(null, fieldInfoDefault.GetValue(null));
  }

  public static void ResetKinseyFormula()
  {
    kinseyFormula = kinseyFormulaDefault;
  }

  public static void ResetKinseyWeightCustom()
  {
    kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();
  }

  public static void ResetSpeciesSettings()
  {
    SpeciesHelper.ResetSpeciesDict(speciesDict);
  }
}


