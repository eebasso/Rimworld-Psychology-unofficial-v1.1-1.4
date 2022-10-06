using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Psychology;

public static class PsycheHelper
{
    public static PsychologyGameComponent GameComp => Current.Game.GetComponent<PsychologyGameComponent>();
    public static readonly Dictionary<KinseyMode, float[]> KinseyModeWeightDict = new Dictionary<KinseyMode, float[]>()
    {
        { KinseyMode.Realistic    , new float[] { 62.4949f, 11.3289f,  9.2658f,  6.8466f,  4.5220f,  2.7806f,  2.7612f } },
        { KinseyMode.Invisible    , new float[] {  7.0701f, 11.8092f, 19.5541f, 23.1332f, 19.5541f, 11.8092f,  7.0701f } },
        { KinseyMode.Uniform      , new float[] { 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f } },
        { KinseyMode.Gaypocalypse , new float[] {  2.7612f,  2.7806f,  4.5220f,  6.8466f,  9.2658f, 11.3289f, 62.4949f } },
    };

    public static HashSet<string> TraitDefNamesThatAffectPsyche = new HashSet<string>();

    public static Dictionary<Gender, Dictionary<int, float>> GenderModifierNodeDefDict = new Dictionary<Gender, Dictionary<int, float>>();
    public static Dictionary<SkillDef, HashSet<int>> SkillModifierNodeDefDict = new Dictionary<SkillDef, HashSet<int>>();
    public static Dictionary<Pair<TraitDef, int>, Dictionary<int, float>> TraitModifierNodeDefDict = new Dictionary<Pair<TraitDef, int>, Dictionary<int, float>>();
    public static Dictionary<WorkTypeDef, Dictionary<int, float>> IncapableModifierNodeDefDict = new Dictionary<WorkTypeDef, Dictionary<int, float>>();

    //public static HashSet<string> SkillDefNamesThatAffectPsyche = new HashSet<string>();
    public static int seed;
    public static float DailyCertaintyChangeScale => 0.0015f * PsychologySettings.ideoPsycheMultiplier;
    public static CompPsychology comp;
    public static SpeciesSettings settings;
    public static bool flag;
    public static float[] CircumstanceTimings = new float[25];
    public static int CircumstanceCount = 0;

    //public static int countPsychologyEnabled;
    //public static float msPsychologyEnabled;

    public static bool PsychologyEnabled(Pawn pawn)
    {
        //Stopwatch stopwatch = new Stopwatch();
        //stopwatch.Start();
        if (HasLatentPsyche(pawn) != true)
        {
            //Log.Message("PsychologyEnabled, HasLatentPsyche != true, pawn = " + pawn.Label + ", species label = " + pawn.def.label);
            return false;
        }
        if (IsSapient(pawn) != true)
        {
            //Log.Message("PsychologyEnabled, IsSapient != true, pawn = " + pawn.Label + ", species label = " + pawn.def.label);
            return false;
        }
        //stopwatch.Stop();
        //TimeSpan ts = stopwatch.Elapsed;
        //countPsychologyEnabled++;
        //msPsychologyEnabled += (float)ts.TotalMilliseconds;
        //Log.Message("PsychologyEnabled, total time: " + msPsychologyEnabled + " ms, average time: " + msPsychologyEnabled / countPsychologyEnabled + " ms");
        return true;
    }

    public static bool HasLatentPsyche(Pawn pawn)
    {
        if (pawn == null)
        {
            Log.Warning("PsychologyEnabled, pawn == null");
            return false;
        }
        //Log.Message("PsychologyEnabled, pawn != null");
        comp = Comp(pawn);
        if (comp == null)
        {
            if (SpeciesHelper.CheckIntelligenceAndAddEverythingToSpeciesDef(pawn.def) != true)
            {
                Log.Message("PsychologyEnabled, Comp(pawn) == null, pawn = " + pawn + ", species = " + pawn.def);
                return false;
            }
            comp = Comp(pawn);
            if (comp == null)
            {
                Log.Error("PsychologyEnabled, comp == null after CheckIntelligenceAndAddEverythingToHumanlikeDef == true");
                return false;
            }
        }
        //Log.Message("PsychologyEnabled, Comp(pawn) != null");
        if (comp.IsPsychologyPawn != true)
        {
            Log.Message("PsychologyEnabled, IsPsychologyPawn != true, pawn = " + pawn + ", species = " + pawn.def);
            return false;
        }
        settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
        if (settings.enablePsyche != true)
        {
            Log.Message("PsychologyEnabled, settings.enablePsyche != true, " + pawn + ", species = " + pawn.def);
            return false;
        }
        //Log.Message("PsychologyEnabled, settings.enablePsyche == true");
        return true;
    }

