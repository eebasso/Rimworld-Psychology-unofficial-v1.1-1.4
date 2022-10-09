using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psychology;

[StaticConstructorOnStartup]
public static class PersonalityNodeIdeoUtility
{
    public static Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>> memesAffectedByNodes = new Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>>();
    public static Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>> preceptsAffectedByNodes = new Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>>();
    //public static Dictionary<PersonalityNodeDef, List<PersonalityNodeMemeCertaintyModifier>> nodeDefsWithMemes;
    //public static Dictionary<PersonalityNodeDef, List<PersonalityNodePreceptCertaintyModifier>> nodeDefsWithPrecepts;

    static PersonalityNodeIdeoUtility()
    {
        //List<Pair<PersonalityNodeDef, float>> list = new List<Pair<PersonalityNodeDef, float>>();
        foreach (PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefsListForReading)
        {
            if (def.memeCertaintyModifiers != null && def.memeCertaintyModifiers.Count > 0)
            {
                //Log.Message("memeCertaintyModifiers exists for " + def.defName);
                foreach (PersonalityNodeMemeCertaintyModifier mcm in def.memeCertaintyModifiers)
                {
                    if (!memesAffectedByNodes.ContainsKey(mcm.meme))
                    {
                        memesAffectedByNodes.Add(mcm.meme, new Dictionary<PersonalityNodeDef, float>());
                    }
                    memesAffectedByNodes[mcm.meme][def] = mcm.modifier;
                }
            }
            //Log.Message("Check preceptCertaintyModifiers for " + def.defName);
            if (def.preceptCertaintyModifiers != null && def.preceptCertaintyModifiers.Count > 0)
            {
                //Log.Message("preceptCertaintyModifiers exist for " + def.defName);
                foreach (PersonalityNodePreceptCertaintyModifier pcm in def.preceptCertaintyModifiers)
                {
                    if (!preceptsAffectedByNodes.ContainsKey(pcm.precept))
                    {
                        preceptsAffectedByNodes.Add(pcm.precept, new Dictionary<PersonalityNodeDef, float>());
                    }
                    preceptsAffectedByNodes[pcm.precept][def] = pcm.modifier;
                }
            }
        }
    }

    public static float[] FavoredDisplacmentForIdeo(Ideo ideo, bool randomize = false, bool normalize = true)
    {
        float[] favoredVector = new float[PersonalityNodeMatrix.order];
        int i;
        bool noContributions = true;
        foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in memesAffectedByNodes)
        {
            if (ideo.HasMeme(kvp0.Key) != true)
            {
                continue;
            }
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                favoredVector[PersonalityNodeMatrix.indexDict[kvp1.Key]] += kvp1.Value;
                noContributions = false;
            }
        }
        foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp0 in preceptsAffectedByNodes)
        {
            if (ideo.HasPrecept(kvp0.Key) != true)
            {
                continue;
            }
            foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
            {
                favoredVector[PersonalityNodeMatrix.indexDict[kvp1.Key]] += kvp1.Value;
                noContributions = false;
            }
        }
        if (noContributions == true)
        {
            return favoredVector;
        }
        float favoredNorm = PersonalityNodeMatrix.DotProduct(favoredVector, favoredVector);
        if (randomize)
        {
            // Pick out a random unit vector
            float randomNorm = 0f;
            float[] randomVector = new float[PersonalityNodeMatrix.order];
            while (randomNorm == 0f)
            {
                float rand;
                for (i = 0; i < randomVector.Count(); i++)
                {
                    rand = Rand.Gaussian();
                    randomVector[i] = rand;
                    randomNorm += rand * rand;
                }
                randomNorm = Mathf.Sqrt(randomNorm);
            }
            float alpha = 1f;
            for (i = 0; i < randomVector.Count(); i++)
            {
                favoredVector[i] += alpha * favoredNorm * randomVector[i] / randomNorm;
            }
        }
        if (!normalize)
        {
            return favoredVector;
        }
        // Norm could still be zero by coincidental cancellations between modifiers
        if (favoredNorm > 0f)
        {
            favoredNorm = Mathf.Sqrt(favoredNorm);
            for (i = 0; i < PersonalityNodeMatrix.order; i++)
            {
                favoredVector[i] /= favoredNorm;
            }
        }
        return favoredVector;
    }



}


