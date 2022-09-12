using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace Psychology;

public static class PsycheHelper
{
    public static PsychologyGameComponent GameComp => Current.Game.GetComponent<PsychologyGameComponent>();
    public static Dictionary<KinseyMode, List<float>> KinseyModeWeightDict = new Dictionary<KinseyMode, List<float>>()
    {
        { KinseyMode.Realistic    , new List<float> { 62.4949f, 11.3289f,  9.2658f,  6.8466f,  4.5220f,  2.7806f,  2.7612f } } ,
        { KinseyMode.Invisible    , new List<float> {  7.0701f, 11.8092f, 19.5541f, 23.1332f, 19.5541f, 11.8092f,  7.0701f } },
        { KinseyMode.Uniform      , new List<float> { 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f } },
        { KinseyMode.Gaypocalypse , new List<float> {  2.7612f,  2.7806f,  4.5220f,  6.8466f,  9.2658f, 11.3289f, 62.4949f } }
    };
    public static HashSet<string> TraitDefNamesThatAffectPsyche = new HashSet<string>();
    public static HashSet<string> SkillDefNamesThatAffectPsyche = new HashSet<string>();
    //public static HashSet<string> PsycheEnabledSpeciesList = new HashSet<string>();
    public static SpeciesSettings settings = new SpeciesSettings();

    public static bool PsychologyEnabled(Pawn pawn)
    {
        if (pawn == null)
        {
            Log.Message("PsychologyEnabled, pawn == null");
            return false;
        }
        if (!IsHumanlike(pawn))
        {
            Log.Message("PsychologyEnabled, IsHumanlike(pawn) == false");
            return false;
        }
        Log.Message("PsychologyEnabled, pawn.def.defName = " + pawn.def.defName + ", pawn = " + pawn.Label);
        settings = SpeciesHelper.GetOrMakeSettingsFromHumanlikeDef(pawn.def, true);
        if (settings.enablePsyche == false)
        {
            Log.Message("PsychologyEnabled, settings.enablePsyche = false");
            return false;
        }
        Log.Message("PsychologyEnabled, settings.enablePsyche = true");
        return DoesCompExist(pawn);
    }

    public static bool IsHumanlike(Pawn pawn)
    {
        return pawn.def?.race?.intelligence == Intelligence.Humanlike;
    }

    //public static bool IsSapient(Pawn pawn)
    //{
    //    return true;
    //}

    public static bool DoesCompExist(Pawn pawn)
    {
        CompPsychology comp = Comp(pawn);
        if (comp == null)
        {
            Log.Message("PsychologyEnabled, Comp(pawn) == null, pawn = " + pawn.Label + ", species label = " + pawn.def.label);
            return false;
        }
        Log.Message("Comp(pawn) != null");
        if (!comp.IsPsychologyPawn)
        {
            Log.Message("PsychologyEnabled, IsPsychologyPawn = false, pawn = " + pawn.Label);
            return false;
        }
        Log.Message("PsychologyEnabled, IsPsychologyPawn = true, pawn = " + pawn.Label);
        return true;
    }

    public static CompPsychology Comp(Pawn pawn)
    {
        return pawn.GetComp<CompPsychology>();
    }

    public static void Look<T>(ref HashSet<T> valueHashSet, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
    {
        List<T> list = null;
        if (Scribe.mode == LoadSaveMode.Saving && valueHashSet != null)
        {
            list = new List<T>();
            foreach (T current in valueHashSet)
            {
                list.Add(current);
            }
        }
        Scribe_Collections.Look<T>(ref list, false, label, lookMode, ctorArgs);
        if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
        {
            if (list == null)
            {
                valueHashSet = null;
            }
            else
            {
                valueHashSet = new HashSet<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    valueHashSet.Add(list[i]);
                }
            }
        }
    }

