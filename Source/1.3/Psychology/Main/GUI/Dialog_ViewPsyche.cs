//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;

namespace Psychology;

public class Dialog_ViewPsyche : Window
{
    public Pawn pawn;
    public bool EditAllowedBool;
    public float editWidth;
    //public Rect psycheRect;
    public override float Margin => 0f;

    public Dialog_ViewPsyche(Pawn editFor, bool editBool = false)
    {
        pawn = editFor;
        EditAllowedBool = editBool;
    }

    public override Vector2 InitialSize
    {
        get
        {
            Rect totalRect = PsycheCardUtility.PsycheRect;
            if (EditAllowedBool)
            {
                editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
                totalRect.width += editWidth;
            }
            return totalRect.size;
        }
    }

    //[LogPerformance]
    public override void DoWindowContents(Rect inRect)
    {
        //bool flag = false;
        //if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
        //{
        //    flag = true;
        //    Event.current.Use();
        //}
        //GUI.EndGroup();

        //Rect psycheRect = PsycheCardUtility.PsycheRect;
        //inRect = psycheRect;
        //if (EditAllowedBool)
        //{
        //    inRect.width += editWidth;
        //}
        //Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
        //Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);

        //GUI.BeginGroup(inRect);
        //PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true);
        //if (EditAllowedBool)
        //{
        //    Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
        //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
        //    PsychColor.DrawLineVertical(editRect.x, editRect.y, editRect.height, PsychColor.LineColor);
        //    GUI.color = Color.white;
        //}
        //GUI.EndGroup();

        //if (flag)
        //{
        //    this.Close(true);
        //}

        doCloseX = false;
        doCloseButton = false;
        Rect psycheRect = PsycheCardUtility.PsycheRect;
        psycheRect.position = inRect.position;
        PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true);
        if (EditAllowedBool)
        {
            Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
            EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            GUI.color = PsychColor.LineColor;
            Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
            GUI.color = Color.white;
        }
    }
}
