//using System;
//using System.Collections.Generic;
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
        private bool EditAllowedBool;

        public Dialog_ViewPsyche(Pawn editFor, bool editBool = false)
        {
            pawn = editFor;
            EditAllowedBool = editBool;
        }

        //[LogPerformance]
        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                flag = true;
                Event.current.Use();
            }
            GUI.EndGroup();

            //Rect psycheRect = PsycheCardUtility.CalculatePsycheRect(pawn);
            Rect psycheRect = PsycheCardUtility.PsycheRect;
            inRect = psycheRect;
            Rect editRect = new Rect(0f, 0f, 1f, 1f);
            if (EditAllowedBool)
            {
                float editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
                editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
                inRect.width += editWidth;
            }

            Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
            Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);

            GUI.BeginGroup(inRect);
            PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true);
            if (EditAllowedBool)
            {
                EditPsycheUtility.DrawEditPsyche(editRect, pawn);
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
                GUI.color = Color.white;
            }
            GUI.EndGroup();

            if (flag)
            {
                this.Close(true);
            }
        }
    }
}
