using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;

namespace Psychology
{
    public class Dialog_ViewPsyche : Window
    {

        private Pawn pawn;

        public Dialog_ViewPsyche(Pawn editFor)
        {
            pawn = editFor;
        }

        [LogPerformance]
        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                flag = true;
                Event.current.Use();
            }
            GUI.EndGroup();

            //Vector2 center = Find.WindowStack.currentlyDrawnWindow.windowRect.center;
            Rect mainRect = PsycheCardUtility.CalculateTotalRect(pawn);
            inRect = new Rect(mainRect.x, mainRect.y, mainRect.width, mainRect.height + 30f + 2f * PsycheCardUtility.BoundaryPadding);
            Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
            //Find.WindowStack.currentlyDrawnWindow.windowRect.center = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);
            //Find.WindowStack.currentlyDrawnWindow.windowRect.center = new Vector2(0.2f * Screen.width, 0.2f * Screen.height);
            Find.WindowStack.currentlyDrawnWindow.windowRect.position = new Vector2(200f, 200f);

            GUI.BeginGroup(inRect);
            PsycheCardUtility.DrawPsycheCard(mainRect, pawn, false);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(inRect.x, mainRect.yMax, inRect.width);
            GUI.color = Color.white;
            Rect okRect = new Rect(inRect.width / 3f, inRect.yMax - 30f - PsycheCardUtility.BoundaryPadding, inRect.width / 3f, 30f);
            if (Widgets.ButtonText(okRect, "CloseButton".Translate(), true, false, true) || flag)
            {
                this.Close(true);
            }
            GUI.EndGroup();
        }
    }
}

//Find.WindowStack.currentlyDrawnWindow.windowRect.x += 0.5f * (Find.WindowStack.currentlyDrawnWindow.windowRect.width - inRect.width);
//Find.WindowStack.currentlyDrawnWindow.windowRect.y += 0.5f * (Find.WindowStack.currentlyDrawnWindow.windowRect.height - inRect.height);
//Find.WindowStack.currentlyDrawnWindow.windowRect.width = inRect.width;
//Find.WindowStack.currentlyDrawnWindow.windowRect.height = inRect.height;
//Find.WindowStack.currentlyDrawnWindow.windowRect.center = center;