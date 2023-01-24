using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;
using HarmonyLib;

namespace Psychology;

public static class ExtensionHelper
{
  public static bool IsUntreatedInsomniac(this Pawn p)
  {
    return p?.story?.traits?.HasTrait(TraitDefOfPsychology.Insomniac) == true && p.health?.hediffSet?.HasHediff(HediffDefOfPsychology.SleepingPills) != true;
  }

  public static bool PsychologyEnabled(this Pawn p) => PsycheHelper.PsychologyEnabled(p);

  public static bool RomanceEnabled(this Pawn p, bool isDating) => SpeciesHelper.RomanceEnabled(p, isDating);

  public static Pawn_PsycheTracker Psyche(this Pawn p) => PsycheHelper.Comp(p).Psyche;

  public static Pawn_SexualityTracker Sexuality(this Pawn p) => PsycheHelper.Comp(p).Sexuality;

}

