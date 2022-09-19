using UnityEngine;using RimWorld;using Verse;using Verse.Sound;using System;using System.Runtime.Remoting.Contexts;using HarmonyLib;using UnityEngine.UIElements.Experimental;using System.Reflection;using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;
using static Pawnmorph.MutationRuleDef;
using System.Runtime;

namespace Psychology.Harmony;
[StaticConstructorOnStartup]
public class Pawnmorpher_Patches{
    public static MethodInfo originalInfo;
    public static HarmonyMethod harmonyMethod;

    static Pawnmorpher_Patches()
    {
        HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate.Pawnmorph");

        harmonyInstance.PatchAll();

        foreach (ThingDef pawnDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (pawnDef.category != ThingCategory.Pawn)
            {
                continue;
            }
            SpeciesHelper.AddEverythingExceptCompPsychology(pawnDef);
        }

        //originalInfo = AccessTools.Method(typeof(PsycheHelper), nameof(PsycheHelper.IsHumanlike));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(PsycheHelper_IsHumanlike_Prefix));
        //harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        //originalInfo = AccessTools.Method(typeof(PsycheHelper), nameof(PsycheHelper.Comp));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(PsycheHelper_Comp_Prefix));
        //harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        //originalInfo = AccessTools.PropertyGetter(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.IsVisible));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_IsVisible_Prefix));
        //harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        //originalInfo = AccessTools.PropertyGetter(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.PawnToShowInfoAbout));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_PawnToShowInfoAbout_Postfix));
        //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        //originalInfo = AccessTools.Method(typeof(ITab_Pawn_Psyche), nameof(ITab_Pawn_Psyche.FillTabPawnHook));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(ITabPawnPsyche_FillTabPawnHook_Postfix));
        //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        //originalInfo = AccessTools.Method(typeof(SpeciesHelper), nameof(SpeciesHelper.Initialize));
        //harmonyMethod = new HarmonyMethod(typeof(Pawnmorpher_Patches), nameof(SpeciesHelper_Initialize_Postfix));
        //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        Log.Message("Psychology: completed patches for compatibility with Pawnmorpher.");

    }



    //public static bool PsycheHelper_IsHumanlike_Prefix(ref bool __result, Pawn pawn)
    //{
    //    Log.Message("IsHumanlike_PawnmorpherPrefix, FormerHumanUtilities.IsHumanlike");
    //    __result = Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn);
    //    return false;
    //}

    //public static bool PsycheHelper_Comp_Prefix(ref CompPsychology __result, Pawn pawn)
    //{
    //    //Log.Message("PsycheHelper_Comp_Prefix, step 0");
    //    if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) != true)
    //    {
    //        //Log.Message("PsycheHelper_Comp_Prefix, IsFormerHuman(pawn) != true, pawn = " + pawn.Label);
    //        return true;
    //    }
    //    //Log.Message("PsycheHelper_Comp_Prefix, IsFormerHuman(pawn) == true, pawn = " + pawn.Label);
    //    Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
    //    if (originalPawn == null)
    //    {
    //        //Log.Message("PsycheHelper_Comp_Prefix, originalPawn == null, pawn = " + pawn.Label);
    //        return true;
    //    }
    //    //Log.Message("PsycheHelper_Comp_Prefix, originalPawn = " + originalPawn.Label + ", pawn = " + pawn.Label);
    //    CompPsychology compPsychology = originalPawn.GetComp<CompPsychology>();
    //    if (compPsychology == null)
    //    {
    //        //Log.Message("PsycheHelper_Comp_Prefix, originalPawnCompPsychology == null, pawn = " + pawn.Label);
    //        return true;
    //    }
    //    __result = compPsychology;
    //    return false;
    //}

    // This is different from PsychologyEnabled because some pawns can lose sentient while still having former personalities
    //public static bool ITabPawnPsyche_IsVisible_Prefix(ref bool __result, ITab_Pawn_Psyche __instance)
    //{
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, start");
    //    Pawn pawn = __instance.PawnToShowInfoAbout;
    //    if (pawn == null)
    //    {
    //        Log.Message("ITabPawnPsyche_IsVisible_Prefix, pawn == null");
    //        __result = false;
    //        return false;
    //    }
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, pawn != null");

