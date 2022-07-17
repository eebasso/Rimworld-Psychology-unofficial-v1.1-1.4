using HarmonyLib;
using Verse;

namespace Psychology.Harm
{
    [StaticConstructorOnStartup]
    public static class HarmonyInitialize
    {
        public static Harmony harmonyInstance;

        static HarmonyInitialize()
        {
            Log.Message("Initializing Psychology Patches");
            harmonyInstance = new Harmony("Community.Psychology.UnofficialUpdate");
            harmonyInstance.PatchAll();
            Log.Message("Done with PatchAll");
            if (ModsConfig.IsActive("void.charactereditor"))
            {
                Log.Message("Running CEditor patch");
                HarmonyPatches.DoCharacterEditorPatch(harmonyInstance);
            }
            if (ModsConfig.IsActive("EdB.PrepareCarefully"))
            {
                Log.Message("Running PrepareCarefully patch");
                HarmonyPatches.DoPrepareCarefullyPatch(harmonyInstance);
            }
        }
    }
    public class HarmonyPatches
    {
        public static void DoCharacterEditorPatch(Harmony harmonyInstance)
        {
            harmonyInstance.Patch(
                    AccessTools.Method(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents)),
                    prefix: new HarmonyMethod(typeof(CharacterEditor_DialogPsychology_Patch), nameof(CharacterEditor_DialogPsychology_Patch.DoWindowContents))
            );
        }
        public static void DoPrepareCarefullyPatch(Harmony harmonyInstance)
        {
            harmonyInstance.Patch(
                    AccessTools.Method(typeof(EdB.PrepareCarefully.PanelBackstory), nameof(EdB.PrepareCarefully.PanelBackstory.Draw)),
                    prefix: new HarmonyMethod(typeof(EdBPrepareCarefully_PanelBackstory_Patch), nameof(EdBPrepareCarefully_PanelBackstory_Patch.Draw))
            );
        }
    }
}

