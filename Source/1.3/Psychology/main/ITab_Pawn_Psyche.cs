using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
//using Verse.Sound;

namespace Psychology
{
    public class ITab_Pawn_Psyche : ITab
    {
        public ITab_Pawn_Psyche()
        {
            size = new Vector2(200f, 200f);
            //size = new Vector2(630f, 510f);
            labelKey = "TabPsyche";
            this.tutorTag = "Psyche";

        }

        public override bool IsVisible
        {
            get
            {
                //Log.Message("Inside IsVisible");
                return PsycheHelper.PsychologyEnabled(PawnToShowInfoAbout);
            }
        }

        private Pawn PawnToShowInfoAbout
        {
            get
            {
                //Log.Message("Inside PawnToShowInfoAbout");
                if (base.SelPawn != null)
                {
                    //Log.Message("Selected pawn birthLastName = " + base.SelPawn?.story?.birthLastName);
                    return base.SelPawn;
                }
                if (base.SelThing is Corpse corpse)
                {
                    //Log.Message("Selected corpse birthLastName = " + corpse?.InnerPawn?.story?.birthLastName);
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Psyche tab found no selected pawn to display.");
            }
        }

        protected override void FillTab()
        {
            // Initialize pawn
            Pawn pawn = PawnToShowInfoAbout;
            // Get total rectangle
            Rect psycheRect = PsycheCardUtility.CalculatePsycheRect(pawn);
            Rect totalRect = psycheRect;
            Rect editRect = new Rect(0f, 0f, 1f, 1f);
            if (Prefs.DevMode)
            {
                editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);
                totalRect.width += editRect.width;
            }
            size = totalRect.size;
            GUI.BeginGroup(totalRect);
            PsycheCardUtility.DrawPsycheCard(psycheRect, pawn);
            if (Prefs.DevMode)
            {
                EditPsycheUtility.DrawEditPsyche(editRect, pawn);
                GUI.color = PsychColor.LineColor;
                Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
                GUI.color = Color.white;
            }
            GUI.EndGroup();
        }
    }
}
