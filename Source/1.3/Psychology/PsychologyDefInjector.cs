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

//[StaticConstructorOnStartup]
public static class PsychologyDefInjector
{
    public static Backstory child = new Backstory();

    //static PsychologyDefInjector()
    public static void Initialize()
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

