﻿using System;
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
using System.Xml.Linq;
using System.Runtime;
using System.Security.Cryptography;
//using System.Security.Cryptography;
//using Verse.Sound;

namespace Psychology;

[StaticConstructorOnStartup]
public class SpeciesHelper
{
  public static HashSet<ThingDef> registeredSpecies = new HashSet<ThingDef>();
  //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
  public static List<string> mindlessList = new List<string>() { "ChjDroid", "ChjBattleDroid", "Android1Tier", "M7Mech", "M8Mech" };
  public static List<string> androidLikeList = new List<string>() { "ChjAndroid", "Android2Tier", "Android3Tier", "Android4Tier", "Android5Tier" };
  public static List<string> elfLikeList = new List<string>() { };
  public static List<string> mindlessSubstringList = new List<string> { "Robot", "AIPawn" };
  public static List<string> androidLikeSubstringList = new List<string> { "Android" };
  public static List<string> elfLikeSubstringList = new List<string>() { "Elf" };
  //public static List<string> animalLikeSubstringList = new List<string>() { "morph", "Morph" };

  public static SpeciesSettings mindlessSettings = new SpeciesSettings(false, false, -1f, -1f);
  public static SpeciesSettings androidLikeSettings = new SpeciesSettings(true, false, 0f, 0f);
  public static SpeciesSettings elfLikeSettings = new SpeciesSettings(EnablePsyche: true, EnableAgeGap: false);
  public static SpeciesSettings animalLikeSettings = new SpeciesSettings(true, true, -1f, -1f);

  //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
  public static ThinkTreeDef zombieThinkTree;
  public static bool zombieNotNull;

  // At startup, add everything to every humanlike def, but only register Human to show up settings window
  //static SpeciesHelper()
  //{
  //    zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
  //    zombieNotNull = zombieThinkTree != null;
  //    foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs)
  //    {
  //        if (CheckIntelligenceAndAddEverythingToSpeciesDef(t, checkIntelligence: true, register: false, allowAddComp: true) != true)
  //        {
  //            continue;
  //        }
  //        if (t.defName != "Human")
  //        {
  //            continue;
  //        }
  //        if (registeredSpecies.Add(t))
  //        {
  //            ////Log.Message("SpeciesHelper(), registered = " + t);
  //        }
  //    }
  //    SettingsWindowUtility.Initialize();
  //}