    public static void InitializeDefNamesThatAffectPsyche()
    {
        foreach (PersonalityNodeDef pDef in PersonalityNodeParentMatrix.defList)
        {
            if (pDef.traitModifiers != null && pDef.traitModifiers.Any())
            {
                foreach (PersonalityNodeTraitModifier traitMod in pDef.traitModifiers)
                {
                    TraitDefNamesThatAffectPsyche.Add(traitMod.trait.defName);
                }
            }
            if (pDef.skillModifiers != null && pDef.skillModifiers.Any())
            {
                foreach (PersonalityNodeSkillModifier skillMod in pDef.skillModifiers)
                {
                    SkillDefNamesThatAffectPsyche.Add(skillMod.skill.defName);
                }
            }
        }
    }

    //public static void InitializePsycheEnabledSpeciesList()
    //{
    //    foreach (KeyValuePair<string, SpeciesSettings> kvp in PsychologySettings.speciesDict)
    //    {
    //        if (kvp.Value.enablePsyche)
    //        {
    //            PsycheEnabledSpeciesList.Add(kvp.Key);
    //        }
    //    }
    //}

    public static float DatingAgeToVanilla(float customAge, float minDatingAge)
    {
        return customAge * 14f / minDatingAge;
    }

    public static float LovinAgeToVanilla(float customAge, float minLovinAge)
    {
        return customAge * 16f / minLovinAge;
    }

    public static float DatingAgeFromVanilla(float vanillaAge, float minDatingAge)
    {
        return vanillaAge * minDatingAge / 14f;
    }

    public static float LovinAgeFromVanilla(float vanillaAge, float minDatingAge)
    {
        return vanillaAge * minDatingAge / 16f;
    }

    public static float RandGaussianSeeded(int specialSeed1, int specialSeed2, float centerX = 0f, float widthFactor = 1f)
    {
        float value = Rand.ValueSeeded(specialSeed1);
        float value2 = Rand.ValueSeeded(specialSeed2);
        return Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin((float)Math.PI * 2f * value2) * widthFactor + centerX;
    }

    // This saddle shaped function is used to calculate the opinion modifier due to differences in personality rating on a given convo topic
    // The inputs x, y are the ratings and should range between 0 and 1.
    // The gamma parameter controls how fast agreement drops off as a function of the difference between x and y
    // The f0 parameter controls the value of the function when x = y = 1/2
    // This function is invariant under both x,y -> y,x and x,y -> 1-x,1-y transformations
    // This function reaches its maximum of +1 when x and y are both 0 or 1 (complete agreement on a topic)
    // This function reaches its minimum of -1 when x=0 and y=1 or vice versa (complete disagreement on a topic)
    public static float SaddleShapeFunction(float x, float y, float f0 = 0.5f, float gamma = 4f)
    {
        return (f0 - (1f + f0 + gamma) * Mathf.Pow(x - y, 2) + (1f - f0) * Mathf.Pow(x + y - 1f, 2)) / (1f + gamma * Mathf.Pow(x - y, 2));
    }

