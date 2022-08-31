using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology;

public class EditPsycheUtility
{
    static Vector2 NodeScrollPosition = Vector2.zero;
    static List<Pair<string, float>> CachedList = new List<Pair<string, float>>();
    static Dictionary<string, string> Descriptions = new Dictionary<string, string>();
    static Dictionary<string, PersonalityNode> Nodes = new Dictionary<string, PersonalityNode>();
    static int pawnKinseyRating = 0;
    static float pawnSexDrive = 0f;
    static float pawnRomanticDrive = 0f;

    static float TitleHeight;
    static float WarningHeight;
    static Vector2 KinseySize;
    static Vector2 SexDriveSize;
    static Vector2 RomDriveSize;
    static float SexualityWidth;
    static float SexualityHeight;
    static float NodeWidth;
    static float NodeHeight;

    static float ScrollHeight;
    static float ButtonWidth;
    static float ButtonHeight;
    public static float EditWidth = 0f;

    static string TitleText = "";
    static string CachedTitleText = "";
    static string WarningText = "PersonalityNodeWarning".Translate();
    static string KinseyRatingText = "KinseyRating".Translate();
    static string SexDriveText = "SexDrive".Translate();
    static string RomDriveText = "RomanticDrive".Translate();
    static string SaveButtonText = "SaveButton".Translate();
    static string ResetButtonText = "ResetButton".Translate();
    static string RandomButtonText = "Random".Translate();

    static Vector2 ScalingVector = new Vector2(1.035f, 1.025f);
    static float SliderWidth = 200f;
    //static float SliderHeight = 0f;
    static float SliderBarWidth = PsycheCardUtility.SliderWidth;
    static float SliderShiftDown = 2.5f;
    static float HighlightPadding = PsycheCardUtility.HighlightPadding;
    static float BoundaryPadding = PsycheCardUtility.BoundaryPadding;
    static float EditMargin = PsycheCardUtility.BoundaryPadding + PsycheCardUtility.HighlightPadding;

    public static float CalculateEditWidth(Pawn pawn)
    {
        var name = pawn.Name.ToStringFull;
        TitleText = (name != null && name != "") ? name : "PsycheEditorNewColonist".Translate().ToString();
        if ((EditWidth != 0f && TitleText == CachedTitleText) || !PsycheHelper.PsychologyEnabled(pawn))
        {
            return EditWidth;
        }
        CachedTitleText = TitleText;
        
        if (PsychologySettings.enableKinsey)
        {
            pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
            pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
            pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            KinseySize = ScalingVector * Text.CalcSize(KinseyRatingText);
            SexDriveSize = ScalingVector * Text.CalcSize(SexDriveText);
            RomDriveSize = ScalingVector * Text.CalcSize(RomDriveText);
            SexualityWidth = Mathf.Max(KinseySize.x, SexDriveSize.x, RomDriveSize.x);
            SexualityHeight = Mathf.Max(KinseySize.y, SexDriveSize.y, RomDriveSize.y);
        }
        //SexualityHeight = Mathf.Max(SliderHeight, SexualityHeight);

        CachedList.Clear();
        Descriptions.Clear();
        Nodes.Clear();
        foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
        {
            string nodeLabel = node.def.label.CapitalizeFirst();
            Vector2 nodeSize = ScalingVector * Text.CalcSize(nodeLabel);
            NodeWidth = Mathf.Max(NodeWidth, nodeSize.x);
            NodeHeight = Mathf.Max(NodeHeight, nodeSize.y);
            CachedList.Add(new Pair<string, float>(nodeLabel, node.rawRating));
            //string descriptionString = node.def.description.ReplaceFirst("{0}", node.def.descriptionLabel)
            string descriptionString = node.def.description.ReplaceFirst("{0}", node.def.descriptionLabel.Colorize(PsycheCardUtility.TitleColor));
            descriptionString += ((string)"AntonymColon".Translate()).ReplaceFirst("{0}", node.def.antonymLabel.Colorize(PsycheCardUtility.TitleColor));
            Descriptions.Add(nodeLabel, descriptionString);
            Nodes.Add(nodeLabel, node);
        }
        CachedList.SortBy(n => n.First);
        //NodeHeight = Mathf.Max(SliderHeight, NodeHeight);

        float labelWidth = Mathf.Max(SexualityWidth, NodeWidth);
        SexualityWidth = labelWidth;
        NodeWidth = labelWidth;
        EditWidth = EditMargin + labelWidth + BoundaryPadding + SliderWidth + HighlightPadding + SliderBarWidth + EditMargin;

        float width = EditWidth - 2f * EditMargin;
        Text.Font = GameFont.Medium;
        TitleHeight = Text.CalcHeight(TitleText, width);
        Text.Font = GameFont.Small;
        WarningHeight = Text.CalcHeight(WarningText, width) + 5f;

        Vector2 saveButtonSize = ScalingVector * Text.CalcSize(SaveButtonText);
        Vector2 resetButtonSize = ScalingVector * Text.CalcSize(ResetButtonText);
        Vector2 randomButtonSize = ScalingVector * Text.CalcSize(RandomButtonText);
        ButtonWidth = 30f + Mathf.Max(saveButtonSize.x, resetButtonSize.x, randomButtonSize.x);
        ButtonHeight = 5f + Mathf.Max(saveButtonSize.y, resetButtonSize.y, randomButtonSize.y);

        return EditWidth;
    }

