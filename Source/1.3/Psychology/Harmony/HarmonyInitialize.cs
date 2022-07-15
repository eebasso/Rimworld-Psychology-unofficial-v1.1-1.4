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
            harmonyInstance = new Harmony("Psychology");
            harmonyInstance.PatchAll();

            if (ModsConfig.IsActive("CharacterEditor"))
            {
                harmonyInstance.Patch(
                    AccessTools.Method(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents)),
                    postfix: new HarmonyMethod(typeof(), nameof())
                );

            }

            if (!ModsConfig.IsActive("EdB.PrepareCarefully"))
            {
                harmonyInstance.Patch(
                    AccessTools.Method(typeof(VerbTracker), "GetVerbsCommands"),
                    postfix: new HarmonyMethod(patchType, nameof(VerbTrackerGetVerbsCommands_Postfix))
                );
                harmonyInstance.Patch(
                    AccessTools.Method(typeof(PawnGenerator), "PostProcessGeneratedGear"),
                    postfix: new HarmonyMethod(patchType, nameof(PawnGeneratorPostProcessGeneratedGear_Postfix))
                );
            }
        }
    }
}

