using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Psychology
{
    //[StaticConstructorOnStartup]
    public class PersonalityNodeIdeoUtility
    {
        public static Dictionary<MemeDef, List<Pair<PersonalityNodeDef, float>>> memesAffectedByNodes = new Dictionary<MemeDef, List<Pair<PersonalityNodeDef, float>>>();
        public static Dictionary<PreceptDef, List<Pair<PersonalityNodeDef, float>>> preceptsAffectedByNodes = new Dictionary<PreceptDef, List<Pair<PersonalityNodeDef, float>>>();
        //public static Dictionary<PersonalityNodeDef, List<PersonalityNodeMemeCertaintyModifier>> nodeDefsWithMemes;
        //public static Dictionary<PersonalityNodeDef, List<PersonalityNodePreceptCertaintyModifier>> nodeDefsWithPrecepts;

        //static PersonalityNodeIdeoUtility()
        public static void Initialize()
        {
            //List<Pair<PersonalityNodeDef, float>> list = new List<Pair<PersonalityNodeDef, float>>();
            foreach (PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
            {
                //Log.Message("Check memeCertaintyModifiers for " + def.defName);
                if (def.memeCertaintyModifiers != null && def.memeCertaintyModifiers.Count > 0)
                {
                    //Log.Message("memeCertaintyModifiers exists for " + def.defName);
                    foreach (PersonalityNodeMemeCertaintyModifier mcm in def.memeCertaintyModifiers)
                    {
                        MemeDef meme = mcm.meme;
                        if (!memesAffectedByNodes.ContainsKey(meme))
                        {
                            memesAffectedByNodes.Add(meme, new List<Pair<PersonalityNodeDef, float>>());
                        }
                        memesAffectedByNodes[meme].Add(new Pair<PersonalityNodeDef, float>(def, mcm.modifier));
                        //MemeDef meme = mcm.meme;
                        //Pair<PersonalityNodeDef, float> pair = new Pair<PersonalityNodeDef, float>(def, mcm.modifier);
                        //if (!memesAffectedByNodes.ContainsKey(meme))
                        //{
                        //    List<Pair<PersonalityNodeDef, float>> list = new List<Pair<PersonalityNodeDef, float>>() { pair };
                        //    memesAffectedByNodes.Add(meme, list);
                        //}
                        //else
                        //{
                        //    memesAffectedByNodes[meme].Add(pair);
                        //}
                    }
                }
                //Log.Message("Check preceptCertaintyModifiers for " + def.defName);
                if (def.preceptCertaintyModifiers != null && def.preceptCertaintyModifiers.Count > 0)
                {
                    //Log.Message("preceptCertaintyModifiers exist for " + def.defName);
                    foreach (PersonalityNodePreceptCertaintyModifier pcm in def.preceptCertaintyModifiers)
                    {   
                        PreceptDef precept = pcm.precept;
                        if (!preceptsAffectedByNodes.ContainsKey(precept))
                        {
                            preceptsAffectedByNodes.Add(precept, new List<Pair<PersonalityNodeDef, float>>());
                        }
                        preceptsAffectedByNodes[precept].Add(new Pair<PersonalityNodeDef, float>(def, pcm.modifier));
                        //PreceptDef precept = pcm.precept;
                        //Pair<PersonalityNodeDef, float> pair = new Pair<PersonalityNodeDef, float>(def, pcm.modifier);
                        //if (!preceptsAffectedByNodes.ContainsKey(precept))
                        //{
                        //    List<Pair<PersonalityNodeDef, float>> list = new List<Pair<PersonalityNodeDef, float>>() { pair };
                        //    preceptsAffectedByNodes.Add(precept, list);
                        //}
                        //else
                        //{
                        //    preceptsAffectedByNodes[precept].Add(pair);
                        //}
                    }
                }
            }

            //foreach (PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
            //{
            //    if (def.memeCertaintyModifiers != null && def.memeCertaintyModifiers.Count > 0)
            //    {
            //        nodeDefsWithMemes.Add(def, def.memeCertaintyModifiers);
            //    }
            //    if (def.preceptCertaintyModifiers != null && def.preceptCertaintyModifiers.Count > 0)
            //    {
            //        nodeDefsWithPrecepts.Add(def, def.preceptCertaintyModifiers);
            //    }
            //}
        }
    }
}

