using UnityEngine;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(EdB.PrepareCarefully.PanelBackstory), nameof(EdB.PrepareCarefully.PanelBackstory.DrawRandomizeButton))]
    public static class EdBPrepareCarefully_PanelTraits_PsychologyPatch
    {
        [HarmonyPrefix]
        public static void DrawRandomizeButton(float y, float width)
        {
            Log.Error("DID THIS WORK?");
            Rect psycheRect = new Rect(width - 102f, y + 9f, 30f, 30f);
            GUI.DrawTexture(psycheRect, ContentFinder<Texture2D>.Get("Buttons/ButtonPsyche", true));
            
        }
    }
}