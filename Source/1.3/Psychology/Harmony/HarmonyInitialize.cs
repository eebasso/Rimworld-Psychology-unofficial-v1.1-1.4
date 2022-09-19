using HarmonyLib;
using Verse;
using RimWorld;
using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine.UIElements.Experimental;
using System.Reflection;

namespace Psychology.Harmony;

[StaticConstructorOnStartup]
public static class HarmonyInitialize
{
    public static HarmonyLib.Harmony harmonyInstance;

    static HarmonyInitialize()
    {
        harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate");
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
    public static MethodInfo originalInfo;
    public static HarmonyMethod harmonyMethod;

    public static void KinseyEnabledPatches(HarmonyLib.Harmony harmonyInstance)
    {
        originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait));
        harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.GainTrait_KinseyEnabledPrefix));
        harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);

        originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef) });
        harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.HasTrait_KinseyEnabledPostfix));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);

        originalInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef), typeof(int) });
        harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.HasTrait_KinseyEnabledPostfix2));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
    }

    //public static void IdeoPatches(HarmonyLib.Harmony harmonyInstance)
    //{
    //    //originalInfo = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) });
    //    //harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GeneratePawn_IdeoCache_Postfix));
    //    //harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
    //}
}


