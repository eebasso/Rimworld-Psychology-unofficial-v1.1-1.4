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
        //Log.Message("Initializing Psychology Patches");
        harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate");
        harmonyInstance.PatchAll();
        if (!PsychologySettings.taraiSiblingsGenerated)
        {
            ManualPatches.TaraiSiblingsPatch(harmonyInstance);
        }
        if (PsychologySettings.enableKinsey)
        {
            ManualPatches.KinseyEnabledPatches(harmonyInstance);
        }
        if (ModsConfig.IdeologyActive)
        {
            ManualPatches.IdeoPatches(harmonyInstance);
        }
        Log.Message("Psychology: implemented all vanilla Harmony patches");

        if (ModsConfig.IsActive("void.charactereditor"))
        {
            ManualPatches.CharacterEditorPatch(harmonyInstance);
            Log.Message("Psychology: patched CharacterEditor for compatibility");
        }
        if (ModsConfig.IsActive("EdB.PrepareCarefully"))
        {
            ManualPatches.PrepareCarefullyPatch(harmonyInstance);
            Log.Message("Psychology: patched PrepareCarefully for compatibility");
        }

    }
}
public class ManualPatches
{
    public static void CharacterEditorPatch(HarmonyLib.Harmony harmonyInstance)
    {
        Log.Message("DoCharacterEditorPatch: Step 0");
        //MethodInfo methodInfo = AccessTools.Method(AccessTools.TypeByName("CharacterEditor.DialogPsychology"), "DoWindowContents");
        //MethodInfo methodInfo = AccessTools.Method(Type.GetType("CharacterEditor.DialogPsychology"), "DoWindowContents");
        //MethodInfo methodInfo = AccessTools.Method("CharacterEditor.DialogPsychology:DoWindowContents");
        MethodInfo methodInfo = AccessTools.Method(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents));
        //Log.Message("DoCharacterEditorPatch: Step 1");
        //HarmonyMethod transpilerMethod = new HarmonyMethod(typeof(CharacterEditor_DialogPsychology_Patch), nameof(CharacterEditor_DialogPsychology_Patch.DoWindowContentsTranspiler));
        //Log.Message("DoCharacterEditorPatch: Step 2");
        //harmonyInstance.Patch(methodInfo, transpiler: transpilerMethod);
        //Log.Message("DoCharacterEditorPatch: Step 3");
        Log.Message("DoCharacterEditorPatch: Step 1");
        HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(CharacterEditor_DialogPsychology_Patch), nameof(CharacterEditor_DialogPsychology_Patch.DoWindowContentsPrefix));
        Log.Message("DoCharacterEditorPatch: Step 2");
        harmonyInstance.Patch(methodInfo, prefix: harmonyMethod);
        Log.Message("DoCharacterEditorPatch: Step 3");
    }

    public static void PrepareCarefullyPatch(HarmonyLib.Harmony harmonyInstance)
    {
        MethodInfo originalInfo = AccessTools.Method(AccessTools.TypeByName("EdB.PrepareCarefully.PanelBackstory"), "Draw");
        MethodInfo patchInfo = typeof(EdBPrepareCarefully_PanelBackstory_Patch).GetMethod(nameof(EdBPrepareCarefully_PanelBackstory_Patch.Transpiler));
        harmonyInstance.Patch(originalInfo, transpiler: new HarmonyMethod(patchInfo));

        //harmonyInstance.Patch(
        //        AccessTools.Method(Type.GetType("EdB.PrepareCarefully.PanelBackstory"), "Draw"),
        //        transpiler: new HarmonyMethod(typeof(EdBPrepareCarefully_PanelBackstory_Patch), nameof(EdBPrepareCarefully_PanelBackstory_Patch.Transpiler))
        //);
        //MethodInfo originalInfo = AccessTools.Method(Type.GetType("EdB.PrepareCarefully.PanelBackstory"), "Draw");
        //MethodInfo originalInfo = Type.GetType("EdB.PrepareCarefully.PanelBackstory").GetMethod("Draw", BindingFlags.vir );
    }

    public static void TaraiSiblingsPatch(HarmonyLib.Harmony harmonyInstance)
    {
        MethodInfo methodInfo = AccessTools.Method(typeof(PawnGenerator), "GenerateTraits");
        HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GenerateTraits_TaraiSiblings_Postfix));
        harmonyInstance.Patch(methodInfo, postfix: harmonyMethod);
    }

    public static void KinseyEnabledPatches(HarmonyLib.Harmony harmonyInstance)
    {
        MethodInfo methodInfo = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait));
        HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(TraitSet_ManualPatches), nameof(TraitSet_ManualPatches.GainTrait_KinseyEnabledPrefix));
        harmonyInstance.Patch(methodInfo, prefix: harmonyMethod);

        //MethodInfo methodInfo = AccessTools.Method(typeof(PawnGenerator), "GenerateTraits");
        //HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GenerateTraits_KinseyEnabled));
        //harmonyInstance.Patch(methodInfo, postfix: harmonyMethod);
    }

    public static void IdeoPatches(HarmonyLib.Harmony harmonyInstance)
    {
        MethodInfo methodInfo = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) });
        HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(PawnGenerator_ManualPatches), nameof(PawnGenerator_ManualPatches.GeneratePawn_IdeoCache_Postfix));
        harmonyInstance.Patch(methodInfo, postfix: harmonyMethod);
    }
}


