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
using System.Xml.Linq;
using System.Runtime;
//using System.Security.Cryptography;
//using Verse.Sound;

namespace Psychology;

[StaticConstructorOnStartup]
public class SpeciesHelper
{
    public static List<ThingDef> humanlikeDefs = new List<ThingDef>();
    //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
    public static List<string> mindlessList = new List<string>() { "ChjDroid", "ChjBattleDroid", "Android1Tier", "M7Mech", "M8Mech" };
    public static List<string> androidLikeList = new List<string>() { "ChjAndroid", "Android2Tier", "Android3Tier", "Android4Tier", "Android5Tier" };
    public static List<string> elfLikeList = new List<string>() { };
    public static List<string> mindlessSubstringList = new List<string> { "Robot", "AIPawn" };
    public static List<string> androidLikeSubstringList = new List<string> { "Android" };
    public static List<string> elfLikeSubstringList = new List<string>() { "Elf" };
    public static List<string> animalLikeSubstringList = new List<string>() { "morph", "Morph" };

    public static SpeciesSettings mindlessSettings = new SpeciesSettings(false, false, -1f, -1f);
    public static SpeciesSettings androidLikeSettings = new SpeciesSettings(true, false, 0f, 0f);
    public static SpeciesSettings elfLikeSettings = new SpeciesSettings(EnablePsyche: true, EnableAgeGap: false);
    public static SpeciesSettings animalLikeSettings = new SpeciesSettings(true, true, -1f, -1f);

    //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
    public static ThinkTreeDef zombieThinkTree;
    public static bool zombieNotNull;

    static SpeciesHelper()
    {
        Initialize();
    }

    public static void Initialize()
    {
        //Log.Message("SpeciesHelper()");
        IEnumerable<ThingDef> humanlikeDefsEnumerable = from def in DefDatabase<ThingDef>.AllDefs
                                                        where def.race?.intelligence == Intelligence.Humanlike
                                                        orderby def.label ascending
                                                        select def;

        //Log.Message("humanlikeDefs.Count() = " + humanlikeDefs.Count());
        //ResetSpeciesDict(speciesDictDefault);
        //List<string> registered = new List<string>();
        string defName;
        foreach (ThingDef t in humanlikeDefsEnumerable)
        {
            defName = t.defName;
            if (!PsychologySettings.speciesDict.ContainsKey(defName))
            {
                //Log.Message("PsychologySettings.speciesDict.ContainsKey(defName) == false");
                //PsychologySettings.speciesDict.Add(defName, speciesDictDefault[defName]);
                PsychologySettings.speciesDict[defName] = DefaultSettingsForSpeciesDef(t);
                Log.Message("PsychologySettings.speciesDict, added defName = " + defName);
            }
            //else
            //{
            //    Log.Message("PsychologySettings.speciesDict.ContainsKey(defName) == true");
            //}
            //Log.Message("SpeciesHelper.Initialize, add Comps for defName = " + defName);
            AddCompsToHumanlikeDef(t);
            //registered.Add(defName);
            //Log.Message("SpeciesHelper.Initialize, check speciesDict for defName = " + defName);
        }
        SettingsWindowUtility.Initialize();
    }

    public static SpeciesSettings GetOrMakeSpeciesSettingsFromThingDef(ThingDef pawnDef, bool noDating = false)
    {
        //if (humanlikeDef?.race?.intelligence != Intelligence.Humanlike)
        //{
        //    return null;
        //}
        if (PsychologySettings.speciesDict.TryGetValue(pawnDef.defName, out SpeciesSettings settings) != true)
        {
            settings = DefaultSettingsForSpeciesDef(pawnDef);
            if (noDating)
            {
                settings.minDatingAge = -1f;
                settings.minLovinAge = -1f;
            }
            PsychologySettings.speciesDict[pawnDef.defName] = settings;
            Log.Message("PsychologySettings.speciesDict, added defName = " + pawnDef.defName);
            AddEverythingExceptCompPsychology(pawnDef);
            if (pawnDef.race?.intelligence == Intelligence.Humanlike)
            {
                AddCompsToHumanlikeDef(pawnDef);
            }
            SettingsWindowUtility.Initialize();
        }
        return settings;
    }

