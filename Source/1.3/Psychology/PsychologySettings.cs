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

namespace Psychology
{
    public class PsychologySettings : ModSettings
    {
        public static bool enableKinseyDefault = true;
        public static bool enableKinsey = enableKinseyDefault;

        public static KinseyMode kinseyFormulaDefault = KinseyMode.Realistic;
        public static KinseyMode kinseyFormula = kinseyFormulaDefault;

        public static List<float> kinseyWeightCustomDefault = new List<float>() { 62.5f, 11.3f, 9.3f, 6.8f, 4.5f, 2.8f, 2.8f };
        public static List<float> kinseyWeightCustom = kinseyWeightCustomDefault;

        public static bool enableEmpathyDefault = true;
        public static bool enableEmpathy = enableEmpathyDefault;

        public static bool enableIndividualityDefault = true;
        public static bool enableIndividuality = enableIndividualityDefault;

        public static bool enableElectionsDefault = true;
        public static bool enableElections = enableElectionsDefault;

        public static bool enableDateLettersDefault = true;
        public static bool enableDateLetters = enableDateLettersDefault;

        public static bool enableImprisonedDebuffDefault = true; // v1.1
        public static bool enableImprisonedDebuff = enableImprisonedDebuffDefault; // v1.1

        public static bool enableAnxietyDefault = true; // v1.1
        public static bool enableAnxiety = enableAnxietyDefault; // v1.1

        public static float conversationDurationDefault = 60f;
        public static float conversationDuration = conversationDurationDefault;

        public static float romanceChanceMultiplierDefault = 1f; // v1.1
        public static float romanceChanceMultiplier = romanceChanceMultiplierDefault; // v1.1

        public static float romanceOpinionThresholdDefault = 5f; // v1.1
        public static float romanceOpinionThreshold = romanceOpinionThresholdDefault; // v1.1

        public static float mayorAgeDefault = 20f; // v1.1
        public static float mayorAge = mayorAgeDefault; // v1.1

        public static float traitOpinionMultiplierDefault = 0.25f; // v1.2
        public static float traitOpinionMultiplier = traitOpinionMultiplierDefault; // v1.2

        public static byte displayOptionDefault = 4; // v1.3
        public static byte displayOption = displayOptionDefault; // v1.3

        public static bool useColorsDefault = true; // v1.3
        public static bool useColors = useColorsDefault; // v1.3

        public static bool listAlphabeticalDefault = false; // v1.3
        public static bool listAlphabetical = listAlphabeticalDefault; // v1.3

        public static bool useAntonymsDefault = true; // v1.3
        public static bool useAntonyms = useAntonymsDefault; // v1.3

        public static Dictionary<string, SpeciesSettings> speciesDict = new Dictionary<string, SpeciesSettings>();

        public enum KinseyMode
        {
            Realistic,
            Uniform,
            Invisible,
            Gaypocalypse,
            Custom
        };

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
            enableKinsey = enableKinseyDefault;
        }

        public void ResetKinseyFormula()
        {
            kinseyFormula = kinseyFormulaDefault;
        }