    public static bool IsSapient(Pawn pawn)
    {
        return SpeciesHelper.IsHumanlikeIntelligence(pawn.def);
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

    public static void InitializeDictionariesForPersonalityNodeDefs()
    {
        Dictionary<PersonalityNodeDef, int> indexDict = PersonalityNodeMatrix.indexDict;
        int index;

        foreach (Gender gender in Gender.GetValues(typeof(Gender)))
        {
            GenderModifierNodeDefDict[gender] = new Dictionary<int, float>();
        }

        foreach (PersonalityNodeDef pDef in PersonalityNodeMatrix.defList)
        {
            if (pDef.femaleModifier != default && pDef.femaleModifier != 0f)
            {
                index = indexDict[pDef];
                GenderModifierNodeDefDict[Gender.Male][index] = -pDef.femaleModifier;
                GenderModifierNodeDefDict[Gender.Female][index] = pDef.femaleModifier;
            }

            if (pDef.skillModifiers.NullOrEmpty() != true)
            {
                foreach (PersonalityNodeSkillModifier skillMod in pDef.skillModifiers)
                {
                    if (SkillModifierNodeDefDict.ContainsKey(skillMod.skill) != true)
                    {
                        SkillModifierNodeDefDict[skillMod.skill] = new HashSet<int>();
                    }
                    SkillModifierNodeDefDict[skillMod.skill].Add(indexDict[pDef]);
                }
            }

            if (pDef.traitModifiers.NullOrEmpty() != true)
            {
                foreach (PersonalityNodeTraitModifier traitMod in pDef.traitModifiers)
                {
                    TraitDefNamesThatAffectPsyche.Add(traitMod.trait.defName);
                    Pair<TraitDef, int> pair = new Pair<TraitDef, int>(traitMod.trait, traitMod.degree);
                    if (TraitModifierNodeDefDict.ContainsKey(pair) != true)
                    {
                        TraitModifierNodeDefDict[pair] = new Dictionary<int, float>();
                    }
                    TraitModifierNodeDefDict[pair][indexDict[pDef]] = traitMod.modifier;
                }
            }

            if (pDef.incapableModifiers.NullOrEmpty() != true)
            {
                foreach (PersonalityNodeIncapableModifier incapableMod in pDef.incapableModifiers)
                {
                    if (IncapableModifierNodeDefDict.ContainsKey(incapableMod.type) != true)
                    {
                        IncapableModifierNodeDefDict[incapableMod.type] = new Dictionary<int, float>();
                    }
                    IncapableModifierNodeDefDict[incapableMod.type][indexDict[pDef]] = incapableMod.modifier;
                }
            }
        }
    }



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

    public static float LovinAgeFromVanilla(float vanillaAge, float minLovinAge)
    {
        return vanillaAge * minLovinAge / 16f;
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
        x = Mathf.Abs(x);
        // A&S formula 7.1.26
        float t = 1f / (1f + p * x);
        float z = (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(-0.5f * x * x);
        return sign ? 1f - z : z;
    }

    public static float NormalCDFInv(float p)
    {
        p = Mathf.Clamp(p, 0.0001f, 0.9999f);
        if (p < 0.5f)
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
        if (!TryGetPawnSeed(pawn))
        {
            return;
        }
        if (!PsychologyEnabled(pawn) || pawn?.story?.traits == null)
        {
            return;
        }
        if (TryRemoveTraitDef(pawn, TraitDefOf.Asexual))
        {
            Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Asexual trait from pawn = " + pawn.Label);
            Comp(pawn).Sexuality.AsexualTraitReroll();
        }
        if (TryRemoveTraitDef(pawn, TraitDefOf.Bisexual))
        {
            Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Bisexual trait from pawn = " + pawn.Label);
            Comp(pawn).Sexuality.BisexualTraitReroll();
        }
        if (TryRemoveTraitDef(pawn, TraitDefOf.Gay))
        {
            Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Gay trait from pawn = " + pawn.Label);
            Comp(pawn).Sexuality.GayTraitReroll();
        }
    }

    //public static void CorrectTraitsForPawnKinseyDisabled(Pawn pawn)
    //{
    //    if (!PsycheHelper.PsychologyEnabled(pawn) || pawn?.story?.traits == null)
    //    {
    //        return;
    //    }
    //    int kinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
    //    if (PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f)
    //    {
    //        TryGainTraitDef(pawn, TraitDefOf.Asexual);
    //    }
    //    if (kinseyRating < 2)
    //    {
    //        // If pawn is mostly heterosexual
    //        TryRemoveTraitDef(pawn, TraitDefOf.Bisexual);
    //        TryRemoveTraitDef(pawn, TraitDefOf.Gay);
    //    }
    //    else if (kinseyRating < 5)
    //    {
    //        // If pawn is mostly bisexual
    //        TryGainTraitDef(pawn, TraitDefOf.Bisexual);
    //        TryRemoveTraitDef(pawn, TraitDefOf.Gay);
    //    }
    //    else
    //    {
    //        // If pawn is mostly homosexual
    //        TryRemoveTraitDef(pawn, TraitDefOf.Bisexual);
    //        TryGainTraitDef(pawn, TraitDefOf.Gay);
    //    }
    //}

    public static bool HasTraitDef(Pawn pawn, TraitDef traitDef)
    {
        foreach (Trait trait in pawn.story.traits.allTraits)
        {
            if (trait.def == traitDef)
            {
                return true;
            }
        }
        return false;
    }

    public static bool TryGainTraitDef(Pawn pawn, TraitDef traitDef)
    {
        if (HasTraitDef(pawn, traitDef) != true)
        {
            return false;
        }
        pawn.story.traits.GainTrait(new Trait(traitDef));
        return true;
    }

    public static bool TryRemoveTraitDef(Pawn pawn, TraitDef traitDef)
    {
        foreach (Trait trait in pawn.story.traits.allTraits)
        {
            if (trait.def == traitDef)
            {
                pawn.story.traits.RemoveTrait(trait);
                return true;
            }
        }
        return false;
    }

    public static int PawnSeed(Pawn pawn)
    {
        int thingIDSeed = pawn.thingIDNumber;
        int worldIDSeed = Find.World.info.Seed;
        return Gen.HashCombineInt(thingIDSeed, worldIDSeed);
        //if (TryGetPawnSeed(pawn) != true)
        //{
        //    string thingID = pawn?.ThingID != null ? pawn?.ThingID : "null";
        //    string label = pawn?.Label != null ? pawn?.Label : "null";
        //    string defName = pawn?.def?.defName != null ? pawn?.def?.defName : "null";
        //    Log.Error("Used random pawn seed, would prefer this is not happen, thingID = " + thingID + ", + pawn.Label = " + label + ", pawn.def.defName = " + defName);
        //}
        //return seed;
    }

    public static bool TryGetPawnSeed(Pawn pawn)
    {
        return true;
        //bool success = true;
        //int thingIDSeed;
        //int worldIDSeed;
        ////int childhoodSeed;
        //List<string> exceptions = new List<string>();
        //try
        //{
        //    thingIDSeed = pawn.thingIDNumber;
        //}
        //catch (Exception ex)
        //{
        //    exceptions.Add("thingIDSeed: " + ex);
        //    thingIDSeed = Mathf.CeilToInt(1e7f * Rand.Value);
        //    success = false;
        //}
        //try
        //{
        //    worldIDSeed = Find.World.info.Seed;
        //}
        //catch (Exception ex)
        //{
        //    exceptions.Add("worldIDSeed: " + ex);
        //    worldIDSeed = Mathf.CeilToInt(1e7f * Rand.Value);
        //    success = false;
        //}
        //try
        //{
        //    childhoodSeed = GenText.StableStringHash(pawn.story.Childhood.untranslatedTitle);
        //}
        //catch (Exception ex)
        //{
        //    exceptions.Add("childhoodSeed: " + ex);
        //    childhoodSeed = 23;
        //    //success = false;
        //}
        //try
        //{
        //    firstNameSeed = GenText.StableStringHash((pawn.Name as NameTriple).First);
        //}
        //catch (Exception ex)
        //{
        //    exceptions.Add("(pawn.Name as NameTriple).First: " + ex);
        //    firstNameSeed = Mathf.CeilToInt(1e7f * Rand.Value);
        //    success = false;
        //}
        //seed = Gen.HashCombineInt(thingIDSeed, worldIDSeed);
        //if (success != true)
        //{
        //    Log.Warning("TryGetPawnSeed errors: " + string.Join(" | ", exceptions));
        //}
        //return success;
    }

    public static void TryGainMemoryReplacedPartBleedingHeart(Pawn pawn, Pawn billDoer)
    {
        if (billDoer?.needs?.mood != null)
        {
            billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
        }
    }

    //public static float[] KinseyProbabilities()
    //{
    //    float[] pList = new float[7];
    //    KinseyMode kinseyMode = PsychologySettings.kinseyFormula;
    //    if (kinseyMode != KinseyMode.Custom)
    //    {
    //        KinseyModeWeightDict[kinseyMode].CopyTo(pList, 0);
    //    }
    //    else
    //    {
    //        PsychologySettings.kinseyWeightCustom.ToArray().CopyTo(pList, 0);
    //    }
    //    float sum = pList.Sum();
    //    if (sum == 0f)
    //    {
    //        pList = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
    //        sum = 7f;
    //    }
    //    for (int i = 0; i < 7; i++)
    //    {
    //        pList[i] = pList[i] / sum;
    //    }
    //    return pList;
    //}

    public static float RelativisticAddition(float u, float v)
    {
        return (u + v) / (1f + u * v);
    }

    
}

