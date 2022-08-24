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
using HugsLib.Settings;
using HugsLib;
using UnityEngine;
using HarmonyLib;

namespace Psychology
{
    public class PsychologyBase : ModBase
    {
        protected override bool HarmonyAutoPatch => false;
        public static SettingHandle<bool> toggleKinsey;
        public static SettingHandle<KinseyMode> kinseyModeSettingHandle;
        public static SettingHandle<float> kinsey0WeightSettingHandle;
        public static SettingHandle<float> kinsey1WeightSettingHandle;
        public static SettingHandle<float> kinsey2WeightSettingHandle;
        public static SettingHandle<float> kinsey3WeightSettingHandle;
        public static SettingHandle<float> kinsey4WeightSettingHandle;
        public static SettingHandle<float> kinsey5WeightSettingHandle;
        public static SettingHandle<float> kinsey6WeightSettingHandle;
        public static SettingHandle<bool> toggleEmpathy;
        public static SettingHandle<bool> toggleIndividuality;
        public static SettingHandle<bool> toggleElections;
        public static SettingHandle<float> convoDurationSettingHandle;
        public static SettingHandle<bool> toggleDateLetters;
        public static SettingHandle<bool> toggleImprisonedDebuff; // v1.1
        public static SettingHandle<bool> toggleAnxiety; // v1.1
        public static SettingHandle<float> romanceChanceMultiplierSettingHandle; // v1.1
        public static SettingHandle<float> romanceOpinionThresholdSettingHandle; // v1.1
        public static SettingHandle<float> mayorAgeSettingHandle; // v1.1
        public static SettingHandle<float> traitOpinionMultiplierSettingHandle; // v1.2
        public static SettingHandle<byte> DistanceFromMiddleSettingsHandle;
        public static SettingHandle<bool> UseColorsSettingsHandle; // v1.3
        public static SettingHandle<bool> ListAlphabeticalSettingsHandle; // v1.3
        public static SettingHandle<bool> UseAntonymsSettingsHandle; // v1.3
        public static SettingHandle<SpeciesRomanceHandler> SpeciesRomanceSettingsHandle; // v1.3
        public List<Thing> corpsesOnMapList = new List<Thing>();
        public static bool showSpeciesRomanceOptions = false;
        public static Backstory child = new Backstory();
        public enum KinseyMode
        {
            Realistic,
            Uniform,
            Invisible,
            Gaypocalypse,
            Custom
        };

        public static void UpdatePersonalityDisplaySetting()
        {
            DistanceFromMiddleSettingsHandle.Value = PsycheCardUtility.DistanceFromMiddle;
            UseColorsSettingsHandle.Value = PsycheCardUtility.UseColorsBool;
            ListAlphabeticalSettingsHandle.Value = PsycheCardUtility.AlphabeticalBool;
            UseAntonymsSettingsHandle.Value = PsycheCardUtility.UseAntonymsBool;
        }

        static public bool ActivateKinsey()
        {
            //return kinsey;
            return toggleKinsey.Value;
        }

        static public KinseyMode KinseyFormula()
        {
            //return mode;
            return kinseyModeSettingHandle.Value;
        }

        static public float kinsey0Weight()
        {
            return kinsey0WeightSettingHandle.Value;
        }

        static public float kinsey1Weight()
        {
            return kinsey1WeightSettingHandle.Value;
        }

        static public float kinsey2Weight()
        {
            return kinsey2WeightSettingHandle.Value;
        }

        static public float kinsey3Weight()
        {
            return kinsey3WeightSettingHandle.Value;
        }

        static public float kinsey4Weight()
        {
            return kinsey4WeightSettingHandle.Value;
        }

        static public float kinsey5Weight()
        {
            return kinsey5WeightSettingHandle.Value;
        }

        static public float kinsey6Weight()
        {
            return kinsey6WeightSettingHandle.Value;
        }

        static public bool IndividualityOn()
        {
            return toggleIndividuality.Value;
        }

        static public bool ActivateElections()
        {
            return toggleElections.Value;
        }

        static public float ConvoDuration()
        {
            return convoDurationSettingHandle.Value;
        }

        static public bool SendDateLetters()
        {
            return toggleDateLetters.Value;
        }