    //    if (Pawnmorph.FormerHumanUtilities.IsHumanlike(pawn) != true)
    //    {
    //        Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsHumanlike(pawn) != true, pawn = " + pawn.Label + ", def = " + pawn.def.defName);
    //        __result = false;
    //        return false;
    //    }
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsHumanlike(pawn) == true, pawn = " + pawn.Label + ", def = " + pawn.def.defName);

    //    if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) != true)
    //    {
    //        Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsFormerHuman(pawn) != true");
    //        return true;
    //    }
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, IsFormerHuman(pawn) == true");

    //    Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
    //    if (originalPawn == null)
    //    {
    //        Log.Message("ITabPawnPsyche_IsVisible_Prefix, originalPawn == null");
    //        return true;
    //    }
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, originalPawn != null");

    //    SpeciesSettings settings = SpeciesHelper.GetOrMakeSettingsFromHumanlikeDef(originalPawn.def, true);
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, get/set SpeciesSettings for originalPawn.def = " + originalPawn.def.defName);

    //    if (settings.enablePsyche != true)
    //    {
    //        Log.Message("ITabPawnPsyche_IsVisible_Prefix, settings.enablePsyche != true");
    //        __result = false;
    //        return false;
    //    }
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, settings.enablePsyche == true");

    //    __result = PsycheHelper.DoesCompExist(originalPawn);
    //    Log.Message("ITabPawnPsyche_IsVisible_Prefix, DoesCompExist = " + __result);
    //    return false;
    //}

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

    //public static void ITabPawnPsyche_FillTabPawnHook_Postfix(ref Pawn __result)
    //{
    //    if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(__result) == false)
    //    {
    //        return;
    //    }
    //    Pawn originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(__result);
    //    if (originalPawn == null)
    //    {
    //        return;
    //    }
    //    __result = originalPawn;
    //}

    //public static bool PsycheHelper_TryGetPawnSeed_Prefix(ref Pawn pawn)
    //{
    //    if (Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) != true)
    //    {
    //        return true;
    //    }
    //    pawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
    //    return true;
    //}

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
}

[HarmonyPatch(typeof(Psychology.PsycheHelper), nameof(Psychology.PsycheHelper.HasLatentPsyche))]
public class Pawnmorph_Patches_PsycheHelper_HasLatentPsyche
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return CodeInstruction.Call(typeof(Pawnmorph_Patches_PsycheHelper_HasLatentPsyche), nameof(HasLatentPsyche), new Type[] { typeof(Pawn) });
    }

    // Two approaches: 1) add comps per individual pawn 2) add comps to ThingDef and add checks on whether to enable them
    // Pawnmorpher seems to do the first approach, so for now let's try to follow that mindset
    // Add comp check, if comp is null, add check to see if comp should be add
    public static bool HasLatentPsyche(Pawn pawn)
    {
        if (pawn == null)
        {
            Log.Message("PsychologyEnabled_Patch, pawn == null");
            return false;
        }
        CompPsychology comp = pawn.GetComp<CompPsychology>();
        if (comp == null)
        {
            Pawn originalPawn = Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) ? Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn) : null;
            if (originalPawn == null)
            {
                return false;
            }
            CompPsychology originalComp = pawn.GetComp<CompPsychology>();
            if (originalComp == null)
            {
                return false;
            }
            if (originalComp.IsPsychologyPawn != true)
            {
                return false;
            }
            SpeciesHelper.AddEverythingExceptCompPsychology(pawn.def);
            comp = new CompPsychology { parent = pawn };
            pawn.AllComps.Add(comp);

            comp.LDRTick = originalComp.LDRTick;
            comp.AlreadyBuried = originalComp.AlreadyBuried;
            comp.Psyche = new Pawn_PsycheTracker(pawn);
            comp.Psyche.DeepCopyFromOtherTracker(originalComp.Psyche);
            comp.Sexuality = new Pawn_SexualityTracker(pawn);
            comp.Sexuality.DeepCopyFromOtherTracker(originalComp.Sexuality);
        }
        SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def, noDating: true);
        if (settings?.enablePsyche != true)
        {
            return false;
        }
        if (comp.IsPsychologyPawn != true)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Psychology.PsycheHelper), nameof(Psychology.PsycheHelper.IsSapient))]
public class Pawnmorph_Patches_PsycheHelper_IsSapient
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return CodeInstruction.Call(typeof(Pawnmorph.FormerHumanUtilities), nameof(Pawnmorph.FormerHumanUtilities.IsHumanlike), new Type[] { typeof(Pawn) });
    }
}