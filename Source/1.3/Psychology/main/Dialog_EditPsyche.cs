using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Psychology
{
    public class Dialog_EditPsyche : Window
    {
        private Pawn pawn;
        private static Vector2 nodeScrollPosition = Vector2.zero;
        private List<Pair<string, float>> cachedList = new List<Pair<string, float>>();
        private Dictionary<string, string> descriptions = new Dictionary<string, string>();
        private int pawnKinseyRating = 0;
        private float pawnSexDrive = 0f;
        private float pawnRomanticDrive = 0f;
        private float sexualityWidth = 0f;
        private float sexualityHeight = 0f;
        private float nodeWidth = 0f;
        private float labelHeight = 0f;
        private string kinseyRatingText = "KinseyRating".Translate();
        private string sexDriveText = "SexDrive".Translate();
        private string romDriveText = "RomanticDrive".Translate();
        private float BoundaryPadding = PsycheCardUtility.BoundaryPadding;
        private float HighlightPadding = PsycheCardUtility.HighlightPadding;

        public Dialog_EditPsyche(Pawn editFor)
        {
            pawn = editFor;
            Text.Font = GameFont.Small;
            if (PsychologyBase.ActivateKinsey())
            {
                pawnKinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.sexDrive;
                pawnRomanticDrive = PsycheHelper.Comp(pawn).Sexuality.romanticDrive;
                Vector2 kinseySize = Text.CalcSize(kinseyRatingText);
                Vector2 sexDriveSize = Text.CalcSize(sexDriveText);
                Vector2 romDriveSize = Text.CalcSize(romDriveText);
                sexualityWidth = 1.025f * Mathf.Max(kinseySize.x, sexDriveSize.x, romDriveSize.x);
                sexualityHeight = 1.025f * Mathf.Max(kinseySize.y, sexDriveSize.y, romDriveSize.y);
            }
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                string nodeLabel = node.def.label.CapitalizeFirst();
                Vector2 nodeSize = Text.CalcSize(nodeLabel);
                nodeWidth = Mathf.Max(nodeWidth, 1.05f * nodeSize.x);
                labelHeight = Mathf.Max(labelHeight, nodeSize.y);
                cachedList.Add(new Pair<string, float>(nodeLabel, node.rawRating));
                string descriptionString = node.def.description.ReplaceFirst("{0}", node.def.descriptionLabel) + " " + "AntonymColon".Translate() + " " + node.def.antonymLabel;
                descriptions.Add(nodeLabel, descriptionString);
                //try
                //{
                //    descriptions.Add(node.def.label.CapitalizeFirst(), node.def.description);
                //}
                //catch(ArgumentException e)
                //{
                //    Log.Error("[Psychology] "+"DuplicateDefLabel".Translate(node.def.defName));
                //    descriptions.Add(node.def.defName.CapitalizeFirst(), node.def.description);
                //}
            }
            sexualityWidth = Mathf.Max(26f, sexualityWidth);
            labelHeight = Mathf.Max(26f, labelHeight);
            cachedList.SortBy(n => n.First);
        }

        [LogPerformance]
        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            bool flag2 = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                flag2 = true;
                Event.current.Use();
            }
            Rect mainRect = inRect.ContractedBy(BoundaryPadding);

            Text.Font = GameFont.Medium;
            string titleText = (pawn.LabelShort != null || pawn.LabelShort != "") ? "PsycheEditor".Translate(pawn.LabelShortCap) : "PsycheEditorNewColonist".Translate();
            Widgets.Label(mainRect, titleText);
            mainRect.yMin += Text.CalcHeight(titleText, mainRect.width);
            Text.Font = GameFont.Small;

            string warningText = "PersonalityNodeWarning".Translate();
            Widgets.Label(mainRect, warningText);
            mainRect.yMin += Text.CalcHeight(warningText, mainRect.width);

            float x0 = mainRect.x + HighlightPadding;
            float width0 = sexualityWidth;
            float x1 = x0 + width0 + BoundaryPadding;
            float width1 = mainRect.xMax - HighlightPadding - x1;
            float y0 = mainRect.y + 10f;
            float y1 = y0 + sexualityHeight;
            float y2 = y1 + sexualityHeight;
            float y3 = y2 + sexualityHeight + BoundaryPadding;
            if (PsychologyBase.ActivateKinsey())
            {
                Rect kinseyRatingLabelRect = new Rect(x0, y0, width0, sexualityHeight);
                Rect kinseyRatingSliderRect = new Rect(x1, y0, width1, sexualityHeight);
                Rect sexDriveLabelRect = new Rect(x0, y1, width0, sexualityHeight);
                Rect sexDriveSliderRect = new Rect(x1, y1, width1, sexualityHeight);
                Rect romDriveLabelRect = new Rect(x0, y2, width0, sexualityHeight);
                Rect romDriveSliderRect = new Rect(x1, y2, width1, sexualityHeight);

                TextAnchor oldAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(kinseyRatingLabelRect, kinseyRatingText);
                Widgets.Label(sexDriveLabelRect, sexDriveText);
                Widgets.Label(romDriveLabelRect, romDriveText);
                Text.Anchor = oldAnchor;

                pawnKinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(kinseyRatingSliderRect, pawnKinseyRating, 0f, 6f, true, leftAlignedLabel: "0", rightAlignedLabel: "6"));
                pawnSexDrive = Widgets.HorizontalSlider(sexDriveSliderRect, pawnSexDrive, 0f, 1f, true);
                pawnRomanticDrive = Widgets.HorizontalSlider(romDriveSliderRect, pawnRomanticDrive, 0f, 1f, true);

                kinseyRatingLabelRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(kinseyRatingLabelRect);
                TooltipHandler.TipRegion(kinseyRatingLabelRect, delegate
                {
                    return "KinseyDescription".Translate();
                }, 14924);
                sexDriveLabelRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(sexDriveLabelRect);
                TooltipHandler.TipRegion(sexDriveLabelRect, delegate
                {   
                    return "SexDriveDescription".Translate();
                }, 14925);
                romDriveLabelRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(romDriveLabelRect);
                TooltipHandler.TipRegion(romDriveLabelRect, delegate
                {
                    return "RomanticDriveDescription".Translate();
                }, 14926);

            }
            else
            {
                GUI.color = Color.red;
                Rect warningRect = new Rect(x0, y0, mainRect.width, 3f * labelHeight);
                Widgets.Label(warningRect, "SexualityDisabledWarning".Translate());
                GUI.color = Color.white;
            }

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(x0, y3, mainRect.width);
            GUI.color = Color.white;

            Rect nodeRect = new Rect(x0, y3 + BoundaryPadding, mainRect.width, mainRect.yMax - y3 - BoundaryPadding - 60f);
            Rect viewRect = new Rect(0f, 0f, nodeRect.width - 20f, cachedList.Count * labelHeight + 5f);
            Widgets.BeginScrollView(nodeRect, ref nodeScrollPosition, viewRect);
            float num3 = 0f;
            for (int i = 0; i < cachedList.Count; i++)
            {
                string label = cachedList[i].First;
                Rect highlightRect = new Rect(BoundaryPadding + HighlightPadding, num3, nodeWidth, labelHeight);

                Widgets.Label(highlightRect, label);
                highlightRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(highlightRect);
                TooltipHandler.TipRegion(highlightRect, () => descriptions[label], 436532 + Mathf.RoundToInt(num3));

                Rect sliderRect = new Rect(highlightRect.xMax + BoundaryPadding, num3, viewRect.xMax - highlightRect.xMax - 2f * BoundaryPadding, labelHeight);
                float newVal = Widgets.HorizontalSlider(sliderRect, cachedList[i].Second, 0f, 1f, true);
                cachedList[i] = new Pair<string, float>(cachedList[i].First, newVal);
                num3 += labelHeight;
            }
            Widgets.EndScrollView();

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(x0, nodeRect.yMax + BoundaryPadding, mainRect.width);
            GUI.color = Color.white;

            Rect okRect = new Rect(0.25f * inRect.width - 20f, mainRect.yMax - 15f, 0.25f * inRect.width, 30f);
            Rect cancelRect = new Rect(okRect.xMax + 40f, okRect.y, okRect.width, okRect.height);

            if (Widgets.ButtonText(okRect, "AcceptButton".Translate(), true, false, true) || flag)
            {
                PsycheCardUtility.CloudTicker = 0;
                PsycheCardUtility.ListTicker = 0;
                Log.Message("Set CloudTicker and ListTicker to 0.");
                foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
                {
                    node.rawRating = (from n in cachedList
                                      where n.First == node.def.label.CapitalizeFirst()
                                      select n).First().Second;
                    node.cachedRating = -1f;
                }
                if (PsychologyBase.ActivateKinsey())
                {
                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = pawnSexDrive;
                    PsycheHelper.Comp(pawn).Sexuality.romanticDrive = pawnRomanticDrive;
                    PsycheHelper.Comp(pawn).Sexuality.kinseyRating = pawnKinseyRating;
                }
                this.Close(false);
            }
            if (Widgets.ButtonText(cancelRect, "CancelButton".Translate(), true, false, true) || flag2)
            {
                this.Close(true);
            }
        }
    }
}
