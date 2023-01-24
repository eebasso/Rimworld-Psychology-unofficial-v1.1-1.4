﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.UIElements;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnCertainty))]
public class SocialCardUtility_DrawPawnCertainty
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
  {
    List<CodeInstruction> codeList = codes.ToList();
    MethodInfo methodInfo = AccessTools.Method(typeof(TooltipHandler), nameof(TooltipHandler.TipRegion), new Type[] { typeof(Rect), typeof(Func<string>), typeof(int) });

    int tipRegionIndex = 0;
    bool foundIt = false;
    for (int i = 0; i < codeList.Count(); i++)
    {
      if (codeList[i].Calls(methodInfo))
      {
        tipRegionIndex = i;
        foundIt = true;
        break;
      }
    }
    if (foundIt != true)
    {
      Log.Error("Psychology: transpiler for SocialCardUtility.DrawPawnCertainty could not find TipRegion");
      foreach (CodeInstruction c in codes)
      {
        yield return c;
      }
      yield break;
    }
    for (int i = 0; i < codeList.Count(); i++)
    {
      yield return codeList[i];
      if (i == tipRegionIndex - 6)
      {
        // Loads local object whose field is "tip." This one stays on the stack until the end when we store "tip"
        // ldloc.s 5
        yield return new CodeInstruction(codeList[tipRegionIndex - 12]);

        // Loads local object whose field is "tip." This one gets used up by the next line to load "tip"
        // ldloc.s 5
        yield return new CodeInstruction(codeList[tipRegionIndex - 12]);

        // Loads tip as a field from the local object
        // ldfld valuetype Verse.TaggedString Rimworld.SocialCardUtility/'<>c__DisplayClass28_0'::tip
        yield return new CodeInstruction(codeList[tipRegionIndex - 11]);

        // Loads method argument "pawn"
        // ldarg.0
        yield return new CodeInstruction(OpCodes.Ldarg_0);

        // Calls method and put new tip on stack
        // Complicated mess
        yield return CodeInstruction.Call(typeof(SocialCardUtility_DrawPawnCertainty), nameof(AddPersonalityEffectsToTip), new Type[] { typeof(TaggedString), typeof(Pawn) });

        // Stores new tip as a field into local object
        // stfld valuetype Verse.TaggedString Rimworld.SocialCardUtility/'<>c__DisplayClass28_0'::tip
        yield return new CodeInstruction(codeList[tipRegionIndex - 6]);
      }
    }

  }

  public static TaggedString AddPersonalityEffectsToTip(TaggedString tip, Pawn pawn)
  {
    ////Log.Message("AddPersonalityEffectsToTip, start");
    if (PsycheHelper.PsychologyEnabled(pawn) != true)
    {
      ////Log.Message("AddPersonalityEffectsToTip, PsychologyEnabled(pawn) != true");
      return tip;
    }
    ////Log.Message("AddPersonalityEffectsToTip, PsychologyEnabled(pawn) == true");
    if (pawn.Ideo == null)
    {
      ////Log.Message("AddPersonalityEffectsToTip, pawn.Ideo == null");
      return tip;
    }
    ////Log.Message("AddPersonalityEffectsToTip, pawn.Ideo != null");
    Pawn_PsycheTracker pt = PsycheHelper.Comp(pawn).Psyche;

    pt.CalculateAdjustedRatings();
    ////Log.Message("AddPersonalityEffectsToTip, CompatibilityWithIdeo");
    float changeFromPsyche = pt.CalculateCertaintyChangePerDay(pawn.Ideo, true);
    ////Log.Message("AddPersonalityEffectsToTip, add to text");
    int idnumber = pawn.thingIDNumber;
    PsycheHelper.GameComp.CachedCertaintyChangePerDayDict[idnumber] = changeFromPsyche;


    string text = "\n\nEffects from personality: " + (changeFromPsyche > 0f ? "+" : "") + changeFromPsyche.ToStringPercent();
    string textFromDict;

    ////Log.Message("AddPersonalityEffectsToTip, step 1");
    foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> memeDict in pt.dailyCertaintyFromMemesAndNodes)
    {
      ////Log.Message("AddPersonalityEffectsToTip, step 2");
      if (pawn.Ideo.HasMeme(memeDict.Key) != true)
      {
        continue;
      }
      ////Log.Message("AddPersonalityEffectsToTip, step 2");
      
      textFromDict = TextFromDict(memeDict.Value, pt);
      if (!textFromDict.NullOrEmpty())
      {
        text += "\n -  " + memeDict.Key.LabelCap + ": " + textFromDict;
      }
    }
    ////Log.Message("AddPersonalityEffectsToTip, step 4");
    foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> preceptDict in pt.dailyCertaintyFromPerceptsAndNodes)
    {
      ////Log.Message("AddPersonalityEffectsToTip, step 5");
      if (pawn.Ideo.HasPrecept(preceptDict.Key) != true)
      {
        continue;
      }
      ////Log.Message("AddPersonalityEffectsToTip, step 6");
      textFromDict = TextFromDict(preceptDict.Value, pt);
      if (!textFromDict.NullOrEmpty())
      {
        text += "\n -  " + preceptDict.Key.issue.LabelCap + ", " + preceptDict.Key.label + ": " + textFromDict;
      }
    }

    ////Log.Message("Colorize text");
    text = text.Colorize(Color.grey);
    ////Log.Message("AddPersonalityEffectsToTip, text = " + text);
    return tip + text;
  }

  public static string TextFromDict(Dictionary<PersonalityNodeDef, float> dict, Pawn_PsycheTracker pt)
  {
    string text = "";
    string percent;
    float displacement;
    float yAxis;
    int categoryInt;
    string categoryText;
    string label;
    foreach (KeyValuePair<PersonalityNodeDef, float> kvp in dict)
    {
      if (kvp.Value == 0f)
      {
        continue;
      }
      displacement = 2f * pt.GetPersonalityRating(kvp.Key) - 1f;
      yAxis = PsychologySettings.useAntonyms ? Mathf.Abs(displacement) : displacement;
      categoryInt = Mathf.RoundToInt(3f + 3f * yAxis * Mathf.Sqrt(Mathf.Abs(yAxis)));
      categoryText = ("Psyche" + PsycheCardUtility.NodeDescriptions[categoryInt]).Translate();
      label = categoryText + " " + (PsychologySettings.useAntonyms && displacement < 0f ? kvp.Key.antonymLabel : kvp.Key.label).CapitalizeFirst();
      percent = (kvp.Value > 0f ? "+" : "") + kvp.Value.ToStringPercent();
      ////Log.Message("node label = " + label);
      text += "\n   -  " + label + ": " + percent;
    }
    return text;
  }

}

