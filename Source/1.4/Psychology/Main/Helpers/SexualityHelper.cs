using System;
using System.Collections;
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

[StaticConstructorOnStartup]
public static class SexualityHelper
{
  public static SimpleCurve vanillaLovinIntervalHoursFromAgeCurve;
  public const float mtbDrivePower = -2f;
  public const float curveFactorPower = -1f / 3.24f;
  public const float averageSexDrive = 0.85f;

  static SexualityHelper()
  {
    SimpleCurve curve = (SimpleCurve)AccessTools.Field(typeof(JobDriver_Lovin), "LovinIntervalHoursFromAgeCurve").GetValue(null);
    List<CurvePoint> list = curve.Points.ListFullCopy();
    vanillaLovinIntervalHoursFromAgeCurve = new SimpleCurve(list);
  }

  public static float LovinMtbHoursPsychology(Pawn pawn)
  {
    float mtbHours = LovinMtbHoursFromAgeAndDrive(pawn);
    if (mtbHours < 0f)
    {
      return -1f;
    }
    float factors = LovinMtbHoursExtraFactors(pawn);
    if (factors < 0f)
    {
      return -1f;
    }
    return mtbHours * factors;
  }

  public static float LovinMtbHoursFromAgeAndDrive(Pawn pawn)
  {
    if (!SpeciesHelper.RomanceEnabled(pawn, false) || pawn?.ageTracker?.Adult != true)
    {
      // No underage lovin
      return -1f; 
    }
    CalculateSexDrive(pawn, out float adjustedSexDrive, out float mtbHoursAverage);
    return adjustedSexDrive > 0f && mtbHoursAverage >= 0f ? mtbHoursAverage * Mathf.Pow(adjustedSexDrive / averageSexDrive, mtbDrivePower) : -1f;
  }

  public static void CalculateSexDrive(Pawn pawn, out float adjustedSexDrive, out float mtbHoursAverage)
  {
    if (!SpeciesHelper.RomanceLifestageAgeCheck(pawn, false))
    {
      adjustedSexDrive = 0f;
      mtbHoursAverage = -1f;
      return;
    }

    SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
    float minAge = settings.minLovinAge;
    if (minAge < 0f)
    {
      adjustedSexDrive = 0f;
      mtbHoursAverage = -1f;
      return;
    }

    float sexDrive = PsychologySettings.enableKinsey ? PsycheHelper.Comp(pawn).Sexuality.sexDrive : pawn.story?.traits?.HasTrait(TraitDefOf.Asexual) != true ? averageSexDrive : 0.5f * Pawn_SexualityTracker.asexualCutoff;

    GetLovinCurveAndAverage(pawn, out mtbHoursAverage, out SimpleCurve ageCurve, out bool useVanilla);

    if (minAge == 0f)
    {
      adjustedSexDrive = sexDrive;
      return;
    }

    CalculateLovinCurveAndDriveFactors(pawn, ageCurve, mtbHoursAverage, useVanilla, out float lovinMtbIntervalHoursCurveFactor, out float psychologyFactor);

    if (useVanilla)
    {
      //adjustedSexDrive = sexDrive * Mathf.Pow(lovinMtbIntervalHoursCurveFactor, 0.5f * curveFactorPower) * Mathf.Pow(psychologyFactor, 0.5f);
      adjustedSexDrive = sexDrive * psychologyFactor;
    }
    else
    {
      adjustedSexDrive = sexDrive * Mathf.Pow(lovinMtbIntervalHoursCurveFactor, curveFactorPower);
    }
  }

  public static void GetLovinCurveAndAverage(Pawn pawn, out float mtbHoursAverage, out SimpleCurve ageCurve, out bool useVanilla)
  {
    ageCurve = AlienRaceLovinIntervalHoursFromAgeCurveHook(pawn);
    useVanilla = ageCurve == null;
    if (useVanilla)
    {
      ageCurve = vanillaLovinIntervalHoursFromAgeCurve;
    }
    mtbHoursAverage = MeanOfCurve(ageCurve);
  }

  public static SimpleCurve AlienRaceLovinIntervalHoursFromAgeCurveHook(Pawn pawn) => null;

  public static void CalculateLovinCurveAndDriveFactors(Pawn pawn, SimpleCurve ageCurve, float mtbHoursAverage, bool useVanilla, out float lovingMtbHoursCurveFactor, out float psychologyFactor)
  {
    float bioAge = pawn.ageTracker.AgeBiologicalYearsFloat;
    SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
    float minAge = settings.minLovinAge;

    if (minAge == 0f)
    {
      lovingMtbHoursCurveFactor = 1f;
      psychologyFactor = 1f;
      return;
    }

    float effectiveAge = bioAge;

    if (useVanilla)
    {
      effectiveAge = PsycheHelper.LovinBioAgeToVanilla(bioAge, minAge);
      psychologyFactor = PsychologyLovinAgeFactor(effectiveAge, pawn.gender);
    }
    else
    {
      psychologyFactor = 1f;
    }
    float mtbHoursFromCurve = ageCurve.Evaluate(effectiveAge);
    lovingMtbHoursCurveFactor = mtbHoursAverage > 0f ? mtbHoursFromCurve / mtbHoursAverage : -1f;
  }

  public static float PsychologyLovinAgeFactor(float vanillaAge, Gender gender)
  {
    switch (gender)
    {
      case Gender.Female:
        return PsychologySettings.FemaleSexDriveAgeCurve.Evaluate(vanillaAge);
      case Gender.Male:
        return PsychologySettings.MaleSexDriveAgeCurve.Evaluate(vanillaAge);
      // Maybe one day other genders will come to the Rim...
      default:
        return 0.5f * (PsychologySettings.FemaleSexDriveAgeCurve.Evaluate(vanillaAge) + PsychologySettings.MaleSexDriveAgeCurve.Evaluate(vanillaAge));
    }
  }

  public static float MeanOfCurve(SimpleCurve curve)
  {
    List<CurvePoint> curvePoints = curve.Points;
    if (curvePoints.NullOrEmpty())
    {
      Log.Warning("CalculateMeanOfCurve, curvePoints were null or empty");
      return 0f;
    }
    float dx;
    float xrange = 0f;
    float area = 0f;
    float yMax = curvePoints.Max(cp => cp.y);

    for (int i = 0; i < curvePoints.Count - 1; i++)
    {
      CurvePoint cp1 = curvePoints[i];
      CurvePoint cp2 = curvePoints[i + 1];
      dx = cp2.x - cp1.x;
      xrange += dx;
      area += 0.5f * (cp2.y + cp1.y) * dx;
    }
    return xrange > 0f ? area / xrange : curvePoints[0].y;
  }

  public static float LovinMtbHoursExtraFactors(Pawn pawn)
  {
    float pain = pawn.health.hediffSet.PainTotal;
    if (pain >= 1f)
    {
      return -1f;
    }
    float painFactor = 1f / (1f - pain);

    float consciousnessFactor = 1f;
    float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
    if (level < 0.5f)
    {
      if (level <= 0f)
      {
        return -1f;
      }
      consciousnessFactor = 1f / (2f * level);
    }

    float geneFactor = 1f;
    if (ModsConfig.BiotechActive && pawn.genes != null)
    {
      foreach (Gene gene in pawn.genes.GenesListForReading)
      {
        geneFactor *= gene.def.lovinMTBFactor;
      }
    }

    painFactor *= painFactor;
    consciousnessFactor *= consciousnessFactor;
    return painFactor * consciousnessFactor * geneFactor;
  }

}

