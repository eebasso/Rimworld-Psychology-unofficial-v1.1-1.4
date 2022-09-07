using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
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

    public Pawn_SexualityTracker(Pawn pawn)
    {
        this.pawn = pawn;
        GenerateSexuality();
    }

    //[LogPerformance]
    public virtual bool IncompatibleSexualityKnown(Pawn recipient)
    {
        if (this.knownSexualities.ContainsKey(recipient))
        {
            return ((knownSexualities[recipient] - 4) >= 0) != (recipient.gender == this.pawn.gender);
        }
        return false;
    }

    //[LogPerformance]
    public virtual void LearnSexuality(Pawn p)
    {
        if (p != null && PsycheHelper.PsychologyEnabled(pawn) && !knownSexualities.Keys.Contains(p))
        {
            knownSexualities.Add(p, PsycheHelper.Comp(p).Sexuality.kinseyRating);
        }
    }

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
        Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1, false);
        Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1, false);
        Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
    }

    //public virtual void GenerateSexuality(int inputSeed = 0)
    //{
    //    GenerateSexuality(1f, 1f, 1f, 1f, 1f, 1f, 1f, inputSeed);
    //}

    public virtual void GenerateSexuality(float b0 = 1f, float b1 = 1f, float b2 = 1f, float b3 = 1f, float b4 = 1f, float b5 = 1f, float b6 = 1f, int inputSeed = 0)
    {
        //kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
        //sexDrive = GenerateSexDrive(inputSeed);
        //romanticDrive = GenerateRomanticDrive(inputSeed);
        GenerateKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
        GenerateSexDrive(inputSeed);
        GenerateRomanticDrive(inputSeed);
    }

    public virtual void GenerateKinsey(float b0 = 1f, float b1 = 1f, float b2 = 1f, float b3 = 1f, float b4 = 1f, float b5 = 1f, float b6 = 1f, int inputSeed = 0)
    {
        kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
    }

    public virtual void GenerateSexDrive(int inputSeed = 0)
    {
        sexDrive = RandSexDrive(inputSeed);
    }

    public virtual void GenerateRomanticDrive(int inputSeed = 0)
    {
        romanticDrive = RandRomanticDrive(inputSeed);
    }

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

    public virtual int RandKinsey(float b0 = 1f, float b1 = 1f, float b2 = 1f, float b3 = 1f, float b4 = 1f, float b5 = 1f, float b6 = 1f, int inputSeed = 0)
    {
        List<float> bList = new List<float> { b0, b1, b2, b3, b4, b5, b6 };
        List<float> kList = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        float kSum = 0f;
        if (PsychologySettings.kinseyFormula != KinseyMode.Custom)
        {
            kList = PsycheHelper.KinseyModeWeightDict[PsychologySettings.kinseyFormula];
            kSum = kList.Sum();
        }
        else
        {
            kList = PsychologySettings.kinseyWeightCustom;
            kSum = kList.Sum();
            if (kSum == 0f)
            {
                kList = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
                kSum = 7f;
            }
        }
        float kbSum = 0f;
        List<float> kCumSumList = new List<float>();
        List<float> kbCumSumList = new List<float>();
        for (int i = 0; i < bList.Count(); i++)
        {
            kbSum += kList[i] * bList[i];
            kCumSumList.Add(kSum);
            kbCumSumList.Add(kbSum);
        }
        if (kbCumSumList[6] > 0f)
        {
            return RandKinseyByWeight(kbCumSumList, inputSeed);
        }
        return RandKinseyByWeight(kCumSumList, inputSeed);
    }

    public virtual int RandKinseyByWeight(List<float> kCumSumList, int inputSeed = 0)
    {
        float randValue = Rand.ValueSeeded(17 * PsycheHelper.PawnSeed(this.pawn) + 11 * inputSeed + 31) * kCumSumList[6];
        for (int s = 0; s < 6; s++)
        {
            if (randValue <= kCumSumList[s])
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

    public virtual float RandRomanticDrive(int inputSeed = 0)
    {
        return RandSexDrive(859456 + 3 * inputSeed);
    }

    public float AdjustedSexDrive
    {
        get
        {
            float age = pawn.ageTracker.AgeBiologicalYears;
            SpeciesSettings settings = PsychologySettings.speciesDict[pawn.def.defName];
            float minLovinAge = settings.minLovinAge;
            if (!settings.enablePsyche || minLovinAge < 0f)
            {
                return 0f;
            }
            if (!settings.enableAgeGap)
            {
                return age > minLovinAge ? this.sexDrive : 0f;
            }
            if (minLovinAge == 0f)
            {
                return this.sexDrive;
            }
            float scaledAge = PsycheHelper.LovinAgeToVanilla(age, minLovinAge);
            float ageFactor = 1f;
            if (pawn.gender == Gender.Female)
            {
                ageFactor = FemaleSexDriveCurve.Evaluate(scaledAge);
            }
            else if (pawn.gender == Gender.Male)
            {
                ageFactor = MaleSexDriveCurve.Evaluate(scaledAge);
            }
            else
            {
                // Maybe one day other genders will come to the Rim...
                ageFactor = 0.5f * (FemaleSexDriveCurve.Evaluate(scaledAge) + MaleSexDriveCurve.Evaluate(scaledAge));
            }
            return ageFactor * this.sexDrive;
        }
    }

    public float AdjustedRomanticDrive
    {
        get
        {
            float age = pawn.ageTracker.AgeBiologicalYears;
            SpeciesSettings settings = PsychologySettings.speciesDict[pawn.def.defName];
            float minDatingAge = settings.minDatingAge;
            if (!settings.enablePsyche || minDatingAge < 0f)
            {
                return 0f;
            }
            if (!settings.enableAgeGap)
            {
                return age > minDatingAge ? this.sexDrive : 0f;
            }
            if (minDatingAge == 0f)
            {
                return this.sexDrive;
            }
            float scaledAge = PsycheHelper.LovinAgeToVanilla(age, minDatingAge);
            float ageFactor = RomanticDriveCurve.Evaluate(scaledAge);
            return ageFactor * this.romanticDrive;
        }
    }

    public static readonly SimpleCurve FemaleSexDriveCurve = new SimpleCurve
    {
        {
            new CurvePoint(12, 0f),
            true
        },
        {
            new CurvePoint(15, 1f),
            true
        },
        {
            new CurvePoint(35, 1.6f),
            true
        },
        {
            new CurvePoint(50, 1f),
            true
        },
        {
            new CurvePoint(80, 0.6f),
            true
        },
    };

    public static readonly SimpleCurve MaleSexDriveCurve = new SimpleCurve
    {
        {
            new CurvePoint(12, 0f),
            true
        },
        {
            new CurvePoint(15, 1f),
            true
        },
        {
            new CurvePoint(20, 1.6f),
            true
        },
        {
            new CurvePoint(50, 1f),
            true
        },
        {
            new CurvePoint(80, 0.6f),
            true
        },
    };

    public static readonly SimpleCurve RomanticDriveCurve = new SimpleCurve
    {
        {
            new CurvePoint(10f, 0f),
            true
        },
        {
            new CurvePoint(15f, 1.3f),
            true
        },
        {
            new CurvePoint(25f, 1.6f),
            true
        },
        {
            new CurvePoint(35f, 1.3f),
            true
        },
        {
            new CurvePoint(50f, 1f),
            true
        },
        {
            new CurvePoint(80f, 0.8f),
            true
        },
    };

    
}


