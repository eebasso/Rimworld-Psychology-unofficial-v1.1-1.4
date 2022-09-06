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

public class CharacterEditor_DialogPsychology_Patch
{
    //// Transpiler method
    //public static IEnumerable<CodeInstruction> DoWindowContentsTranspiler(IEnumerable<CodeInstruction> codes)
    //{
    //    Log.Message("DoWindowContentsTranspiler: Step 0");
    //    Type CEditorType = AccessTools.TypeByName("CharacterEditor.CEditor");
    //    Log.Message("DoWindowContentsTranspiler: Step 1");
    //    MethodInfo methodInfo0 = AccessTools.Method(CEditorType, "get_API");
    //    Log.Message("DoWindowContentsTranspiler: Step 2");
    //    MethodInfo methodInfo1 = AccessTools.Method(CEditorType, "get_Pawn");
    //    Log.Message("DoWindowContentsTranspiler: Step 3");
    //    // Load inRect on the stack
    //    Log.Message("DoWindowContentsTranspiler: Step 4");
    //    yield return new CodeInstruction(OpCodes.Ldarg_1);
    //    // Load Character.CEditor.API
    //    Log.Message("DoWindowContentsTranspiler: Step 5");
    //    yield return new CodeInstruction(OpCodes.Call, methodInfo0);
    //    // Load Character.CEditor.API.Pawn
    //    Log.Message("DoWindowContentsTranspiler: Step 6");
    //    yield return new CodeInstruction(OpCodes.Callvirt, methodInfo1);
    //    // Call DrawPsycheWindow
    //    Log.Message("DoWindowContentsTranspiler: Step 7");
    //    yield return CodeInstruction.Call(typeof(CharacterEditor_DialogPsychology_Patch), nameof(DrawPsycheWindow), new Type[] { typeof(Rect), typeof(Pawn) });
    //    Log.Message("DoWindowContentsTranspiler: Step 8");
    //}

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

    // Prefix method
    //static Func<object> Get_API = AccessTools.MethodDelegate<Func<object>>(AccessTools.Method("CharacterEditor.CEditor:get_API"));
    //static Func<object, Pawn> Get_Pawn = AccessTools.MethodDelegate<Func<object, Pawn>>(AccessTools.Method("CharacterEditor.CEditor:get_Pawn"));

    //public static bool DoWindowContentsPrefix(Rect inRect)
    //{
    //    Log.Message("DoWindowContentsPrefix: Step 0");
    //    GUI.EndGroup();
    //    Log.Message("DoWindowContentsPrefix: Step 1");
    //    Pawn pawn = Get_Pawn(Get_API());
    //    Log.Message("DoWindowContentsPrefix: Step 2");
    //    if (!PsycheHelper.PsychologyEnabled(pawn))
    //    {
    //        return false;
    //    }
    //    Log.Message("DoWindowContentsPrefix: Step 3");
    //    Rect psycheRect = PsycheCardUtility.PsycheRect;
    //    Log.Message("DoWindowContentsPrefix: Step 4");
    //    Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);
    //    Log.Message("DoWindowContentsPrefix: Step 5");
    //    inRect = new Rect(psycheRect.x, psycheRect.y, psycheRect.width + editRect.width, psycheRect.height);
    //    Log.Message("DoWindowContentsPrefix: Step 6");
    //    Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
    //    Log.Message("DoWindowContentsPrefix: Step 7");
    //    Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);
    //    Log.Message("DoWindowContentsPrefix: Step 8");

    //    GUI.BeginGroup(inRect);
    //    Log.Message("DoWindowContentsPrefix: Step 9");
    //    PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true, true);
    //    Log.Message("DoWindowContentsPrefix: Step 10");
    //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
    //    Log.Message("DoWindowContentsPrefix: Step 11");
    //    UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
    //    Log.Message("DoWindowContentsPrefix: Step 12");
    //    GUI.EndGroup();
    //    Log.Message("DoWindowContentsPrefix: Step 13");
    //    return false;
    //}

    public static bool DoWindowContentsPrefix(Rect inRect)
    {
        Rect oldRect = inRect;
        GUI.EndGroup();
        Pawn pawn = CharacterEditor.CEditor.API.Pawn;

        if (!PsycheHelper.PsychologyEnabled(pawn))
        {
            return false;
        }

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

        // Added this, might cause errors?
        GUI.BeginGroup(oldRect);
        return false;
    }


}

