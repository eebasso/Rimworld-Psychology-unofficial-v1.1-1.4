using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology;

[StaticConstructorOnStartup]
public static class EditPsycheUtility
{
    public static Vector2 NodeScrollPosition = Vector2.zero;
    //public static List<Pair<string, float>> CachedList = new List<Pair<string, float>>();
    public static Dictionary<string, string> Descriptions = new Dictionary<string, string>();
    //public static Dictionary<string, PersonalityNode> Nodes = new Dictionary<string, PersonalityNode>();
    //public static int pawnKinseyRating = 0;
    //public static float pawnSexDrive = 0f;
    //public static float pawnRomanticDrive = 0f;
    public static List<Pair<string, PersonalityNodeDef>> SortedLabelNodeDefList = new List<Pair<string, PersonalityNodeDef>>();

    public static float TitleHeight;
    public static float TitleWidth;
    //public static float WarningHeight;
    public static Vector2 KinseySize;
    public static Vector2 SexDriveSize;
    public static Vector2 RomDriveSize;
    public static float SexualityWidth;
    public static float SexualityHeight;
    public static float NodeWidth;
    public static float NodeHeight;

    public static float ScrollHeight;
    public static float ButtonWidth;
    public static float ButtonHeight;
    public static float EditWidth = 0f;

    public static string TitleText = "";
    //public static string CachedTitleText = "";
    public static string WarningText = "PersonalityNodeWarning".Translate();
    public static string KinseyRatingText = "KinseyRating".Translate();
    public static string SexDriveText = "SexDrive".Translate();
    public static string RomDriveText = "RomanticDrive".Translate();
    public static string SaveButtonText = "SaveButton".Translate();
    public static string ResetButtonText = "ResetButton".Translate();
    public static string RandomButtonText = "Random".Translate();

    public static Vector2 ScalingVector = new Vector2(1.035f, 1.025f);
    public static float SliderWidth = 200f;
    //public static float SliderHeight = 0f;
    //public static float SliderBarWidth = PsycheCardUtility.SliderWidth;
    public static float SliderShiftDown = 2.5f;
    public static float HighlightPadding = PsycheCardUtility.HighlightPadding;
    public static float BoundaryPadding = PsycheCardUtility.BoundaryPadding;
    public static float EditMargin = PsycheCardUtility.BoundaryPadding + PsycheCardUtility.HighlightPadding;

    static EditPsycheUtility()
    {

        //if ((EditWidth != 0f && TitleText == CachedTitleText) || !PsycheHelper.PsychologyEnabled(pawn))
        //{
        //    return EditWidth;
        //}
        //CachedTitleText = TitleText;

        if (PsychologySettings.enableKinsey)
        {
            //pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
            //pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
            //pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            KinseySize = ScalingVector * Text.CalcSize(KinseyRatingText);
            SexDriveSize = ScalingVector * Text.CalcSize(SexDriveText);
            RomDriveSize = ScalingVector * Text.CalcSize(RomDriveText);
            SexualityWidth = Mathf.Max(KinseySize.x, SexDriveSize.x, RomDriveSize.x);
            SexualityHeight = Mathf.Max(KinseySize.y, SexDriveSize.y, RomDriveSize.y);
        }
        //SexualityHeight = Mathf.Max(SliderHeight, SexualityHeight);

        //CachedList.Clear();
        //Nodes.Clear();
        Descriptions.Clear();
        SortedLabelNodeDefList.Clear();

        foreach (PersonalityNodeDef def in DefDatabase<PersonalityNodeDef>.AllDefs)
        {
            string nodeLabel = def.label.CapitalizeFirst();
            Vector2 nodeSize = ScalingVector * Text.CalcSize(nodeLabel);
            NodeWidth = Mathf.Max(NodeWidth, nodeSize.x);
            NodeHeight = Mathf.Max(NodeHeight, nodeSize.y);
            //CachedList.Add(new Pair<string, float>(nodeLabel, node.rawRating));
            //string descriptionString = node.def.description.ReplaceFirst("{0}", node.def.descriptionLabel)
            string descriptionString = def.description.ReplaceFirst("{0}", def.descriptionLabel.Colorize(UIAssets.TitleColor));
            descriptionString += ((string)"AntonymColon".Translate()).ReplaceFirst("{0}", def.antonymLabel.Colorize(UIAssets.TitleColor));
            Descriptions.Add(nodeLabel, descriptionString);
            SortedLabelNodeDefList.Add(new Pair<string, PersonalityNodeDef>(nodeLabel, def));
            //Nodes.Add(nodeLabel, node);
        }
        SortedLabelNodeDefList.SortBy(n => n.First);

        //CachedList.SortBy(n => n.First);
        //NodeHeight = Mathf.Max(SliderHeight, NodeHeight);

        

        
        //WarningHeight = Text.CalcHeight(WarningText + " buffer buffer buffer buffer  buffer buffer buffer buffer buffer buffer buffer buffer ", width) + 5f;
        //WarningHeight = UIAssets.TextCalcHeight(WarningText, width) + 5f;

        Vector2 saveButtonSize = ScalingVector * Text.CalcSize(SaveButtonText);
        Vector2 resetButtonSize = ScalingVector * Text.CalcSize(ResetButtonText);
        Vector2 randomButtonSize = ScalingVector * Text.CalcSize(RandomButtonText);
        ButtonWidth = 30f + Mathf.Max(saveButtonSize.x, resetButtonSize.x, randomButtonSize.x);
        ButtonHeight = 5f + Mathf.Max(saveButtonSize.y, resetButtonSize.y, randomButtonSize.y);
    }

