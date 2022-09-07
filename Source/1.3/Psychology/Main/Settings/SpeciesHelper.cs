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
using System.Security.Cryptography;
using Verse.Sound;

namespace Psychology;

[StaticConstructorOnStartup]
public class SpeciesHelper
{
    public static IEnumerable<ThingDef> humanlikeDefs;
    //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
    public static List<string> mindlessList = new List<string>() { "ChjDroid", "ChjBattleDroid", "Android1Tier", "M7Mech", "M8Mech" };
    public static List<string> androidLikeList = new List<string>() { "ChjAndroid", "Android2Tier", "Android3Tier", "Android4Tier", "Android5Tier" };
    public static List<string> elfLikeList = new List<string>() { };
    public static List<string> mindlessSubstringList = new List<string> { "Robot", "AIPawn" };
    public static List<string> androidLikeSubstringList = new List<string> { "Android" };
    public static List<string> elfLikeSubstringList = new List<string>() { "Elf" };
    public static SpeciesSettings mindlessSettings = new SpeciesSettings(false, false, -1f, -1f);
    public static SpeciesSettings androidLikeSettings = new SpeciesSettings(true, false, 0f, 0f);
    public static SpeciesSettings elfLikeSettings = new SpeciesSettings(EnablePsyche: true, EnableAgeGap: false);
    public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();

    static SpeciesHelper()
    {
        humanlikeDefs = from def in DefDatabase<ThingDef>.AllDefs
                        where def.race?.intelligence == Intelligence.Humanlike
                        orderby def.label ascending
                        select def;
        ResetSpeciesDict(speciesDictDefault);
        List<string> registered = new List<string>();
        foreach (ThingDef t in humanlikeDefs)
        {
            string defName = t.defName;
            registered.Add(defName);
            if (!PsychologySettings.speciesDict.ContainsKey(defName))
            {
                PsychologySettings.speciesDict.Add(defName, speciesDictDefault[defName]);
            }
            if (!PsychologySettings.speciesDict[defName].enablePsyche)
            {
                continue;
            }
            RegisterPsycheEnabledDef(t);
        }
        Log.Message("Psychology: Registered humanlike species: " + string.Join(", ", registered.ToArray()));
        //Log.Message("SettingsWindowUtility.Initialize()");
        SettingsWindowUtility.Initialize();
    }

    public static void RegisterPsycheEnabledDef(ThingDef t)
    {
        if (t.inspectorTabsResolved == null)
        {
            t.inspectorTabsResolved = new List<InspectTabBase>(1);
        }
        if (t.race.corpseDef.inspectorTabsResolved == null)
        {
            t.race.corpseDef.inspectorTabsResolved = new List<InspectTabBase>(1);
        }
        t.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
        t.race.corpseDef.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
        if (t.recipes == null)
        {
            t.recipes = new List<RecipeDef>(6);
        }
        if (t.comps == null)
        {
            t.comps = new List<CompProperties>(1);
        }
        t.comps.Add(new CompProperties_Psychology());
        if (!t.race.hediffGiverSets.NullOrEmpty())
        {
            if (t.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
            {
                t.race.hediffGiverSets.Add(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
            }
        }
    }

    public static void ResetSpeciesDict(Dictionary<string, SpeciesSettings> speciesDict)
    {
        speciesDict.Clear();
        ThinkTreeDef zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
        bool zombieNotNull = zombieThinkTree != null;
        
        foreach (ThingDef def in humanlikeDefs)
        {
            // Allow hook for other mods
            AddSpeciesHook(speciesDict, def);
            // Allow XML override
            string name = def.defName;
            SpeciesSettingsDef speciesDef = DefDatabase<SpeciesSettingsDef>.GetNamedSilentFail(name);
            if (speciesDef != null)
            {
                speciesDict.Add(name, new SpeciesSettings(speciesDef.enablePsyche, speciesDef.enableAgeGap, speciesDef.minDatingAge, speciesDef.minLovinAge));
                continue;
            }
            // Start with default
            SpeciesSettings settings = new SpeciesSettings();
            // Set human to default
            if (name == "Human")
            {
                speciesDict.Add(name, settings);
                continue;
            }
            // Use explicit defNames from well-known mods
            if (mindlessList.Contains(name))
            {
                speciesDict.Add(name, mindlessSettings);
                continue;
            }
            if (androidLikeList.Contains(name))
            {
                speciesDict.Add(name, androidLikeSettings);
                continue;
            }
            if (elfLikeList.Contains(name))
            {
                speciesDict.Add(name, elfLikeSettings);
                continue;
            }
            // Use heuristics based on defNames
            if (mindlessSubstringList.Exists(entry => name.Contains(entry)) || (zombieNotNull && def.race.thinkTreeMain == zombieThinkTree))
            {
                settings.enablePsyche = false;
                //speciesDict.Add(name, mindlessSettings);
                //continue;
            }
            if (androidLikeSubstringList.Exists(entry => name.Contains(entry)))
            {
                settings.enableAgeGap = false;
                settings.minDatingAge = 0f;
                settings.minLovinAge = 0f;
                //speciesDict.Add(name, androidLikeSettings);
                //continue;
            }
            if (elfLikeSubstringList.Exists(entry => name.Contains(entry)))
            {
                settings.enableAgeGap = false;
                //speciesDict.Add(name, elfLikeSettings);
                //continue;
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
            // Go to default
            speciesDict.Add(name, settings);
        }
    }

    public static void AddSpeciesHook(Dictionary<string, SpeciesSettings> speciesDict, ThingDef t)
    {
        // For other modders
    }

    //public static void AddMindlessSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    //{
    //    speciesDict.Add(defName, new SpeciesSettings(false, false, -1f, -1f));
    //}

    //public static void AddNoChildhoodSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    //{
    //    speciesDict.Add(defName, new SpeciesSettings(true, false, 0f, 0f));
    //}
    //public static void AddElflikeSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    //{

    //}
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