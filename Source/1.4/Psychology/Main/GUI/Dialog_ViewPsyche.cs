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
  public bool showEditPanel;
  //public float editWidth;
  public Rect psycheRect;
  public Rect editRect;
  public Rect totalRect;

  //public Rect psycheRect;
  //public override float Margin => 0f;

  public override Vector2 InitialSize => totalRect.size;
  //{
  //  get
  //  {
  //    Rect totalRect = PsycheCardUtility.PsycheRect;
  //    if (showEditPanel)
  //    {
  //      editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
  //      totalRect.width += editWidth;
  //    }
  //    return totalRect.size;
  //  }
  //}

  public Dialog_ViewPsyche(Pawn editFor, bool editBool = false)
  {
    pawn = editFor;
    showEditPanel = editBool;
    psycheRect = PsycheCardUtility.PsycheRect;
    totalRect = PsycheCardUtility.PsycheRect;
    if (showEditPanel)
    {
      float editWidth = EditPsycheUtility.CalculateEditWidth(pawn);
      totalRect.width += editWidth;
      editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
    }
    doCloseButton = false;
    doCloseX = true;
  }

  



  public override void DoWindowContents(Rect inRect)
  {
    GUI.EndGroup();

    bool flag = false;
    if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
    {
      flag = true;
      Event.current.Use();
    }

    //Rect psycheRect = PsycheCardUtility.PsycheRect;
    //Rect drawRect = new Rect(0f, 0f, InitialSize.x, InitialSize.y);
    //if (showEditPanel)
    //{
    //  drawRect.width += editWidth;
    //}
    //Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
    //Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);

    GUI.BeginGroup(totalRect);
    GUI.color = Color.white;
    PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true, showEditPanel);
    if (showEditPanel)
    {
      //Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
      EditPsycheUtility.DrawEditPsyche(editRect, pawn);
      UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
    }
    GUI.EndGroup();

    if (flag)
    {
      this.Close(true);
    }

    GUI.BeginGroup(inRect);

    //doCloseX = false;
    //doCloseButton = false;
    //Rect psycheRect = PsycheCardUtility.PsycheRect;
    //psycheRect.position = inRect.position;
    //PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, true);
    //if (EditAllowedBool)
    //{
    //    Rect editRect = new Rect(psycheRect.xMax, psycheRect.y, editWidth, psycheRect.height);
    //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
    //    GUI.color = UIAssets.LineColor;
    //    Widgets.DrawLineVertical(editRect.x, editRect.y, editRect.height);
    //    GUI.color = Color.white;
    //}
  }
}
