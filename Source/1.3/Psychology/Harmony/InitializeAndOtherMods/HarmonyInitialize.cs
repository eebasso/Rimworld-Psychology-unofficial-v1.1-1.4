using HarmonyLib;
using Verse;

namespace Psychology.Harmony
{
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
                    prefix: new HarmonyMethod(typeof(EdBPrepareCarefully_PanelBackstory_Patch), nameof(EdBPrepareCarefully_PanelBackstory_Patch.Draw))
            );
        }
    }
}

