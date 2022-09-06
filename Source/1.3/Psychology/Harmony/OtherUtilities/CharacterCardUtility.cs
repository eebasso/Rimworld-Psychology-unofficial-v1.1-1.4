using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
public static class CharacterCardUtility_ButtonPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddPsycheDisplay(IEnumerable<CodeInstruction> instr, ILGenerator gen)
    {
        int doNames = 0;
        foreach (CodeInstruction itr in instr)
        {
            yield return itr;
            if (itr.opcode == OpCodes.Call && itr.operand as MethodInfo == typeof(CharacterCardUtility).GetMethod(nameof(CharacterCardUtility.DoNameInputRect)))
            {
                doNames++;
                if (doNames == 3)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterCardUtility_ButtonPatch), "PsycheCardButton", new Type[] { typeof(Rect), typeof(Pawn) }));
                }
            }
        }
    }

    public static void PsycheCardButton(Rect panelRect, Pawn pawn)
    {
        if (PsycheHelper.PsychologyEnabled(pawn))
        {
            Rect rect = new Rect(panelRect.xMax + 300f, 0f, 30f, 30f);
            Color oldColor = GUI.color;
            GUI.color = rect.Contains(Event.current.mousePosition) ? UIAssets.ButtonLightColor : UIAssets.ButtonDarkColor;
            GUI.DrawTexture(rect, UIAssets.PsycheButton);
            if (Widgets.ButtonInvisible(rect, false))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                Find.WindowStack.Add(new Dialog_ViewPsyche(pawn, Prefs.DevMode));
            }
            GUI.color = oldColor;
        }
    }
}
