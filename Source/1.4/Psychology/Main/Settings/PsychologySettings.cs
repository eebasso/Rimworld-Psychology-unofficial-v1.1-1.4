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

[StaticConstructorOnStartup]
public class PsychologySettings : ModSettings
{
  public static HashSet<string> BoolSettingNameList = new HashSet<string>();
  public static HashSet<string> FloatSettingNameList = new HashSet<string>();
  public static HashSet<string> ValueTypeSettingNameList = new HashSet<string>();

  /* Sexuality */
  private const bool enableKinseyDefault = true;
  public static bool enableKinsey = enableKinseyDefault;

  private const KinseyMode kinseyFormulaDefault = KinseyMode.Realistic;
  public static KinseyMode kinseyFormula = kinseyFormulaDefault;

  private static readonly List<float> kinseyWeightCustomDefault = new List<float>() { 100f, 0f, 50f, 100f, 50f, 0f, 100f };
  public static List<float> kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();

  private const float minRomanticDriveAge = 10f;
  private const float minSexDriveAge = 13f;

  private static readonly List<CurvePoint> femaleSexDriveAgeCurvePointsDefault = new List<CurvePoint>
  {
    new CurvePoint(minSexDriveAge, 0f),
    new CurvePoint(15f, 1f),
    new CurvePoint(35f, 1.6f),
    new CurvePoint(40f, 1.6f),
    new CurvePoint(50f, 1f),
    new CurvePoint(80f, 0.6f),
  };
  private static List<CurvePoint> femaleSexDriveAgeCurvePoints = femaleSexDriveAgeCurvePointsDefault.ListFullCopy();

  private static readonly List<CurvePoint> maleSexDriveAgeCurveDefault = new List<CurvePoint>
  {
    new CurvePoint(minSexDriveAge, 0f),
    new CurvePoint(15f, 1f),
    new CurvePoint(18f, 1.6f),
    new CurvePoint(23f, 1.6f),
    new CurvePoint(50f, 1f),
    new CurvePoint(80f, 0.6f),
  };
  private static List<CurvePoint> maleSexDriveAgeCurve = maleSexDriveAgeCurveDefault.ListFullCopy();

  private static readonly List<CurvePoint> romanticDriveAgeCurvePointsDefault = new List<CurvePoint>
  {
    new CurvePoint(minRomanticDriveAge, 0f),
    new CurvePoint(15f, 1.1f),
    new CurvePoint(25f, 1.3f),
    new CurvePoint(35f, 1.2f),
    new CurvePoint(50f, 1f),
    new CurvePoint(80f, 0.8f),
  };
  private static List<CurvePoint> romanticDriveAgeCurvePoints = romanticDriveAgeCurvePointsDefault.ListFullCopy();

  /* Romance */
  public static bool enableDateLetters = true;
  private const bool enableDateLettersDefault = true;

  public static float romanceChanceMultiplier = 1f; // v1.1
  private const float romanceChanceMultiplierDefault = 1f; // v1.1

  public static float romanceOpinionThreshold = 5f; // v1.1
  private const float romanceOpinionThresholdDefault = 5f; // v1.1

  // Conversations
  private const float conversationDurationDefault = 60f;
  public static float conversationDuration = 60f;
  
  private const float convoMaxOpinionChangeDefault = 40f;
  public static float convoMaxOpinionChange = 40f;
  
  public const float convoMeanHoursDefault = 1.3f;
  public static float convoMeanHours = 1.3f;

  private const float convoTimeScaleHoursDefault = 1f;
  public static float convoTimeScaleHours = 1f;

  private const float convoPersonalityEffectMultiplierDefault = 1f;
  public static float convoPersonalityEffectMultiplier = 1f;

  // Elections
  private const bool enableElectionsDefault = true;
  public static bool enableElections = true;

  private const float mayorAgeDefault = 20f; // v1.1
  public static float mayorAge = 20f; // v1.1

  private const float visitMayorMtbHoursDefault = 10f;
  public static float visitMayorMtbHours = 10f;

  // Thoughts
  private const bool enableEmpathyDefault = true;
  public static bool enableEmpathy = true;

  private const bool enableIndividualityDefault = true;
  public static bool enableIndividuality = true;

  private const float mentalBreakAnxietyChanceDefault = 0.05f;
  public static float mentalBreakAnxietyChance = 0.05f;

  public const float imprisonedDebuffDefault = 40f;
  public static float imprisonedDebuff = 40f;

