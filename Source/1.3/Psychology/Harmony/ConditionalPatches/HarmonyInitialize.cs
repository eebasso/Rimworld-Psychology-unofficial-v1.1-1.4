using HarmonyLib;
using Verse;
using RimWorld;
using System;

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
        Log.Message("Psychology: implemented all vanilla Harmony patches");
        if (ModsConfig.IsActive("void.charactereditor"))
        {
            SpecialPatches.DoCharacterEditorPatch(harmonyInstance);
            Log.Message("Psychology: patched CharacterEditor for compatibility");
        }
        if (ModsConfig.IsActive("EdB.PrepareCarefully"))
        {
            SpecialPatches.DoPrepareCarefullyPatch(harmonyInstance);
            Log.Message("Psychology: patched PrepareCarefully for compatibility");
        }
        if (!PsychologySettings.taraiSiblingsGenerated)
        {
            SpecialPatches.TaraiSiblingsPatch(harmonyInstance);
        }
        if (PsychologySettings.enableKinsey)
        {
            SpecialPatches.KinseyEnabledPatches(harmonyInstance);
        }
    }
}
public class SpecialPatches
{
    public static void DoCharacterEditorPatch(HarmonyLib.Harmony harmonyInstance)
    {
        harmonyInstance.Patch(
                AccessTools.Method(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents)),
                prefix: new HarmonyMethod(typeof(CharacterEditor_DialogPsychology_Patch), nameof(CharacterEditor_DialogPsychology_Patch.DoWindowContents))
        );
    }

    public static void DoPrepareCarefullyPatch(HarmonyLib.Harmony harmonyInstance)
    {
        harmonyInstance.Patch(
                AccessTools.Method(typeof(EdB.PrepareCarefully.PanelBackstory), nameof(EdB.PrepareCarefully.PanelBackstory.Draw)),
                postfix: new HarmonyMethod(typeof(EdBPrepareCarefully_PanelBackstory_Patch), nameof(EdBPrepareCarefully_PanelBackstory_Patch.Draw))
        );
    }

    public static void TaraiSiblingsPatch(HarmonyLib.Harmony harmonyInstance)
    {
        harmonyInstance.Patch(
                AccessTools.Method(typeof(PawnGenerator), "GenerateTraits"),
                postfix: new HarmonyMethod(typeof(PawnGenerator_ConditionalPatches), nameof(PawnGenerator_ConditionalPatches.GenerateTraitsTaraiSiblings))
        );
    }

    public static void KinseyEnabledPatches(HarmonyLib.Harmony harmonyInstance)
    {
        harmonyInstance.Patch(
            AccessTools.Method(typeof(PawnGenerator), "GenerateTraits"),
            postfix: new HarmonyMethod(typeof(PawnGenerator_ConditionalPatches), nameof(PawnGenerator_ConditionalPatches.GenerateTraitsKinseyEnabled))
            );

        //harmonyInstance.Patch(
        //    AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)),
        //    postfix: new HarmonyMethod(typeof(PawnGenerator_ConditionalPatches), nameof(TraitSet_ConditionalPatches.GainTrait))
        //    );
    }

}

