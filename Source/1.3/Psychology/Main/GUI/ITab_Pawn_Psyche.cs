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
        size = new Vector2(200f, 200f);
        labelKey = "TabPsyche";
        this.tutorTag = "Psyche";
    }

    public override bool IsVisible
    {
        get
        {
            Log.Message("IsVisible");
            return PsycheHelper.PsychologyEnabled(PawnToShowInfoAbout);
        }
    }

    public Pawn PawnToShowInfoAbout
    {
        get
        {
            //Log.Message("Inside PawnToShowInfoAbout");
            if (base.SelPawn != null)
            {
                Log.Message("Selected pawn label = " + base.SelPawn?.Label);
                return base.SelPawn;
            }
            if (base.SelThing is Corpse corpse)
            {
                Log.Message("Selected corpse label = " + corpse?.InnerPawn?.Label);
                return corpse.InnerPawn;
            }
            throw new InvalidOperationException("Psyche tab found no selected pawn to display.");
        }
    }

    public override void FillTab()
    {
        // Get pawn
        Pawn pawn = FillTabPawnHook(PawnToShowInfoAbout);
        // Modify pawn hook
        
        // Get total rectangle
        Rect psycheRect = PsycheCardUtility.PsycheRect;
        Rect totalRect = psycheRect;
        Rect editRect = new Rect(0f, 0f, 1f, 1f);
        if (Prefs.DevMode)
        {
            editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);
            totalRect.width += editRect.width;
        }
        size = totalRect.size;
        GUI.BeginGroup(totalRect);
        PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, false);
        if (Prefs.DevMode)
        {
            EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
        }
        GUI.EndGroup();
    }

    public Pawn FillTabPawnHook(Pawn pawn)
    {
        return pawn;
    }
}
