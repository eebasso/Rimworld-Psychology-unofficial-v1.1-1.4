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

namespace Psychology;
[StaticConstructorOnStartup]
public class SpeciesHelper
{
    public static IEnumerable<ThingDef> humanlikeDefs;
    //public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
    public static List<string> mindlessList = new List<string>() { "ChjDroid", "ChjBattleDroid", "Android1Tier", "M7Mech", "M8Mech" };
    public static List<string> sentientAndroidList = new List<string>() { "ChjAndroid", "Android2Tier", "Android3Tier", "Android4Tier", "Android5Tier" };
    public static List<string> elflikeList = new List<string>() { };
    public static List<string> mindlessSubstringList = new List<string> { "Robot", "AIPawn" };
    public static List<string> sentientAndroidSubstringList = new List<string> { "Android" };

    static SpeciesHelper()
    {
        humanlikeDefs = from def in DefDatabase<ThingDef>.AllDefs
                        where def.race?.intelligence == Intelligence.Humanlike
                        orderby def.label ascending
                        select def;
        Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
        ResetSpeciesDict(speciesDictDefault);
        foreach (ThingDef t in humanlikeDefs)
        {
            string defName = t.defName;
            if (!PsychologySettings.speciesDict.ContainsKey(defName))
            {
                PsychologySettings.speciesDict.Add(defName, speciesDictDefault[defName]);
            }
            if (!PsychologySettings.speciesDict[defName].enablePsyche)
            {
                continue;
            }
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
        //Log.Message("SettingsWindowUtility.Initialize()");
        SettingsWindowUtility.Initialize();
    }

    public static void ResetSpeciesDict(Dictionary<string, SpeciesSettings> speciesDict)
    {
        speciesDict.Clear();
        ThinkTreeDef zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
        bool zombieNotNull = zombieThinkTree != null;
        List<string> registered = new List<string>();
        foreach (ThingDef t in humanlikeDefs)
        {
            string name = t.defName;
            AddSpeciesHook(speciesDict, t);
            SpeciesSettingsDef speciesDef = DefDatabase<SpeciesSettingsDef>.GetNamedSilentFail(name);
            if (speciesDef != null)
            {
                speciesDict.Add(name, new SpeciesSettings(speciesDef.enablePsyche, speciesDef.enableAgeGap, speciesDef.minDatingAge, speciesDef.minLovinAge));
            }
            else if (mindlessList.Contains(name))
            {
                speciesDict.Add(name, new SpeciesSettings(false, false, -1f, -1f));
            }
            else if (sentientAndroidList.Contains(name))
            {
                speciesDict.Add(name, new SpeciesSettings(true, false, 0f, 0f));
            }
            else if (elflikeList.Contains(name))
            {
                speciesDict.Add(name, new SpeciesSettings(EnablePsyche: true, EnableAgeGap: false));
            }
            else if (mindlessSubstringList.Exists(entry => name.Contains(entry)) || (zombieNotNull && t.race.thinkTreeMain == zombieThinkTree))
            {
                speciesDict.Add(name, new SpeciesSettings(false, false, -1f, -1f));
            }
            else if (sentientAndroidSubstringList.Exists(entry => name.Contains(entry)))
            {
                speciesDict.Add(name, new SpeciesSettings(true, false, 0f, 0f));
            }
            else
            {
                speciesDict.Add(name, new SpeciesSettings());
            }
            registered.Add(t.defName);
            if (Prefs.DevMode && Prefs.LogVerbose)
            {
                Log.Message("Psychology :: Registered " + string.Join(", ", registered.ToArray()));
            }
        }
    }

    public static void AddSpeciesHook(Dictionary<string, SpeciesSettings> speciesDict, ThingDef t)
    {
        // For other modders
    }

    public static void AddMindlessSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    {
        speciesDict.Add(defName, new SpeciesSettings(false, false, -1f, -1f));
    }

    public static void AddNoChildhoodSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    {
        speciesDict.Add(defName, new SpeciesSettings(true, false, 0f, 0f));
    }
    public static void AddElflikeSpecies(Dictionary<string, SpeciesSettings> speciesDict, string defName)
    {

    }
}