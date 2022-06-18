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

        [LogPerformance]
        public bool IncompatibleSexualityKnown(Pawn recipient)
        {
            if (this.knownSexualities.ContainsKey(recipient))
            {
                return ((knownSexualities[recipient] - 4) >= 0) != (recipient.gender == this.pawn.gender);
            }
            return false;
        }

        [LogPerformance]
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

        public void GenerateSexuality()
        {
            kinseyRating = RandKinsey(1f, 1f, 1f, 1f, 1f, 1f, 1f);
            sexDrive = GenerateSexDrive();
            romanticDrive = GenerateRomanticDrive();
        }

        public void GenerateSexuality(float b0, float b1, float b2, float b3, float b4, float b5, float b6)
        {
            kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6);
            sexDrive = GenerateSexDrive();
            romanticDrive = GenerateRomanticDrive();
        }

        /*
         * Average roll: 0.989779
         * Percent chance of rolling each number:
         * 0: 62.4949 %
         * 1: 11.3289 %
         * 2: 9.2658 %
         * 3: 6.8466 %
         * 4: 4.522 %
         * 5: 2.7612 %
         * 6: 2.7806 %
         * Percent chance of being predominantly straight: 83.08959999999999 %
         * Percent chance of being predominantly gay: 10.0638 %
         * Percent chance of being more or less straight: 73.82379999999999 %
         * Percent chance of being more or less bisexual: 20.6344 %
         * Percent chance of being more or less gay: 5.5418 %
         * Sample size: 100,000
         */
        //public int RandKinsey()
        //{
        //    if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Realistic)
        //    {
        //        return Mathf.Clamp((int)Rand.GaussianAsymmetric(0f, 1f, 3.13f), 0, 6);
        //    }
        //    else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Invisible)
        //    {
        //        return Mathf.Clamp((int)Rand.GaussianAsymmetric(3.5f, 1.7f, 1.7f), 0, 6);
        //    }
        //    else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Gaypocalypse)
        //    {
        //        return Mathf.Clamp((int)Rand.GaussianAsymmetric(7f, 3.13f, 1f), 0, 6);
        //    }
        //    else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Uniform)
        //    {
        //        return (int)(Rand.Value * 6f);
        //    }
        //    throw new NotImplementedException();
        //}
        public static int RandKinsey(float b0, float b1, float b2, float b3, float b4, float b5, float b6)
        {
            float k0 = 1f;
            float k1 = 1f;
            float k2 = 1f;
            float k3 = 1f;
            float k4 = 1f;
            float k5 = 1f;
            float k6 = 1f;

            if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Realistic)
            {
                k0 *= 62.4949f;
                k1 *= 11.3289f;
                k2 *= 9.2658f;
                k3 *= 6.8466f;
                k4 *= 4.5220f;
                k5 *= 2.7806f;
                k6 *= 2.7612f;
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Invisible)
            {
                k0 *= 7.07013f;
                k1 *= 11.8092f;
                k2 *= 19.5541f;
                k3 *= 23.1332f;
                k4 *= 19.5541f;
                k5 *= 11.8092f;
                k6 *= 7.07013f;
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Gaypocalypse)
            {
                k0 *= 2.7612f;
                k1 *= 2.7806f;
                k2 *= 4.5220f;
                k3 *= 6.8466f;
                k4 *= 9.2658f;
                k5 *= 11.3289f;
                k6 *= 62.4949f;
            }
            else if (PsychologyBase.KinseyFormula() == PsychologyBase.KinseyMode.Custom)
            {
                k0 *= PsychologyBase.kinsey0Weight();
                k1 *= PsychologyBase.kinsey1Weight();
                k2 *= PsychologyBase.kinsey2Weight();
                k3 *= PsychologyBase.kinsey3Weight();
                k4 *= PsychologyBase.kinsey4Weight();
                k5 *= PsychologyBase.kinsey5Weight();
                k6 *= PsychologyBase.kinsey6Weight();
            }
            float kbsum = k0 * b0 + k1 * b1 + k2 * b2 + k3 * b3 + k4 * b4 + k5 * b5 + k6 * b6;
            if (kbsum > 0f)
            {
                return RandKinseyCustom(k0 * b0, k1 * b1, k2 * b2, k3 * b3, k4 * b4, k5 * b5, kbsum);
            }
            float ksum = k0 + k1 + k2 + k3 + k4 + k5 + k6;
            if (ksum > 0f)
            {
                return RandKinseyCustom(k0, k1, k2, k3, k4, k5, ksum);
            }
            float bsum = b0 + b1 + b2 + b3 + b4 + b5 + b6;
            if (bsum > 0f)
            {
                return RandKinseyCustom(b0, b1, b2, b3, b4, b5, bsum);
            }
            else
            {
                return Rand.RangeInclusive(0, 6);
            }
        }

        public static int RandKinseyCustom(float k0, float k1, float k2, float k3, float k4, float k5, float ksum)
        {
            float kinseyRandSeed = Rand.Value;
            float kinseyFrac = k0 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)0;
            }
            kinseyFrac += k1 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)1;
            }
            kinseyFrac += k2 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)2;
            }
            kinseyFrac += k3 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)3;
            }
            kinseyFrac += k4 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)4;
            }
            kinseyFrac += k5 / ksum;
            if (kinseyRandSeed < kinseyFrac)
            {
                return (int)5;
            }
            else
            {
                return (int)6;
            }
        }

        public float GenerateSexDrive()
        {
            float drive = 1.1f;
            int kill = 0;
            while ((drive < 0f || 1f < drive) && kill < 500)
            {
                drive = 0.85f * (Rand.Value + Rand.Value) - 0.1f;
                kill++;
            }
            return Mathf.Clamp01(drive);
        }

        public float GenerateRomanticDrive()
        {
            float drive = 1.1f;
            int kill = 0;
            while ((drive < 0f || 1f < drive) && kill < 500)
            {
                drive = 0.85f * (Rand.Value + Rand.Value) - 0.1f;
                kill++;
            }
            return Mathf.Clamp01(drive);
        }

        public float AdjustedSexDrive
        {
            get
            {
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
                float ageFactor = 1f;
                if (pawn.gender == Gender.Female)
                {
                    ageFactor = FemaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }
                else if (pawn.gender == Gender.Male)
                {
                    ageFactor = MaleSexDriveCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
                }
                if (ageFactor < 0.001f)
                {
                    return 0f;
                }
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
                new CurvePoint(16, 1f),
                true
            },
            {
                new CurvePoint(20, 1.1f),
                true
            },
            {
                new CurvePoint(30, 1.5f),
                true
            },
            {
                new CurvePoint(40, 1.5f),
                true
            },
            {
                new CurvePoint(50, 0.8f),
                true
            },
            {
                new CurvePoint(65, 0.6f),
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
                new CurvePoint(16, 1f),
                true
            },
            {
                new CurvePoint(20, 1.5f),
                true
            },
            {
                new CurvePoint(30, 1.5f),
                true
            },
            {
                new CurvePoint(40, 1f),
                true
            },
            {
                new CurvePoint(50, 0.8f),
                true
            },
            {
                new CurvePoint(65, 0.6f),
                true
            }
        };

        //private static readonly SimpleCurve FemaleSexDriveCurve = new SimpleCurve
        //{
        //    {
        //        new CurvePoint(10, 0f),
        //        true
        //    },
        //    {
        //        new CurvePoint(30, 3f),
        //        true
        //    },
        //    {
        //        new CurvePoint(65, 1f),
        //        true
        //    },
        //    {
        //        new CurvePoint(80, 0.6f),
        //        true
        //    }
        //};

        //private static readonly SimpleCurve MaleSexDriveCurve = new SimpleCurve
        //{
        //    {
        //        new CurvePoint(12, 0f),
        //        true
        //    },
        //    {
        //        new CurvePoint(16, 3f),
        //        true
        //    },
        //    {
        //        new CurvePoint(21, 2f),
        //        true
        //    },
        //    {
        //        new CurvePoint(25, 1f),
        //        true
        //    },
        //    {
        //        new CurvePoint(40, 0.8f),
        //        true
        //    },
        //    {
        //        new CurvePoint(65, 0.6f),
        //        true
        //    }
        //};



        public int kinseyRating;
        public float sexDrive;
        public float romanticDrive;
        private List<Pawn> knownSexualitiesWorkingKeys;
        private List<int> knownSexualitiesWorkingValues;
        private Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
        private Pawn pawn;
    }
}
