//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;namespace Psychology;public class Dialog_UpdateIntro : Window{    public override Vector2 InitialSize    {        get        {
            return new Vector2(Dialog_UpdateIntro_SCOS.windowRectWidth, Dialog_UpdateIntro_SCOS.windowRectHeight);
        }    }    public override void DoWindowContents(Rect inRect)    {
        doCloseButton = false;

        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Medium;
        Widgets.Label(Dialog_UpdateIntro_SCOS.IntroTitleRect, Dialog_UpdateIntro_SCOS.IntroTitleText);

        Text.Font = GameFont.Small;        Widgets.Label(Dialog_UpdateIntro_SCOS.IntroDescRect, Dialog_UpdateIntro_SCOS.IntroDescText);
        if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.UpdateButtonRect, Dialog_UpdateIntro_SCOS.UpdateButtonText))
        {
            this.Close();
            Find.WindowStack.Add(new Dialog_UpdateYesNo());
        }        if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.CloseButtonRect, Dialog_UpdateIntro_SCOS.CloseButtonText))
        {
            this.Close();
        }        Text.Font = oldFont;
    }}[StaticConstructorOnStartup]public static class Dialog_UpdateIntro_SCOS
{
    public static float windowRectWidth = 500f;
    public static float windowRectHeight;    public static float inRectWidth = 500f - 2f * Window.StandardMargin;    public static float inRectHeight;

    public static string IntroTitleText = "UpdateIntroMessageTitle".Translate();
    public static Rect IntroTitleRect;

    public static string IntroDescText = "UpdateIntroMessage".Translate();
    public static Rect IntroDescRect;

    public static string UpdateButtonText = "ApplyUpdateButton".Translate();
    public static Rect UpdateButtonRect;

    public static string CloseButtonText = "CloseButton".Translate();
    public static Rect CloseButtonRect;

    static Dialog_UpdateIntro_SCOS()
    {
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Medium;

        float introHeight = Text.CalcSize(IntroTitleText).y;
        IntroTitleRect = new Rect(0f, 0f, inRectWidth, 40f);
        Text.Font = GameFont.Small;

        float introDescHeight = Text.CalcHeight(IntroDescText, inRectWidth) + 10f;
        //float introDescHeight = 140f;
        IntroDescRect = new Rect(0f, IntroTitleRect.yMax, inRectWidth, introDescHeight);

        float buttonWidth = Mathf.Max(Text.CalcSize(UpdateButtonText).x + 20f, Text.CalcSize(CloseButtonText).x + 20f, Window.CloseButSize.x);
        float buttonY = IntroDescRect.yMax + Window.StandardMargin;
        UpdateButtonRect = new Rect(-Window.StandardMargin + windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);
        CloseButtonRect = new Rect(-Window.StandardMargin + 2f * windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);

        inRectHeight = UpdateButtonRect.yMax - IntroTitleRect.y;
        windowRectHeight = inRectHeight + 2f * Window.StandardMargin;

        Text.Font = oldFont;
    }
}