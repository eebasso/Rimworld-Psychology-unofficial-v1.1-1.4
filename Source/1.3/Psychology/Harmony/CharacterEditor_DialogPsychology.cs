using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;
using CharacterEditor;

namespace Psychology.Harm
{
    [HarmonyPatch(typeof(DialogPsychology), nameof(DialogPsychology.DoWindowContents))]
    public class CharacterEditor_DialogPsychology_Patch
    {

        [HarmonyPrefix]
        public static bool DoWindowContents(Rect inRect)
        {
            GUI.EndGroup();

            Pawn pawn = CEditor.API.Pawn;

            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                return false;
            }

            Rect psycheRect = PsycheCardUtility.CalculatePsycheRect(pawn);
            Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);

            inRect = new Rect(psycheRect.x, psycheRect.y, psycheRect.width + editRect.width, psycheRect.height);
            Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
            Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);
            //Find.WindowStack.currentlyDrawnWindow.windowRect.x = 0.5f * UI.screenWidth - inRect.width;
            //Find.WindowStack.currentlyDrawnWindow.windowRect.y = 0.5f * (UI.screenHeight - inRect.height);

            GUI.BeginGroup(inRect);
            PsycheCardUtility.DrawPsycheCard(psycheRect, pawn);
            EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
            GUI.color = Color.white;
            GUI.EndGroup();

            return false;
        }
    }
}