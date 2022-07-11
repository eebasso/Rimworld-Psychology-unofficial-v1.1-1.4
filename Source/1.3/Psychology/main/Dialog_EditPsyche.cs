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
        private float nodeWidth = 0f;
        private float labelHeight = 0f;
        private string kinseyRatingText = "KinseyRating".Translate();
        private string sexDriveText = "SexDrive".Translate();
        private string romDriveText = "RomanticDrive".Translate();

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
                sexualityWidth = Mathf.Max(kinseySize.x, sexDriveSize.x, romDriveSize.x);
            }
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                string nodeLabel = node.def.label.CapitalizeFirst();
                Vector2 nodeSize = Text.CalcSize(nodeLabel);
                nodeWidth = Mathf.Max(nodeWidth, 1.05f * nodeSize.x);
                labelHeight = Mathf.Max(labelHeight, nodeSize.y);
                cachedList.Add(new Pair<string, float>(nodeLabel, node.rawRating));
                string descriptionString = node.def.description.ReplaceFirst("{0}", node.def.descriptionLabel) + " " + "AntonymColon".Translate() + " " + node.def.oppositeName;
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
            Rect mainRect = inRect.ContractedBy(17f);
            mainRect.height -= 20f;

            Rect okRect = new Rect(0.25f * inRect.width - 20f, mainRect.yMax + 10f, 0.25f * inRect.width, 30f);
            Rect cancelRect = new Rect(okRect.xMax + 40f, okRect.y, okRect.width, okRect.height);

            Text.Font = GameFont.Medium;
            if (pawn.LabelShort != null || pawn.LabelShort != "")
            {
                Widgets.Label(mainRect, "PsycheEditor".Translate(pawn.LabelShortCap));
            }
            else
            {
                Widgets.Label(mainRect, "PsycheEditorNewColonist".Translate());
            }
            mainRect.yMin += 35f;
            Text.Font = GameFont.Small;

            string warningText = "PersonalityNodeWarning".Translate();
            Widgets.Label(mainRect, warningText);
            mainRect.yMin += Text.CalcHeight(warningText, mainRect.width);

            Rect nodeRect = new Rect(mainRect.x, mainRect.y, mainRect.width, mainRect.height - 3 * labelHeight - 20f);
            Rect viewRect = new Rect(0f, 0f, nodeRect.width - 20f, cachedList.Count * labelHeight + 20f);
            Widgets.BeginScrollView(nodeRect, ref nodeScrollPosition, viewRect);
            float num3 = 0f;
            for (int i = 0; i < cachedList.Count; i++)
            {
                string label = cachedList[i].First;
                Rect rect = new Rect(10f, num3, nodeWidth, labelHeight);
                Rect rect2 = new Rect(rect.xMax, num3, viewRect.xMax - rect.xMax - 10f, labelHeight);
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.Label(rect, label);
                TooltipHandler.TipRegion(rect, () => descriptions[label], 436532 + Mathf.RoundToInt(num3));
                float newVal = Widgets.HorizontalSlider(rect2, cachedList[i].Second, 0f, 1f, true);
                cachedList[i] = new Pair<string, float>(cachedList[i].First, newVal);
                num3 += labelHeight;
            }
            Widgets.EndScrollView();

            if (PsychologyBase.ActivateKinsey())
            {
                float x1 = mainRect.x;
                float width1 = nodeWidth;
                float x2 = x1 + width1;
                float width2 = mainRect.xMax - x2;
                float y1 = nodeRect.yMax + 10f;
                float y2 = y1 + labelHeight;
                float y3 = y2 + labelHeight;

                Rect kinseyRatingLabelRect = new Rect(x1, y1, width1, labelHeight);
                Rect kinseyRatingSliderRect = new Rect(x2, y1, width2, labelHeight);
                Widgets.Label(kinseyRatingLabelRect, kinseyRatingText);
                pawnKinseyRating = Mathf.RoundToInt(Widgets.HorizontalSlider(kinseyRatingSliderRect, pawnKinseyRating, 0f, 6f, true, leftAlignedLabel: "0", rightAlignedLabel: "6"));

                Rect sexDriveLabelRect = new Rect(x1, y2, width1, labelHeight);
                Rect sexDriveSliderRect = new Rect(x2, y2, width2, labelHeight);
                Widgets.Label(sexDriveLabelRect, sexDriveText);
                pawnSexDrive = Widgets.HorizontalSlider(sexDriveSliderRect, pawnSexDrive, 0f, 1f, true);

                Rect romDriveLabelRect = new Rect(x1, y3, width1, labelHeight);
                Rect romDriveSliderRect = new Rect(x2, y3, width2, labelHeight);
                Widgets.Label(romDriveLabelRect, romDriveText);
                pawnRomanticDrive = Widgets.HorizontalSlider(romDriveSliderRect, pawnRomanticDrive, 0f, 1f, true);
            }
            else
            {
                GUI.color = Color.red;
                Rect warningRect = new Rect(mainRect.x, nodeRect.yMax + 10f, mainRect.width, labelHeight * 3);
                Widgets.Label(warningRect, "SexualityDisabledWarning".Translate());
                GUI.color = Color.white;
            }

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawLineHorizontal(mainRect.x, nodeRect.yMax, nodeRect.width);
            GUI.color = Color.white;

            if (Widgets.ButtonText(okRect, "AcceptButton".Translate(), true, false, true) || flag)
            {
                PsycheCardUtility.CloudTicker = PsycheCardUtility.MaxTicks;
                PsycheCardUtility.ListTicker = PsycheCardUtility.MaxTicks;
                foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
                {
                    node.rawRating = (from n in cachedList
                                      where n.First == node.def.label.CapitalizeFirst()
                                      select n).First().Second;
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
