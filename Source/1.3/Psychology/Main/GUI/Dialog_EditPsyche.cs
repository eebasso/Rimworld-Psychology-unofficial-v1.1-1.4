//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;

namespace Psychology;

public class Dialog_EditPsyche : Window
{
    public Pawn pawn;
    public override float Margin => 0f;
    public override Vector2 InitialSize
    {
        get
        {
            float editHeight = PsycheCardUtility.PsycheRect.height;
            float editWidth = EditPsycheUtility.CalculateEditWidth(this.pawn);
            return new Vector2(editWidth, editHeight);
        }
    }

    public Dialog_EditPsyche(Pawn editFor)
    {
        pawn = editFor;
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

        //GUI.EndGroup();
        //float editHeight = PsycheCardUtility.PsycheRect.height;
        //float editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
        //inRect = new Rect(0f, 0f, editWidth, editHeight);
        //Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
        //Find.WindowStack.currentlyDrawnWindow.windowRect.x = 0.5f * UI.screenWidth;
        //Find.WindowStack.currentlyDrawnWindow.windowRect.y = 0.5f * (UI.screenHeight - inRect.height);

        EditPsycheUtility.DrawEditPsyche(inRect, pawn);

        if (flag)
        {
            this.Close(true);
        }
        
    }
}
