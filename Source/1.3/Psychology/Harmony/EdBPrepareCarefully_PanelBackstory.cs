using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;
using EdB.PrepareCarefully;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PanelBackstory), nameof(PanelBackstory.Draw))]
    public static class EdBPrepareCarefully_PanelTraits_PsychologyPatch
    {
        [HarmonyPrefix]
        public static void Draw(State state, float y)
        {
            Pawn pawn = state.CurrentPawn.Pawn;
            float y2 = y + PanelModule.Margin.y;
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                Rect psycheRect = new Rect(230f, y2 + 5f, 28f, 28f);
                Color oldColor = GUI.color;
                GUI.color = psycheRect.Contains(Event.current.mousePosition) ? new Color(0.97647f, 0.97647f, 0.97647f) : new Color(0.623529f, 0.623529f, 0.623529f);
                GUI.DrawTexture(psycheRect, PsychologyTexCommand.PsycheButton);
                GUI.color = oldColor;
                if (Widgets.ButtonInvisible(psycheRect, false))
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    Find.WindowStack.Add(new Dialog_ViewPsyche(pawn, true));
                }
            }
        }
    }
}