using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using System;
using System.Runtime.Remoting.Contexts;
using HarmonyLib;
using UnityEngine.UIElements.Experimental;
using System.Reflection;
using System.Collections.Generic;using System.Reflection.Emit;using System.Linq;

namespace Psychology.Harmony;

//[HarmonyPatch(typeof(EdB.PrepareCarefully.PanelBackstory), nameof(EdB.PrepareCarefully.PanelBackstory.Draw))]
public static class EdBPrepareCarefully_PanelBackstory_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        //Log.Message("Transpiler: Step 0");
        Type CustomPawnType = AccessTools.TypeByName("EdB.PrepareCarefully.CustomPawn");
        //Log.Message("Transpiler: Step 1");
        List<CodeInstruction> codeList = codes.ToList();
        //Log.Message("Transpiler: Step 2");
        bool bool0 = false;
        bool bool1 = false;
        //Log.Message("Transpiler: Step 3");
        for (int i = 0; i < codeList.Count(); i++)
        {
            //Log.Message("Transpiler: Step 4");
            //Log.Message("Transpiler: Step 3a");
            yield return codeList[i];
            //Log.Message("Transpiler: Step 3b");
            if (i < 1)
            {
                continue;
            }
            Type codeType = codeList[i - 1]?.operand?.GetType();
            bool0 = codeList[i - 1].opcode == OpCodes.Callvirt;
            bool1 = codeList[i].opcode == OpCodes.Stloc_1;
            if (bool0 && bool1)
            {
                //Log.Message("Transpiler: Step 5");
                FieldInfo fieldInfo = AccessTools.Field(CustomPawnType, "pawn");
                //Log.Message("Transpiler: Step 6");
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                //Log.Message("Transpiler: Step 7");
                yield return new CodeInstruction(OpCodes.Ldfld, fieldInfo);
                //Log.Message("Transpiler: Step 8");
                yield return new CodeInstruction(OpCodes.Ldarg_2);
                //Log.Message("Transpiler: Step 9");
                yield return CodeInstruction.Call(typeof(EdBPrepareCarefully_PanelBackstory_Patch), nameof(EdBPsycheButton), new Type[] { typeof(Pawn), typeof(float) });
                //Log.Message("Transpiler: Step 10");
            }
        }
        //Log.Message("Transpiler: Step 11");
    }

    public static void EdBPsycheButton(Pawn pawn, float y)
    {
        string warningText = "WARNING";
        string warningTooltip = "Psychology: "
            + "<b>Prepare Carefully leads to countless glitches,</b> ".Colorize(Color.red)
            + "corrupts the default data structures, and interacts poorly with Psychology. The player is strongly encouraged to "
            + "<b>use Character Editor instead,</b> ".Colorize(new Color(0.1f, 0.6f, 1f))
            + "as it achieves the same capabilities, including modifying initial item loadouts, while avoiding game-breaking problems. "
            + "<b>You've been warned.</b>".Colorize(Color.red);
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Medium;
        Vector2 warningSize = Text.CalcSize(warningText);
        Rect warningRect = new Rect(240f - warningSize.x - 10f, y, warningSize.x, warningSize.y);
        Rect obnoxiouslyBigTipRect = new Rect(0f, 0f, 325f, 800f);

        GUI.color = Color.red;
        Widgets.Label(warningRect, warningText);
        Text.Font = oldFont;
        Widgets.DrawHighlightIfMouseover(obnoxiouslyBigTipRect);
        GUI.color = Color.white;

        TooltipHandler.TipRegion(obnoxiouslyBigTipRect, delegate
        {
            return warningTooltip;
        }, warningTooltip.GetHashCode());
        if (!PsycheHelper.PsychologyEnabled(pawn))
        {
            return;
        }
        //Log.Message("PsycheButton: Step 1");
        Rect buttonRect = new Rect(240f, y + 5f, 28f, 28f);
        //Log.Message("PsycheButton: Step 2");
        Color oldColor = GUI.color;
        //Log.Message("PsycheButton: Step 3");
        GUI.color = buttonRect.Contains(Event.current.mousePosition) ? UIAssets.ButtonLightColor : UIAssets.ButtonDarkColor;
        //Log.Message("PsycheButton: Step 4");
        GUI.DrawTexture(buttonRect, UIAssets.PsycheButton);
        //Log.Message("PsycheButton: Step 5");
        GUI.color = oldColor;
        //Log.Message("PsycheButton: Step 6a");
        if (Widgets.ButtonInvisible(buttonRect, false))
        {
            //Log.Message("PsycheButton: Step 6b");
            SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
            //Log.Message("PsycheButton: Step 6c");
            Find.WindowStack.Add(new Dialog_ViewPsyche(pawn, true));
        }
        //Log.Message("PsycheButton: Step 7");
    }
}