    //[LogPerformance]
    public static void DrawEditPsyche(Rect totalRect, Pawn pawn)
    {
        Text.Font = GameFont.Small;
        GUIStyle style = Text.fontStyles[1];
        TextAnchor oldAnchor = Text.Anchor;
        float yCompression = 0.9f;
        float highlightShift = 0.063f;

        totalRect.width = CalculateEditWidth(pawn);
        ScrollHeight = totalRect.height - (BoundaryPadding + TitleHeight + WarningHeight + 3f * yCompression * SexualityHeight + 2f * BoundaryPadding + 0f + 2f * BoundaryPadding  + ButtonHeight + BoundaryPadding);

        GUI.BeginGroup(totalRect);
        totalRect.position = Vector2.zero;
        Rect mainRect = totalRect.ContractedBy(BoundaryPadding);
        mainRect.xMin += HighlightPadding;
        mainRect.xMax -= HighlightPadding;

        Text.Font = GameFont.Medium;
        Widgets.Label(mainRect, TitleText);
        Text.Font = GameFont.Small;
        mainRect.yMin += TitleHeight;

        Widgets.Label(mainRect, WarningText);
        mainRect.yMin += WarningHeight;

        if (PsychologySettings.enableKinsey)
        {
            Rect sexualityLabelRect = new Rect(mainRect.x, mainRect.y, SexualityWidth, SexualityHeight);
            Rect sexualitySliderRect = new Rect(sexualityLabelRect.xMax + BoundaryPadding, sexualityLabelRect.y + SliderShiftDown, SliderWidth, SexualityHeight);
            Rect sexualityTooltipRect = sexualityLabelRect;
            sexualityTooltipRect.xMin -= HighlightPadding;
            sexualityTooltipRect.height *= yCompression;
            sexualityTooltipRect.y += highlightShift * SexualityHeight;
            Rect sexualityHighlightRect = sexualityTooltipRect;
            sexualityHighlightRect.xMax = sexualitySliderRect.xMax + HighlightPadding;
            Text.Font = GameFont.Tiny;
            for (int k = 0; k < 7; k++)
            {
                float numberCenterX = Mathf.Lerp(sexualitySliderRect.x + 6f, sexualitySliderRect.xMax - 6f, k / 6f);
                float numberCenterY = sexualitySliderRect.y - 3f;
                Rect numberRect = new Rect(Vector2.zero, ScalingVector * Text.CalcSize(k.ToString()));
                numberRect.center = new Vector2(numberCenterX, numberCenterY);
                Widgets.Label(numberRect, k.ToString());
            }
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(sexualityLabelRect, KinseyRatingText);
            sexualityLabelRect.y += sexualityHighlightRect.height;
            Widgets.Label(sexualityLabelRect, SexDriveText);
            sexualityLabelRect.y += sexualityHighlightRect.height;
            Widgets.Label(sexualityLabelRect, RomDriveText);
            Text.Anchor = oldAnchor;

            pawnKinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(sexualitySliderRect, pawnKinseyRating, 0f, 6f, true));
            sexualitySliderRect.y += sexualityHighlightRect.height;
            pawnSexDrive = Widgets.HorizontalSlider(sexualitySliderRect, pawnSexDrive, 0f, 1f, true);
            sexualitySliderRect.y += sexualityHighlightRect.height;
            pawnRomanticDrive = Widgets.HorizontalSlider(sexualitySliderRect, pawnRomanticDrive, 0f, 1f, true);

            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"KinseyDescription".Translate()).ReplaceFirst("{0}", "KinseyDescription0".Translate().Colorize(PsycheCardUtility.TitleColor));
            }, 14924);
            sexualityTooltipRect.y += sexualityHighlightRect.height;
            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"SexDriveDescription".Translate()).ReplaceFirst("{0}", "SexDriveDescription0".Translate().Colorize(PsycheCardUtility.TitleColor));
            }, 14925);
            sexualityTooltipRect.y += sexualityHighlightRect.height;
            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"RomanticDriveDescription".Translate()).ReplaceFirst("{0}", "RomanticDriveDescription0".Translate().Colorize(PsycheCardUtility.TitleColor));
            }, 14926);

            Widgets.DrawHighlightIfMouseover(sexualityHighlightRect);
            sexualityHighlightRect.y += sexualityHighlightRect.height;
            Widgets.DrawHighlightIfMouseover(sexualityHighlightRect);
            sexualityHighlightRect.y += sexualityHighlightRect.height;
            Widgets.DrawHighlightIfMouseover(sexualityHighlightRect);
        }
        else
        {
            Rect warningRect = new Rect(mainRect.x, mainRect.y, mainRect.width, 3f * yCompression * SexualityHeight);
            Widgets.Label(warningRect, "SexualityDisabledWarning".Translate().Colorize(Color.red));
        }
        mainRect.yMin += 3f * yCompression * SexualityHeight + BoundaryPadding;

        PsychColor.DrawLineHorizontal(mainRect.x - HighlightPadding, mainRect.y, mainRect.width + 2f * HighlightPadding, PsychColor.ModEntryLineColor);
        mainRect.yMin += BoundaryPadding;

        Rect scrollRect = new Rect(mainRect.x - HighlightPadding, mainRect.y, totalRect.xMax - mainRect.x - BoundaryPadding, ScrollHeight);
        Rect viewRect = new Rect(0f, 0f, scrollRect.width - SliderBarWidth, CachedList.Count * yCompression * NodeHeight + 4f);

        Rect labelRect = new Rect(HighlightPadding, 0f, NodeWidth, NodeHeight);
        Rect sliderRect = new Rect(labelRect.xMax + BoundaryPadding, SliderShiftDown, SliderWidth, NodeHeight);
        Rect tooltipRect = labelRect;
        tooltipRect.xMin = 0f;
        tooltipRect.height *= yCompression;
        tooltipRect.y += highlightShift * NodeHeight;
        Rect highlightRect = tooltipRect;
        highlightRect.xMax = viewRect.xMax;
        Widgets.BeginScrollView(scrollRect, ref NodeScrollPosition, viewRect);
        for (int i = 0; i < CachedList.Count; i++)
        {
            string label = CachedList[i].First;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = oldAnchor;
            TooltipHandler.TipRegion(tooltipRect, () => Descriptions[label], 436532 + i);
            float newVal = Widgets.HorizontalSlider(sliderRect, CachedList[i].Second, 0f, 1f, true);
            CachedList[i] = new Pair<string, float>(CachedList[i].First, newVal);
            Widgets.DrawHighlightIfMouseover(highlightRect);
            labelRect.y += highlightRect.height;
            sliderRect.y += highlightRect.height;
            tooltipRect.y += highlightRect.height;
            highlightRect.y += highlightRect.height;
        }
        Widgets.EndScrollView();
        mainRect.yMin += ScrollHeight + BoundaryPadding;

        PsychColor.DrawLineHorizontal(mainRect.x - HighlightPadding, mainRect.y, mainRect.width + 2f * HighlightPadding, PsychColor.ModEntryLineColor);
        mainRect.yMin += BoundaryPadding;

        float blankWidth = 0.33f * Mathf.Max(EditMargin, totalRect.width - 2f * ButtonWidth);
        Rect resetRect = new Rect(blankWidth, mainRect.yMax - ButtonHeight, ButtonWidth, ButtonHeight);
        Rect randomRect = new Rect(resetRect.xMax + blankWidth, resetRect.y, ButtonWidth, ButtonHeight);

        for (int i = 0; i < CachedList.Count; i++)
        {
            PersonalityNode node = Nodes[CachedList[i].First];
            node.rawRating = CachedList[i].Second;
            node.cachedRating = -1f;
        }
        PsycheCardUtility.Ticker = 0;

        if (PsychologySettings.enableKinsey)
        {
            bool bool1 = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != pawnKinseyRating;
            bool bool2 = PsycheHelper.Comp(pawn).Sexuality.sexDrive != pawnSexDrive;
            bool bool3 = PsycheHelper.Comp(pawn).Sexuality.romanticDrive != pawnRomanticDrive;
            if (bool1 || bool2 || bool3)
            {
                PsycheHelper.Comp(pawn).Sexuality.kinseyRating = pawnKinseyRating;
                PsycheHelper.Comp(pawn).Sexuality.sexDrive = pawnSexDrive;
                PsycheHelper.Comp(pawn).Sexuality.romanticDrive = pawnRomanticDrive;
                PsycheCardUtility.Ticker = 0;
            }
        }

        if (Widgets.ButtonText(resetRect, ResetButtonText, true, false, true))
        {
            PsycheHelper.Comp(pawn).Psyche.RandomizeUpbringingAndRatings();
            for (int i = 0; i < CachedList.Count; i++)
            {
                string nodeLabel = CachedList[i].First;
                PersonalityNode node = Nodes[nodeLabel];
                CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
            }
            if (PsychologySettings.enableKinsey)
            {
                PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            }
            PsycheCardUtility.Ticker = 0;
        }
        TooltipHandler.TipRegion(resetRect, delegate
        {
            return "PsycheResetTooltip".Translate();
        }, 412832);

        if (Widgets.ButtonText(randomRect, RandomButtonText, true, false, true))
        {
            int randomSeed = Mathf.CeilToInt(1e+7f * Rand.Value);
            PsycheHelper.Comp(pawn).Psyche.RandomizeUpbringingAndRatings(randomSeed);
            for (int i = 0; i < CachedList.Count; i++)
            {
                string nodeLabel = CachedList[i].First;
                PersonalityNode node = Nodes[nodeLabel];
                CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
            }
            if (PsychologySettings.enableKinsey)
            {
                PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(Mathf.CeilToInt(1e+7f * Rand.Value));
                pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            }
            PsycheCardUtility.Ticker = 0;
        }
        GUI.EndGroup();
    }
}