    public static void AddCompsToHumanlikeDef(ThingDef humanlikeDef)
    {
        humanlikeDefs.AddDistinct(humanlikeDef);
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
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatChemicalInterest);
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatChemicalFascination);
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatDepression);
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatInsomnia);
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.CureAnxiety);
        pawnDef.recipes.AddDistinct(RecipeDefOfPsychology.TreatPyromania);

        if (!pawnDef.race.hediffGiverSets.NullOrEmpty())
        {
            if (pawnDef.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
            {
                pawnDef.race.hediffGiverSets.AddDistinct(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
            }
        }
    }

    public static void AddInspectorTabToDefAndCorpseDef(ThingDef t)
    {
        //Log.Message("AddInspectorTabToDefAndCorpseDef to thingdef = " + t.defName);
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
        speciesDict.Clear();
        zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
        zombieNotNull = zombieThinkTree != null;

        //Log.Message("ResetSpeciesDict humanlikeDefs.Count() = " + humanlikeDefs.Count());
        foreach (ThingDef def in humanlikeDefs)
        {
            SpeciesSettings settings = DefaultSettingsForSpeciesDef(def);
            speciesDict.Add(def.defName, settings);
        }
        //Log.Message("ResetSpeciesDict humanlikeDefs.Count() = " + humanlikeDefs.Count());
    }

    public static SpeciesSettings DefaultSettingsForSpeciesDef(ThingDef def)
    {
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
        // Use explicit defNames from well-known mods
        if (mindlessList.Contains(name))
        {
            return CopySpeciesSettingsFrom(mindlessSettings);
        }
        if (androidLikeList.Contains(name))
        {
            return CopySpeciesSettingsFrom(androidLikeSettings);
        }
        if (elfLikeList.Contains(name))
        {
            return CopySpeciesSettingsFrom(elfLikeSettings);
        }
        // Use heuristics based on defNames
        if (animalLikeSubstringList.Exists(entry => name.Contains(entry)))
        {
            // No beastiality from Pawnmorphs please
            return CopySpeciesSettingsFrom(animalLikeSettings);
        }
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
            SpeciesHelperAlienRace.HeuristicSettings(ref settings, def);
        }
        if (def.race?.lifeStageAges != null)
        {
            if (def.race.lifeStageAges.Exists(x => x.def.defName.Contains("Adult") && x?.minAge != null))
            {
                float adultAge = (def.race.lifeStageAges.First(x => x.def.defName.Contains("Adult"))).minAge;
                if (def.race.lifeStageAges.Exists(x => x.def.defName.Contains("Teenager") && x?.minAge != null))
                {
                    float teenAge = (def.race.lifeStageAges.First(x => x.def.defName.Contains("Teenager"))).minAge;
                    settings.minDatingAge = teenAge + (adultAge - teenAge) * (14f - 13f) / (18f - 13f);
                    settings.minLovinAge = teenAge + (adultAge - teenAge) * (16f - 13f) / (18f - 13f);
                }
                else
                {
                    settings.minDatingAge = adultAge * 14f / 18f;
                    settings.minLovinAge = adultAge * 16f / 18f;
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

    public static SpeciesSettings CopySpeciesSettingsFrom(SpeciesSettings settingsToCopy)
    {
        SpeciesSettings newSettings = new SpeciesSettings();
        newSettings.enablePsyche = settingsToCopy.enablePsyche;
        newSettings.enableAgeGap = settingsToCopy.enableAgeGap;
        newSettings.minDatingAge = settingsToCopy.minDatingAge;
        newSettings.minLovinAge = settingsToCopy.minLovinAge;
        return newSettings;
    }
}public static class SpeciesHelperAlienRace
{
    public static void HeuristicSettings(ref SpeciesSettings settings, ThingDef def)
    {
        AlienRace.ThingDef_AlienRace alienDef = def as AlienRace.ThingDef_AlienRace;
        if (alienDef != null)
        {
            if (alienDef?.alienRace?.compatibility?.IsSentient != null)
            {
                settings.enablePsyche = alienDef.alienRace.compatibility.IsSentient;
            }
            bool bool1 = true;
            bool bool2 = true;
            if (alienDef?.alienRace?.generalSettings?.immuneToAge != null)
            {
                bool1 = !alienDef.alienRace.generalSettings.immuneToAge;
            }
            if (alienDef?.alienRace?.compatibility?.IsFlesh != null)
            {
                bool2 = alienDef.alienRace.compatibility.IsFlesh;
            }
            settings.enableAgeGap = bool1 && bool2;
        }
    }
}