  static SpeciesHelper()
  {
    zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
    zombieNotNull = zombieThinkTree != null;
    foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs)
    {
      CheckIntelligenceAndAddEverythingToSpeciesDef(t, checkIntelligence: true, register: false, allowAddComp: true);
      //if (CheckIntelligenceAndAddEverythingToSpeciesDef(t, checkIntelligence: true, register: false, allowAddComp: true) != true)
      //{
      //  continue;
      //}
      ////if (t.defName != "Human")
      ////{
      ////    continue;
      ////}
      //if (registeredSpecies.Add(t))
      //{
      //  //Log.Message("Psychology, registered species: " + t);
      //}
    }
    SettingsWindowUtility.SaveBackupOfSettings();
    SettingsWindowUtility.Initialize();
  }

  // Things that need to be done by these methods:
  // - Check intelligence
  // - Add everything except comp psychology to ThingDef
  // - Add comp psychology to ThingDef
  // - Create settings for ThingDef
  // - Add ThingDef to registered species
  // Principles:
  // - If we register a thingdef, we want to create settings for it
  public static bool IsHumanlikeIntelligence(ThingDef thingDef)
  {
    return thingDef?.race?.intelligence < Intelligence.Humanlike;
    //Intelligence? intelligence = thingDef?.race?.intelligence;
    //if (intelligence == null)
    //{
    //  return false;
    //}
    //if ((int)intelligence < (int)Intelligence.Humanlike)
    //{
    //  return false;
    //}
    //return true;
  }

  public static bool CheckIntelligenceAndAddEverythingToSpeciesDef(ThingDef thingDef, bool checkIntelligence = true, bool register = true, bool allowAddComp = true)
  {
    if (thingDef.category != ThingCategory.Pawn)
    {
      return false;
    }
    if (checkIntelligence && !IsHumanlikeIntelligence(thingDef))
    {
      return false;
    }
    string defName = thingDef.defName;
    if (!PsychologySettings.speciesDict.ContainsKey(defName))
    {
      PsychologySettings.speciesDict[defName] = DefaultSettingsForSpeciesDef(thingDef);
      ////Log.Message("CheckIntelligenceAndAddEverythingToHumanlikeDef, PsychologySettings.speciesDict, added defName = " + defName);
    }
    RegisterAndAddCompsToSpeciesDef(thingDef, register: register, addCompPsychology: allowAddComp);
    return true;
  }

  // To be used on game loaded
  //public static void RegisterHumanlikeSpeciesLoadedGame()
  //{
  //    ThingDef pawnDef;
  //    foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
  //    {
  //        pawnDef = pawn?.def;
  //        if (pawnDef == null)
  //        {
  //            continue;
  //        }
  //        if (CheckIntelligenceAndAddEverythingToSpeciesDef(pawnDef, checkIntelligence: true, register: true, allowAddComp: true))
  //        {
  //            PsycheHelper.PsychologyEnabled(pawn);
  //        }
  //    }
  //}

  // To be used after game (save file) is loaded
  public static SpeciesSettings GetOrMakeSpeciesSettingsFromThingDef(ThingDef pawnDef)
  {
    if (!PsychologySettings.speciesDict.TryGetValue(pawnDef.defName, out SpeciesSettings settings))
    {
      settings = DefaultSettingsForSpeciesDef(pawnDef);
      PsychologySettings.speciesDict[pawnDef.defName] = settings;
      ////Log.Message("GetOrMakeSpeciesSettingsFromThingDef, added to PsychologySettings.speciesDict, defName = " + pawnDef.defName);
    }
    RegisterAndAddCompsToSpeciesDef(pawnDef, register: true, addCompPsychology: IsHumanlikeIntelligence(pawnDef));
    return settings;
  }

  public static void RegisterAndAddCompsToSpeciesDef(ThingDef thingDef, bool register, bool addCompPsychology)
  {
    if (register && !registeredSpecies.Add(thingDef))
    {
      return;
    }
    AddEverythingExceptCompPsychology(thingDef);
    if (addCompPsychology)
    {
      AddCompPsychologyToHumanlikeDef(thingDef);
    }
    SettingsWindowUtility.SaveBackupOfSettings();
    SettingsWindowUtility.Initialize();
  }

  public static void AddCompPsychologyToHumanlikeDef(ThingDef humanlikeDef)
  {
    if (humanlikeDef.comps == null)
    {
      humanlikeDef.comps = new List<CompProperties>(1);
    }
    humanlikeDef.comps.AddDistinct(new CompProperties_Psychology());
  }

  public static void AddEverythingExceptCompPsychology(ThingDef pawnDef)
  {
    AddInspectorTabToDefAndCorpseDef(pawnDef);
    if (pawnDef.recipes == null)
    {
      pawnDef.recipes = new List<RecipeDef>(6);
    }
    if (PsychologySettings.enableAnxiety)
    {
      pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.CureAnxiety);
    }
    pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatChemicalInterest);
    pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatChemicalFascination);
    pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatDepression);
    pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatInsomnia);
    pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatPyromania);

    if (!pawnDef.race.hediffGiverSets.NullOrEmpty() && PsychologySettings.enableAnxiety)
    {
      if (pawnDef.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
      {
        pawnDef.race.hediffGiverSets.AddDistinct(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
      }
    }
  }

  public static void AddInspectorTabToDefAndCorpseDef(ThingDef t)
  {
    ////Log.Message("AddInspectorTabToDefAndCorpseDef to thingdef = " + t.defName);
    if (t.inspectorTabsResolved == null)
    {
      t.inspectorTabsResolved = new List<InspectTabBase>(1);
    }
    t.inspectorTabsResolved.AddDistinct(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
    if (t.race?.corpseDef == null)
    {
      Log.Warning("thingDef.race?.corpseDef == null for thingDef = " + t.defName);
      return;
    }
    if (t.race.corpseDef.inspectorTabsResolved == null)
    {
      t.race.corpseDef.inspectorTabsResolved = new List<InspectTabBase>(1);
    }
    t.race.corpseDef.inspectorTabsResolved.AddDistinct(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
  }

  public static void ResetSpeciesDict(Dictionary<string, SpeciesSettings> speciesDict)
  {
    foreach (ThingDef def in registeredSpecies)
    {
      speciesDict[def.defName] = DefaultSettingsForSpeciesDef(def);
    }
    SettingsWindowUtility.Initialize();
  }

  public static SpeciesSettings DefaultSettingsForSpeciesDef(ThingDef def)
  {
    if (def == null)
    {
      Log.Error("DefaultSettingsForSpeciesDef, ThingDef was null");
      return new SpeciesSettings();
    }
    // Allow XML override
    string name = def.defName;
    SpeciesSettingsDef speciesDef = DefDatabase<SpeciesSettingsDef>.GetNamedSilentFail(name);
    if (speciesDef != null)
    {
      return new SpeciesSettings(speciesDef.enablePsyche, speciesDef.enableAgeGap, speciesDef.minDatingAge, speciesDef.minLovinAge);
    }
    // Start with default
    SpeciesSettings settings = new SpeciesSettings();
    // Set human to default
    if (name == "Human")
    {
      return settings;
    }
    // No beastiality please
    if (IsHumanlikeIntelligence(def) != true)
    {
      return new SpeciesSettings(animalLikeSettings);
    }
    // Use explicit defNames from well-known mods
    if (mindlessList.Contains(name))
    {
      return new SpeciesSettings(mindlessSettings);
    }
    if (androidLikeList.Contains(name))
    {
      return new SpeciesSettings(androidLikeSettings);
    }
    if (elfLikeList.Contains(name))
    {
      return new SpeciesSettings(elfLikeSettings);
    }
    // Use heuristics based on defNames
    if (mindlessSubstringList.Exists(entry => name.Contains(entry)) || (zombieNotNull && def.race.thinkTreeMain == zombieThinkTree))
    {
      settings.enablePsyche = false;
    }
    if (androidLikeSubstringList.Exists(entry => name.Contains(entry)))
    {
      settings.enableAgeGap = false;
      settings.minDatingAge = 0f;
      settings.minLovinAge = 0f;
    }
    if (elfLikeSubstringList.Exists(entry => name.Contains(entry)))
    {
      settings.enableAgeGap = false;
    }
    if (ModsConfig.IsActive("erdelf.HumanoidAlienRaces"))
    {
      AlienRaceHeuristicSettingsHook(settings, def);
    }
    if (def.race?.lifeStageAges != null)
    {
      //if (def.race.lifeStageAges.Exists(x => x.def.defName.Contains("Adult") && x?.minAge != null))
      //{
      //    float adultAge = (def.race.lifeStageAges.First(x => x.def.defName.Contains("Adult"))).minAge;
      //    if (def.race.lifeStageAges.Exists(x => x.def.defName.Contains("Teenager") && x?.minAge != null))
      //    {
      //        float teenAge = (def.race.lifeStageAges.First(x => x.def.defName.Contains("Teenager"))).minAge;
      //        settings.minDatingAge = teenAge + (adultAge - teenAge) * (14f - 13f) / (18f - 13f);
      //        settings.minLovinAge = teenAge + (adultAge - teenAge) * (16f - 13f) / (18f - 13f);
      //    }
      //    else
      //    {
      //        settings.minDatingAge = adultAge * 14f / 18f;
      //        settings.minLovinAge = adultAge * 16f / 18f;
      //    }
      //}
      //else
      //{
      //    settings.minDatingAge = -1f;
      //    settings.minLovinAge = -1f;
      //}

      bool foundTeen = false;
      bool foundAdult = false;
      float ageTeen = -1f;
      float ageAdult = -1f;
      foreach (LifeStageAge lifeStageAge in def.race.lifeStageAges)
      {
        if (lifeStageAge?.def?.defName == null || lifeStageAge?.minAge == null)
        {
          continue;
        }
        if (lifeStageAge.def.defName.Contains("Teenage"))
        {
          ageTeen = Mathf.Max(ageTeen, lifeStageAge.minAge);
          foundTeen = true;
        }
        if (lifeStageAge.def.defName.Contains("Adult"))
        {
          ageAdult = Mathf.Max(ageAdult, lifeStageAge.minAge);
          foundAdult = true;
        }
      }
      if (foundAdult)
      {
        if (foundTeen)
        {
          settings.minDatingAge = ageTeen + (ageAdult - ageTeen) * (14f - 13f) / (18f - 13f);
          settings.minLovinAge = ageTeen + (ageAdult - ageTeen) * (16f - 13f) / (18f - 13f);
        }
        else
        {
          settings.minDatingAge = ageAdult * 14f / 18f;
          settings.minLovinAge = ageAdult * 16f / 18f;
        }
      }
      else
      {
        settings.minDatingAge = -1f;
        settings.minLovinAge = -1f;
      }
    }
    return settings;
  }

  public static bool MinLifestageAge(ThingDef def, bool isTeen, out float age)
  {
    age = -1f;
    if (def?.race?.lifeStageAges == null)
    {
      return false;
    }
    bool foundTeen = false;

    if (isTeen)
    {
      foreach (LifeStageAge lifeStageAge in def.race.lifeStageAges)
      {
        if (lifeStageAge?.minAge != null && lifeStageAge.def.defName.Contains("Teenage"))
        {
          age = Mathf.Max(age, lifeStageAge.minAge) * 13f / 13f;
          foundTeen = true;
        }
      }
      if (foundTeen)
      {
        return true;
      }
    }
    if (def.race.lifeStageAges.Exists(x => x?.minAge != null && x.def.defName.Contains("Adult")))
    {
      age = def.race.lifeStageAges.First(x => x?.minAge != null && x.def.defName.Contains("Adult")).minAge * 15f / 18f;
      return true;
    }
    return false;
  }

  /// <summary>
  /// Uses both lifestage and settings info to determine whether <paramref name="pawn"/> can participate in romantic behavior. This checks dating if <paramref name="isDating"/> is true and lovin if false.
  /// </summary>
  /// <param name="pawn">The pawn in question</param>
  /// <param name="isDating">Checks dating if true, and lovin if false.</param>
  /// <returns>True or False, with True meaning that the <paramref name="pawn"/> can participate in dating/lovin.</returns>
  public static bool RomanceEnabled(Pawn pawn, bool isDating)
  {
    return RomanceLifestageAgeCheck(pawn, isDating) && RomanceSettingsAgeCheck(pawn, isDating);
  }

  /// <summary>
  /// Uses lifestage info to determine whether <paramref name="pawn"/> can participate in romantic behavior. This checks dating if <paramref name="isDating"/> is true and lovin if false.
  /// </summary>
  /// <param name="pawn">The pawn in question</param>
  /// <param name="isDating">Checks dating if true, and lovin if false.</param>
  /// <returns>True or False, with True meaning that the <paramref name="pawn"/> can participate in dating/lovin.</returns>
  public static bool RomanceLifestageAgeCheck(Pawn pawn, bool isDating)
  {
    if (!PsycheHelper.PsychologyEnabled(pawn))
    {
      return false;
    }
    if (pawn.DevelopmentalStage.Juvenile())
    {
      return false;
    }
    // Allow teenagers to date but not do lovin
    if (!MinLifestageAge(pawn.def, isDating, out float minLifestageAge))
    {
      //Log.Message("RomanceLifestageAgeCheck, !MinLifestageAge, pawn: " + pawn + ", isDating: " + isDating + ", minLifestageAge: " + minLifestageAge);
      return false;
    }
    Pawn_AgeTracker tracker = pawn?.ageTracker;
    if (tracker == null)
    {
      return false;
    }
    if (tracker.AgeBiologicalYearsFloat < minLifestageAge)
    {
      return false;
    }
    if (tracker.CurLifeStage == LifeStageDefOf.HumanlikeAdult)
    {
      return true;
    }
    if (!isDating && !tracker.Adult)
    {
      return false;
    }
    return true;
  }

  /// <summary>
  /// Uses settings info to determine whether <paramref name="pawn"/> can participate in romantic behavior. This checks dating if <paramref name="isDating"/> is true and lovin if false.
  /// </summary>
  /// <param name="pawn">The pawn in question</param>
  /// <param name="isDating">Checks dating if true, and lovin if false.</param>
  /// <returns>True or False, with True meaning that the <paramref name="pawn"/> can participate in dating/lovin.</returns>
  private static bool RomanceSettingsAgeCheck(Pawn pawn, bool isDating)
  {
    if (!PsycheHelper.PsychologyEnabled(pawn))
    {
      return false;
    }
    SpeciesSettings settings = GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
    if (!settings.enablePsyche)
    {
      return false;
    }
    float minSettingsAge = isDating ? settings.minDatingAge : settings.minLovinAge;
    if (minSettingsAge < 0f)
    {
      return false;
    }
    if (minSettingsAge == 0f)
    {
      return true;
    }
    return pawn.ageTracker != null && minSettingsAge < pawn.ageTracker.AgeBiologicalYearsFloat;
  }

  public static void AlienRaceHeuristicSettingsHook(SpeciesSettings settings, ThingDef def)
  {
    return;
  }



}

//public static class SpeciesHelperAlienRace
//{
//    public static void HeuristicSettings(SpeciesSettings settings, ThingDef def)
//    {
//        //AlienRace.ThingDef_AlienRace alienDef = def as AlienRace.ThingDef_AlienRace;
//        if (def is AlienRace.ThingDef_AlienRace alienDef)
//        {
//            if (alienDef?.alienRace?.compatibility?.IsSentient != null)
//            {
//                settings.enablePsyche = alienDef.alienRace.compatibility.IsSentient;
//            }
//            bool bool1 = true;
//            bool bool2 = true;
//            if (alienDef?.alienRace?.generalSettings?.immuneToAge != null)
//            {
//                bool1 = !alienDef.alienRace.generalSettings.immuneToAge;
//            }
//            if (alienDef?.alienRace?.compatibility?.IsFlesh != null)
//            {
//                bool2 = alienDef.alienRace.compatibility.IsFlesh;
//            }
//            settings.enableAgeGap = bool1 && bool2;
//        }
//    }
//}


