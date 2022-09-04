using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
//using Verse.Sound;

namespace Psychology;

public class ITab_Pawn_Psyche : ITab
{
    public ITab_Pawn_Psyche()
    {
        //size = new Vector2(200f, 200f);
        //size = new Vector2(630f, 510f);
        labelKey = "TabPsyche";
        this.tutorTag = "Psyche";
    }

    public override bool IsVisible
    {
        get
        {
            return PsycheHelper.PsychologyEnabledFast(PawnToShowInfoAbout);
        }
    }

    public Pawn PawnToShowInfoAbout
    {
        get
        {
            if (base.SelPawn != null)
            {
                return base.SelPawn;
            }
            if (base.SelThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }
            throw new InvalidOperationException("Psyche tab found no selected pawn to display.");
        }
    }

    public override void FillTab()
    {
        // Initialize pawn
        Pawn pawn = PawnToShowInfoAbout;
        // Get total rectangle
        Rect psycheRect = PsycheCardUtility.PsycheRect;
        Rect totalRect = psycheRect;
        float width = 0f;
        //Rect editRect = new Rect(0f, 0f, 1f, 1f);
        if (Prefs.DevMode)
        {
            width = EditPsycheUtility.CalculateEditWidth(pawn);
            totalRect.width += width;
        }
        size = totalRect.size;
        GUI.BeginGroup(totalRect);
        PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, false);
        if (Prefs.DevMode)
        {
            Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, width, psycheRect.height);
            EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
        }
        GUI.EndGroup();
    }
}
