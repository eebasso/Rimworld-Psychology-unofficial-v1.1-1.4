using UnityEngine;using RimWorld;using Verse;using Verse.Sound;using System;using System.Runtime.Remoting.Contexts;using HarmonyLib;using UnityEngine.UIElements.Experimental;using System.Reflection;using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;

namespace Psychology.Harmony;
[StaticConstructorOnStartup]
public class Pawnmorpher_Patches{
    public static MethodInfo originalInfo;
    public static HarmonyMethod harmonyMethod;

    static Pawnmorpher_Patches()
    {
        HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate.Pawnmorph");

        originalInfo = AccessTools.Method(typeof(PsycheHelper), nameof(PsycheHelper.IsHumanlike));
        harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(PsycheHelper_IsHumanlike_Prefix));
        harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        originalInfo = AccessTools.Method(typeof(PsycheHelper), nameof(PsycheHelper.Comp));
        harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(PsycheHelper_Comp_Prefix));
        harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        originalInfo = AccessTools.PropertyGetter(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.IsVisible));
        harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_IsVisible_Prefix));
        harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        //originalInfo = AccessTools.PropertyGetter(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.PawnToShowInfoAbout));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_PawnToShowInfoAbout_Postfix));
        //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        originalInfo = AccessTools.Method(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.FillTabPawnHook));
        harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_FillTabPawnHook_Postfix));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        //originalInfo = AccessTools.Method(typeof(SpeciesHelper), nameof(SpeciesHelper.Initialize));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(SpeciesHelper_Initialize_Postfix));
        //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        foreach (ThingDef pawnDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (pawnDef.category == ThingCategory.Pawn)
            {
                SpeciesHelper.AddInspectorTabToDefAndCorpseDef(pawnDef);
            }
        }

        Log.Message("Psychology: completed patches for compatibility with Pawnmorpher.");

    }

    public static bool PsycheHelper_IsHumanlike_Prefix(ref bool __result, Pawn pawn)
    {
        Log.Message("IsHumanlike_PawnmorpherPrefix, FormerHumanUtilities.IsHumanlike");
        __result = Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn);
        return false;
    }

    public static bool PsycheHelper_Comp_Prefix(ref CompPsychology __result, Pawn pawn)
    {
        Log.Message("PsycheHelper_Comp_Prefix, step 0");
        if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) != true)
        {
            Log.Message("PsycheHelper_Comp_Prefix, IsFormerHuman(pawn) != true, pawn = " + pawn.Label);
            return true;
        }
        Log.Message("PsycheHelper_Comp_Prefix, IsFormerHuman(pawn) == true, pawn = " + pawn.Label);
        Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
        if (originalPawn == null)
        {
            Log.Message("PsycheHelper_Comp_Prefix, originalPawn == null, pawn = " + pawn.Label);
            return true;
        }
        Log.Message("PsycheHelper_Comp_Prefix, originalPawn = " + originalPawn.Label + ", pawn = " + pawn.Label);
        CompPsychology compPsychology = originalPawn.GetComp<CompPsychology>();
        if (compPsychology == null)
        {
            Log.Message("PsycheHelper_Comp_Prefix, originalPawnCompPsychology == null, pawn = " + pawn.Label);
            return true;
        }
        __result = compPsychology;
        return false;
    }

    // This is different from PsychologyEnabled because some pawns can lose sentient while still having former personalities
    public static bool ITabPawnPsyche_IsVisible_Prefix(ref bool __result, ITab_Pawn_Psyche __instance)
    {
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, start");
        Pawn pawn = __instance.PawnToShowInfoAbout;
        if (pawn == null)
        {
            Log.Message("ITabPawnPsyche_IsVisible_Prefix, pawn == null");
            __result = false;
            return false;
        }
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, pawn != null");
        if (Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn) != true)
        {
            Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsHumanlike(pawn) != true, pawn = " + pawn.Label);
            __result = false;
            return false;
        }
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsHumanlike(pawn) == true, pawn = " + pawn.Label);
        if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) != true)
        {
            Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsFormerHuman(pawn) != true");
            return true;
        }
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsFormerHuman(pawn) == true");
        Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
        if (originalPawn == null)
        {
            Log.Message("ITabPawnPsyche_IsVisible_Prefix, originalPawn == null");
            return true;
        }
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, originalPawn != null");
        SpeciesSettings settings = SpeciesHelper.GetOrMakeSettingsFromHumanlikeDef(originalPawn.def, true);
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, get/set SpeciesSettings for originalPawn.def = " + originalPawn.def.defName);
        if (settings.enablePsyche != true)
        {
            Log.Message("ITabPawnPsyche_IsVisible_Prefix, settings.enablePsyche != true");
            __result = false;
            return false;
        }
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, settings.enablePsyche == true");
        __result = PsycheHelper.DoesCompExist(originalPawn);
        Log.Message("ITabPawnPsyche_IsVisible_Prefix, DoesCompExist = " + __result);
        return false;
    }

    //public static void ITabPawnPsyche_PawnToShowInfoAbout_Postfix(ref Pawn __result)
    //{
    //    Log.Message("ITabPawnPsyche_PawnToShowInfoAbout_Postfix, step 0");
    //    if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(__result) == false)
    //    {
    //        Log.Message("ITabPawnPsyche_PawnToShowInfoAbout_Postfix, IsFormerHuman(pawn) == false, pawn = " + __result.Label);
    //        return;
    //    }
    //    Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(__result);
    //    if (originalPawn == null)
    //    {
    //        Log.Message("ITabPawnPsyche_PawnToShowInfoAbout_Postfix, GetOriginalPawnOfFormerHuman(__result) == null, pawn = " + __result.Label);
    //        return;
    //    }
    //    Log.Message("ITabPawnPsyche_PawnToShowInfoAbout_Postfix, pawn = " + __result.Label + ", originalPawn = " + originalPawn.Label);
    //    __result = originalPawn;
    //}

    public static void ITabPawnPsyche_FillTabPawnHook_Postfix(ref Pawn __result)
    {
        if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(__result) == false)
        {
            return;
        }
        Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(__result);
        if (originalPawn == null)
        {
            return;
        }
        __result = originalPawn;
    }

    //public static void SpeciesHelper_Initialize_Postfix()
    //{
    //    foreach (ThingDef pawnDef in DefDatabase<ThingDef>.AllDefs)
    //    {
    //        if (pawnDef.category == ThingCategory.Pawn)
    //        {
    //            SpeciesHelper.AddInspectorTabToDefAndCorpseDef(pawnDef);
    //        }
    //    }
    //}

    //public static void PsychologyGameComponent_LoadedGame()
    //{
    //    foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
    //    {
    //        SpeciesHelper.AddInspectorTabToDefAndCorpseDef(pawn.def);
    //    }
    //}

    //public static bool PsycheHelper_PsychologyEnabled_Prefix(ref bool __result, Pawn pawn)
    //{
    //    Log.Message("PsycheHelper_PsychologyEnabled_Prefix, start");
    //    if (pawn == null)
    //    {
    //        Log.Message("PsycheHelper_PsychologyEnabled_Prefix, pawn was null");
    //        __result = false;
    //        return false;
    //    }
    //    Log.Message("IsHumanlike_PawnmorpherPrefix, FormerHumanUtilities.IsHumanlike");
    //    __result = Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn);
    //    return false;
    //}

    //public static bool PsychologyEnabled_PawnmorpherPrefix(ref bool __result, Pawn pawn)
    //{
    //    if (pawn == null)
    //    {
    //        Log.Message("PsychologyEnabled_PawnmorpherPrefix, pawn was null");
    //        __result = false;
    //        return false;
    //    }
    //    if(!Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn))
    //    {
    //        Log.Message("PsychologyEnabled_PawnmorpherPrefix, pawn was not humanlike, pawn = " + pawn.Label);
    //        __result = false;
    //        return false;
    //    }
    //    if (PsychologySettings.speciesDict.ContainsKey(pawn.def.defName))
    //    {
    //        Log.Message("PsychologyEnabled_PawnmorpherPrefix, speciesDict.ContainsKey(pawn.def.defName), pawn = " + pawn.Label + ", defName = " + pawn.def.defName);
    //        return true;
    //    }
    //    Log.Message("PsychologyEnabled_PawnmorpherPrefix, !speciesDict.ContainsKey(pawn.def.defName), pawn = " + pawn.Label);
    //    PsychologySettings.speciesDict.Add(pawn.def.defName, new SpeciesSettings());
    //    Log.Message("PsychologyEnabled_PawnmorpherPrefix, defName = " + pawn.def.defName + " registered");
    //    SpeciesHelper.AddCompsToPsycheEnabledDef(pawn.def);
    //    Log.Message("PsychologyEnabled_PawnmorpherPrefix, added comps to " + pawn.def.defName);
    //    return true;
    //}
}