  private const float traitOpinionMultiplierDefault = 0.25f; // v1.3
  public static float traitOpinionMultiplier = 0.25f; // v1.3

  /* Other settings */
  private const bool enableAnxietyDefault = true; // v1.1
  public static bool enableAnxiety = enableAnxietyDefault; // v1.1

  private const float personalityExtremenessDefault = 0.33f; // v1.3
  public static float personalityExtremeness = 0.33f; // v1.3

  private const float ideoPsycheMultiplierDefault = 1f;
  public static float ideoPsycheMultiplier = 1f;

  public static Dictionary<string, SpeciesSettings> speciesDict = new Dictionary<string, SpeciesSettings>();

  // Hookup settings
  private const float hookupRateMultiplierDefault = 1f;
  public static float hookupRateMultiplier = 1f;

  private const float minOpinionForHookupDefault = 5f;
  public static float minOpinionForHookup = 5f;

  private const float hookupCheatMultiplierDefault = 0.05f;
  public static float hookupCheatMultiplier = 0.05f;

  // Hidden settings
  private const int displayOptionDefault = 4; // v1.3
  public static int displayOption = 4; // v1.3

  private const bool useColorsDefault = true; // v1.3
  public static bool useColors = true; // v1.3

  private const bool listAlphabeticalDefault = false; // v1.3
  public static bool listAlphabetical = false; // v1.3

  private const bool useAntonymsDefault = true; // v1.3
  public static bool useAntonyms = true; // v1.3

  /* DEPRECATED SETTINGS */


  private const float convoOpinionMultiplierDefault = 1f;
  private static float convoOpinionMultiplier = convoOpinionMultiplierDefault;

  public static SimpleCurve FemaleSexDriveAgeCurve
  {
    get => SimpleCurveMinAgeEnforced(femaleSexDriveAgeCurvePoints, minSexDriveAge);
    set => femaleSexDriveAgeCurvePoints = FilteredCurvePointsMinAgeEnforced(value.Points, minSexDriveAge);
  }

  public static SimpleCurve MaleSexDriveAgeCurve
  {
    get => SimpleCurveMinAgeEnforced(maleSexDriveAgeCurve, minSexDriveAge);
    set => maleSexDriveAgeCurve = FilteredCurvePointsMinAgeEnforced(value.Points, minSexDriveAge);
  }

  public static SimpleCurve RomanticDriveAgeCurve
  {
    get => SimpleCurveMinAgeEnforced(romanticDriveAgeCurvePoints, minRomanticDriveAge);
    set => romanticDriveAgeCurvePoints = FilteredCurvePointsMinAgeEnforced(value.Points, minRomanticDriveAge);
  }

  static PsychologySettings()
  {
    kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();
    femaleSexDriveAgeCurvePoints = femaleSexDriveAgeCurvePointsDefault.ListFullCopy();
    maleSexDriveAgeCurve = maleSexDriveAgeCurveDefault.ListFullCopy();
    romanticDriveAgeCurvePoints = romanticDriveAgeCurvePointsDefault.ListFullCopy();
  }

