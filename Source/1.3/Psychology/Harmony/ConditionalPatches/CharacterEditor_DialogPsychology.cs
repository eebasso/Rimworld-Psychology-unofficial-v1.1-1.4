using UnityEngine;
using RimWorld;
using Verse;
//using Verse.Sound;
//using HarmonyLib;
//using CharacterEditor;
using System;
using Verse.Sound;
using System.Runtime.Remoting.Contexts;
using HarmonyLib;
using UnityEngine.UIElements.Experimental;
using System.Reflection;
using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(CharacterEditor.DialogPsychology), nameof(CharacterEditor.DialogPsychology.DoWindowContents))]
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
        UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
        GUI.EndGroup();

        return false;
    }

    //public static void DrawPsycheWindow(Rect inRect, Pawn pawn)
    //{
    //    GUI.EndGroup();
    //    if (!PsycheHelper.PsychologyEnabled(pawn))
    //    {
    //        return;
    //    }
    //    Rect psycheRect = PsycheCardUtility.PsycheRect;
    //    float editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
    //    Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
    //    inRect = new Rect(psycheRect.x, psycheRect.y, psycheRect.width + editWidth, psycheRect.height);
    //    Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
    //    Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);
    //    GUI.BeginGroup(inRect);
    //    PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true, true);
    //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
    //    UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
    //    GUI.EndGroup();
    //}

    ////[HarmonyPrefix]
    //public static bool DoWindowContents(Rect inRect)
    //{
    //    GUI.EndGroup();
    //    Pawn pawn = CharacterEditor.CEditor.API.Pawn;

    //    if (!PsycheHelper.PsychologyEnabled(pawn))
    //    {
    //        return false;
    //    }

    //    //Rect psycheRect = PsycheCardUtility.CalculatePsycheRect(pawn);
    //    Rect psycheRect = PsycheCardUtility.PsycheRect;
    //    Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);

    //    inRect = new Rect(psycheRect.x, psycheRect.y, psycheRect.width + editRect.width, psycheRect.height);
    //    Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
    //    Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);

    //    GUI.BeginGroup(inRect);
    //    PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true, true);
    //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
    //    UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
    //    GUI.EndGroup();

    //    return false;
    //}
}