    public static float CalculateEditWidth(Pawn pawn)
    {
        var name = pawn.Name.ToStringFull;
        TitleText = (name != null && name != "") ? name : "PsycheEditorNewColonist".Translate().ToString();
        float labelWidth = Mathf.Max(SexualityWidth, NodeWidth);
        SexualityWidth = labelWidth;
        NodeWidth = labelWidth;
        EditWidth = EditMargin + labelWidth + BoundaryPadding + SliderWidth + HighlightPadding + GenUI.ScrollBarWidth + EditMargin;
        TitleWidth = labelWidth + BoundaryPadding + SliderWidth;
        Text.Font = GameFont.Medium;
        TitleHeight = Text.CalcHeight(TitleText, TitleWidth);
        Text.Font = GameFont.Small;
        return EditWidth;
    }

    public static void DrawEditPsyche(Rect totalRect, Pawn pawn)
    {
        Text.Font = GameFont.Small;
        GUIStyle style = Text.fontStyles[1];
        TextAnchor oldAnchor = Text.Anchor;
        float yCompression = 0.9f;
        float highlightShift = 0.063f;

        totalRect.width = CalculateEditWidth(pawn);
        //ScrollHeight = totalRect.height - (BoundaryPadding + TitleHeight + WarningHeight + 3f * yCompression * SexualityHeight + 2f * BoundaryPadding + 0f + 2f * BoundaryPadding + ButtonHeight + BoundaryPadding);
        ScrollHeight = totalRect.height - (BoundaryPadding + TitleHeight + BoundaryPadding + 3f * yCompression * SexualityHeight + 2f * BoundaryPadding + 0f + 2f * BoundaryPadding + ButtonHeight + BoundaryPadding);

        GUI.BeginGroup(totalRect);
        totalRect.position = Vector2.zero;
        Rect mainRect = totalRect.ContractedBy(BoundaryPadding);
        mainRect.xMin += HighlightPadding;
        mainRect.xMax -= HighlightPadding;

        Rect titleHighlightRect = new Rect(mainRect.x, mainRect.y, TitleWidth, TitleHeight);
        Text.Font = GameFont.Medium;
        Widgets.Label(mainRect, TitleText);
        Text.Font = GameFont.Small;
        titleHighlightRect.xMin -= HighlightPadding;
        titleHighlightRect.xMax += HighlightPadding;
        Widgets.DrawHighlightIfMouseover(titleHighlightRect);
        TooltipHandler.TipRegion(titleHighlightRect, delegate
        {
            return WarningText;
        }, WarningText.GetHashCode());
        mainRect.yMin += TitleHeight;

        //Widgets.Label(mainRect, WarningText);
        //mainRect.yMin += WarningHeight;
        mainRect.yMin += BoundaryPadding;

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

            Pawn_SexualityTracker st = PsycheHelper.Comp(pawn).Sexuality;
            st.kinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(sexualitySliderRect, st.kinseyRating, 0f, 6f, true));
            sexualitySliderRect.y += sexualityHighlightRect.height;
            st.sexDrive = Widgets.HorizontalSlider(sexualitySliderRect, st.sexDrive, 0f, 1f, true);
            sexualitySliderRect.y += sexualityHighlightRect.height;
            st.romanticDrive = Widgets.HorizontalSlider(sexualitySliderRect, st.romanticDrive, 0f, 1f, true);

            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"KinseyDescription".Translate()).ReplaceFirst("{0}", "KinseyDescription0".Translate().Colorize(UIAssets.TitleColor));
            }, 14924);
            sexualityTooltipRect.y += sexualityHighlightRect.height;
            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"SexDriveDescription".Translate()).ReplaceFirst("{0}", "SexDriveDescription0".Translate().Colorize(UIAssets.TitleColor));
            }, 14925);
            sexualityTooltipRect.y += sexualityHighlightRect.height;
            TooltipHandler.TipRegion(sexualityTooltipRect, delegate
            {
                return ((string)"RomanticDriveDescription".Translate()).ReplaceFirst("{0}", "RomanticDriveDescription0".Translate().Colorize(UIAssets.TitleColor));
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

        UIAssets.DrawLineHorizontal(mainRect.x - HighlightPadding, mainRect.y, mainRect.width + 2f * HighlightPadding, UIAssets.ModEntryLineColor);
        mainRect.yMin += BoundaryPadding;

        Rect scrollRect = new Rect(mainRect.x - HighlightPadding, mainRect.y, totalRect.xMax - mainRect.x - BoundaryPadding, ScrollHeight);
        Rect viewRect = new Rect(0f, 0f, scrollRect.width - GenUI.ScrollBarWidth, SortedLabelNodeDefList.Count * yCompression * NodeHeight + 4f);

        Rect labelRect = new Rect(HighlightPadding, 0f, NodeWidth, NodeHeight);
        Rect sliderRect = new Rect(labelRect.xMax + BoundaryPadding, SliderShiftDown, SliderWidth, NodeHeight);
        Rect tooltipRect = labelRect;
        tooltipRect.xMin = 0f;
        tooltipRect.height *= yCompression;
        tooltipRect.y += highlightShift * NodeHeight;
        Rect highlightRect = tooltipRect;
        highlightRect.xMax = viewRect.xMax;
        Widgets.BeginScrollView(scrollRect, ref NodeScrollPosition, viewRect);
        Pawn_PsycheTracker pt = PsycheHelper.Comp(pawn).Psyche;
        for (int i = 0; i < SortedLabelNodeDefList.Count; i++)
        {
            string label = SortedLabelNodeDefList[i].First;
            PersonalityNodeDef def = SortedLabelNodeDefList[i].Second;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = oldAnchor;
            TooltipHandler.TipRegion(tooltipRect, () => Descriptions[label], 436532 + i);
            pt.nodeDict[def].rawRating = Widgets.HorizontalSlider(sliderRect, pt.nodeDict[def].rawRating, 0f, 1f, true);
            Widgets.DrawHighlightIfMouseover(highlightRect);
            labelRect.y += highlightRect.height;
            sliderRect.y += highlightRect.height;
            tooltipRect.y += highlightRect.height;
            highlightRect.y += highlightRect.height;
        }
        Widgets.EndScrollView();
        mainRect.yMin += ScrollHeight + BoundaryPadding;

        UIAssets.DrawLineHorizontal(mainRect.x - HighlightPadding, mainRect.y, mainRect.width + 2f * HighlightPadding, UIAssets.ModEntryLineColor);
        mainRect.yMin += BoundaryPadding;

        float blankWidth = 0.33f * Mathf.Max(EditMargin, totalRect.width - 2f * ButtonWidth);
        Rect resetRect = new Rect(blankWidth, mainRect.yMax - ButtonHeight, ButtonWidth, ButtonHeight);
        Rect randomRect = new Rect(resetRect.xMax + blankWidth, resetRect.y, ButtonWidth, ButtonHeight);

        //for (int i = 0; i < CachedList.Count; i++)
        //{
        //    PersonalityNode node = Nodes[CachedList[i].First];
        //    if (node.rawRating != CachedList[i].Second)
        //    {
        //        PsycheCardUtility.Ticker = -1;
        //    }
        //    node.rawRating = CachedList[i].Second;
        //}
        //if (PsychologySettings.enableKinsey)
        //{
        //    bool bool1 = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != pawnKinseyRating;
        //    bool bool2 = PsycheHelper.Comp(pawn).Sexuality.sexDrive != pawnSexDrive;
        //    bool bool3 = PsycheHelper.Comp(pawn).Sexuality.romanticDrive != pawnRomanticDrive;
        //    if (bool1 || bool2 || bool3)
        //    {
        //        PsycheHelper.Comp(pawn).Sexuality.kinseyRating = pawnKinseyRating;
        //        PsycheHelper.Comp(pawn).Sexuality.sexDrive = pawnSexDrive;
        //        PsycheHelper.Comp(pawn).Sexuality.romanticDrive = pawnRomanticDrive;
        //        PsycheCardUtility.Ticker = -1;
        //    }
        //}
        if (Widgets.ButtonText(resetRect, ResetButtonText, true, true, true))
        {
            PsycheHelper.Comp(pawn).Psyche.RandomizeRatings();
            //for (int i = 0; i < CachedList.Count; i++)
            //{
            //    string nodeLabel = CachedList[i].First;
            //    PersonalityNode node = Nodes[nodeLabel];
            //    CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
            //}
            //if (PsychologySettings.enableKinsey)
            //{
            //    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
            //    pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
            //    pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
            //    pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            //}
            //PsycheCardUtility.Ticker = -1;
        }
        TooltipHandler.TipRegion(resetRect, delegate
        {
            return "PsycheResetTooltip".Translate();
        }, 412832);

        if (Widgets.ButtonText(randomRect, RandomButtonText, true, true, true))
        {
            int randomSeed = Mathf.CeilToInt(1e+7f * Rand.Value);
            //PsycheHelper.Comp(pawn).Psyche.RandomizeRatings(randomSeed);
            //for (int i = 0; i < CachedList.Count; i++)
            //{
            //    string nodeLabel = CachedList[i].First;
            //    PersonalityNode node = Nodes[nodeLabel];
            //    CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
            //}
            //if (PsychologySettings.enableKinsey)
            //{
            //    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(Mathf.CeilToInt(1e+7f * Rand.Value));
            //    pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
            //    pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
            //    pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
            //}
            //PsycheCardUtility.Ticker = -1;
        }
        PsycheCardUtility.Ticker = -1;
        GUI.EndGroup();
    }
}