        static public bool ImprisonedDebuff()
        {
            return toggleImprisonedDebuff.Value;
        }

        static public bool AnxietyEnabled()
        {
            return toggleAnxiety.Value;
        }

        static public float RomanceChance()
        {
            return romanceChanceMultiplierSettingHandle.Value;
        }

        static public float RomanceThreshold()
        {
            return romanceOpinionThresholdSettingHandle.Value;
        }

        static public float MayorAge()
        {
            return mayorAgeSettingHandle.Value;
        }

        public static float TraitOpinionMultiplier()
        {
            ModSettingsPack test = HugsLibController.Instance.Settings.GetModSettings("Community.Psychology.UnofficialUpdate");
            if (test.ValueExists("TraitOpinionMultiplier"))
            {
                return Convert.ToSingle(test.PeekValue("TraitOpinionMultiplier"));
            }
            return 0.25f;
        }

        public override string ModIdentifier
        {
            get
            {
                return "Psychology";
            }
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

        public override void SettingsChanged()
        {
            if (!toggleElections.Value)
            {
                MayorUtility.DeleteAllMayorHediffs();
            }
        }

        public override void DefsLoaded()
        {
            if (!ModIsActive)
            {
                return;
            }
            /* Mod settings */
            toggleKinsey = Settings.GetHandle<bool>("EnableSexuality", "SexualityChangesTitle".Translate(), "SexualityChangesTooltip".Translate(), true);

            kinseyModeSettingHandle = Settings.GetHandle<KinseyMode>("KinseyMode", "KinseyModeTitle".Translate(), "KinseyModeTooltip".Translate(), KinseyMode.Realistic, null, "KinseyMode_");
            kinseyModeSettingHandle.VisibilityPredicate = delegate { return toggleKinsey.Value; };

            kinsey0WeightSettingHandle = Settings.GetHandle<float>("KW0", "KWTitle".Translate(0), "KWTooltip".Translate(0), 62.5f, (String s) => float.Parse(s) >= 0f);
            kinsey1WeightSettingHandle = Settings.GetHandle<float>("KW1", "KWTitle".Translate(1), "KWTooltip".Translate(1), 11.3f, (String s) => float.Parse(s) >= 0f);
            kinsey2WeightSettingHandle = Settings.GetHandle<float>("KW2", "KWTitle".Translate(2), "KWTooltip".Translate(2), 9.3f, (String s) => float.Parse(s) >= 0f);
            kinsey3WeightSettingHandle = Settings.GetHandle<float>("KW3", "KWTitle".Translate(3), "KWTooltip".Translate(3), 6.8f, (String s) => float.Parse(s) >= 0f);
            kinsey4WeightSettingHandle = Settings.GetHandle<float>("KW4", "KWTitle".Translate(4), "KWTooltip".Translate(4), 4.5f, (String s) => float.Parse(s) >= 0f);
            kinsey5WeightSettingHandle = Settings.GetHandle<float>("KW5", "KWTitle".Translate(5), "KWTooltip".Translate(5), 2.8f, (String s) => float.Parse(s) >= 0f);
            kinsey6WeightSettingHandle = Settings.GetHandle<float>("KW6", "KWTitle".Translate(6), "KWTooltip".Translate(6), 2.8f, (String s) => float.Parse(s) >= 0f);
            kinsey0WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey1WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey2WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey3WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey4WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey5WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };
            kinsey6WeightSettingHandle.VisibilityPredicate = delegate { return kinseyModeSettingHandle.Value == KinseyMode.Custom && toggleKinsey.Value; };

            toggleEmpathy = Settings.GetHandle<bool>("EnableEmpathy", "EmpathyChangesTitle".Translate(), "EmpathyChangesTooltip".Translate(), true);

            toggleIndividuality = Settings.GetHandle<bool>("EnableIndividuality", "IndividualityTitle".Translate(), "IndividualityTooltip".Translate(), true);

            toggleElections = Settings.GetHandle<bool>("EnableElections", "ElectionsTitle".Translate(), "ElectionsTooltip".Translate(), true);

            convoDurationSettingHandle = Settings.GetHandle<float>("ConversationDuration", "DurationTitle".Translate(), "DurationTooltip".Translate(), 60f, (String s) => float.Parse(s) >= 15f && float.Parse(s) <= 180f);

            toggleDateLetters = Settings.GetHandle<bool>("SendDateLetters", "SendDateLettersTitle".Translate(), "SendDateLettersTooltip".Translate(), true);

            toggleImprisonedDebuff = Settings.GetHandle<bool>("ImprisonedOpinion", "ImprisonedTitle".Translate(), "ImprisonedTooltip".Translate(), true);

            toggleAnxiety = Settings.GetHandle<bool>("AllowAnxiety", "AllowAnxietyTitle".Translate(), "AllowAnxietyTooltip".Translate(), true);

            romanceChanceMultiplierSettingHandle = Settings.GetHandle<float>("RomanceChanceMultiplier", "RomanceMultiplierTitle".Translate(), "RomanceMultiplierTooltip".Translate(), 1f, (String s) => float.Parse(s) >= 0f);

            romanceOpinionThresholdSettingHandle = Settings.GetHandle<float>("RomanceChanceThreshold", "RomanceChanceThresholdTitle".Translate(), "RomanceChanceThresholdTooltip".Translate(), 5f, Validators.FloatRangeValidator(-90f, 90f));

            mayorAgeSettingHandle = Settings.GetHandle<float>("MayorAge", "MayorAgeTitle".Translate(), "MayorAgeTooltip".Translate(), 20f, (String s) => float.Parse(s) >= 0f);

            traitOpinionMultiplierSettingHandle = Settings.GetHandle<float>("TraitOpinionMultiplier", "TraitOpinionMultplierTitle".Translate(), "TraitOpinionMultplierTooltip".Translate(), 0.25f, Validators.FloatRangeValidator(0f, 2f));

            DistanceFromMiddleSettingsHandle = Settings.GetHandle("DisplayOptions", "DisplayOptionsTitle", "DisplayOptionsDesc", (byte)4);
            DistanceFromMiddleSettingsHandle.NeverVisible = true;

            UseColorsSettingsHandle = Settings.GetHandle("UseColors", "UseColorsTitle", "UseColorsDesc", true);
            UseColorsSettingsHandle.NeverVisible = true;

            ListAlphabeticalSettingsHandle = Settings.GetHandle("ListAlpha", "ListAlphaTitle", "ListAlphaDesc", false);
            ListAlphabeticalSettingsHandle.NeverVisible = true;

            UseAntonymsSettingsHandle = Settings.GetHandle("UseAntonyms", "UseAntonymsTitle", "UseAntonymsDesc", true);
            UseAntonymsSettingsHandle.NeverVisible = true;

            var toolToggle = Settings.GetHandle<bool>("ShowSpeciesRomanceOptions", "Customize romance behavior per species", null);
            toolToggle.Unsaved = true;
            toolToggle.CustomDrawerFullWidth = rect =>
            {
                if (Widgets.ButtonText(rect, "Customize romance behavior by species"))
                {
                    showSpeciesRomanceOptions = !showSpeciesRomanceOptions;
                }
                return false;
            };
            Log.Message("humanlikeDef");
            IEnumerable<ThingDef> humanlikeDefs = from def in DefDatabase<ThingDef>.AllDefs
                                                  where def.race?.intelligence == Intelligence.Humanlike
                                                  orderby def.label ascending
                                                  select def;

            Log.Message("SpeciesRomanceSettingsHandle");
            SpeciesRomanceSettingsHandle = Settings.GetHandle<SpeciesRomanceHandler>("SpeciesRomanceSettings", "SpeciesRomanceSettings", null);
            Log.Message("SpeciesRomanceSettingsHandle");
            SpeciesRomanceSettingsHandle.VisibilityPredicate = delegate { return showSpeciesRomanceOptions; };
            if (SpeciesRomanceSettingsHandle.Value == null)
            {
                SpeciesRomanceSettingsHandle.Value = new SpeciesRomanceHandler();
            }
            // foreach defaultsetting in defaultsettinglist
            // if SpeciesRomanceSettingsHandle.Value.InnerList does not contain key from defaultsetting
            // add(key,value) to SpeciesRomanceSettingsHandle.Value.InnerList from defaultsetting
            float rowHeight = 32f;
            float rowSpacing = 2f;
            float checkboxSize = 24f;
            Vector2 checkboxVec = new Vector2(0.5f * checkboxSize, 0.5f * checkboxSize);

            SpeciesRomanceSettingsHandle.CustomDrawerHeight = (rowHeight + rowSpacing) * (humanlikeDefs.Count() + 1);
            SpeciesRomanceSettingsHandle.CustomDrawerFullWidth = rect =>
            {
                Dictionary<string, SpeciesRomanceSettings> selection = SpeciesRomanceSettingsHandle.Value.InnerList;
                string buffer;
                Rect labelRect = new Rect(rect.x, rect.y, 0.4f * rect.width, rowHeight);
                Rect psychologyEnabledRect = new Rect(labelRect.xMax, rect.y, 0.15f * rect.width, rowHeight);
                Rect ageGapRect = new Rect(psychologyEnabledRect.xMax, rect.y, 0.15f * rect.width, rowHeight);
                Rect minDatingAgeRect = new Rect(ageGapRect.xMax, rect.y, 0.15f * rect.width, rowHeight);
                Rect minLovinAgeRect = new Rect(minDatingAgeRect.xMax, rect.y, 0.15f * rect.width, rowHeight);

                //Vector2 psychologyEnabledVec = psychologyEnabledRect.center - checkboxVec;
                //Vector2 ageGapVec = ageGapRect.center - checkboxVec;
                Vector2 psychologyEnabledVec = new Vector2();

                Widgets.Label(labelRect, "Species name");
                Widgets.Label(psychologyEnabledRect, "Psyche?");
                Widgets.Label(ageGapRect, "Age gap?");
                Widgets.Label(minDatingAgeRect, "Dating age");
                Widgets.Label(minLovinAgeRect, "Lovin age");

                foreach (ThingDef def in humanlikeDefs)
                {
                    labelRect.y += rowHeight + rowSpacing;
                    psychologyEnabledVec.y += rowHeight + rowSpacing;
                    ageGapVec.y += rowHeight + rowSpacing;
                    minDatingAgeRect.y += rowHeight + rowSpacing;
                    minLovinAgeRect.y += rowHeight + rowSpacing;

                    string defName = def.defName;
                    string label = def.label.CapitalizeFirst();
                    if (!selection.ContainsKey(defName))
                    {
                        selection.Add(defName, new SpeciesRomanceSettings());
                    }
                    Widgets.Label(labelRect, label);
                    Widgets.Checkbox(psychologyEnabledVec , ref selection[defName].psychologyEnabled);
                    if (selection[defName].psychologyEnabled)
                    {
                        Widgets.Checkbox(ageGapVec, ref selection[defName].caresAboutAgeGap);
                        //selection[defName].minDatingAge = Convert.ToSingle(Widgets.TextField(minDatingAgeRect, Convert.ToString(selection[defName].minDatingAge)));
                        //selection[defName].minLovinAge = Convert.ToSingle(Widgets.TextField(minDatingAgeRect, Convert.ToString(selection[defName].minLovinAge)));
                        buffer = Convert.ToString(selection[defName].minDatingAge);
                        Widgets.TextFieldNumeric<float>(minDatingAgeRect, ref selection[defName].minDatingAge, ref buffer);
                        if (selection[defName].minLovinAge < selection[defName].minDatingAge)
                        {
                            selection[defName].minLovinAge = selection[defName].minDatingAge;
                        }
                        buffer = Convert.ToString(selection[defName].minLovinAge);
                        Widgets.TextFieldNumeric<float>(minLovinAgeRect, ref selection[defName].minLovinAge, ref buffer);
                        if (selection[defName].minLovinAge < selection[defName].minDatingAge)
                        {
                            selection[defName].minDatingAge = selection[defName].minLovinAge;
                        }
                    }

                    
                }
                SpeciesRomanceSettingsHandle.Value.InnerList = selection;
                return false;
            };

            PsycheCardUtility.DistanceFromMiddle = DistanceFromMiddleSettingsHandle.Value; //1.3
            PsycheCardUtility.DistanceFromMiddleCached = PsycheCardUtility.DistanceFromMiddle;
            PsycheCardUtility.UseColorsBool = UseColorsSettingsHandle.Value; //1.3
            PsycheCardUtility.UseColorsBoolCached = PsycheCardUtility.UseColorsBool;
            PsycheCardUtility.AlphabeticalBool = ListAlphabeticalSettingsHandle.Value; //1.3
            PsycheCardUtility.AlphabeticalBoolCached = PsycheCardUtility.AlphabeticalBool;
            PsycheCardUtility.UseAntonymsBool = UseAntonymsSettingsHandle.Value; //1.3
            PsycheCardUtility.UseAntonymsBoolCached = PsycheCardUtility.UseAntonymsBool;

            /* Conditional vanilla Def edits */
            ThoughtDef knowGuestExecuted = AddNullifyingTraits("KnowGuestExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowGuestExecuted != null && toggleEmpathy)
            {
                knowGuestExecuted = ModifyThoughtStages(knowGuestExecuted, new int[] { -1, -2, -4, -5 });
            }
            ThoughtDef knowColonistExecuted = AddNullifyingTraits("KnowColonistExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistExecuted != null && toggleEmpathy)
            {
                knowColonistExecuted = ModifyThoughtStages(knowColonistExecuted, new int[] { -1, -2, -4, -5 });
            }
            ThoughtDef knowPrisonerDiedInnocent = AddNullifyingTraits("KnowPrisonerDiedInnocent", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowPrisonerDiedInnocent != null && toggleEmpathy)
            {
                knowPrisonerDiedInnocent = ModifyThoughtStages(knowPrisonerDiedInnocent, new int[] { -4 });
            }
            ThoughtDef knowColonistDied = AddNullifyingTraits("KnowColonistDied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistDied != null && toggleEmpathy)
            {
                knowColonistDied = ModifyThoughtStages(knowColonistDied, new int[] { -2 });
            }
            ThoughtDef colonistAbandoned = AddNullifyingTraits("ColonistBanished", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (colonistAbandoned != null && toggleEmpathy)
            {
                colonistAbandoned = ModifyThoughtStages(colonistAbandoned, new int[] { -2 });
            }
            ThoughtDef colonistAbandonedToDie = AddNullifyingTraits("ColonistBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (colonistAbandonedToDie != null && toggleEmpathy)
            {
                colonistAbandonedToDie = ModifyThoughtStages(colonistAbandonedToDie, new int[] { -4 });
            }
            ThoughtDef prisonerAbandonedToDie = AddNullifyingTraits("PrisonerBanishedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (prisonerAbandonedToDie != null && toggleEmpathy)
            {
                prisonerAbandonedToDie = ModifyThoughtStages(prisonerAbandonedToDie, new int[] { -3 });
            }
            ThoughtDef knowPrisonerSold = AddNullifyingTraits("KnowPrisonerSold", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowPrisonerSold != null && toggleEmpathy)
            {
                knowPrisonerSold = ModifyThoughtStages(knowPrisonerSold, new int[] { -4 });
            }
            ThoughtDef knowGuestOrganHarvested = AddNullifyingTraits("KnowGuestOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowGuestOrganHarvested != null && toggleEmpathy)
            {
                knowGuestOrganHarvested = ModifyThoughtStages(knowGuestOrganHarvested, new int[] { -4 });
            }
            ThoughtDef knowColonistOrganHarvested = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistOrganHarvested != null && toggleEmpathy)
            {
                knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
            }
            ThoughtDef beauty = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
            if (knowColonistOrganHarvested != null && toggleEmpathy)
            {
                knowColonistOrganHarvested = ModifyThoughtStages(knowColonistOrganHarvested, new int[] { -4 });
            }

            /* ThingDef injection reworked by notfood */
            var zombieThinkTree = DefDatabase<ThinkTreeDef>.GetNamedSilentFail("Zombie");

            IEnumerable<ThingDef> things = from def in DefDatabase<ThingDef>.AllDefs
                                           where def.race?.intelligence == Intelligence.Humanlike
                                           && !def.defName.Contains("AIPawn") && !def.defName.Contains("Robot")
                                           && !def.defName.Contains("ChjDroid") && !def.defName.Contains("ChjBattleDroid")
                                           && (zombieThinkTree == null || def.race.thinkTreeMain != zombieThinkTree)
                                           select def;

            List<string> registered = new List<string>();
            foreach (ThingDef t in things)
            {
                if (t.inspectorTabsResolved == null)
                {
                    t.inspectorTabsResolved = new List<InspectTabBase>(1);
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

            /*
             * Now to enjoy the benefits of having made a popular mod!
             * This will be our little secret.
             */
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

        public override void MapLoaded(Map map)
        {
            if (!ModIsActive || !ActivateKinsey())
            {
                return;
            }
            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawn.story != null && PsycheHelper.PsychologyEnabled(pawn))
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        RemoveTrait(pawn, TraitDefOf.Gay);
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 0f, 0f, 0f, 1f, 2f);
                    }
                    if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                    {
                        RemoveTrait(pawn, TraitDefOf.Bisexual);
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 1f, 2f, 1f, 0f, 0f);
                    }
                    if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                    {
                        RemoveTrait(pawn, TraitDefOf.Asexual);
                        PsycheHelper.Comp(pawn).Sexuality.sexDrive = 0.10f * Rand.ValueSeeded(11 * pawn.HashOffset() + 8);
                    }
                }
            }
        }

        public override void Tick(int currentTick)
        {
            //Self-explanatory.
            if (!PsychologyBase.ActivateElections())
            {
                return;
            }
            //Constituent tick
            if (currentTick % (2 * GenDate.TicksPerHour) == 0)
            {
                //List<Settlement> playerSettlements = Find.WorldObjects.Settlements.FindAll(b => b.Faction.IsPlayer);
                List<Settlement> playerSettlements = Find.WorldObjects.SettlementBases.FindAll(b => b.Faction.IsPlayer);
                foreach (Settlement settlement in playerSettlements)
                {
                    if (!MayorUtility.Mayors.ContainsKey(settlement.Map.Tile))
                    {
                        continue;
                    }
                    IEnumerable<Pawn> constituents = from p in settlement.Map.mapPawns.FreeColonistsSpawned
                                                     where !p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor)
                                                     && p.GetLord() == null && p.GetTimeAssignment() != TimeAssignmentDefOf.Work
                                                     && p.Awake() && !p.Downed && !p.Drafted && p.health.summaryHealth.SummaryHealthPercent >= 1f
                                                     && PsycheHelper.PsychologyEnabled(p)
                                                     select p;
                    if (constituents.Count() == 0)
                    {
                        continue;
                    }
                    Pawn potentialConstituent = constituents.RandomElementByWeight(p => 0.0001f + Mathf.Pow(Mathf.Abs(0.7f - p.needs.mood.CurLevel), 2));
                    if (potentialConstituent == null)
                    {
                        continue;
                    }
                    Pawn mayor = MayorUtility.Mayors[settlement.Map.Tile].First;
                    TimeAssignmentDef timeAssDef = mayor.GetTimeAssignment();
                    bool mayorAvailable = timeAssDef != TimeAssignmentDefOf.Sleep && mayor.GetLord() == null && mayor.Tile == settlement.Map.Tile
                                          && mayor.Awake() && !mayor.Drafted && !mayor.Downed && mayor.health.summaryHealth.SummaryHealthPercent >= 1f
                                          && (mayor.CurJob == null || mayor.CurJob.def != JobDefOf.TendPatient || mayor.CurJob.RecipeDef.workerClass.IsAssignableFrom(typeof(Recipe_Surgery)));
                    if (!mayorAvailable)
                    {
                        continue;
                    }
                    IntVec3 gather = default(IntVec3);
                    string found = null;
                    if (mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable != null)
                    {
                        gather = mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable.parent.Position;
                        found = "office";
                    }
                    else if (mayor.ownership != null && mayor.ownership.OwnedBed != null)
                    {
                        gather = mayor.ownership.OwnedBed.Position;
                        found = "bed";
                    }
                    //if (PsycheHelper.PsychologyEnabled(potentialConstituent) && Rand.Chance((1f - PsycheHelper.Comp(potentialConstituent).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) / 5f) && (found != null || RCellFinder.TryFindGatheringSpot(mayor, GatheringDefOf.Party, true, out gather)) // 1.3 - added ignoreRequiredColonistCount = true
                    //    && (!mayor.Drafted && !mayor.Downed && mayor.health.summaryHealth.SummaryHealthPercent >= 1f && mayor.GetTimeAssignment() != TimeAssignmentDefOf.Work && (mayor.CurJob == null || mayor.CurJob.def != JobDefOf.TendPatient || mayor.CurJob.RecipeDef.workerClass.IsAssignableFrom(typeof(Recipe_Surgery)))))
                    float independent = PsycheHelper.Comp(potentialConstituent).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent);
                    if (Rand.Chance((1f - independent) / 5f) && (found != null || RCellFinder.TryFindGatheringSpot(mayor, GatheringDefOf.Party, true, out gather)))
                    {
                        List<Pawn> pawns = new List<Pawn>();
                        pawns.Add(mayor);
                        pawns.Add(potentialConstituent);
                        Lord meeting = LordMaker.MakeNewLord(mayor.Faction, new LordJob_VisitMayor(gather, potentialConstituent, mayor, potentialConstituent.needs.mood.CurLevel < 1.25f * potentialConstituent.mindState.mentalBreaker.BreakThresholdMinor), mayor.Map, pawns);
                        mayor.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                        potentialConstituent.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                        if (found == "bed")
                        {
                            mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoOffice);
                        }
                        else if (found == null)
                        {
                            mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoBedroom);
                        }
                    }

                }
            }
            //Election tick
            if (currentTick % (7 * GenDate.TicksPerHour) == 0)
            {
                List<Settlement> eligibleSettlements = new List<Settlement>();
                foreach (Settlement settlement in Find.WorldObjects.Settlements)
                {
                    //If the base isn't owned or named by the player, no election can be held.
                    if (!settlement.Faction.IsPlayer || !settlement.namedByPlayer)
                    {
                        continue;
                    }
                    ////If the base is not at least a year old, no election will be held.
                    //if ((Find.TickManager.TicksGame - settlement.creationGameTicks) < GenDate.TicksPerYear)
                    //{
                    //    continue;
                    //}
                    //A base must have at least 7 people in it to hold an election.
                    if (settlement.Map.mapPawns.FreeColonistsSpawnedCount < 7)
                    {
                        continue;
                    }
                    //If an election is already being held, don't start a new one.
                    if (settlement.Map.gameConditionManager.ConditionIsActive(GameConditionDefOfPsychology.Election) || settlement.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
                    {
                        continue;
                    }

                    int settlementTile = settlement.Tile;
                    if (MayorUtility.Mayors.ContainsKey(settlementTile))
                    {
                        // Don't start another election if the mayor was elected this year
                        int yearDiff = GenLocalDate.Year(settlementTile) - (MayorUtility.Mayors[settlementTile].Second as Hediff_Mayor).yearElected;
                        if (yearDiff <= 0)
                        {
                            continue;
                        }
                        //Elections are held starting in Septober (because I guess some maps don't have fall?)
                        if (yearDiff == 1 && GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(settlementTile).x) < Quadrum.Septober)
                        {
                            continue;
                        }
                    }
                    else if (GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(settlementTile).x) < Quadrum.Septober)
                    {
                        continue;
                    }
                    // Start elections during the day
                    if (GenLocalDate.HourOfDay(settlement.Map) < 7 || GenLocalDate.HourOfDay(settlement.Map) > 20)
                    {
                        continue;
                    }
                    eligibleSettlements.Add(settlement);
                }
                if (eligibleSettlements.Count() > 0)
                {
                    // Only pick one election at random to happen per 7 hour tick so they don't all proc at once.
                    Settlement settlement = eligibleSettlements.RandomElement();
                    IncidentParms parms = new IncidentParms();
                    parms.target = settlement.Map;
                    parms.faction = settlement.Faction;
                    FiringIncident fi = new FiringIncident(IncidentDefOfPsychology.Election, null, parms);
                    Find.Storyteller.TryFire(fi);
                }
            }
        }
    }


}

