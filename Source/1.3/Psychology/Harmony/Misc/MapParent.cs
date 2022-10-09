using UnityEngine;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using System.Linq;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(MapParent),nameof(MapParent.Abandon))]
public static class MapParent_Abandon_Patch
{
    public static void Abandon(MapParent __instance)
    {
        PsycheHelper.GameComp.RemoveMayorOfThisColony(__instance.Map.Tile);
    }
}