    public static float NormalCDF(float x)
    {
        // constants
        float a1 = 0.127414796f;
        float a2 = -0.142248368f;
        float a3 = 0.710706871f;
        float a4 = -0.726576014f;
        float a5 = 0.530702716f;
        float p = 0.231641888f;
        // Save the sign of x
        bool sign = x > 0;
        x = Math.Abs(x);
        // A&S formula 7.1.26
        float t = 1f / (1f + p * x);
        float z = (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(-0.5f * x * x);
        return sign ? 1f - z : z;
    }

    public static float NormalCDFInv(float p)
    {
        p = Mathf.Clamp(p, 0.0001f, 0.9999f);
        if (p < 0.5)
        {
            return -RationalApproximation(Mathf.Sqrt(-2f * Mathf.Log(p)));
        }
        else
        {
            return RationalApproximation(Mathf.Sqrt(-2f * Mathf.Log(1f - p)));
        }
    }

    public static float RationalApproximation(float t)
    {
        // Abramowitz and Stegun formula 26.2.23.
        // The absolute value of the error should be less than 4.5 e-4.
        float[] c = { 2.515517f, 0.802853f, 0.010328f };
        float[] d = { 1.432788f, 0.189269f, 0.001308f };
        return t - ((c[2] * t + c[1]) * t + c[0]) / (((d[2] * t + d[1]) * t + d[0]) * t + 1f);
    }

    public static void CorrectTraitsForPawnKinseyEnabled(Pawn pawn)
    {
        if (!PsycheHelper.PsychologyEnabled(pawn) || pawn?.story?.traits == null)
        {
            return;
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            PsycheHelper.RemoveTrait(pawn, TraitDefOf.Asexual);
            PsycheHelper.Comp(pawn).Sexuality.sexDrive = 0.10f * Rand.ValueSeeded(11 * PsycheHelper.PawnSeed(pawn) + 8);
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
        {
            PsycheHelper.RemoveTrait(pawn, TraitDefOf.Bisexual);
            PsycheHelper.Comp(pawn).Sexuality.GenerateKinsey(0f, 0f, 1f, 2f, 1f, 0f, 0f);
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
        {
            PsycheHelper.RemoveTrait(pawn, TraitDefOf.Gay);
            PsycheHelper.Comp(pawn).Sexuality.GenerateKinsey(0f, 0f, 0f, 0f, 0f, 1f, 2f);
        }
    }

    public static void CorrectTraitsForPawnKinseyDisabled(Pawn pawn)
    {
        if (!PsycheHelper.PsychologyEnabled(pawn) || pawn?.story?.traits == null)
        {
            return;
        }
        int kinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
        if (PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f)
        {
            TryGainTrait(pawn, TraitDefOf.Asexual);
        }
        if (kinseyRating < 2)
        {
            // If pawn is mostly heterosexual
            TryRemoveTrait(pawn, TraitDefOf.Bisexual);
            TryRemoveTrait(pawn, TraitDefOf.Gay);
        }
        else if (kinseyRating < 5)
        {
            // If pawn is mostly bisexual
            TryGainTrait(pawn, TraitDefOf.Bisexual);
            TryRemoveTrait(pawn, TraitDefOf.Gay);
        }
        else
        {
            // If pawn is mostly homosexual
            TryGainTrait(pawn, TraitDefOf.Gay);
            TryRemoveTrait(pawn, TraitDefOf.Bisexual);
        }
    }

    public static void TryGainTrait(Pawn pawn, TraitDef traitDef)
    {
        if (!pawn.story.traits.HasTrait(traitDef))
        {
            pawn.story.traits.GainTrait(new Trait(traitDef));
        }
    }

    public static void TryRemoveTrait(Pawn pawn, TraitDef traitDef)
    {
        if (pawn.story.traits.HasTrait(traitDef))
        {
            RemoveTrait(pawn, traitDef);
        }
    }

    public static void RemoveTrait(Pawn pawn, TraitDef traitDef)
    {
        pawn.story.traits.allTraits.RemoveAll(t => t.def == traitDef);
    }

    public static int PawnSeed(Pawn pawn)
    {
        int firstNameSeed = pawn?.Name != null ? pawn.Name is NameTriple name ? name.First.GetHashCode() : 2 : 3;
        int childhoodSeed = pawn?.story?.childhood.baseDesc != null ? pawn.story.childhood.GetHashCode() : 5;
        int adulthoodSeed = pawn?.story?.adulthood != null ? pawn.story.adulthood.GetHashCode() : 7;
        int birthLastNameSeed = pawn?.story?.birthLastName != null ? pawn.story.birthLastName.GetHashCode() : 11;
        return Gen.HashCombineInt(firstNameSeed, childhoodSeed, adulthoodSeed, birthLastNameSeed);
    }

    public static void TryGainMemoryReplacedPartBleedingHeart(Pawn pawn, Pawn billDoer)
    {
        if (billDoer != null && billDoer.needs.mood != null)
        {
            billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
        }
    }
}

