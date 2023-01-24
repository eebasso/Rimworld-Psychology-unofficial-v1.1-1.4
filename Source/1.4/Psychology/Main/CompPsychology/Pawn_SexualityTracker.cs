using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using Psychology.Harmony;
using HarmonyLib;
using System.Runtime;

namespace Psychology;

public class Pawn_SexualityTracker : IExposable
{
  public int kinseyRating;
  public float sexDrive;
  public float romanticDrive;
  public List<Pawn> knownSexualitiesWorkingKeys;
  public List<int> knownSexualitiesWorkingValues;
  public Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
  public Pawn pawn;
  public static readonly float[] onesArray = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };

  public const float asexualCutoff = 0.1f;
  public static readonly int[] heterosexualRatings = new int[] { 0, 1 };
  public static readonly int[] bisexualRatings = new int[] { 2, 3, 4 };
  public static readonly int[] homosexualRatings = new int[] { 5, 6 };

  public bool IsAsexual => sexDrive < asexualCutoff;
  public bool IsCompletelyStraight => kinseyRating == 0;
  public bool IsCompletelyGay => kinseyRating == 6;
  public bool IsAnyAmountOfBisexual => !IsCompletelyStraight && !IsCompletelyGay;

  public bool IsMostlyStraight => kinseyRating <= 1;
  public bool IsMostlyGay => kinseyRating >= 5;
  public bool IsMostlyBisexual => !IsMostlyStraight && !IsMostlyGay;

  public bool IsLeanHeterosexual => kinseyRating <= 2;
  public bool IsLeanHomosexual => kinseyRating >= 4;
  public bool IsEvenlyBisexual => kinseyRating == 3;

  //public float AdjustedRomanticDrive => AdjustedDrive(true);
  public float AdjustedSexDrive
  {
    get
    {
      SexualityHelper.CalculateSexDrive(this.pawn, out float adjustedSexDrive, out _);
      return adjustedSexDrive;
    }
  }

  public float AdjustedRomanticDrive
  {
    get
    {
      SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
      if (!settings.enablePsyche || settings.minDatingAge < 0f)
      {
        return 0f;
      }
      if (settings.minDatingAge == 0f)
      {
        return romanticDrive;
      }
      float bioAge = this.pawn.ageTracker.AgeBiologicalYearsFloat;
      float scaledAge = PsycheHelper.DatingBioAgeToVanilla(bioAge, settings.minDatingAge);
      return romanticDrive * PsychologySettings.RomanticDriveAgeCurve.Evaluate(scaledAge);
    }
  }

  public Pawn_SexualityTracker(Pawn p) => this.pawn = p;

  public bool IncompatibleSexualityKnown(Pawn recipient)
  {
    if (knownSexualities.TryGetValue(recipient, out int knownKinseyRating))
    {
      //return ((knownKinseyRating - 4) >= 0) != (recipient.gender == this.pawn.gender);
      return recipient.gender == this.pawn.gender ? knownKinseyRating < 2 : knownKinseyRating > 4;
    }
    return false;
  }

  public void LearnSexuality(Pawn p)
  {
    if (p != null && PsycheHelper.PsychologyEnabled(pawn))
    {
      knownSexualities[p] = PsycheHelper.Comp(p).Sexuality.kinseyRating;
    }
  }

  public virtual void ExposeData()
  {
    Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
    Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1, false);
    Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1, false);
    Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
  }

  //public void GenerateSexuality(int inputSeed = 0)
  //{
  //  GenerateSexuality(onesArray, inputSeed);
  //}

  public void GenerateSexuality(float[] overrideKinseyArray = null, int inputSeed = 0)
  {
    //kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
    //sexDrive = GenerateSexDrive(inputSeed);
    //romanticDrive = GenerateRomanticDrive(inputSeed);
    GenerateKinsey(overrideKinseyArray, inputSeed);
    GenerateSexDrive(inputSeed);
    GenerateRomanticDrive(inputSeed);
  }

  public virtual void GenerateKinsey(float[] overrideKinseyArray = null, int inputSeed = 0) => kinseyRating = RandKinsey(overrideKinseyArray, inputSeed);

  public virtual void GenerateSexDrive(int inputSeed = 0) => sexDrive = RandSexDrive(inputSeed);

  public virtual void GenerateRomanticDrive(int inputSeed = 0) => romanticDrive = RandRomanticDrive(inputSeed);

  /*
   * Average roll: 0.989779
   * Percent chance of rolling each number:
   * 0: 62.4949 %
   * 1: 11.3289 %
   * 2: 9.2658 %
   * 3: 6.8466 %
   * 4: 4.522 %
   * 5: 2.7806 %
   * 6: 2.7612 %
   * Percent chance of being predominantly straight:  83.0896 %
   * Percent chance of being predominantly gay:       10.0638 %
   * Percent chance of being more or less straight:   73.8238 %
   * Percent chance of being more or less bisexual:   20.6344 %
   * Percent chance of being more or less gay:         5.5418 %
   */

  /// <summary>
  /// Generates a random Kinsey rating based on the current Kinsey formula, along with  weight
  /// </summary>
  /// <param name="extraWeightArray"></param>
  /// <param name="inputSeed"></param>
  /// <returns></returns>
  public virtual int RandKinsey(float[] extraWeightArray = null, int inputSeed = 0)
  {
    if (!PsycheHelper.KinseyModeWeightDict.TryGetValue(PsychologySettings.kinseyFormula, out float[] formulaArray))
    {
      formulaArray = PsychologySettings.kinseyWeightCustom.ToArray();
    }

    float fCumSum = 0f;
    float[] fCumSumArray = new float[7];
    for (int i = 0; i < 7; i++)
    {
      fCumSum += formulaArray[i];
      fCumSumArray[i] = fCumSum;
    }

    if (fCumSum == 0f)
    {
      formulaArray = onesArray;
      fCumSumArray = new float[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f };
    }

    if (extraWeightArray == null)
    {
      return RandKinseyByWeight(fCumSumArray, inputSeed);
    }

    float feCumSum = 0f;
    float[] feCumSumArray = new float[7];
    for (int i = 0; i < 7; i++)
    {
      feCumSum += formulaArray[i] * extraWeightArray[i];
      feCumSumArray[i] = feCumSum;
    }
    return RandKinseyByWeight(feCumSumArray[6] > 0f ? feCumSumArray : fCumSumArray, inputSeed);
  }

  public virtual int RandKinseyByWeight(float[] wCumSumArray, int inputSeed = 0)
  {
    float randValue = Rand.ValueSeeded(17 * PsycheHelper.PawnSeed(this.pawn) + 11 * inputSeed + 31) * wCumSumArray[6];
    for (int s = 0; s < 6; s++)
    {
      if (randValue <= wCumSumArray[s])
      {
        return s;
      }
    }
    return 6;
  }

  public virtual float RandSexDrive(int inputSeed = 0)
  {
    float drive = -1f;
    int kill = 0;
    int pawnSeed = PsycheHelper.PawnSeed(this.pawn);
    int seed1 = 11 * pawnSeed + 2 * inputSeed + 131;
    int seed2 = 13 * pawnSeed + 7 * inputSeed + 89;
    while ((drive < 0f || 1f < drive) && kill < 500)
    {
      //drive = Rand.Gaussian(1.1f, 0.26f);
      drive = PsycheHelper.RandGaussianSeeded(seed1, seed2, 1.1f, 0.26f);
      seed1 += 43;
      seed2 += 67;
      kill++;
    }
    return Mathf.Clamp01(drive);
  }

  public virtual float RandRomanticDrive(int inputSeed = 0) => RandSexDrive(859456 + 3 * inputSeed);

  public void AsexualTraitReroll()
  {
    if (IsAsexual)
    {
      return;
    }
    sexDrive = asexualCutoff * Rand.ValueSeeded(5 * PsycheHelper.PawnSeed(this.pawn) + 8);
  }

  public void BisexualTraitReroll()
  {
    if (IsMostlyBisexual)
    {
      return;
    }
    GenerateKinsey(new float[] { 0f, 0f, 1f, 3f, 1f, 0f, 0f });
    PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
  }

  public void GayTraitReroll()
  {
    if (IsMostlyGay)
    {
      return;
    }
    GenerateKinsey(new float[] { 0f, 0f, 0f, 0f, 0f, 1f, 3f });
    PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
  }

  public void DeepCopyFromOtherTracker(Pawn_SexualityTracker otherTracker)
  {
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 0");
    this.kinseyRating = otherTracker.kinseyRating;
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 1, otherTracker.kinseyRating = " + otherTracker.kinseyRating);
    this.sexDrive = otherTracker.sexDrive;
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 2, otherTracker.sexDrive = " + otherTracker.sexDrive);
    this.romanticDrive = otherTracker.romanticDrive;
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 3, otherTracker.romanticDrive " + otherTracker.romanticDrive);
    if (otherTracker.knownSexualitiesWorkingKeys != null)
    {
      this.knownSexualitiesWorkingKeys = new List<Pawn>();
      foreach (Pawn p in otherTracker.knownSexualitiesWorkingKeys)
      {
        this.knownSexualitiesWorkingKeys.Add(p);
      }
    }
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 4");
    if (otherTracker.knownSexualitiesWorkingValues != null)
    {
      this.knownSexualitiesWorkingValues = new List<int>();
      foreach (int i in otherTracker.knownSexualitiesWorkingValues)
      {
        this.knownSexualitiesWorkingValues.Add(i);
      }
    }
    ////Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 5");
    this.knownSexualities = new Dictionary<Pawn, int>();
    foreach (KeyValuePair<Pawn, int> kvp in otherTracker.knownSexualities)
    {
      this.knownSexualities.Add(kvp.Key, kvp.Value);
    }
  }
}


