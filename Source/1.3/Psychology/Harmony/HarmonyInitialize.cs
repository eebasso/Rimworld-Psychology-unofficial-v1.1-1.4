using HarmonyLib;
using Verse;
using RimWorld;
using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine.UIElements.Experimental;
using System.Reflection;
using Psychology.Harmony;

namespace Psychology.Harmony;

[StaticConstructorOnStartup]
public static class HarmonyInitialize
{
    static HarmonyInitialize()
    {
        HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate");

        // It might be a bit extreme to undo all other patches...
        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("InteractionWorker_RomanceAttempt.RandomSelectionWeight unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("InteractionWorker_RomanceAttempt.SuccessChance unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), "BreakLoverAndFianceRelations"), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("InteractionWorker_RomanceAttempt.BreakLoverAndFianceRelations unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.LovePartnerRelationGenerationChance)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("LovePartnerRelationUtility.LovePartnerRelationGenerationChance unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("LovePartnerRelationUtility.GetLovinMtbHours unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("Pawn_IdeoTracker.IdeoTrackerTick unpatch, " + ex);
        }

        try
        {
            harmonyInstance.Unpatch(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor)), HarmonyPatchType.All);
        }
        catch (Exception ex)
        {
            Log.Warning("Pawn_RelationsTracker.SecondaryLovinChanceFactor unpatch, exception = " + ex);
        }

        harmonyInstance.PatchAll();

        if (PsychologySettings.enableKinsey)
        {
            ManualPatches.KinseyEnabledPatches(harmonyInstance);
        }
        //if (ModsConfig.IdeologyActive)
        //{
        //    ManualPatches.IdeoPatches(harmonyInstance);
        //    Log.Message("Psychology: implemented all ideology patches");
        //}
        Log.Message("Psychology: implemented all vanilla patches");
    }
}

public class ManualPatches
{
    public static void KinseyEnabledPatches(HarmonyLib.Harmony harmonyInstance)
    {
        MethodInfo originalInfo;
        HarmonyMethod harmonyMethod;
        HarmonyMethod harmonyMethod2;

        try
        {
            originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait));
            harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.GainTrait_KinseyEnabledPrefix));
            harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch TraitSet.GainTrait, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef) });
            harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.HasTrait_KinseyEnabledPostfix));
            harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch TraitSet.HasTrait1, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef), typeof(int) });
            harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.HasTrait_KinseyEnabledPostfix2));
            harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch TraitSet.HasTrait2, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(ChildRelationUtility), nameof(ChildRelationUtility.ChanceOfBecomingChildOf), new Type[] { typeof(Pawn), typeof(Pawn), typeof(Pawn), typeof(PawnGenerationRequest?), typeof(PawnGenerationRequest?), typeof(PawnGenerationRequest?) });
            harmonyMethod = new HarmonyMethod(typeof(ChildRelationUtility_ManualPatches), nameof(ChildRelationUtility_ManualPatches.ChanceOfBecomingChildOf_Transpiler));
            harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch ChildRelationUtility.ChanceOfBecomingChildOf, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(PawnRelationWorker_Sibling), nameof(PawnRelationWorker_Sibling.CreateRelation));
            harmonyMethod = new HarmonyMethod(typeof(PawnRelationWorker_Sibling_ManualPatches), nameof(PawnRelationWorker_Sibling_ManualPatches.CreateRelation_Transpiler));
            harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch PawnRelationWorker_Sibling.CreateRelation, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.ValidateTarget));
            harmonyMethod = new HarmonyMethod(typeof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches), nameof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches.ValidateTarget_Transpiler));
            harmonyMethod2 = new HarmonyMethod(typeof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches), nameof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches.ValidateTarget_Postfix));
            harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod, postfix: harmonyMethod2);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch CompAbilityEffect_WordOfLove.ValidateTarget, " + ex);
        }

        try
        {
            originalInfo = AccessTools.Method(typeof(CompAbilityEffect_WordOfLove), nameof(CompAbilityEffect_WordOfLove.Valid), new Type[] { typeof(LocalTargetInfo), typeof(bool) });
            harmonyMethod = new HarmonyMethod(typeof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches), nameof(CompAbilityEffect_WordOfLove_KinseyEnabledPatches.Valid_Transpiler));
            harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);
        }
        catch (Exception ex)
        {
            Log.Error("Psychology: failed to patch CompAbilityEffect_WordOfLove.Valid, " + ex);
        }

    }

    //public static void IdeoPatches(HarmonyLib.Harmony harmonyInstance)
    //{
    //    //originalInfo = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) });
    //    //harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GeneratePawn_IdeoCache_Postfix));
    //    //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
    //}
}