//public static bool kinsey = true;
//public static KinseyMode mode = KinseyMode.Realistic;
//public static float kinsey0WeightConfig = 62.5f;
//public static float kinsey1WeightConfig = 11.3f;
//public static float kinsey2WeightConfig = 9.3f;
//public static float kinsey3WeightConfig = 6.8f;
//public static float kinsey4WeightConfig = 4.5f;
//public static float kinsey5WeightConfig = 2.8f;
//public static float kinsey6WeightConfig = 2.8f;
//public static bool notBabyMode = true;
//public static bool elections = true;
//public static float convoDuration = 60f;
//public static bool imprisonedDebuffEnabled = true; // v1.1
//public static bool anxietyEnabled = true; // v1.1
//public static float romanceChanceConfig = 1f; // v1.1
//public static float romanceThresholdConfig = 5f; // v1.1
//public static float mayorAgeConfig = 20f; // v1.1
//public static bool dateLetters = true;
////public static bool benchmark = false;
//public static float traitOpinionMultplierConfig = 0.25f; // 1.2

//kinsey = toggleKinsey.Value;
//mode = kinseyMode.Value;
//kinsey0WeightConfig = kinsey0WeightSettingHandle.Value;
//kinsey1WeightConfig = kinsey1WeightSettingHandle.Value;
//kinsey2WeightConfig = kinsey2WeightSettingHandle.Value;
//kinsey3WeightConfig = kinsey3WeightSettingHandle.Value;
//kinsey4WeightConfig = kinsey4WeightSettingHandle.Value;
//kinsey5WeightConfig = kinsey5WeightSettingHandle.Value;
//kinsey6WeightConfig = kinsey6WeightSettingHandle.Value;
//notBabyMode = toggleIndividuality.Value;
//elections = toggleElections.Value;
//convoDuration = conversationDuration.Value;
//dateLetters = toggleDateLetters.Value;
//imprisonedDebuffEnabled = imprisonedDebuff.Value; //v1.1
//anxietyEnabled = anxiety.Value; //v1.1
//romanceChanceConfig = romanceChanceMultiplier.Value; //v1.1
//romanceThresholdConfig = romanceChanceThreshold.Value; //v1.1
//mayorAgeConfig = mayorAge.Value; //v1.1
//traitOpinionMultplierConfig = traitOpinionMultplier.Value; //1.2
//PsycheCardUtility.DistanceFromMiddle = DistanceFromMiddleSettingsHandle.Value; //1.3
//PsycheCardUtility.UseColorsBool = UseColorsSettingsHandle.Value; //1.3
//PsycheCardUtility.AlphabeticalBool = ListAlphabeticalSettingsHandle.Value; //1.3
//PsycheCardUtility.UseAntonymsBool = UseAntonymsSettingsHandle.Value; //1.3

