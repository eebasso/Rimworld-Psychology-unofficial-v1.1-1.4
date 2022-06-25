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

        public static int[] GetSignArray(int X, int length)
        {
            int binaryBase = (int)Math.Pow(2, length);
            int X2 = X.Mod(binaryBase) + binaryBase;
            BitArray bitArray = new BitArray(new int[] { X2 });
            return bitArray.Cast<bool>().Select(b => b ? 1 : -1).Take(length).ToArray();
        }
    }
}
