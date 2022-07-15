using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology
{
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
        static float SliderWidth = 200f;
        static float SliderBarWidth = 18f;
        static float ScrollHeight;
        static float ButtonWidth;
        static float ButtonHeight;
        public static float EditWidth = 0f;

        static string TitleText = "PsycheEditor".Translate("LongTestName");
        static string WarningText = "PersonalityNodeWarning".Translate();
        static string KinseyRatingText = "KinseyRating".Translate();
        static string SexDriveText = "SexDrive".Translate();
        static string RomDriveText = "RomanticDrive".Translate();
        static string SaveButtonText = "SaveButton".Translate();
        static string ResetButtonText = "ResetButton".Translate();
        static string RandomButtonText = "Random".Translate();

        static float BoundaryPadding = PsycheCardUtility.BoundaryPadding;
        static float HighlightPadding = PsycheCardUtility.HighlightPadding;
        static float EditMargin = PsycheCardUtility.BoundaryPadding + PsycheCardUtility.HighlightPadding;

        static float SliderShiftDown = 0f;

        static Rect CachedRect = new Rect(0f, 0f, 1f, 1f);

        public static float CalculateEditWidth(Pawn pawn)
        {
            if (EditWidth != 0f || !PsycheHelper.PsychologyEnabled(pawn))
            {
                return EditWidth;
            }
            Vector2 scalingVector = new Vector2(1.035f, 1.025f);
            if (PsychologyBase.ActivateKinsey())
            {
                pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
                KinseySize = scalingVector * Text.CalcSize(KinseyRatingText);
                SexDriveSize = scalingVector * Text.CalcSize(SexDriveText);
                RomDriveSize = scalingVector * Text.CalcSize(RomDriveText);
                SexualityWidth = Mathf.Max(KinseySize.x, SexDriveSize.x, RomDriveSize.x);
                SexualityHeight = Mathf.Max(KinseySize.y, SexDriveSize.y, RomDriveSize.y);
            }
            SexualityHeight = Mathf.Max(26f, SexualityHeight);

            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                string nodeLabel = node.def.label.CapitalizeFirst();
                Vector2 nodeSize = scalingVector * Text.CalcSize(nodeLabel);
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
            NodeHeight = Mathf.Max(26f, NodeHeight);

            float labelWidth = Mathf.Max(SexualityWidth, NodeWidth);
            SexualityWidth = labelWidth;
            NodeWidth = labelWidth;
            EditWidth = EditMargin + HighlightPadding + labelWidth + BoundaryPadding + SliderWidth + HighlightPadding + SliderBarWidth + EditMargin;

            float width = EditWidth - 2f * EditMargin;
            Text.Font = GameFont.Medium;
            TitleHeight = scalingVector.y * Text.CalcHeight(TitleText, width);
            Text.Font = GameFont.Small;
            WarningHeight = scalingVector.y * Text.CalcHeight(WarningText, width);

            Vector2 saveButtonSize = scalingVector * Text.CalcSize(SaveButtonText);
            Vector2 resetButtonSize = scalingVector * Text.CalcSize(ResetButtonText);
            Vector2 randomButtonSize = scalingVector * Text.CalcSize(RandomButtonText);
            ButtonWidth = 30f + Mathf.Max(saveButtonSize.x, resetButtonSize.x, randomButtonSize.x);
            ButtonHeight = 5f + Mathf.Max(saveButtonSize.y, resetButtonSize.y, randomButtonSize.y);

            return EditWidth;
        }

        [LogPerformance]
        public static void DrawEditPsyche(Rect totalRect, Pawn pawn)
        {
            if (CachedRect != totalRect)
            {
                totalRect.width = CalculateEditWidth(pawn);
                ScrollHeight = totalRect.height - (EditMargin + TitleHeight + WarningHeight + 0f + 2f * BoundaryPadding + 3f * SexualityHeight + ButtonHeight + EditMargin);
                CachedRect = totalRect;
            }

            GUI.BeginGroup(totalRect);
            totalRect.position = Vector2.zero;
            Rect mainRect = totalRect.ContractedBy(EditMargin);
            TitleText = (pawn.LabelShort != null || pawn.LabelShort != "") ? "PsycheEditor".Translate(pawn.LabelShortCap) : "PsycheEditorNewColonist".Translate();
            Text.Font = GameFont.Medium;
            Widgets.Label(mainRect, TitleText);
            Text.Font = GameFont.Small;
            mainRect.yMin += TitleHeight;

            Widgets.Label(mainRect, WarningText);
            mainRect.yMin += WarningHeight;

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(mainRect.x, mainRect.y, mainRect.width);
            //Widgets.DrawLineHorizontal(totalRect.x, mainRect.y, totalRect.width);
            GUI.color = Color.white;
            
            Rect nodeRect = new Rect(mainRect.x, mainRect.y, totalRect.xMax - mainRect.x - EditMargin, ScrollHeight);
            Rect viewRect = new Rect(0f, 0f, nodeRect.width - SliderBarWidth, CachedList.Count * NodeHeight);
            float yPosition = 0f;
            Widgets.BeginScrollView(nodeRect, ref NodeScrollPosition, viewRect);
            for (int i = 0; i < CachedList.Count; i++)
            {
                string label = CachedList[i].First;
                Rect highlightRect = new Rect(HighlightPadding, yPosition, NodeWidth, NodeHeight);

                Widgets.Label(highlightRect, label);
                highlightRect.xMin -= HighlightPadding;
                highlightRect.height *= 0.9f;
                TooltipHandler.TipRegion(highlightRect, () => Descriptions[label], 436532 + Mathf.RoundToInt(yPosition));

                Rect sliderRect = new Rect(highlightRect.xMax + BoundaryPadding, yPosition + SliderShiftDown, SliderWidth, NodeHeight);
                float newVal = Widgets.HorizontalSlider(sliderRect, CachedList[i].Second, 0f, 1f, true);
                CachedList[i] = new Pair<string, float>(CachedList[i].First, newVal);

                highlightRect.xMax = viewRect.xMax;
                Widgets.DrawHighlightIfMouseover(highlightRect);

                yPosition += NodeHeight;
            }
            Widgets.EndScrollView();
            
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            //Widgets.DrawLineHorizontal(mainRect.x, mainRect.y - BoundaryPadding, mainRect.width);
            Widgets.DrawLineHorizontal(totalRect.x, mainRect.y, totalRect.width);
            GUI.color = Color.white;
            mainRect.yMin += ScrollHeight + 2f * BoundaryPadding;

            float x0 = mainRect.x + HighlightPadding;
            float x1 = x0 + SexualityWidth + BoundaryPadding;
            float y0 = mainRect.y;
            float y1 = y0 + SexualityHeight;
            float y2 = y1 + SexualityHeight;
            if (PsychologyBase.ActivateKinsey())
            {
                Rect kinseyRatingLabelRect = new Rect(x0, y0, SexualityWidth, SexualityHeight);
                Rect sexDriveLabelRect = new Rect(x0, y1, SexualityWidth, SexualityHeight);
                Rect romDriveLabelRect = new Rect(x0, y2, SexualityWidth, SexualityHeight);

                Rect kinseyRatingSliderRect = new Rect(x1, y0 + SliderShiftDown, SliderWidth, SexualityHeight);
                Rect sexDriveSliderRect = new Rect(x1, y1 + SliderShiftDown, SliderWidth, SexualityHeight);
                Rect romDriveSliderRect = new Rect(x1, y2 + SliderShiftDown, SliderWidth, SexualityHeight);
                
                //TextAnchor oldAnchor = Text.Anchor;
                //Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(kinseyRatingLabelRect, KinseyRatingText);
                Widgets.Label(sexDriveLabelRect, SexDriveText);
                Widgets.Label(romDriveLabelRect, RomDriveText);
                //Text.Anchor = oldAnchor;

                pawnKinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(kinseyRatingSliderRect, pawnKinseyRating, 0f, 6f, true, leftAlignedLabel: "0", rightAlignedLabel: "6"));
                pawnSexDrive = Widgets.HorizontalSlider(sexDriveSliderRect, pawnSexDrive, 0f, 1f, true);
                pawnRomanticDrive = Widgets.HorizontalSlider(romDriveSliderRect, pawnRomanticDrive, 0f, 1f, true);

                kinseyRatingLabelRect.xMin -= HighlightPadding;
                kinseyRatingLabelRect.height *= 0.9f;
                TooltipHandler.TipRegion(kinseyRatingLabelRect, delegate
                {
                    return "KinseyDescription".Translate();
                }, 14924);
                kinseyRatingLabelRect.xMax = kinseyRatingSliderRect.xMax + HighlightPadding;
                Widgets.DrawHighlightIfMouseover(kinseyRatingLabelRect);

                sexDriveLabelRect.xMin -= HighlightPadding;
                sexDriveLabelRect.height *= 0.9f;
                TooltipHandler.TipRegion(sexDriveLabelRect, delegate
                {
                    return "SexDriveDescription".Translate();
                }, 14925);
                sexDriveLabelRect.xMax = sexDriveSliderRect.xMax + HighlightPadding;
                Widgets.DrawHighlightIfMouseover(sexDriveLabelRect);

                romDriveLabelRect.xMin -= HighlightPadding;
                romDriveLabelRect.height *= 0.9f;
                TooltipHandler.TipRegion(romDriveLabelRect, delegate
                {
                    return "RomanticDriveDescription".Translate();
                }, 14926);
                romDriveLabelRect.xMax = romDriveSliderRect.xMax + HighlightPadding;
                Widgets.DrawHighlightIfMouseover(romDriveLabelRect);
            }
            else
            {
                GUI.color = Color.red;
                Rect warningRect = new Rect(x0, y0, mainRect.width, 3f * SexualityHeight);
                Widgets.Label(warningRect, "SexualityDisabledWarning".Translate());
                GUI.color = Color.white;
            }

            float blankWidth = 0.33f * Mathf.Max(EditMargin, totalRect.width - 2f * ButtonWidth);
            Rect resetRect = new Rect(blankWidth, mainRect.yMax - ButtonHeight, ButtonWidth, ButtonHeight);
            Rect randomRect = new Rect(resetRect.xMax + blankWidth, resetRect.y, ButtonWidth, ButtonHeight);
            
            if (Widgets.ButtonText(resetRect, ResetButtonText, true, false, true))
            {
                for (int i = 0; i < CachedList.Count; i++)
                {
                    string nodeLabel = CachedList[i].First;
                    PersonalityNode node = Nodes[nodeLabel];
                    node.Initialize();
                    node.cachedRating = -1f;
                    CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
                }
                if (PsychologyBase.ActivateKinsey())
                {
                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                    pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                    pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                    pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
                }
                PsycheCardUtility.ListTicker = 0;
                PsycheCardUtility.CloudTicker = 0;
            }
            TooltipHandler.TipRegion(resetRect, delegate
            {
                return "PsycheResetTooltip".Translate();
            }, 412832);

            if (Widgets.ButtonText(randomRect, RandomButtonText, true, false, true))
            {
                for (int i = 0; i < CachedList.Count; i++)
                {
                    string nodeLabel = CachedList[i].First;
                    PersonalityNode node = Nodes[nodeLabel];
                    node.rawRating = Rand.Value;
                    node.cachedRating = -1f;
                    CachedList[i] = new Pair<string, float>(nodeLabel, node.rawRating);
                }
                if (PsychologyBase.ActivateKinsey())
                {
                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(Mathf.CeilToInt(1000000f * Rand.Value));
                    pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                    pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                    pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
                }
                PsycheCardUtility.ListTicker = 0;
                PsycheCardUtility.CloudTicker = 0;
            }

            bool nodeBool = false;
            for (int i = 0; i < CachedList.Count; i++)
            {
                PersonalityNode node = Nodes[CachedList[i].First];
                if (node.rawRating != CachedList[i].Second)
                {
                    nodeBool = true;
                    node.rawRating = CachedList[i].Second;
                }
            }
            if (nodeBool)
            {
                foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
                {
                    node.cachedRating = -1f;
                }
                PsycheCardUtility.ListTicker = 0;
                PsycheCardUtility.CloudTicker = 0;
            }
            if (PsychologyBase.ActivateKinsey())
            {
                bool bool1 = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != pawnKinseyRating;
                bool bool2 = PsycheHelper.Comp(pawn).Sexuality.sexDrive != pawnSexDrive;
                bool bool3 = PsycheHelper.Comp(pawn).Sexuality.romanticDrive != pawnRomanticDrive;
                if (bool1 || bool2 || bool3)
                {
                    PsycheHelper.Comp(pawn).Sexuality.kinseyRating = pawnKinseyRating;
                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = pawnSexDrive;
                    PsycheHelper.Comp(pawn).Sexuality.romanticDrive = pawnRomanticDrive;
                    PsycheCardUtility.ListTicker = 0;
                    PsycheCardUtility.CloudTicker = 0;
                }
            }

            GUI.EndGroup();
        }
    }
}

