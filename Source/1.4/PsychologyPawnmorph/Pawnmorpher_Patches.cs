using UnityEngine;using Verse;using System;using HarmonyLib;using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;
using RimWorld;
using System.Runtime;

namespace Psychology.Harmony;
[StaticConstructorOnStartup]
public class PawnmorpherPatches{
    static PawnmorpherPatches()
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
        Log.Message("Psychology: completed patches for compatibility with Pawnmorpher.");
    }
}

[HarmonyPatch(typeof(PsycheHelper), nameof(PsycheHelper.HasLatentPsyche))]
public class PawnmorphPatches_PsycheHelper_HasLatentPsyche
{
    public static SpeciesSettings settings;
    public static CompPsychology comp;
    public static Pawn originalPawn;
    public static CompPsychology originalComp;

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return CodeInstruction.Call(typeof(PawnmorphPatches_PsycheHelper_HasLatentPsyche), nameof(HasLatentPsyche), new Type[] { typeof(Pawn) });
        yield return new CodeInstruction(OpCodes.Ret);
    }

    // Two approaches: 1) add comps per individual pawn 2) add comps to ThingDef and add checks on whether to enable them
    // Pawnmorpher seems to do the first approach, so for now let's try to follow that mindset
    // Add comp check, if comp is null, add check to see if comp should be add
    public static bool HasLatentPsyche(Pawn pawn)
    {
        if (pawn == null)
        {
            Log.Warning("PsychologyEnabled_Patch, pawn == null");
            return false;
        }
        comp = pawn.GetComp<CompPsychology>();
        if (comp == null)
        {
            if (SpeciesHelper.CheckIntelligenceAndAddEverythingToSpeciesDef(pawn.def) != true)
            {
                originalPawn = Pawnmorph.FormerHumanUtilities.IsFormerHuman(pawn) ? Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn) : null;
                if (originalPawn == null)
                {
                    //Log.Message("PsychologyEnabled_Patch, comp == null and originalPawn == null");
                    return false;
                }
                originalComp = originalPawn.GetComp<CompPsychology>();
                if (originalComp == null)
                {
                    //Log.Message("PsychologyEnabled_Patch, originalComp == null");
                    return false;
                }
                if (originalComp.IsPsychologyPawn != true)
                {
                    //Log.Message("PsychologyEnabled_Patch, originalComp.IsPsychologyPawn != true");
                    return false;
                }
                comp = new CompPsychology { parent = pawn };
                comp.DeepCopyFromOtherComp(originalComp);
                pawn.AllComps.Add(comp);
                SpeciesHelper.AddEverythingExceptCompPsychology(pawn.def);
            }
            else
            {
                comp = pawn.GetComp<CompPsychology>();
                if (comp == null)
                {
                    Log.Error("PsychologyEnabled_Patch, comp == null even after CheckIntelligenceAndAddEverythingToHumanlikeDef == true");
                    return false;
                }
            }
        }
        if (comp.IsPsychologyPawn != true)
        {
            //Log.Message("PsychologyEnabled_Patch, comp.IsPsychologyPawn != true");
            return false;
        }
        settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
        if (settings?.enablePsyche != true)
        {
            //Log.Message("PsychologyEnabled_Patch, settings?.enablePsyche != true");
            return false;
        }
        //Log.Message("PsychologyEnabled_Patch, return true for pawn = " + pawn.Label);
        return true;
    }
}

[HarmonyPatch(typeof(Psychology.PsychologyGameComponent), nameof(Psychology.PsychologyGameComponent.LoadedGame))]
public class PawnmorphPatches_PsychologyGameComponent_LoadedGame
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        //CompPsychology comp;
        //Pawn originalPawn;
        //CompPsychology originalComp;

        foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
        {
            PsycheHelper.HasLatentPsyche(pawn);
            //if (pawn == null)
            //{
            //    Log.Warning("LoadedGame_PawnmorphPostfix, pawn == null");
            //    continue;
            //}
            //comp = pawn.GetComp<CompPsychology>();
            //if (comp == null)
            //{
            //    originalPawn = Pawnmorph.FormerHumanUtilities.GetOriginalPawnOfFormerHuman(pawn);
            //    if (originalPawn == null)
            //    {
            //        //Log.Message("LoadedGame_PawnmorphPostfix, comp == null and originalPawn == null");
            //        continue;
            //    }
            //    originalComp = originalPawn.GetComp<CompPsychology>();
            //    if (originalComp == null)
            //    {
            //        //Log.Message("LoadedGame_PawnmorphPostfix, originalComp == null");
            //        continue;
            //    }
            //    if (originalComp.IsPsychologyPawn != true)
            //    {
            //        //Log.Message("LoadedGame_PawnmorphPostfix, originalComp.IsPsychologyPawn != true");
            //        continue;
            //    }
            //    comp = new CompPsychology { parent = pawn };
            //    comp.DeepCopyFromOtherComp(originalComp);
            //    pawn.AllComps.Add(comp);
            //    SpeciesHelper.AddEverythingExceptCompPsychology(pawn.def);
            //}
            //if (comp.IsPsychologyPawn != true)
            //{
            //    //Log.Message("LoadedGame_PawnmorphPostfix, comp.IsPsychologyPawn != true");
            //}
        }
    }
}

[HarmonyPatch(typeof(Psychology.PsycheHelper), nameof(Psychology.PsycheHelper.IsSapient))]
public class Pawnmorph_Patches_PsycheHelper_IsSapient
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return CodeInstruction.Call(typeof(Pawnmorph.FormerHumanUtilities), nameof(Pawnmorph.FormerHumanUtilities.IsHumanlike), new Type[] { typeof(Pawn) });
        yield return new CodeInstruction(OpCodes.Ret);
    }
}

