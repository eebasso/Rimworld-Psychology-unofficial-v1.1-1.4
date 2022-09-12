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
        if (ModsConfig.IdeologyActive)
        {
            ManualPatches.IdeoPatches(harmonyInstance);
        }
        Log.Message("Psychology: implemented all Harmony patches of Vanilla");

        if (ModsConfig.IsActive("void.charactereditor"))
        {
            CharacterEditor_Patches.ManualPatches(harmonyInstance);
            Log.Message("Psychology: patched CharacterEditor for compatibility");
        }
        if (ModsConfig.IsActive("EdB.PrepareCarefully"))
        {
            EdBPrepareCarefully_Patches.ManualPatches(harmonyInstance);
            Log.Message("Psychology: patched PrepareCarefully for compatibility");
        }
        if (ModsConfig.IsActive("tachyonite.pawnmorpherpublic"))
        {
            Pawnmorpher_Patches.ManualPatches(harmonyInstance);
            Log.Message("Psychology: completed compability patches for Pawnmorpher");
        }
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
    }

    public static void IdeoPatches(HarmonyLib.Harmony harmonyInstance)
    {
        originalInfo = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) });
        harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GeneratePawn_IdeoCache_Postfix));
        harmonyInstance.Patch(originalInfo, postfix: harmonyMethod);
    }
}


