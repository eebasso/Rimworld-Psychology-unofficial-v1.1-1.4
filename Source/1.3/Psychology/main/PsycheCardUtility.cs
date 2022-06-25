using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//using System.Text;
//using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Psychology
{
    public class PsycheCardUtility
    {

        private static string[] NodeDescriptions = { "Not", "Slightly", "Less", "Somewhat", "More", "Very", "Extremely" };
        //private static Color[] NodeColors = { new Color(1f, 0.2f, 0.2f, 0.6f), new Color(1f, 0.4f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.6f, 0.2f), new Color(0.8f, 0.8f, 0.8f, 0.2f), new Color(0.6f, 1f, 0.6f, 0.2f), new Color(0.4f, 1f, 0.4f, 0.4f), new Color(0.2f, 1f, 0.2f, 0.6f) };
        private static Color[] NodeColors = { new Color(1f, 0.2f, 0.2f, 0.8f), new Color(1f, 0.4f, 0.4f, 0.6f), new Color(1f, 0.6f, 0.6f, 0.4f), new Color(1f, 1f, 1f, 0.2f), new Color(0.6f, 1f, 0.6f, 0.4f), new Color(0.4f, 1f, 0.4f, 0.6f), new Color(0.2f, 1f, 0.2f, 0.8f) };
        private static Vector2 NodeScrollPosition = Vector2.zero;
        //private static List<Pair<string, int>> NodeStrings = new List<Pair<string, int>>();
        private static float Categories = 6f;
        private const float RowTopPadding = 2f;
        public const float Width = 630f;
        public static int distanceFromMiddle = 4;
        public static bool showWordCloudBool = false;
        private static List<Rect> NodeRects = new List<Rect>();
        private const float pointsOnSpiral = 500f;
        //private const float dalpha = Mathf.PI / 60f;
        private const float dalpha = 1f;
        //private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        //private const float RowLeftRightPadding = 5f;
        private static bool AlphabeticalBool = false;
        private static bool UseAntonymsBool = true;
        private static string[] CreativeLetters = { "C", "r", "e", "a", "t", "i", "v", "e" };
        //private static Color[] CreativeColors = { new Color(1f, 0f, 0f), new Color(1f, 0.5f, 0f), new Color(1f, 1f, 0f), new Color(0f, 1f, 0f), new Color(0f, 1f, 1f), new Color(0f, 0f, 1f), new Color(0.5f, 0f, 1f), new Color(1f, 0, 0.5f) };
        //private static float[] CreativeHues = { 355f, 25f, 55f, 115f, 175f, 215f, 275f, 295f };
        private static Vector3[] CreativeHSVs = { new Vector3(0f, 1f, 1f), new Vector3(30f, 1f, 1f), new Vector3(55f, 1f, 1f), new Vector3(120f, 0.9f, 0.9f), new Vector3(180f, 0.9f, 0.9f), new Vector3(240f, 0.6f, 1f), new Vector3(270f, 0.8f, 1f), new Vector3(300f, 0.8f, 0.9f) };

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn, bool notOnMenu)
        {
            //Log.Message("Start of DrawPsycheCard");
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                GUI.BeginGroup(totalRect);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;

                float sizeScaler = 1.025f;

                // Kinsey rating text
                Vector2 kinseyTextSize = Text.CalcSize("KinseyRating".Translate() + " 0");
                kinseyTextSize.x *= sizeScaler;
                kinseyTextSize.y *= sizeScaler;

                // Sex and romantic drive text
                List<string> sexDriveText = new List<string>();
                List<Vector2> sexDriveSize = new List<Vector2>();
                List<string> romanticDriveText = new List<string>();
                List<Vector2> romanticDriveSize = new List<Vector2>();
                for (int i = 0; i < 5; i++)
                {
                    sexDriveText.Add(("SexDrive" + i.ToString()).Translate());
                    sexDriveSize.Add(Text.CalcSize(sexDriveText[i]));
                    romanticDriveText.Add(("RomanticDrive" + i.ToString()).Translate());
                    romanticDriveSize.Add(Text.CalcSize(romanticDriveText[i]));
                }
                float sexDriveWidth = sizeScaler * Mathf.Max(sexDriveSize[0].x, sexDriveSize[1].x, sexDriveSize[2].x, sexDriveSize[3].x, sexDriveSize[4].x);
                float sexDriveHeight = sizeScaler * Mathf.Max(sexDriveSize[0].y, sexDriveSize[1].y, sexDriveSize[2].y, sexDriveSize[3].y, sexDriveSize[4].y);
                float romanticDriveWidth = sizeScaler * Mathf.Max(romanticDriveSize[0].x, romanticDriveSize[1].x, romanticDriveSize[2].x, romanticDriveSize[3].x, romanticDriveSize[4].x);
                float romanticDriveHeight = sizeScaler * Mathf.Max(romanticDriveSize[0].y, romanticDriveSize[1].y, romanticDriveSize[2].y, romanticDriveSize[3].y, romanticDriveSize[4].y);

                // Personality options text
                List<string> ShowDistanceText = new List<string>();
                List<Vector2> ShowDistanceSize = new List<Vector2>();
                for (int i = 0; i < 4; i++)
                {
                    ShowDistanceText.Add(("ShowDistanceText" + i.ToString()).Translate());
                    ShowDistanceSize.Add(Text.CalcSize(ShowDistanceText[i]));
                }
                ShowDistanceText.Add("WordCloud".Translate());
                ShowDistanceSize.Add(Text.CalcSize(ShowDistanceText[4]));

                float optionsWidth = 10f + Mathf.Max(ShowDistanceSize[0].x, ShowDistanceSize[1].x, ShowDistanceSize[2].x, ShowDistanceSize[3].x, ShowDistanceSize[4].x);
                float optionsHeight = 5f + Mathf.Max(ShowDistanceSize[0].y, ShowDistanceSize[1].y, ShowDistanceSize[2].y, ShowDistanceSize[3].y, ShowDistanceSize[4].y);

                string AlphabeticalText = "Alphabetical".Translate();
                Vector2 AlphabeticalSize = Text.CalcSize(AlphabeticalText);
                float alphabeticalWidth = AlphabeticalSize.x + 29f;
                float alphabeticalHeight = Mathf.Max(AlphabeticalSize.y, 29f);

                string UseAntonymsText = "UseAntonyms".Translate();
                Vector2 UseAntonymsSize = Text.CalcSize(UseAntonymsText);
                float useAntonymsWidth = UseAntonymsSize.x + 29f;
                float useAntonymsHeight = Mathf.Max(UseAntonymsSize.y, 29f);

                string EditPsycheText = "EditPsyche".Translate();
                Vector2 EditPsycheSize = Text.CalcSize(EditPsycheText);
                float editWidth = 10f + EditPsycheSize.x;
                float editHeight = 5f + EditPsycheSize.y;

                // Calculate rectangles for sexuality panel
                float sexualityWidth = Mathf.Max(kinseyTextSize.x, sexDriveWidth, romanticDriveWidth, optionsWidth, alphabeticalWidth, useAntonymsWidth);
                float sexualityHeight = kinseyTextSize.y + sexDriveHeight + romanticDriveHeight + optionsHeight + 5f * RowTopPadding;

                Rect kinseyRect = new Rect(totalRect.xMax - sexualityWidth - 10f, totalRect.y + 10f, sexualityWidth - 29f, kinseyTextSize.y);
                Rect sexDriveRect = new Rect(kinseyRect.x, kinseyRect.yMax + RowTopPadding, sexualityWidth, sexDriveHeight);
                Rect romanticDriveRect = new Rect(kinseyRect.x, sexDriveRect.yMax + RowTopPadding, sexualityWidth, romanticDriveHeight);

                // Calculate rectanges for display options
                Rect optionsRect = new Rect(kinseyRect.x, romanticDriveRect.yMax + RowTopPadding, optionsWidth, optionsHeight);
                Rect alphabeticalRect = new Rect(kinseyRect.x, optionsRect.yMax + 5f, alphabeticalWidth, alphabeticalHeight);
                Rect useAntonymsRect = new Rect(kinseyRect.x, alphabeticalRect.yMax + 5f, useAntonymsWidth, useAntonymsHeight);
                Rect editPsycheRect = new Rect(totalRect.xMax - editWidth - 10f, totalRect.yMax - editHeight - 10f, editWidth, editHeight);

                //Calculate personality rectange
                Rect personalityRect = totalRect;
                personalityRect.xMax = kinseyRect.x;
                personalityRect = personalityRect.ContractedBy(10f);

                // Draw the widgets for sexuality panel
                if (PsychologyBase.ActivateKinsey())
                {
                    string kinseyText = "KinseyRating".Translate() + " " + PsycheHelper.Comp(pawn).Sexuality.kinseyRating;

                    float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
                    float pawnRomDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
                    int sexDriveInt = (pawnSexDrive > 1f) ? 4 : (pawnSexDrive > 0.7f) ? 3 : ((pawnSexDrive > 0.4f) ? 2 : (pawnSexDrive > 0.1f) ? 1 : 0);
                    int romDriveInt = (pawnRomDrive > 1f) ? 4 : (pawnRomDrive > 0.7f) ? 3 : ((pawnRomDrive > 0.4f) ? 2 : (pawnRomDrive > 0.1f) ? 1 : 0);

                    //Kinsey rating
                    Widgets.Label(kinseyRect, kinseyText);
                    kinseyRect.xMin -= 3.5f;
                    Widgets.DrawHighlightIfMouseover(kinseyRect);
                    Func<String> KinseyString = delegate
                    {
                        return "KinseyDescription".Translate();
                    };
                    TooltipHandler.TipRegion(kinseyRect, KinseyString, 89140);

                    //Sex drive
                    Widgets.Label(sexDriveRect, sexDriveText[sexDriveInt]);
                    sexDriveRect.xMin -= 3.5f;
                    Widgets.DrawHighlightIfMouseover(sexDriveRect);
                    Func<String> SexDriveString = delegate
                    {
                        string sexString = "SexDriveDescription".Translate();
                        if (sexDriveInt == 0)
                        {
                            sexString += "\n\n" + "AsexualDescription".Translate();
                        }
                        if (Prefs.DevMode)
                        {
                            string rawRating = PsycheHelper.Comp(pawn).Sexuality.sexDrive.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                            string adjRating = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                            sexString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                        }
                        return sexString;
                    };
                    TooltipHandler.TipRegion(sexDriveRect, SexDriveString, 89141);

                    //Romantic drive
                    Widgets.Label(romanticDriveRect, romanticDriveText[romDriveInt]);
                    romanticDriveRect.xMin -= 3.5f;
                    Widgets.DrawHighlightIfMouseover(romanticDriveRect);
                    Func<String> RomanticDriveString = delegate
                    {
                        string romString = "RomanticDriveDescription".Translate();
                        if (romDriveInt == 0)
                        {
                            romString += "\n\n" + "AromanticDescription".Translate();
                        }
                        if (Prefs.DevMode)
                        {
                            string rawRating = PsycheHelper.Comp(pawn).Sexuality.romanticDrive.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                            string adjRating = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                            romString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                        }
                        return romString;
                    };
                    TooltipHandler.TipRegion(romanticDriveRect, RomanticDriveString, 89141);

                }
                else if (notOnMenu)
                {
                    Rect disabledRect = new Rect(kinseyRect.x, kinseyRect.y, sexualityWidth, romanticDriveRect.yMax - kinseyRect.y);
                    GUI.color = Color.red;
                    Widgets.Label(disabledRect, "SexualityDisabledWarning".Translate());
                    GUI.color = Color.white;
                }

                if (Widgets.ButtonText(optionsRect, ShowDistanceText[distanceFromMiddle], drawBackground: true, doMouseoverSound: true, true))
                {
                    //Log.Message("Inside if(Widgets.ButtonTest(...))");
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption(ShowDistanceText[3], delegate
                    {
                        distanceFromMiddle = 3;
                    }));
                    list.Add(new FloatMenuOption(ShowDistanceText[2], delegate
                    {
                        distanceFromMiddle = 2;
                    }));
                    list.Add(new FloatMenuOption(ShowDistanceText[1], delegate
                    {
                        distanceFromMiddle = 1;
                    }));
                    list.Add(new FloatMenuOption(ShowDistanceText[0], delegate
                    {
                        distanceFromMiddle = 0;
                    }));
                    list.Add(new FloatMenuOption(ShowDistanceText[4], delegate
                    {
                        distanceFromMiddle = 4;
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                //Log.Message("Draw EditPsyche button");
                //Edit Psyche button
                if (Prefs.DevMode)
                {
                    if (Widgets.ButtonText(editPsycheRect, "EditPsyche".Translate(), true, false, true))
                    {
                        Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
                    }
                }

                Rect forbiddenRect = new Rect(personalityRect.xMax, 0f, totalRect.width - personalityRect.xMax, optionsRect.yMax + 10f);
                //if (showWordCloudBool)
                if (distanceFromMiddle == 4)
                {
                    //Log.Message("Draw word cloud");
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Widgets.DrawLineVertical(forbiddenRect.x, forbiddenRect.y, forbiddenRect.height);
                    Widgets.DrawLineHorizontal(forbiddenRect.x, forbiddenRect.yMax, forbiddenRect.width);
                    GUI.color = Color.white;
                    Vector2 cloudCenter = totalRect.center;
                    cloudCenter.x = 0.5f * totalRect.center.x + 0.5f * personalityRect.center.x;
                    PersonalityWordCloud(totalRect, cloudCenter, forbiddenRect, pawn, editPsycheRect);
                }
                else
                {
                    //Log.Message("Draw personality node list");
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Widgets.DrawLineVertical(forbiddenRect.x, totalRect.y, totalRect.height);
                    GUI.color = Color.white;
                    Widgets.CheckboxLabeled(alphabeticalRect, AlphabeticalText, ref AlphabeticalBool);
                    Widgets.CheckboxLabeled(useAntonymsRect, UseAntonymsText, ref UseAntonymsBool);
                    DrawPersonalityNodes(personalityRect, pawn);
                }
                GUI.EndGroup();
            }
        }

        [LogPerformance]
        public static void DrawPersonalityNodes(Rect personalityRect, Pawn pawn)
        {
            Text.Font = GameFont.Small;
            GUIStyle style = Text.fontStyles[1];
            TextAnchor oldAlignment = style.alignment;
            int oldFontSize = style.fontSize;
            TextAnchor newAlignment = TextAnchor.MiddleRight;
            int newFontSize = 15;
            float categoryWidth = 0f;
            float categoryNodeHeight = 0f;
            float nodeWidth = 0f;
            style.alignment = newAlignment;
            style.fontSize = newFontSize;
            foreach (string nodeDescription in NodeDescriptions)
            {
                string categoryText = ("Psyche" + nodeDescription).Translate();
                Vector2 categoryTextSize = Text.CalcSize(categoryText);
                categoryWidth = Mathf.Max(categoryWidth, 1.05f * categoryTextSize.x);
                categoryNodeHeight = Mathf.Max(categoryNodeHeight, categoryTextSize.y);
            }
            var labelNodeList = new List<Tuple<string, float, PersonalityNode>>();
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                float rating = node.AdjustedRating;
                string labelText = node.def.label;
                string antoText = node.def.oppositeName;
                Vector2 labelSize = Text.CalcSize(labelText);
                Vector2 antoSize = Text.CalcSize(antoText);
                nodeWidth = Mathf.Max(nodeWidth, 1.05f * labelSize.x, 1.05f * antoSize.x);
                categoryNodeHeight = Mathf.Max(categoryNodeHeight, labelSize.y, antoSize.y);
                if (UseAntonymsBool)
                {
                    string nodeText = rating > 0.5f ? node.def.label : node.def.oppositeName;
                    labelNodeList.Add(new Tuple<string, float, PersonalityNode>(nodeText, 0.5f + Mathf.Abs(rating - 0.5f), node));
                }
                else
                {
                    labelNodeList.Add(new Tuple<string, float, PersonalityNode>(node.def.label, node.AdjustedRating, node));
                }
            }
            style.alignment = oldAlignment;
            style.fontSize = oldFontSize;
            if (AlphabeticalBool)
            {
                labelNodeList = labelNodeList.OrderBy(tup => tup.Item1).ToList();
            }
            else
            {
                labelNodeList = labelNodeList.OrderBy(tup => -tup.Item2).ToList();
            }
            List<int> personalityIndexList = new List<int>();
            List<int> categoryIndexList = new List<int>();
            List<string> categoryTextList = new List<string>();

            float viewRectHeight = 0f;
            for (int i = 0; i < labelNodeList.Count(); i++)
            {
                float yAxis = 0.5f - labelNodeList[i].Item2;
                float weight = Mathf.Sqrt(2f * Mathf.Abs(yAxis));
                int category = Mathf.RoundToInt(Categories * (0.5f - yAxis * weight));
                if (Mathf.Abs(category - 3) >= distanceFromMiddle)
                {
                    personalityIndexList.Add(i);
                    categoryIndexList.Add(category);
                    categoryTextList.Add(("Psyche" + NodeDescriptions[category]).Translate());
                    viewRectHeight += 1.00f;
                }
            }
            //style.alignment = oldAlignment;
            //style.fontSize = oldFontSize;

            Rect viewRect = new Rect(0f, 0f, 0.9f * personalityRect.width, categoryNodeHeight * viewRectHeight + 20f);
            //Rect viewRect = new Rect(personalityRect.x, personalityRect.y, 0.9f * personalityRect.width, (categoryNodeHeight + RowTopPadding) * categoryIndexList.Count);
            Widgets.BeginScrollView(personalityRect, ref NodeScrollPosition, viewRect);
            float categoryNodeVerticalPosition = personalityRect.y;
            for (int j = 0; j < categoryIndexList.Count; j++)
            {
                string nodeText = labelNodeList[personalityIndexList[j]].Item1;
                float rating = labelNodeList[personalityIndexList[j]].Item2;
                PersonalityNode node = labelNodeList[personalityIndexList[j]].Item3;
                int category = categoryIndexList[j];
                string categoryText = categoryTextList[j];
                float categoryRectX = viewRect.center.x - 0.75f * categoryWidth - 0.5f * nodeWidth;
                Rect categoryRect = new Rect(categoryRectX, categoryNodeVerticalPosition, categoryWidth, categoryNodeHeight);
                Rect nodeRect = new Rect(categoryRect.xMax + 0.5f * categoryWidth, categoryNodeVerticalPosition, nodeWidth, categoryNodeHeight);
                //nodeRect.xMax = viewRect.xMax;
                style.alignment = newAlignment;
                style.fontSize = newFontSize;
                GUI.color = NodeColors[category];
                Widgets.Label(categoryRect, categoryText);
                GUI.color = Color.white;
                Widgets.Label(nodeRect, nodeText.CapitalizeFirst());
                style.alignment = oldAlignment;
                style.fontSize = oldFontSize;

                Rect highlightRect = categoryRect;
                highlightRect.xMin = viewRect.xMin;
                highlightRect.xMax = viewRect.xMax;
                DrawHighlightAndTooltip(highlightRect, node, j);
                categoryNodeVerticalPosition += categoryNodeHeight;
            }
            Widgets.EndScrollView();
        }

        public static void PersonalityWordCloud(Rect totalRect, Vector2 Center, Rect forbiddenRect, Pawn pawn, Rect editPsycheRect)
        {
            Rect cloudRect = totalRect.ContractedBy(10f);
            Rect forbiddenRect1 = forbiddenRect.ExpandedBy(10f);
            Rect forbiddenRect2 = editPsycheRect.ExpandedBy(10f);
            //Vector2 Center = personalityRect.center;
            List<PersonalityNode> allNodes = (from n in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes
                                              orderby Mathf.Abs(n.AdjustedRating - 0.5f) descending, n.def.defName
                                              select n).ToList();
            NodeRects.Clear();
            Text.Font = GameFont.Medium;
            GUIStyle style = Text.fontStyles[2];
            TextAnchor oldAlignment = style.alignment;
            int oldFontSize = style.fontSize;
            style.alignment = TextAnchor.MiddleCenter;
            Rect textRect;
            List<float> CreativeLetterWidths = new List<float>();
            for (int i = 0; i < allNodes.Count(); i++)
            {
                PersonalityNode node = allNodes[i];
                float displacement = node.AdjustedRating - 0.5f;
                style.fontSize = Mathf.FloorToInt(60f * Mathf.Abs(displacement)) + 5;
                GUI.color = RatingColor(node.def, displacement);
                string personalityText = (displacement < 0 ? node.def.oppositeName : node.def.label).CapitalizeFirst();
                Vector2 textSize = Text.CalcSize(personalityText);
                if (personalityText == "Creative")
                {
                    foreach (string letter in CreativeLetters)
                    {
                        CreativeLetterWidths.Add(Text.CalcSize(letter).x);
                    }
                    textSize.x = CreativeLetterWidths.Sum();
                }
                textSize.x *= 1.025f;
                float alpha = textSize.x * textSize.y;
                for (float pointIndex = 0f; pointIndex < pointsOnSpiral; pointIndex++)
                {
                    float dx = pointIndex / pointsOnSpiral * Mathf.Cos(alpha) * cloudRect.width - textSize.x;
                    float dy = pointIndex / pointsOnSpiral * Mathf.Sin(alpha) * cloudRect.height - textSize.y;
                    textRect = new Rect(Center.x + 0.5f * dx, Center.y + 0.5f * dy, textSize.x, textSize.y);
                    Rect tightRect = textRect;
                    tightRect.y = textRect.y + 0.12f * textRect.height;
                    tightRect.yMax = textRect.yMax - 0.035f * textRect.height;
                    if (Prefs.DevMode ? RectDoesNotOverlapWordCloud(cloudRect, tightRect, forbiddenRect1, forbiddenRect2) : RectDoesNotOverlapWordCloud(cloudRect, tightRect, forbiddenRect1))
                    {
                        Rect highlightRect = tightRect;
                        highlightRect.xMin = textRect.x - 0.0125f * textRect.width;
                        //highlightRect.xMax = textRect.xMax + 0.0125f * textRect.width;
                        //GUI.Label(textRect, content, style);
                        if (personalityText == "Creative")
                        {
                            float CreativeX = textRect.x;
                            for (int c = 0; c < CreativeLetters.Count(); c++)
                            {
                                //GUI.color = HSVtoColor(new Vector3(CreativeHues[c], Mathf.Lerp(0.25f, 1f, 2.2f * displacement), Mathf.Lerp(0.75f, 1f, 2.2f * displacement)));
                                //GUI.color = HSVtoColor(new Vector3(CreativeHues[c], 1f, 1f));
                                GUI.color = HSVtoColor(CreativeHSVs[c]);
                                Widgets.Label(new Rect(CreativeX, textRect.y, CreativeLetterWidths[c], textRect.height), CreativeLetters[c]);
                                CreativeX += CreativeLetterWidths[c];
                            }
                        }
                        else
                        {
                            Widgets.Label(textRect, personalityText);
                        }
                        GUI.color = Color.white;
                        DrawHighlightAndTooltip(highlightRect, node, i);
                        NodeRects.Add(tightRect);
                        break;
                    }
                    alpha += dalpha;
                }
            }
            GUI.color = Color.white;
            style.alignment = oldAlignment;
            style.fontSize = oldFontSize;
            Text.Font = GameFont.Small;
        }

        private static bool RectDoesNotOverlapWordCloud(Rect totalRect, Rect foundRectangle, Rect forbiddenRect1, Rect forbiddenRect2)
        {
            if (foundRectangle.Overlaps(forbiddenRect2))
            {
                return false;
            }
            return RectDoesNotOverlapWordCloud(totalRect, foundRectangle, forbiddenRect1);
        }

        private static bool RectDoesNotOverlapWordCloud(Rect totalRect, Rect foundRectangle, Rect forbiddenRect)
        {
            if (foundRectangle.Overlaps(forbiddenRect))
            {
                return false;
            }
            foreach (Rect rect in NodeRects)
            {
                if (foundRectangle.Overlaps(rect))
                {
                    return false;
                }
            }

            if (foundRectangle.x < totalRect.x || totalRect.xMax < foundRectangle.xMax || foundRectangle.y < totalRect.y || totalRect.yMax < foundRectangle.yMax)
            {
                return false;
            }
            return true;
        }

        public static void DrawHighlightAndTooltip(Rect highlightRect, PersonalityNode node, int i)
        {
            Widgets.DrawHighlightIfMouseover(highlightRect);
            Func<String> descriptionString = delegate
            {
                string nodeName = "";
                Color nodeColor = HSVtoColor(node.def.nodeHSV);
                Color oppositeColor = HSVtoColor(node.def.oppositeHSV);
                if (node.def.label == "creative")
                {
                    for (int c = 0; c < CreativeLetters.Count(); c++)
                    {
                        //nodeName += CreativeLetters[c].Colorize(HSVtoColor(new Vector3(CreativeHues[c], 1f, 1f)));
                        nodeName += CreativeLetters[c].Colorize(HSVtoColor(CreativeHSVs[c]));
                    }
                }
                else
                {
                    nodeName = node.def.descriptionLabel.Colorize(nodeColor);
                }
                nodeName = "<b><i>" + nodeName + "</i></b>";
                string tooltipString = node.def.description.ReplaceFirst("{0}", nodeName);
                string antonymString = " " +  "AntonymDescription".Translate() + " ";
                tooltipString += antonymString + "<b><i>" + node.def.oppositeName.Colorize(oppositeColor) + "</i></b>";
                if (node.def.conversationTopics != null)
                {
                    string convoString = "\n\n" + "ConversationTooltip".Translate(string.Join("PsycheComma".Translate(), node.def.conversationTopics.Take(node.def.conversationTopics.Count - 1).ToArray()), node.def.conversationTopics.Last());
                    tooltipString += convoString;
                }
                if (Prefs.DevMode && Prefs.LogVerbose)
                {
                    string rawRating = node.rawRating.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                    string adjRating = node.AdjustedRating.ToString("##0.000%", CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                    tooltipString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                }
                return tooltipString;
            };
            TooltipHandler.TipRegion(highlightRect, descriptionString, 613261 + i * 612);
        }

        public static Color RatingColor(PersonalityNodeDef def, float x)
        {
            Vector3 HSV1 = def.nodeHSV;
            Vector3 HSV2 = def.oppositeHSV;
            //float H1 = HSV1.x;
            //float H2 = HSV2.x;
            //if (H1 < 180f)
            //{
            //    H2 += H1 + 180f < H2 ? -360f : 0f;
            //}
            //else
            //{
            //    H2 += H2 < H1 - 180f ? 360f : 0f;
            //}
            //float cutoff = 0.05f;
            //float r = 0.5f + (x / cutoff);
            //float H = PsycheHelper.Mod(Mathf.Lerp(H2, H1, r), 360f);
            //float S = Mathf.Lerp(0.5f, 0 < x ? HSV2.y : HSV1.y, Mathf.Abs(2f * x / 0.05f));
            //float V = Mathf.Lerp(0.5f, 0 < x ? HSV2.z : HSV1.z, Mathf.Abs(2f * x / 0.05f));
            //float S = Mathf.Lerp(HSV2.y, HSV1.y, r);
            //float V = Mathf.Lerp(HSV2.z, HSV1.z, r);
            //return HSVtoColor(new Vector3(H, S, V));
            return HSVtoColor(x < 0 ? HSV2 : HSV1);
        }

        public static Color HSVtoColor(Vector3 HSV)
        {
            float H = HSV.x;
            float S = HSV.y;
            float V = HSV.z;
            float m = V * (1f - S);
            float z = V * S * (1f - Mathf.Abs((H / 60f % 2f) - 1f)) + m;
            if (0f <= H && H < 60f)
            {
                return new Color(V, z, m);
            }
            if (60f <= H && H < 120f)
            {
                return new Color(z, V, m);
            }
            if (120f <= H && H < 180f)
            {
                return new Color(m, V, z);
            }
            if (180f <= H && H < 240f)
            {
                return new Color(m, z, V);
            }
            if (240f <= H && H < 300f)
            {
                return new Color(z, m, V);
            }
            else
            {
                return new Color(V, m, z);
            }
        }
    }
}
