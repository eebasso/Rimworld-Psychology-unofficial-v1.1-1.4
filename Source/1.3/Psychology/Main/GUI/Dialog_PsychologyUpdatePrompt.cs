//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;namespace Psychology;
public class Dialog_PsychologyUpdatePrompt : Window{
    //static float ButtonWidth;
    //static float ButtonHeight;    //static string SaveButtonText = "SaveButton".Translate();
    //static string ResetButtonText = "ResetButton".Translate();
    //static string RandomButtonText = "Random".Translate();
    //static Vector2 ScalingVector = new Vector2(1.035f, 1.025f);    public static string promptDescription = "Psychology has recieved an update to how personality is generated. The player is encouraged to select the option in the mod settings to reset the personalities of each pawn based on the updated formulas.";
    
    public static float buttonHeight = 30f;
    //public static float boundaryPadding = 10f;
    public static float promptWidth = 500f;
    public static float inRectWidth = promptWidth + 2f * StandardMargin;

    public static float promptHeight;
    public static float inRectHeight;

    //public static float buttonWidth = inRectWidth / 3f;

    public static Rect promptRect;


    public override Vector2 InitialSize
    {
        get
        {
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            promptHeight = Text.CalcHeight(promptDescription, promptWidth) + 10f;
            Text.Font = oldFont;
            promptRect = new Rect(0f, 0f, promptWidth, promptHeight);
            inRectHeight = StandardMargin + promptHeight + FooterRowHeight + StandardMargin;
            return new Vector2(inRectWidth, inRectHeight);
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Small;
        Widgets.Label(promptRect, promptDescription);
        Text.Font = oldFont;
        doCloseButton = true;

        //Rect okButtonRect = new Rect(buttonWidth, promptRect.yMax + boundaryPadding, buttonWidth, buttonHeight);
        //if (Widgets.ButtonText(okButtonRect, "OK".Translate()))
        //{
        //    this.Close();
        //}
    }}