        public void ResetKinseyWeightCustom()
        {
            kinseyWeightCustom = kinseyWeightCustomDefault;
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
            speciesDict = SpeciesHelper.speciesDictDefault;
        }

    }

    public class SpeciesSettings : IExposable
    {
        public bool enablePsyche = true;
        public bool enableAgeGap = true;
        public float minDatingAge = 14f;
        public float minLovinAge = 16f;

        public SpeciesSettings(bool bool1 = true, bool bool2 = true, float float1 = 14f, float float2 = 16f)
        {
            enablePsyche = bool1;
            enableAgeGap = bool2;
            minDatingAge = float1;
            minLovinAge = float2;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref enablePsyche, "enablePsyche", true);
            Scribe_Values.Look(ref enableAgeGap, "enableAgeGap", true);
            Scribe_Values.Look(ref minDatingAge, "minDatingAge", 14f);
            Scribe_Values.Look(ref minLovinAge, "minLovinAge", 16f);
        }
    }

    [StaticConstructorOnStartup]
    public class SpeciesHelper
    {
        public static IEnumerable<ThingDef> humanlikeDefs;
        public static Dictionary<string, SpeciesSettings> speciesDictDefault = new Dictionary<string, SpeciesSettings>();
        public static List<string> mindlessAndroidList = new List<string>() { "ChjDroid", "ChjBattleDroid", "Android1Tier", "M7Mech", "M8Mech" };
        public static List<string> sentientAndroidList = new List<string>() { "ChjAndroid", "Android2Tier", "Android3Tier", "Android4Tier", "Android5Tier" };

        static SpeciesHelper()
        {
            humanlikeDefs = from def in DefDatabase<ThingDef>.AllDefs
                            where def.race?.intelligence == Intelligence.Humanlike
                            orderby def.label ascending
                            select def;
            var zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");
            List<string> registered = new List<string>();
            foreach (ThingDef t in humanlikeDefs)
            {
                string name = t.defName;
                if (name.Contains("AIPawn") || name.Contains("Robot") || mindlessAndroidList.Contains(name) || (zombieThinkTree != null && t.race.thinkTreeMain == zombieThinkTree))
                {
                    speciesDictDefault.Add(name, new SpeciesSettings(false, false, -1f, -1f));
                }
                else if (sentientAndroidList.Contains(name))
                {
                    speciesDictDefault.Add(name, new SpeciesSettings(true, false, 0f, 0f));
                }
                else
                {
                    speciesDictDefault.Add(name, new SpeciesSettings());
                }
                if (!PsychologySettings.speciesDict.ContainsKey(name))
                {
                    PsychologySettings.speciesDict.Add(name, speciesDictDefault[name]);
                }
                if (!PsychologySettings.speciesDict[name].enablePsyche)
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
                registered.Add(t.defName);
            }

            if (Prefs.DevMode && Prefs.LogVerbose)
            {
                Log.Message("Psychology :: Registered " + string.Join(", ", registered.ToArray()));
            }
        }

        


    }

    [StaticConstructorOnStartup]
    public class DefInjector
    {
        static DefInjector()
        {
            /* Conditional vanilla Def edits */
            ThoughtDef knowGuestExecuted = AddNullifyingTraits("KnowGuestExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowGuestExecuted != null && PsychologySettings.enableEmpathy)
            {
                knowGuestExecuted = ModifyThoughtStages(knowGuestExecuted, new int[] { -1, -2, -4, -5 });
            }
            ThoughtDef knowColonistExecuted = AddNullifyingTraits("KnowColonistExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistExecuted != null && PsychologySettings.enableEmpathy)
            {
                knowColonistExecuted = ModifyThoughtStages(knowColonistExecuted, new int[] { -1, -2, -4, -5 });
            }
            ThoughtDef knowPrisonerDiedInnocent = AddNullifyingTraits("KnowPrisonerDiedInnocent", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowPrisonerDiedInnocent != null && PsychologySettings.enableEmpathy)
            {
                knowPrisonerDiedInnocent = ModifyThoughtStages(knowPrisonerDiedInnocent, new int[] { -4 });
            }
            ThoughtDef knowColonistDied = AddNullifyingTraits("KnowColonistDied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistDied != null && PsychologySettings.enableEmpathy)
            {
                knowColonistDied = ModifyThoughtStages(knowColonistDied, new int[] { -2 });
            }
            ThoughtDef colonistAbandoned = AddNullifyingTraits("ColonistBanished", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (colonistAbandoned != null && PsychologySettings.enableEmpathy)
            {
                colonistAbandoned = ModifyThoughtStages(colonistAbandoned, new int[] { -2 });
            }
            ThoughtDef colonistAbandonedToDie = AddNullifyingTraits("ColonistBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (colonistAbandonedToDie != null && PsychologySettings.enableEmpathy)
            {
                colonistAbandonedToDie = ModifyThoughtStages(colonistAbandonedToDie, new int[] { -4 });
            }
            ThoughtDef prisonerAbandonedToDie = AddNullifyingTraits("PrisonerBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (prisonerAbandonedToDie != null && PsychologySettings.enableEmpathy)
            {
                prisonerAbandonedToDie = ModifyThoughtStages(prisonerAbandonedToDie, new int[] { -3 });
            }
            ThoughtDef knowPrisonerSold = AddNullifyingTraits("KnowPrisonerSold", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowPrisonerSold != null && PsychologySettings.enableEmpathy)
            {
                knowPrisonerSold = ModifyThoughtStages(knowPrisonerSold, new int[] { -4 });
            }
            ThoughtDef knowGuestOrganHarvested = AddNullifyingTraits("KnowGuestOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowGuestOrganHarvested != null && PsychologySettings.enableEmpathy)
            {
                knowGuestOrganHarvested = ModifyThoughtStages(knowGuestOrganHarvested, new int[] { -4 });
            }
            ThoughtDef knowColonistOrganHarvested = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistOrganHarvested != null && PsychologySettings.enableEmpathy)
            {
                knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
            }
            ThoughtDef beauty = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistOrganHarvested != null && PsychologySettings.enableEmpathy)
            {
                knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
            }

            /*
             * Now to enjoy the benefits of having made a popular mod!
             * This will be our little secret.
             */
            Backstory child = new Backstory();
            Traverse.Create(child).Field("bodyTypeMale").SetValue("Male");
            Traverse.Create(child).Field("bodyTypeFemale").SetValue("Female");
            child.slot = BackstorySlot.Childhood;
            child.SetTitle("Child soldier", "Child soldier");
            child.SetTitleShort("Scout", "Scout");
            child.baseDesc = "[PAWN_nameDef] was born into a dictatorial outlander society on a nearby rimworld. Their chief export was war, and [PAWN_pronoun] was conscripted at a young age into the military to serve as a scout due to [PAWN_possessive] runner's build. [PAWN_pronoun] learned how to use a gun, patch wounds on the battlefield, and communicate with [PAWN_possessive] squad. It was there [PAWN_pronoun] earned [PAWN_possessive] nickname.";
            Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Shooting", 4);
            Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Medicine", 2);
            Traverse.Create(child).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
            child.requiredWorkTags = WorkTags.Violent;
            child.shuffleable = false;
            child.PostLoad();
            child.ResolveReferences();
            Backstory adultMale = new Backstory();
            Traverse.Create(adultMale).Field("bodyTypeMale").SetValue("Male");
            Traverse.Create(adultMale).Field("bodyTypeFemale").SetValue("Female");
            adultMale.slot = BackstorySlot.Adulthood;
            adultMale.SetTitle("Missing in action", "Missing in action");
            adultMale.SetTitleShort("Ex-P.O.W.", "Ex-P.O.W.");
            adultMale.baseDesc = "Eventually, [PAWN_pronoun] was captured on a mission by one of [PAWN_possessive] faction's many enemies. [PAWN_pronoun] was tortured for information, the techniques of which [PAWN_pronoun] never forgot. When they could get no more out of [PAWN_objective], [PAWN_pronoun] was sent to a prison camp, where [PAWN_pronoun] worked for years before staging an escape and fleeing into civilization.";
            Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Crafting", 4);
            Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Construction", 3);
            Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Mining", 2);
            Traverse.Create(adultMale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
            adultMale.spawnCategories = new List<string>();
            adultMale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
            adultMale.shuffleable = false;
            adultMale.PostLoad();
            adultMale.ResolveReferences();
            Backstory adultFemale = new Backstory();
            Traverse.Create(adultFemale).Field("bodyTypeMale").SetValue("Male");
            Traverse.Create(adultFemale).Field("bodyTypeFemale").SetValue("Female");
            adultFemale.slot = BackstorySlot.Adulthood;
            adultFemale.SetTitle("Battlefield medic", "Battlefield medic");
            adultFemale.SetTitleShort("Medic", "Medic");
            adultFemale.baseDesc = "[PAWN_pronoun] continued to serve in the military, being promoted through the ranks as [PAWN_possessive] skill increased. [PAWN_pronoun] learned how to treat more serious wounds as [PAWN_possessive] role slowly transitioned from scout to medic, as well as how to make good use of army rations. [PAWN_pronoun] built good rapport with [PAWN_possessive] squad as a result.";
            Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Shooting", 4);
            Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Medicine", 3);
            Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Cooking", 2);
            Traverse.Create(adultFemale).Field("skillGains").GetValue<Dictionary<string, int>>().Add("Social", 1);
            adultFemale.spawnCategories = new List<string>();
            adultFemale.spawnCategories.AddRange(new string[] { "Civil", "Raider", "Slave", "Trader", "Traveler" });
            adultFemale.shuffleable = false;
            adultFemale.PostLoad();
            adultFemale.ResolveReferences();
            PawnBio male = new PawnBio();
            male.childhood = child;
            male.adulthood = adultMale;
            male.gender = GenderPossibility.Male;
            male.name = NameTriple.FromString("Jason 'Jackal' Tarai");
            male.PostLoad();
            SolidBioDatabase.allBios.Add(male);
            PawnBio female = new PawnBio();
            female.childhood = child;
            female.adulthood = adultFemale;
            female.gender = GenderPossibility.Female;
            female.name = NameTriple.FromString("Elizabeth 'Eagle' Tarai");
            female.PostLoad();
            SolidBioDatabase.allBios.Add(female);
            BackstoryDatabase.AddBackstory(child);
            BackstoryDatabase.AddBackstory(adultMale);
            BackstoryDatabase.AddBackstory(adultFemale);
        }

        public static void RemoveTrait(Pawn pawn, TraitDef trait)
        {
            pawn.story.traits.allTraits.RemoveAll(t => t.def == trait);
        }

        public static ThoughtDef AddNullifyingTraits(String name, TraitDef[] traits)
        {
            ThoughtDef thought = ThoughtDef.Named(name);
            if (thought != null)
            {
                if (thought.nullifyingTraits == null)
                {
                    thought.nullifyingTraits = new List<TraitDef>();
                }
                foreach (TraitDef conflict in traits)
                {
                    thought.nullifyingTraits.Add(conflict);
                }
            }
            return thought;
        }

        public static ThoughtDef ModifyThoughtStages(ThoughtDef thought, int[] stages)
        {
            for (int stage = 0; stage < thought.stages.Count; stage++)
            {
                thought.stages[stage].baseMoodEffect = stages[stage];
            }
            return thought;
        }
    }

}