//// Set values
//kinsey = toggleKinsey.Value;
//if (PsychologyBase.ActivateKinsey())
//{
//    mode = kinseyMode.Value;
//    kinsey0WeightConfig = kinsey0WeightSettingHandle.Value;
//    kinsey1WeightConfig = kinsey1WeightSettingHandle.Value;
//    kinsey2WeightConfig = kinsey2WeightSettingHandle.Value;
//    kinsey3WeightConfig = kinsey3WeightSettingHandle.Value;
//    kinsey4WeightConfig = kinsey4WeightSettingHandle.Value;
//    kinsey5WeightConfig = kinsey5WeightSettingHandle.Value;
//    kinsey6WeightConfig = kinsey6WeightSettingHandle.Value;
//}
//notBabyMode = toggleIndividuality.Value;
//elections = toggleElections.Value;
//dateLetters = toggleDateLetters.Value;
////benchmark = toggleBenchmarking.Value;
//convoDuration = conversationDuration.Value;
//imprisonedDebuffEnabled = imprisonedDebuff.Value; // Imprisoned opinion penalty, added in v1.1
//anxietyEnabled = anxiety.Value; //v1.1
//romanceChanceConfig = romanceChanceMultiplier.Value; // Romance chance, added in v1.1
//romanceThresholdConfig = romanceChanceThreshold.Value; //v1.1
//mayorAgeConfig = mayorAge.Value; //v1.1
//traitOpinionMultplierConfig = traitOpinionMultplier.Value; //1.3