  public override void ExposeData()
  {
    /* Options in settings window */
    ScribeValueAndAddToNameList(nameof(enableKinsey));
    ScribeValueAndAddToNameList(nameof(enableEmpathy));
    ScribeValueAndAddToNameList(nameof(enableIndividuality));
    ScribeValueAndAddToNameList(nameof(enableDateLetters));
    ScribeValueAndAddToNameList(nameof(enableElections));
    ScribeValueAndAddToNameList(nameof(enableAnxiety));
    ScribeValueAndAddToNameList(nameof(mayorAge));
    ScribeValueAndAddToNameList(nameof(visitMayorMtbHours));
    ScribeValueAndAddToNameList(nameof(mentalBreakAnxietyChance));
    ScribeValueAndAddToNameList(nameof(imprisonedDebuff));
    ScribeValueAndAddToNameList(nameof(conversationDuration));
    ScribeValueAndAddToNameList(nameof(convoMaxOpinionChange));
    ScribeValueAndAddToNameList(nameof(convoMeanHours));
    ScribeValueAndAddToNameList(nameof(convoTimeScaleHours));
    ScribeValueAndAddToNameList(nameof(convoPersonalityEffectMultiplier));
    ScribeValueAndAddToNameList(nameof(romanceChanceMultiplier));
    ScribeValueAndAddToNameList(nameof(romanceOpinionThreshold));
    ScribeValueAndAddToNameList(nameof(traitOpinionMultiplier));
    ScribeValueAndAddToNameList(nameof(personalityExtremeness));
    ScribeValueAndAddToNameList(nameof(ideoPsycheMultiplier));
    ScribeValueAndAddToNameList(nameof(hookupRateMultiplier));
    ScribeValueAndAddToNameList(nameof(minOpinionForHookup));
    ScribeValueAndAddToNameList(nameof(hookupCheatMultiplier));

    Scribe_Values.Look(ref kinseyFormula, "Psychology_KinseyFormula", kinseyFormulaDefault);
    Scribe_Collections.Look(ref kinseyWeightCustom, "Psychology_KinseyWeightCustom", LookMode.Value);
    Scribe_Collections.Look(ref speciesDict, "Psychology_SpeciesSettings", LookMode.Value, LookMode.Deep);
    Scribe_Collections.Look(ref femaleSexDriveAgeCurvePoints, "Psychology_FemaleSexDriveAgeCurve", LookMode.Value);


    /* Hidden settings */
    Scribe_Values.Look(ref displayOption, "Psychology_DisplayOption", displayOptionDefault);
    Scribe_Values.Look(ref useColors, "Psychology_UseColors", useColorsDefault);
    Scribe_Values.Look(ref listAlphabetical, "Psychology_ListAlphabetical", listAlphabeticalDefault);
    Scribe_Values.Look(ref useAntonyms, "Psychology_UseAntonyms", useAntonymsDefault);




    /* Deprecated settings. Set each to default and use forceSave = false to essentially delete them from savefile */
    //enableAnxiety = enableAnxietyDefault;
    //Scribe_Values.Look(ref enableAnxiety, "Psychology_EnableAnxiety", enableAnxietyDefault);
    //enableAnxiety = enableAnxietyDefault;

    convoOpinionMultiplier = convoOpinionMultiplierDefault;
    Scribe_Values.Look(ref convoOpinionMultiplier, "Psychology_ConvoOpinionMultiplier", convoOpinionMultiplierDefault);
    convoOpinionMultiplier = convoOpinionMultiplierDefault;

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
        ValueTypeSettingNameList.Add(settingName);
        ////Log.Message("Psychology: added " + settingName + " as a bool setting");
      }
      return;
    }
    if (setting is float floatSetting && settingDefault is float floatSettingDefault)
    {
      Scribe_Values.Look(ref floatSetting, scribeName, floatSettingDefault);
      fieldInfo.SetValue(null, floatSetting);
      if (FloatSettingNameList.Add(settingName))
      {
        ValueTypeSettingNameList.Add(settingName);
        ////Log.Message("Psycholog: added " + settingName + " as a float setting");
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
    foreach (string settingName in ValueTypeSettingNameList)
    {
      ResetValueTypeSettingToDefault(settingName);
    }
    ResetKinseyFormula();
    ResetKinseyWeightCustom();
    ResetSpeciesSettings();
    ResetAgeCurves();
  }

  public static void ResetValueTypeSettingToDefault(string settingName)
  {
    FieldInfo fieldInfoSetting = AccessTools.Field(typeof(PsychologySettings), settingName);
    FieldInfo fieldInfoDefault = AccessTools.Field(typeof(PsychologySettings), settingName + "Default");
    fieldInfoSetting.SetValue(null, fieldInfoDefault.GetValue(null));    
  }

  public static void ResetKinseyFormula() => kinseyFormula = kinseyFormulaDefault;

  public static void ResetKinseyWeightCustom() => kinseyWeightCustom = kinseyWeightCustomDefault.ListFullCopy();

  public static void ResetSpeciesSettings() => SpeciesHelper.ResetSpeciesDict(speciesDict);

  public static void ResetAgeCurves()
  {
    romanticDriveAgeCurvePoints = romanticDriveAgeCurvePointsDefault.ListFullCopy();
    femaleSexDriveAgeCurvePoints = femaleSexDriveAgeCurvePointsDefault.ListFullCopy();
    maleSexDriveAgeCurve = maleSexDriveAgeCurveDefault.ListFullCopy();
  }

  public static SimpleCurve SimpleCurveMinAgeEnforced(List<CurvePoint> list, float minAge) => new SimpleCurve(FilteredCurvePointsMinAgeEnforced(list, minAge));

  public static List<CurvePoint> FilteredCurvePointsMinAgeEnforced(List<CurvePoint> list, float minAge)
  {
    List<CurvePoint> list2 = (from point in list
                              where point.x > minAge
                              select point).ToList();
    list2.Add(new CurvePoint(minAge, 0f));
    return list2;
  }

}


