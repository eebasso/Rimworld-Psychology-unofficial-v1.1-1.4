using UnityEngine;
//using RimWorld;
using Verse;
//using Verse.Sound;
//using HarmonyLib;
//using CharacterEditor;

namespace Psychology.Harm
{
    //[HarmonyPatch(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents))]
    public class CharacterEditor_DialogPsychology_Patch
    {
        //[HarmonyPrefix]
        public static bool DoWindowContents(Rect inRect)
        {
            GUI.EndGroup();
            Pawn pawn = CharacterEditor.CEditor.API.Pawn;

            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                return false;
            }

            //Rect psycheRect = PsycheCardUtility.CalculatePsycheRect(pawn);
            Rect psycheRect = PsycheCardUtility.PsycheRect;
            Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);

            inRect = new Rect(psycheRect.x, psycheRect.y, psycheRect.width + editRect.width, psycheRect.height);
            Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
            Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);

            GUI.BeginGroup(inRect);
            PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true, true);
            EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
            GUI.color = Color.white;
            GUI.EndGroup();

            return false;
        }
    }
}