[HarmonyPatch(typeof(Pawnmorph.FormerHumanUtilities), nameof(Pawnmorph.FormerHumanUtilities.TransferEverything))]
public class Pawnmorph_Patches_FormerHumanUtilities_TransferEverything
{
    [HarmonyPostfix]
    public static void Postfix(Pawn original, Pawn transformedPawn)
    {
        Log.Warning("Pawnmorph_Patches_FormerHumanUtilities_TransferEverything.Postfix fired!");
        if (PsycheHelper.TryGetPawnSeed(original) != true)
        {
            Log.Error("Pawnmorph_Patches_FormerHumanUtilities_TransferEverything.Postfix, TryGetPawnSeed failed on original");
            return;
        }
        if (original == null || transformedPawn == null)
        {
            Log.Error("Pawnmorph_Patches_FormerHumanUtilities_TransferEverything.Postfix, at least one pawn was null");
            return;
        }
        CompPsychology originalComp = original.GetComp<CompPsychology>();
        if (originalComp == null)
        {
            return;
        }
        if (originalComp.IsPsychologyPawn != true)
        {
            return;
        }
        CompPsychology transformedComp = transformedPawn.GetComp<CompPsychology>();
        if (transformedComp == null)
        {
            transformedComp = new CompPsychology { parent = transformedPawn };
            transformedPawn.AllComps.Add(transformedComp);
            transformedComp = transformedPawn.GetComp<CompPsychology>();
        }
        transformedComp.DeepCopyFromOtherComp(originalComp);
        SpeciesHelper.AddEverythingExceptCompPsychology(transformedPawn.def);
    }
}

[HarmonyPatch(typeof(Pawnmorph.MergedPawnUtilities), nameof(Pawnmorph.MergedPawnUtilities.TransferToMergedPawn))]
public class Pawnmorph_Patches_MergedPawnUtilities_TransferToMergedPawn
{
    [HarmonyPostfix]
    public static void TransferToMergedPawn(IReadOnlyList<Pawn> originals, Pawn mergedPawn)
    {
        List<CompPsychology> originalCompList = new List<CompPsychology>();
        foreach (Pawn p in originals)
        {
            if (p == null)
            {
                Log.Error("Pawnmorph_Patches_MergedPawnUtilities_TransferToMergedPawn.Postfix, at least one pawn was null");
                continue;
            }
            if (PsycheHelper.TryGetPawnSeed(p) != true)
            {
                Log.Error("Pawnmorph_Patches_MergedPawnUtilities_TransferToMergedPawn.Postfix, TryGetPawnSeed failed on original");
                continue;
            }
            CompPsychology originalComp = p.GetComp<CompPsychology>();
            if (originalComp == null)
            {
                continue;
            }
            if (originalComp.IsPsychologyPawn != true)
            {
                Log.Error("Pawnmorph_Patches_MergedPawnUtilities_TransferToMergedPawn.Postfix, IsPsychologyPawn != true");
                continue;
            }
            originalCompList.Add(originalComp);
        }
        if (originalCompList.NullOrEmpty())
        {
            return;
        }
        int num = originalCompList.Count();

        // Get and initialize comp
        CompPsychology mergedComp = mergedPawn.GetComp<CompPsychology>();
        if (mergedComp == null)
        {
            mergedComp = new CompPsychology { parent = mergedPawn };
            mergedPawn.AllComps.Add(mergedComp);
            mergedComp = mergedPawn.GetComp<CompPsychology>();
            if (mergedComp == null)
            {
                Log.Error("Pawnmorph_Patches_MergedPawnUtilities_TransferToMergedPawn, mergedComp was still null after initializing");
            }
        }
        mergedComp.Sexuality = new Pawn_SexualityTracker(mergedPawn);
        mergedComp.Psyche = new Pawn_PsycheTracker(mergedPawn);
        mergedComp.Sexuality.GenerateSexuality();
        mergedComp.Psyche.Initialize();

        // Sexuality
        float kinseyAverage = 0f;
        float mergedSexDrive = 0f;
        float mergedRomDrive = 0f;
        foreach (CompPsychology originalComp in originalCompList)
        {
            kinseyAverage += originalComp.Sexuality.kinseyRating;
            mergedSexDrive += originalComp.Sexuality.sexDrive;
            mergedRomDrive += originalComp.Sexuality.romanticDrive;
        }
        kinseyAverage /= num;
        mergedSexDrive /= num;
        mergedRomDrive /= num;

        int mergedSeed = PsycheHelper.PawnSeed(mergedPawn);
        int kinseyFloor = Mathf.FloorToInt(kinseyAverage);
        int kinseyCeil = Mathf.CeilToInt(kinseyAverage);
        mergedComp.Sexuality.kinseyRating = Rand.ValueSeeded(5 * mergedSeed + 11) < kinseyAverage - kinseyFloor ? kinseyFloor : kinseyCeil;
        mergedComp.Sexuality.sexDrive = mergedSexDrive;
        mergedComp.Sexuality.romanticDrive = mergedRomDrive;

        // Psyche
        float mergedRating;
        foreach (PersonalityNodeDef nodeDef in PersonalityNodeMatrix.defList)
        {
            mergedRating = 0f;
            foreach (CompPsychology originalComp in originalCompList)
            {
                mergedRating += PsycheHelper.NormalCDFInv(originalComp.Psyche.GetPersonalityRating(nodeDef));
            }
            mergedRating /= Mathf.Sqrt(num);
            mergedRating = PsycheHelper.NormalCDF(mergedRating);
            mergedComp.Psyche.GetPersonalityNodeOfDef(nodeDef).rawRating = mergedRating;
        }

    }
}