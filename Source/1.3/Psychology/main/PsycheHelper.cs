using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Psychology
{
    public static class PsycheHelper
    {
        public static bool PsychologyEnabled(Pawn pawn)
        {
            return pawn != null && pawn.GetComp<CompPsychology>() != null && pawn.GetComp<CompPsychology>().isPsychologyPawn;
        }

        [LogPerformance]
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

        public static float Mod(this float x, float m)
        {
            float r = x % m;
            return r < 0 ? r + m : r;
        }

        public static int Mod(this int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static int[] GetSignArray(this int X, int length)
        {
            //int binaryBase = (int)Math.Pow(2, length);
            //int X2 = X.Mod(binaryBase) + binaryBase;
            //BitArray bitArray = new BitArray(new int[] { X2 });
            //return bitArray.Cast<bool>().Select(b => b ? 1 : -1).Take(length).ToArray();
            int[] signs = new int[length];
            signs[0] = X % 2 == 0 ? -1 : 1;
            for (int b = 1; b < length; b++)
            {
                X /= 2;
                signs[b] = X % 2 == 0 ? -1 : 1;
            }
            return signs;
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
            float z = (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(- 0.5f * x * x);
            return sign ? 1f - z : z;
        }

        public static float NormalCDFInv(float p)
        {
            p = Mathf.Clamp(p, 0.00001f, 0.99999f);
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
    }
}