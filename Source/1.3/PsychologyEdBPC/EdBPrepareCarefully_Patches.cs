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

[StaticConstructorOnStartup]
public static class EdBPrepareCarefully_Patches
{
    public static MethodInfo originalInfo;
    public static HarmonyMethod harmonyMethod;

    static EdBPrepareCarefully_Patches()
    {
        HarmonyLib.Harmony harmonyInstance = new HarmonyLib.Harmony("Community.Psychology.UnofficialUpdate.EdBPC");

        originalInfo = AccessTools.Method(AccessTools.TypeByName("EdB.PrepareCarefully.PanelBackstory"), "Draw");
        harmonyMethod = new HarmonyMethod(typeof(EdBPrepareCarefully_Patches).GetMethod(nameof(EdBPrepareCarefully_Patches.PanelBackstory_DrawTranspiler)));
        harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);

        Log.Message("Psychology: completed patches for compatibility with Prepare Carefully.");
    }

    public static IEnumerable<CodeInstruction> PanelBackstory_DrawTranspiler(IEnumerable<CodeInstruction> codes)
    {
        Type CustomPawnType = AccessTools.TypeByName("EdB.PrepareCarefully.CustomPawn");
        List<CodeInstruction> codeList = codes.ToList();
        bool bool0 = false;
        bool bool1 = false;
        for (int i = 0; i < codeList.Count(); i++)
        {
            yield return codeList[i];
            if (i < 1)
            {
                continue;
            }
            Type codeType = codeList[i - 1]?.operand?.GetType();
            bool0 = codeList[i - 1].opcode == OpCodes.Callvirt;
            bool1 = codeList[i].opcode == OpCodes.Stloc_1;
            if (bool0 && bool1)
            {
                FieldInfo fieldInfo = AccessTools.Field(CustomPawnType, "pawn");
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                yield return new CodeInstruction(OpCodes.Ldfld, fieldInfo);
                yield return new CodeInstruction(OpCodes.Ldarg_2);
                yield return CodeInstruction.Call(typeof(EdBPrepareCarefully_Patches), nameof(EdBPsycheButton), new Type[] { typeof(Pawn), typeof(float) });
            }
        }
    }

    public static void EdBPsycheButton(Pawn pawn, float y)
    {

        string warningText = "EdBPrepareCarefullyWarning".Translate();
        string warningTooltip = (string)"EdBPrepareCarefullyWarningTooltip".Translate();
        warningTooltip = warningTooltip.Replace("{0}", "<b>" + "EdBPrepareCarefullyWarningTooltip0".Translate().Colorize(Color.red) + "</b>");
        warningTooltip = warningTooltip.Replace("{1}", "<b>" + "EdBPrepareCarefullyWarningTooltip1".Translate().Colorize(new Color(0.1f, 0.6f, 1f)) + "</b>");
        warningTooltip = warningTooltip.Replace("{2}", "<b>" + "EdBPrepareCarefullyWarningTooltip2".Translate().Colorize(Color.red) + "</b>");

        //string warningTooltip = "Psychology: "
        //    + "<b>Prepare Carefully leads to countless glitches,</b> ".Colorize(Color.red)
        //    + "corrupts the default data structures, and interacts poorly with Psychology. The player is strongly encouraged to "
        //    + "<b>use Character Editor instead,</b> ".Colorize(new Color(0.1f, 0.6f, 1f))
        //    + "as it achieves the same capabilities, including modifying initial item loadouts, while avoiding game-breaking problems. "
        //    + "<b>You've been warned.</b>".Colorize(Color.red);

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
