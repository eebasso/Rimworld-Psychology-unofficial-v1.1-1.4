using UnityEngine;
using Verse;namespace Psychology;
public class Dialog_UpdateYesNo : Window{    public float windowRectWidth = 500f;
    public static float windowRectHeight;    public float inRectWidth = 500f - 2f * StandardMargin;    public static float inRectHeight;    public string PromptDescription = "UpdateYesNoDescription".Translate();
    public Rect PromptRect;
    
    public string YesButtonText = "Yes".Translate();
    public Rect YesButtonRect;
    
    public string NoButtonText = "No".Translate();
    public Rect NoButtonRect;

    public Dialog_UpdateYesNo()
    {
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Small;
        float promptHeight = Text.CalcHeight(PromptDescription, inRectWidth);
        PromptRect = new Rect(0f, 0f, inRectWidth, promptHeight);

        float buttonY = PromptRect.yMax + StandardMargin;
        YesButtonRect = new Rect(-StandardMargin + windowRectWidth / 3f - 0.5f * CloseButSize.x, buttonY, CloseButSize.x, CloseButSize.y);
        NoButtonRect = new Rect(-StandardMargin + 2f * windowRectWidth / 3f - 0.5f * CloseButSize.x, buttonY, CloseButSize.x, CloseButSize.y);

        inRectHeight = YesButtonRect.yMax - PromptRect.y;
        windowRectHeight = inRectHeight + 2f * StandardMargin;

        Text.Font = oldFont;
    }

    public override Vector2 InitialSize
    {
        get
        {
            return new Vector2(windowRectWidth, windowRectHeight);
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        doCloseButton = false;

        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Small;
        Widgets.Label(PromptRect, PromptDescription);
        Text.Font = oldFont;

        if (Widgets.ButtonText(YesButtonRect, YesButtonText))
        {
            if (PsycheHelper.GameComp != null)
            {
                PsycheHelper.GameComp.RandomizeRatingsForAllPawns();
            }
            this.Close();
        }        if (Widgets.ButtonText(NoButtonRect, NoButtonText))
        {
            this.Close();
        }
    }}