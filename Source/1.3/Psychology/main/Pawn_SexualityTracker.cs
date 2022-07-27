using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Psychology
{
    public class Pawn_SexualityTracker : IExposable
    {
        public Pawn_SexualityTracker(Pawn pawn)
        {
            this.pawn = pawn;
            GenerateSexuality();
        }

        //[LogPerformance]
        public bool IncompatibleSexualityKnown(Pawn recipient)
        {
            if (this.knownSexualities.ContainsKey(recipient))
            {
                return ((knownSexualities[recipient] - 4) >= 0) != (recipient.gender == this.pawn.gender);
            }
            return false;
        }

        //[LogPerformance]
        public void LearnSexuality(Pawn p)
        {
            if (p != null && PsycheHelper.PsychologyEnabled(pawn) && !knownSexualities.Keys.Contains(p))
            {
                knownSexualities.Add(p, PsycheHelper.Comp(p).Sexuality.kinseyRating);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
            Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1, false);
            Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1, false);
            Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
        }

        public void GenerateSexuality(int inputSeed = 0)
        {
            GenerateSexuality(1f, 1f, 1f, 1f, 1f, 1f, 1f, inputSeed);
        }

        public void GenerateSexuality(float b0, float b1, float b2, float b3, float b4, float b5, float b6, int inputSeed = 0)
        {
            kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
            sexDrive = GenerateSexDrive(inputSeed);
            romanticDrive = GenerateRomanticDrive(inputSeed);
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
        
        public int RandKinsey(float b0, float b1, float b2, float b3, float b4, float b5, float b6, int inputSeed = 0)
        {
            List<float> bList = new List<float> { b0, b1, b2, b3, b4, b5, b6 };
            List<float> kList = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
            if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Realistic)
            {
                kList = new List<float> { 62.4949f, 11.3289f, 9.2658f, 6.8466f, 4.5220f, 2.7806f, 2.7612f };
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Invisible)
            {
                kList = new List<float> { 7.07013f, 11.8092f, 19.5541f, 23.1332f, 19.5541f, 11.8092f, 7.07013f };
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Gaypocalypse)
            {
                kList = new List<float> { 2.7612f, 2.7806f, 4.5220f, 6.8466f, 9.2658f, 11.3289f, 62.4949f };
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Custom)
            {
                kList = new List<float> { PsychologyBase.kinsey0Weight(), PsychologyBase.kinsey1Weight(), PsychologyBase.kinsey2Weight(), PsychologyBase.kinsey3Weight(), PsychologyBase.kinsey4Weight(), PsychologyBase.kinsey5Weight(), PsychologyBase.kinsey6Weight() };
                if (kList.Sum() == 0f)
                {
                    kList = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
                }
            }
            float kSum = 0f;
            float kbSum = 0f;
            List<float> kCumSumList = new List<float>();
            List<float> kbCumSumList = new List<float>();
            for (int i = 0; i < bList.Count(); i++)
            {
                kSum += kList[i];
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

        public int RandKinseyByWeight(List<float> kCumSumList, int inputSeed = 0)
        {
            float randValue = Rand.ValueSeeded(17 * pawn.GetHashCode() + 31 + 11 * inputSeed) * kCumSumList[6];
            for (int s = 0; s < 6; s++)
            {
                if (randValue <= kCumSumList[s])
                {
                    return s;
                }
            }
            return 6;
        }

        public float GenerateSexDrive(int inputSeed = 0)
        {
            float drive = -1f;
            int kill = 0;
            int pawnSeed = this.pawn.GetHashCode();
            int seed1 = 11 * pawnSeed + 131 + 2 * inputSeed;
            int seed2 = 13 * pawnSeed + 89 + 7 * inputSeed;
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

        public float GenerateRomanticDrive(int inputSeed = 0)
        {
            return GenerateSexDrive(859456 + 3 * inputSeed);
        }

        public float AdjustedSexDrive
        {
            get
            {
                if (!PsychologyBase.ActivateKinsey())
                {
                    return 1f;
                }
                float ageFactor = 1f;
                if (pawn.gender == Gender.Female)
                {
                    ageFactor = FemaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }
                else if (pawn.gender == Gender.Male)
                {
                    ageFactor = MaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }



                return ageFactor * this.sexDrive;
            }
        }

        public float AdjustedRomanticDrive
        {
            get
            {
                if (!PsychologyBase.ActivateKinsey())
                {
                    return 1f;
                }
                float ageFactor = 1f;
                //if (pawn.gender == Gender.Female)
                //{
                //    ageFactor = FemaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                //}
                //else if (pawn.gender == Gender.Male)
                //{
                //    ageFactor = MaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                //}
                //if (ageFactor < 0.001f)
                //{
                //    return 0f;
                //}


                return 0.5f * (1f + ageFactor) * this.romanticDrive;
            }
        }

        private static readonly SimpleCurve FemaleSexDriveCurve = new SimpleCurve
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

        private static readonly SimpleCurve MaleSexDriveCurve = new SimpleCurve
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

        private static readonly SimpleCurve RomanticDriveCurve = new SimpleCurve
        {
            {
                new CurvePoint(10, 0f),
                true
            },
            {
                new CurvePoint(15, 1f),
                true
            },
            {
                new CurvePoint(25, 1.6f),
                true
            },
            {
                new CurvePoint(50, 1f),
                true
            },
            {
                new CurvePoint(80, 1f),
                true
            },
        };

        public int kinseyRating;
        public float sexDrive;
        public float romanticDrive;
        private List<Pawn> knownSexualitiesWorkingKeys;
        private List<int> knownSexualitiesWorkingValues;
        private Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
        private Pawn pawn;
    }
}
