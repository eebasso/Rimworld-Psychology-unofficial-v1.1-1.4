using HugsLib.Settings;
using UnityEngine;
using Verse;using System;namespace Psychology;public class Dialog_UpdateIntro : Window{    public override Vector2 InitialSize    {        get        {
            return new Vector2(Dialog_UpdateIntro_SCOS.windowRectWidth, Dialog_UpdateIntro_SCOS.windowRectHeight);
        }    }    public override void DoWindowContents(Rect inRect)    {
        doCloseButton = false;

        GUI.color = Color.white;
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Small;
        GUIStyle style = Text.fontStyles[1];
        TextAnchor OldAnchor = Text.Anchor;

        style.fontSize = Dialog_UpdateIntro_SCOS.TitleFontSize;
        Text.Anchor = Dialog_UpdateIntro_SCOS.TitleAnchor;
        Widgets.Label(Dialog_UpdateIntro_SCOS.IntroTitleRect, Dialog_UpdateIntro_SCOS.IntroTitleText);
        style.fontSize = Dialog_UpdateIntro_SCOS.DescFontSize;
        Text.Anchor = Dialog_UpdateIntro_SCOS.DescAnchor;        Widgets.Label(Dialog_UpdateIntro_SCOS.IntroDescRect, Dialog_UpdateIntro_SCOS.IntroDescText);

        style.fontSize = Dialog_UpdateIntro_SCOS.OldSmallFontSize;
        Text.Anchor = OldAnchor;
        Text.Font = oldFont;

        //if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.UpdateButtonRect, Dialog_UpdateIntro_SCOS.UpdateButtonText))
        //{
        //    this.Close();
        //    Find.WindowStack.Add(new Dialog_UpdateYesNo());
        //}
        if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.CloseButtonRect, Dialog_UpdateIntro_SCOS.CloseButtonText))
        {
            this.Close();
        }

        if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.SettingsButtonRect, Dialog_UpdateIntro_SCOS.SettingsButtonText))
        {
            try
            {
                Mod psychologyMod = LoadedModManager.ModHandles.FirstOrFallback(x => x.SettingsCategory().Contains("Psychology"));
                try
                {
                    Find.WindowStack.Add(new Dialog_VanillaModSettings(psychologyMod));
                }
                catch
                {
                    Log.Error("Psychology: failed to open mod settings winodw");
                }
            }
            catch
            {
                Log.Error("Psychology: failed to find mod from ModHandles");
            }
        }
    }}[StaticConstructorOnStartup]public static class Dialog_UpdateIntro_SCOS
{
    public static float windowRectWidth = 500f;
    public static float windowRectHeight;    public static float inRectWidth = 500f - 2f * Window.StandardMargin;    public static float inRectHeight;

    public static string IntroTitleText = "UpdateIntroMessageTitle".Translate();
    public static Rect IntroTitleRect;

    public static string IntroDescText = "UpdateIntroMessage".Translate();
    public static Rect IntroDescRect;

    //public static string UpdateButtonText = "ApplyUpdateButton".Translate();
    //public static Rect UpdateButtonRect;

    public static string CloseButtonText = "CloseButton".Translate();
    public static Rect CloseButtonRect;

    public static string SettingsButtonText = "ModSettings".Translate();
    public static Rect SettingsButtonRect;

    public static int OldSmallFontSize = Text.fontStyles[1].fontSize;
    public const int TitleFontSize = 30;
    public const int DescFontSize = 18;
    public const TextAnchor TitleAnchor = TextAnchor.UpperCenter;
    public const TextAnchor DescAnchor = TextAnchor.MiddleLeft;

    static Dialog_UpdateIntro_SCOS()
    {
        GUI.color = Color.white;
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Small;
        GUIStyle style = Text.fontStyles[1];
        TextAnchor OldAnchor = Text.Anchor;

        style.fontSize = TitleFontSize;
        Text.Anchor = TitleAnchor;
        Vector2 introSize = Text.CalcSize(IntroTitleText) * PsycheCardUtility.calcSizeScalingVector;
        inRectWidth = Mathf.Max(inRectWidth, introSize.x);
        windowRectWidth = inRectWidth + 2f * Window.StandardMargin;
        float introHeight = Mathf.Max(40f, 1.035f * introSize.y);
        IntroTitleRect = new Rect(0f, 0f, inRectWidth, introHeight);

        style.fontSize = DescFontSize;
        Text.Anchor = DescAnchor;
        float introDescHeight = Text.CalcHeight(IntroDescText, inRectWidth) * PsycheCardUtility.calcSizeScalingVector.y;
        //float introDescHeight = 140f;
        IntroDescRect = new Rect(0f, IntroTitleRect.yMax, inRectWidth, introDescHeight);

        //float buttonWidth = Mathf.Max(Text.CalcSize(UpdateButtonText).x + 20f, Text.CalcSize(CloseButtonText).x + 20f, Window.CloseButSize.x);
        float buttonWidth = Text.CalcSize(CloseButtonText).x * PsycheCardUtility.calcSizeScalingVector.x + 20f;
        buttonWidth = Mathf.Max(buttonWidth, Window.CloseButSize.x);
        buttonWidth = Mathf.Max(buttonWidth, Text.CalcSize(SettingsButtonText).x * PsycheCardUtility.calcSizeScalingVector.x + 20f);
        float buttonY = IntroDescRect.yMax + Window.StandardMargin;

        //UpdateButtonRect = new Rect(-Window.StandardMargin + windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);
        //CloseButtonRect = new Rect(-Window.StandardMargin + 2f * windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);
        CloseButtonRect = new Rect(-Window.StandardMargin + 1f * windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);
        SettingsButtonRect = new Rect(-Window.StandardMargin + 2f * windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);

        inRectHeight = CloseButtonRect.yMax - IntroTitleRect.y;
        windowRectHeight = inRectHeight + 2f * Window.StandardMargin;

        style.fontSize = OldSmallFontSize;
        Text.Anchor = OldAnchor;
        Text.Font = oldFont;
    }
}