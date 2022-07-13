using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;

namespace Psychology
{
    [StaticConstructorOnStartup]
    public class PsycheCardUtility
    {
        public const float RowTopPadding = 0.5f;
        public const float BoundaryPadding = 7f;
        public const float HighlightPadding = 3.5f;

        //private static TextAnchor OldSmallAlignment = Text.fontStyles[1].alignment;
        //private static TextAnchor OldMediumAlignment = Text.fontStyles[2].alignment;
        //private static TextAnchor NewSmallAlignment = TextAnchor.MiddleLeft;
        //private static TextAnchor NewMediumAlignment = TextAnchor.MiddleCenter;
        private static int OldSmallFontSize = Text.fontStyles[1].fontSize;
        private static int OldMediumFontSize = Text.fontStyles[2].fontSize;
        private static byte SexualityFontSize = 16;
        private static byte OptionsFontSize = 16;
        private static byte FiveFactorTitleFontSize = 19;
        private static byte CategoryNodeFontSize = 16;

        private static Color TitleColor = ColoredText.TipSectionTitleColor;
        private static Color RatingColor = ColoredText.TipSectionTitleColor;
        private static string RatingFormat = "##0.000%";

        public static byte DistanceFromMiddle = 4;
        public static byte DistanceFromMiddleCached = 4;
        public static bool UseColorsBool = true;
        public static bool UseColorsBoolCached = true;
        public static bool AlphabeticalBool = false;
        public static bool AlphabeticalBoolCached = false;
        public static bool UseAntonymsBool = true;
        public static bool UseAntonymsBoolCached = true;

        private static Pawn PawnCached;
        public const int MaxTicks = 1000;
        public static int ListTicker = 0;
        public static int CloudTicker = 0;

        private static Vector2 KinseyTextSize;
        private static List<string> SexDriveText = new List<string>();
        private static List<Vector2> SexDriveSize = new List<Vector2>();
        private static float SexDriveWidth;
        private static float SexDriveHeight;
        private static List<string> RomDriveText = new List<string>();
        private static List<Vector2> RomDriveSize = new List<Vector2>();
        private static float RomDriveWidth;
        private static float RomDriveHeight;

        private static List<string> OptionsText = new List<string>();
        private static List<Vector2> OptionsSize = new List<Vector2>();
        private static float OptionsWidth;
        private static float OptionsHeight;

        private static string UseColorsText = "UseColors".Translate();
        private static Vector2 UseColorsSize;
        private static float UseColorsWidth;
        private static float UseColorsHeight;

        private static string AlphabeticalText = "ListAlphabetical".Translate();
        private static Vector2 AlphabeticalSize;
        private static float AlphabeticalWidth;
        private static float AlphabeticalHeight;

        private static string UseAntonymsText = "UseAntonyms".Translate();
        private static Vector2 UseAntonymsSize;
        private static float UseAntonymsWidth;
        private static float UseAntonymsHeight;

        private static string FiveFactorText = "FiveFactorModelTitle".Translate();
        private static Vector2 FiveFactorSize;

        private static string EditText = "EditPsyche".Translate();
        private static Vector2 EditSize;
        private static float EditWidth;
        private static float EditHeight;

        private static float SexualityWidth;
        private static float KinseyWidth;

        private static readonly string[] BigFiveLetters = { "O", "C", "E", "A", "N" };
        private static readonly Vector2[] BigFiveSpacings = { new Vector2(0f, -1f), new Vector2(1f, -0.32492f), new Vector2(0.726543f, 1f), new Vector2(-0.726543f, 1f), new Vector2(-1f, -0.32492f) };
        private static readonly Vector3[] BigFiveHSVs = { new Vector3(125f, 1f, 0.95f), new Vector3(185f, 1f, 0.95f), new Vector3(55f, 1f, 0.95f), new Vector3(285f, 0.9f, 1f), new Vector3(345f, 1f, 0.95f) };
        private static readonly Color[] BigFiveColors = new Color[5];
        private const byte RadialCategories = 4;
        private const float SideScaling = 0.809017f;
        private static Texture2D PsycheLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
        private static Material PsycheYellowMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 0f, 0.4f));

        private const int Categories = 6;
        private static readonly string[] NodeDescriptions = { "Not", "Slightly", "Less", "Somewhat", "More", "Very", "Extremely" };
        private static readonly Color[] NodeColors = { new Color(1f, 0.2f, 0.2f, 0.8f), new Color(1f, 0.4f, 0.4f, 0.6f), new Color(1f, 0.6f, 0.6f, 0.4f), new Color(1f, 1f, 1f, 0.2f), new Color(0.6f, 1f, 0.6f, 0.4f), new Color(0.4f, 1f, 0.4f, 0.6f), new Color(0.2f, 1f, 0.2f, 0.8f) };
        private static Vector2 NodeScrollPosition = Vector2.zero;

        private static List<Rect> CloudTextRects = new List<Rect>();
        private static List<Rect> CloudTightRects = new List<Rect>();
        //private static List<Rect> CloudHighlightRects = new List<Rect>();
        private static List<string> CloudTexts = new List<string>();
        private static List<PersonalityNode> CloudNodes = new List<PersonalityNode>();
        private static List<int> CloudFontSizes = new List<int>();
        private const float PointsOnSpiral = 2000f;
        private const float kSpiral = 50f;
        private static float xMinTightScaling = 0.0125f;
        private static float xMaxTightScaling = 0.0125f;
        private static float yMinTightScaling = -0.135f;
        private static float yMaxTightScaling = -0.000f;

        private static readonly string[] CreativeLetters = { "C", "r", "e", "a", "t", "i", "v", "e" };
        private static readonly Vector3[] CreativeHSVs = { new Vector3(0f, 0.9f, 1f), new Vector3(30f, 1f, 1f), new Vector3(65f, 0.9f, 0.85f), new Vector3(125f, 0.9f, 0.85f), new Vector3(195f, 1f, 1f), new Vector3(210f, 1f, 1f), new Vector3(275f, 0.8f, 1f), new Vector3(305f, 1f, 0.85f) };
        private static string CreativeRainbow = "";

        public static bool NeedToCalculateNodeWidthBool = true;
        public static float NodeWidth = 0f;
        private static List<Tuple<string, PersonalityNode, string>> LabelNodeList = new List<Tuple<string, PersonalityNode, string>>();
        private static float CategoryWidth = 0f;
        private static float CategoryNodeHeight = 0f;
        //private static float ViewRectHeight = 0f;

        static PsycheCardUtility()
        {
            Text.Font = GameFont.Small;
            GUIStyle style = Text.fontStyles[1];
            Vector2 scalingVector = new Vector2(1.025f, 1.025f);
            for (int i = 0; i < 5; i++)
            {
                BigFiveColors[i] = HSVtoColor(BigFiveHSVs[i]);
            }

            //TextAnchor oldAnchor = Text.Anchor;
            //Text.Anchor = TextAnchor.MiddleRight;
            style.fontSize = SexualityFontSize;
            // Kinsey rating text
            KinseyTextSize = Text.CalcSize("KinseyRating".Translate() + " 0") * scalingVector;
            // Sex and romantic drive text
            for (int i = 0; i < 5; i++)
            {
                SexDriveText.Add(("SexDrive" + i.ToString()).Translate());
                RomDriveText.Add(("RomanticDrive" + i.ToString()).Translate());
                SexDriveSize.Add(Text.CalcSize(SexDriveText[i]) * scalingVector);
                RomDriveSize.Add(Text.CalcSize(RomDriveText[i]) * scalingVector);
            }
            SexDriveWidth = SexDriveSize.Max(s => s.x);
            SexDriveHeight = SexDriveSize.Max(s => s.y);
            RomDriveWidth = RomDriveSize.Max(s => s.x);
            RomDriveHeight = RomDriveSize.Max(s => s.y);
            style.fontSize = OldSmallFontSize;

            // Personality options text
            for (int i = 0; i < 5; i++)
            {
                OptionsText.Add(("OptionsText" + i.ToString()).Translate());
                OptionsSize.Add(Text.CalcSize(OptionsText[i]) * scalingVector);
            }
            OptionsWidth = 20f + OptionsSize.Max(s => s.x);
            OptionsHeight = 5f + OptionsSize.Max(s => s.y);

            UseColorsSize = Text.CalcSize(UseColorsText) * scalingVector;
            UseColorsWidth = UseColorsSize.x + 29f;
            UseColorsHeight = Mathf.Max(UseColorsSize.y, 24f);

            AlphabeticalSize = Text.CalcSize(AlphabeticalText) * scalingVector;
            AlphabeticalWidth = AlphabeticalSize.x + 29f;
            AlphabeticalHeight = Mathf.Max(AlphabeticalSize.y, 24f);

            UseAntonymsSize = Text.CalcSize(UseAntonymsText) * scalingVector;
            UseAntonymsWidth = UseAntonymsSize.x + 29f;
            UseAntonymsHeight = Mathf.Max(UseAntonymsSize.y, 24f);

            style.fontSize = FiveFactorTitleFontSize;
            //FiveFactorText = "<b>" + FiveFactorText + "</b>";
            FiveFactorSize = Text.CalcSize(FiveFactorText) * scalingVector;
            style.fontSize = OldSmallFontSize;

            EditSize = Text.CalcSize(EditText) * scalingVector;
            EditWidth = 20f + EditSize.x;
            EditHeight = 5f + EditSize.y;

            SexualityWidth = Mathf.Max(KinseyTextSize.x, SexDriveWidth, RomDriveWidth, OptionsWidth, UseColorsWidth, AlphabeticalWidth, UseAntonymsWidth, FiveFactorSize.x);
            KinseyWidth = SexualityWidth + BoundaryPadding - 27f;

            for (int c = 0; c < CreativeLetters.Count(); c++)
            {
                CreativeRainbow += CreativeLetters[c].Colorize(HSVtoColor(CreativeHSVs[c]));
            }

            foreach (string nodeDescription in NodeDescriptions)
            {
                string categoryText = ("Psyche" + nodeDescription).Translate();
                Vector2 categoryTextSize = Text.CalcSize(categoryText);
                CategoryWidth = Mathf.Max(CategoryWidth, 1.025f * categoryTextSize.x);
                CategoryNodeHeight = Mathf.Max(CategoryNodeHeight, categoryTextSize.y);
            }

            //Text.Anchor = oldAnchor;
        }

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn, bool notOnMenu)
        {
            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                return;
            }
            if (PawnCached == null)
            {
                PawnCached = pawn;
            }

            Rect kinseyRect = new Rect(totalRect.xMax - SexualityWidth - BoundaryPadding, totalRect.y + BoundaryPadding, KinseyWidth, KinseyTextSize.y);
            Rect sexDriveRect = new Rect(kinseyRect.x, kinseyRect.yMax + RowTopPadding, SexualityWidth, SexDriveHeight);
            Rect romDriveRect = new Rect(kinseyRect.x, sexDriveRect.yMax + RowTopPadding, SexualityWidth, RomDriveHeight);

            // Calculate rectanges for display options
            Rect optionsRect = new Rect(kinseyRect.x - HighlightPadding, romDriveRect.yMax + RowTopPadding, OptionsWidth, OptionsHeight);
            Rect useColorsRect = new Rect(kinseyRect.x, optionsRect.yMax + 8f, UseColorsWidth, UseColorsHeight);
            Rect alphabeticalRect = new Rect(kinseyRect.x, useColorsRect.yMax + 8f, AlphabeticalWidth, AlphabeticalHeight);
            Rect useAntonymsRect = new Rect(kinseyRect.x, alphabeticalRect.yMax + 8f, UseAntonymsWidth, UseAntonymsHeight);
            Rect editRect = new Rect(totalRect.xMax - EditWidth - BoundaryPadding, totalRect.yMax - EditHeight - BoundaryPadding, EditWidth, EditHeight);

            //Calculate personality rectangle
            Rect personalityRect = totalRect;
            personalityRect.xMax = kinseyRect.x - HighlightPadding;
            personalityRect = personalityRect.ContractedBy(BoundaryPadding);
            Rect forbiddenRect = new Rect(personalityRect.xMax, 0f, totalRect.width - personalityRect.xMax, useColorsRect.yMax + BoundaryPadding);
            Rect fiveFactorRect = new Rect(forbiddenRect.center.x - 0.5f * FiveFactorSize.x, useAntonymsRect.yMax + 2f * BoundaryPadding, FiveFactorSize.x, FiveFactorSize.y);

            GUI.BeginGroup(totalRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            GUIStyle style = Text.fontStyles[1];

            TextAnchor OldAnchor = Text.Anchor;
            
            // Draw the widgets for sexuality panel
            if (PsychologyBase.ActivateKinsey())
            {
                string kinseyText = "KinseyRating".Translate() + " " + PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
                float pawnRomDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
                int sexDriveInt = (pawnSexDrive > 1.3f) ? 4 : (pawnSexDrive > 0.85f) ? 3 : (pawnSexDrive > 0.5f) ? 2 : (pawnSexDrive > 0.1f) ? 1 : 0;
                int romDriveInt = (pawnRomDrive > 1.3f) ? 4 : (pawnRomDrive > 0.67f) ? 3 : (pawnRomDrive > 0.33f) ? 2 : (pawnRomDrive > 0.1f) ? 1 : 0;

                //Kinsey rating
                Text.Anchor = TextAnchor.MiddleLeft;
                style.fontSize = SexualityFontSize;
                Widgets.Label(kinseyRect, kinseyText);
                Widgets.Label(sexDriveRect, SexDriveText[sexDriveInt]);
                Widgets.Label(romDriveRect, RomDriveText[romDriveInt]);
                style.fontSize = OldSmallFontSize;
                Text.Anchor = OldAnchor;

                kinseyRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(kinseyRect);
                TooltipHandler.TipRegion(kinseyRect, delegate
                {
                    return ((string)"KinseyDescription".Translate()).ReplaceFirst("{0}", "KinseyDescription0".Translate().Colorize(TitleColor));
                }, 89140);

                //Sex drive

                sexDriveRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(sexDriveRect);
                TooltipHandler.TipRegion(sexDriveRect, delegate
                {
                    string sexString = ((string)"SexDriveDescription".Translate()).ReplaceFirst("{0}", "SexDriveDescription0".Translate().Colorize(TitleColor));
                    if (sexDriveInt == 0)
                    {
                        sexString += "\n\n" + (string)"AsexualDescription".Translate();
                    }
                    if (Prefs.DevMode)
                    {
                        string rawRating = PsycheHelper.Comp(pawn).Sexuality.sexDrive.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(RatingColor);
                        string adjRating = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(RatingColor);
                        sexString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                    }
                    return sexString;
                }, 89141);

                //Romantic drive

                romDriveRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(romDriveRect);
                TooltipHandler.TipRegion(romDriveRect, delegate
                {
                    string romString = ((string)"RomanticDriveDescription".Translate()).ReplaceFirst("{0}", "RomanticDriveDescription0".Translate().Colorize(TitleColor));
                    if (romDriveInt == 0)
                    {
                        romString += "\n\n" + (string)"AromanticDescription".Translate();
                    }
                    if (Prefs.DevMode)
                    {
                        string rawRating = PsycheHelper.Comp(pawn).Sexuality.romanticDrive.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(RatingColor);
                        string adjRating = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(RatingColor);
                        romString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                    }
                    return romString;
                }, 89142);

            }
            else if (notOnMenu)
            {
                Rect disabledRect = new Rect(kinseyRect.x, kinseyRect.y, kinseyRect.width, romDriveRect.yMax - kinseyRect.y);
                //style.fontSize = SexualityFontSize;
                GUI.color = Color.red;
                Widgets.Label(disabledRect, "SexualityDisabledWarning".Translate());
                GUI.color = Color.white;
                //style.fontSize = OldSmallFontSize;
            }

            if (Widgets.ButtonText(optionsRect, OptionsText[DistanceFromMiddle], drawBackground: true, doMouseoverSound: true, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption(OptionsText[4], delegate
                {
                    DistanceFromMiddle = 4;
                }));
                list.Add(new FloatMenuOption(OptionsText[3], delegate
                {
                    DistanceFromMiddle = 3;
                }));
                list.Add(new FloatMenuOption(OptionsText[2], delegate
                {
                    DistanceFromMiddle = 2;
                }));
                list.Add(new FloatMenuOption(OptionsText[1], delegate
                {
                    DistanceFromMiddle = 1;
                }));
                list.Add(new FloatMenuOption(OptionsText[0], delegate
                {
                    DistanceFromMiddle = 0;
                }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
            Widgets.CheckboxLabeled(useColorsRect, UseColorsText, ref UseColorsBool);

            if (DistanceFromMiddle == 4)
            {
                //Log.Message("Draw word cloud");
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(forbiddenRect.x, forbiddenRect.y, forbiddenRect.height);
                Widgets.DrawLineHorizontal(forbiddenRect.x, forbiddenRect.yMax, forbiddenRect.width);
                GUI.color = Color.white;
                Vector2 cloudCenter = totalRect.center;
                cloudCenter.x = 0.5f * totalRect.center.x + 0.5f * personalityRect.center.x;
                //Vector2 cloudCenter = new Vector2(0.5f * totalRect.center.x + 0.25f * forbiddenRect.x, 0.5f * totalRect.center.x + 0.25f * forbiddenRect.yMax + 0.25f * totalRect.yMax);
                PersonalityWordCloud(totalRect, cloudCenter, forbiddenRect, pawn, editRect);
            }
            else
            {
                Widgets.CheckboxLabeled(alphabeticalRect, AlphabeticalText, ref AlphabeticalBool);
                Widgets.CheckboxLabeled(useAntonymsRect, UseAntonymsText, ref UseAntonymsBool);

                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(forbiddenRect.x, totalRect.y, totalRect.height);
                Widgets.DrawLineHorizontal(forbiddenRect.x, useAntonymsRect.yMax + BoundaryPadding, forbiddenRect.width);
                GUI.color = Color.white;

                /* DRAW FIVE FACTOR MODEL */
                Text.Anchor = TextAnchor.MiddleCenter;
                style.fontSize = FiveFactorTitleFontSize;
                Widgets.Label(fiveFactorRect, FiveFactorText);
                style.fontSize = OldSmallFontSize;
                Text.Anchor = OldAnchor;
                fiveFactorRect.xMin -= HighlightPadding;
                fiveFactorRect.xMax += HighlightPadding;
                Widgets.DrawHighlightIfMouseover(fiveFactorRect);
                TooltipHandler.TipRegion(fiveFactorRect, delegate
                {
                    return ((string)"FiveFactorDescription".Translate()).ReplaceFirst("{0}", "FiveFactorDescription0".Translate().Colorize(TitleColor));
                }, 521312);

                float pentaRadius = 0.38f * forbiddenRect.width;
                Vector2 pentaCenter = new Vector2(forbiddenRect.center.x, fiveFactorRect.yMax + 0.5f * forbiddenRect.width + BoundaryPadding);
                for (byte r = 1; r <= RadialCategories; r++)
                {
                    List<Vector2> pentaPoints = GetPentagonVerticies(pentaCenter, r * pentaRadius / RadialCategories);
                    if (r != RadialCategories)
                    {
                        for (byte p = 0; p < 5; p++)
                        {

                            PsycheCardDrawLine(pentaPoints[p], pentaPoints[(p + 1) % 5], new Color(1f, 1f, 1f), 0.5f);
                        }
                    }
                    else
                    {
                        for (byte p = 0; p < 5; p++)
                        {
                            PsycheCardDrawLine(pentaPoints[p], pentaPoints[(p + 1) % 5], new Color(1f, 1f, 1f), 0.5f);
                            PsycheCardDrawLine(pentaCenter, pentaPoints[p], new Color(1f, 1f, 1f), 0.5f);
                        }
                    }
                }
                //float[] bigFiveList = { 0.8f, 0.3f, 0.5f, 0.1f, 0.6f };
                float[] bigFiveList = BigFiveRatings(pawn);

                List<Vector2> yellowVerticies = new List<Vector2> { };
                List<int[]> yellowTriangles = new List<int[]> { };
                for (byte p = 0; p < 5; p++)
                {
                    yellowVerticies.Add(RadialProjectionFromCenter(pentaCenter, Mathf.Max(0.001f, bigFiveList[p]) * pentaRadius, 72f * p));
                    yellowTriangles.Add(new int[] { p, (p + 1) % 5, 5 });
                }
                yellowVerticies.Add(pentaCenter);
                PolygonUtility.DrawPolygon(yellowVerticies, yellowTriangles, PsycheYellowMat);
                //for (int p = 0; p < 5; p++)
                //{
                //    PsycheCardDrawLine(yellowVerticies[p], yellowVerticies[(p + 1) % 5], new Color(1f, 1f, 0.25f, 0.75f), 2.5f);
                //}
                float highlightRadius = 0.506f * forbiddenRect.width;
                for (byte p = 0; p < 5; p++)
                {
                    string letter = BigFiveLetters[p];
                    //string letterBold = ("<b>" + letter + "</b>").Colorize(BigFiveColors[p]);
                    string letterBold = ("<b>" + letter + "</b>").Colorize(UseColorsBool ? BigFiveColors[p] : Color.white);

                    style.fontSize = SexualityFontSize;
                    Vector2 letterSize = Text.CalcSize(letterBold);
                    Vector2 tightSize = letterSize * new Vector2(1.1f, 0.65f);
                    float displacement = 0.60f * (tightSize * BigFiveSpacings[p]).magnitude;
                    Vector2 position = RadialProjectionFromCenter(pentaCenter, pentaRadius + displacement, 72 * p);
                    Rect letterRect = new Rect(position.x - 0.5f * letterSize.x, position.y - 0.45f * letterSize.y, letterSize.x, letterSize.y);
                    Widgets.Label(letterRect, letterBold);
                    style.fontSize = OldSmallFontSize;

                    List<Vector2> verticies = new List<Vector2> { };
                    verticies.Add(pentaCenter);
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, SideScaling * highlightRadius, 72f * (p - 0.5f)));
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, highlightRadius, 72f * p));
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, SideScaling * highlightRadius, 72f * (p + 0.5f)));
                    List<int[]> triangles = new List<int[]> { };
                    //Log.Message("verticies = " + verticies.ToString());
                    PolygonUtility.DrawPolygonHighlightIfMouseover(verticies, triangles);

                    Color bigFiveTitleColor = UseColorsBool ? BigFiveColors[p] : ColoredText.TipSectionTitleColor;
                    float bigFiveRating = bigFiveList[p];
                    PolygonTooltipHandler.TipRegion(verticies, delegate
                    {
                        string bigFiveDescription = ("BigFive" + letter + "Description").Translate();
                        bigFiveDescription = bigFiveDescription.ReplaceFirst("{0}", ("BigFive" + letter + "Title").Translate().Colorize(bigFiveTitleColor));
                        if (Prefs.DevMode)
                        {
                            bigFiveDescription += "\n\nRating: " + bigFiveRating.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(RatingColor);
                        }
                        return bigFiveDescription;
                    }, 38975 + p * 237);
                }

                /* Draw personality node list */
                PersonalityTraitList(personalityRect, pawn);
            }

            //Edit Psyche button
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonText(editRect, EditText, true, false, true))
                {
                    Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
                }
            }

            
            GUI.EndGroup();
        }

        public static string ColorizedNodeText(PersonalityNode node, float displacement)
        {
            bool antoBool = UseAntonymsBool && displacement < 0f;
            string nodeText = (antoBool ? node.def.antonymLabel : node.def.label).CapitalizeFirst();
            Color nodeColor = antoBool ? HSVtoColor(node.def.antonymHSV) : HSVtoColor(node.def.nodeHSV);
            nodeText = UseColorsBool ? nodeText == "Creative" ? CreativeRainbow : nodeText.Colorize(nodeColor) : nodeText;
            return nodeText;
        }

        [LogPerformance]
        public static void PersonalityTraitList(Rect personalityRect, Pawn pawn)
        {
            if (ListTicker % 100 == 0)
            {
                Log.Message("ListTicker = " + ListTicker);
            }

            if (ListTicker > 0 && pawn == PawnCached && DistanceFromMiddle == DistanceFromMiddleCached && UseColorsBool == UseColorsBoolCached && UseAntonymsBool == UseAntonymsBoolCached && AlphabeticalBool == AlphabeticalBoolCached && LabelNodeList.Any())
            {
                DrawTraitList(personalityRect);
                return;
            }
            ListTicker = MaxTicks;
            PawnCached = pawn;
            DistanceFromMiddleCached = DistanceFromMiddle;
            UseColorsBoolCached = UseColorsBool;
            UseAntonymsBoolCached = UseAntonymsBool;
            AlphabeticalBoolCached = AlphabeticalBool;
            LabelNodeList.Clear();
            //ViewRectHeight = 0f;
            if (NeedToCalculateNodeWidthBool)
            {
                CalculateNodeWidth(pawn);
                NeedToCalculateNodeWidthBool = false;
            }

            List<string> nodeTextList = new List<string>();
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                float displacement = node.AdjustedRating - 0.5f;
                string nodeText = ColorizedNodeText(node, displacement);
                float yAxis = UseAntonymsBool ? Mathf.Abs(2f * displacement) : 2f * displacement;
                int category = Mathf.RoundToInt(0.5f * Categories * (1f + yAxis * Mathf.Sqrt(Mathf.Abs(yAxis))));
                if (Mathf.Abs(category - 3) >= DistanceFromMiddle)
                {
                    string categoryText = ("Psyche" + NodeDescriptions[category]).Translate();
                    LabelNodeList.Add(new Tuple<string, PersonalityNode, string>(nodeText, node, categoryText.Colorize(NodeColors[category])));
                    nodeTextList.Add(UseAntonymsBool && displacement < 0f ? node.def.antonymLabel : node.def.label);
                }
            }

            if (AlphabeticalBool)
            {
                LabelNodeList = LabelNodeList.OrderBy(tup => nodeTextList[LabelNodeList.IndexOf(tup)]).ToList();
            }
            else if (UseAntonymsBool)
            {
                LabelNodeList = LabelNodeList.OrderBy(tup => -Mathf.Abs(tup.Item2.AdjustedRating - 0.5f)).ToList();
            }
            else
            {
                LabelNodeList = LabelNodeList.OrderBy(tup => -tup.Item2.AdjustedRating).ToList();
            }

            DrawTraitList(personalityRect);
        }

        public static void CalculateNodeWidth(Pawn pawn)
        {
            Text.Font = GameFont.Small;
            //GUIStyle style = Text.fontStyles[1];
            //style.alignment = NewSmallAlignment;
            //style.fontSize = NewSmallFontSize;
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                string labelText = node.def.label.CapitalizeFirst();
                string antoText = node.def.antonymLabel.CapitalizeFirst();
                Vector2 labelSize = Text.CalcSize(labelText);
                Vector2 antoSize = Text.CalcSize(antoText);
                NodeWidth = Mathf.Max(NodeWidth, 1.025f * labelSize.x, 1.025f * antoSize.x);
                CategoryNodeHeight = Mathf.Max(CategoryNodeHeight, labelSize.y, antoSize.y);
            }
            //style.alignment = OldSmallAlignment;
            //style.fontSize = OldSmallFontSize;
        }

        public static void DrawTraitList(Rect personalityRect)
        {
            Text.Font = GameFont.Small;
            TextAnchor oldAnchor = Text.Anchor;
            float yCompression = 0.9f;
            float highlightShift = 0.7f * (1f - yCompression);
            float xSpacing = BoundaryPadding;
            Rect viewRect = new Rect(0f, 0f, personalityRect.width - 20f, yCompression * CategoryNodeHeight * LabelNodeList.Count() + 10f);
            //Rect viewRect = new Rect(personalityRect.x, personalityRect.y, 0.9f * personalityRect.width, (categoryNodeHeight + RowTopPadding) * categoryIndexList.Count);
            Widgets.BeginScrollView(personalityRect, ref NodeScrollPosition, viewRect);
            float categoryNodeVerticalPosition = personalityRect.y;
            float categoryRectX = viewRect.center.x - 0.5f * (CategoryWidth + xSpacing + NodeWidth);
            Rect categoryRect = new Rect(categoryRectX, categoryNodeVerticalPosition, CategoryWidth, CategoryNodeHeight);
            Rect nodeRect = new Rect(categoryRect.xMax + xSpacing, categoryNodeVerticalPosition, NodeWidth, CategoryNodeHeight);
            Rect highlightRect = categoryRect;
            highlightRect.xMin = viewRect.xMin;
            highlightRect.xMax = viewRect.xMax;
            highlightRect.height *= yCompression;
            //float categoryRectX = viewRect.xMin + BoundaryPadding + HighlightPadding;
            for (int j = 0; j < LabelNodeList.Count(); j++)
            {
                categoryRect.y = categoryNodeVerticalPosition;
                nodeRect.y = categoryNodeVerticalPosition;
                highlightRect.y = categoryNodeVerticalPosition + highlightShift * CategoryNodeHeight;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(categoryRect, LabelNodeList[j].Item3);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nodeRect, LabelNodeList[j].Item1);
                Text.Anchor = oldAnchor;
                DrawHighlightAndTooltip(highlightRect, LabelNodeList[j].Item2, j);
                categoryNodeVerticalPosition += yCompression * CategoryNodeHeight;
            }
            Widgets.EndScrollView();
        }

        public static void PersonalityWordCloud(Rect totalRect, Vector2 cloudCenter, Rect forbiddenRect, Pawn pawn, Rect editPsycheRect)
        {
            if (CloudTicker % 100 == 0)
            {
                Log.Message("CloudTicker = " + CloudTicker);
            }
            if (CloudTicker > 0 && pawn == PawnCached && DistanceFromMiddle == DistanceFromMiddleCached && UseColorsBool == UseColorsBoolCached && CloudTextRects.Any())
            {
                CloudTicker--;
                DrawWordCloud();
                return;
            }
            Log.Message("Recompute cloud");
            CloudTicker = MaxTicks;
            PawnCached = pawn;
            DistanceFromMiddleCached = DistanceFromMiddle;
            UseColorsBoolCached = UseColorsBool;
            CloudTextRects.Clear();
            CloudTightRects.Clear();
            CloudTexts.Clear();
            CloudNodes.Clear();
            CloudFontSizes.Clear();

            Rect cloudRect = totalRect.ContractedBy(BoundaryPadding);
            Rect forbiddenRect1 = forbiddenRect.ExpandedBy(BoundaryPadding);
            Rect forbiddenRect2 = editPsycheRect.ExpandedBy(BoundaryPadding);
            List<PersonalityNode> allNodes = (from n in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes
                                              orderby Mathf.Abs(n.AdjustedRating - 0.5f) descending, n.def.defName
                                              select n).ToList();
            Text.Font = GameFont.Medium;
            GUIStyle style = Text.fontStyles[2];
            float lengthX = totalRect.xMax - cloudCenter.x;
            float lengthY = cloudCenter.y - totalRect.y;
            float dt = 1f / PointsOnSpiral;
            float dl = kSpiral * dt;
            for (int i = 0; i < allNodes.Count(); i++)
            {
                PersonalityNode node = allNodes[i];
                float displacement = node.AdjustedRating - 0.5f;
                //style.fontSize = Mathf.FloorToInt(60f * Mathf.Abs(displacement)) + 5;
                //style.fontSize = Mathf.RoundToInt(Mathf.Clamp(35f * Mathf.Pow(Mathf.Abs(2 * displacement), 0.5f), 10f, 35f));
                //style.fontSize = Mathf.FloorToInt(30f * Mathf.Pow(Mathf.Abs(2 * displacement), 0.25f)) + 5;
                //style.fontSize = Mathf.RoundToInt(Mathf.Lerp(35f, 8f, i / 36f));
                
                UseAntonymsBool = true;
                string personalityText = ColorizedNodeText(node, displacement);
                UseAntonymsBool = UseAntonymsBoolCached;
                style.fontSize = Mathf.RoundToInt(Mathf.Lerp(40f, 10f, Mathf.Pow(i / 36f, 0.75f)));
                Vector2 textSize = Text.CalcSize(personalityText.Colorize(Color.white));
                //textSize.x *= 1.0125f;
                Rect textRect = new Rect(cloudCenter - 0.5f * textSize, textSize);
                Rect tightRect = textRect;
                tightRect.xMin = textRect.xMin - xMinTightScaling * textRect.width;
                tightRect.xMax = textRect.xMax + xMaxTightScaling * textRect.width;
                tightRect.yMin = textRect.yMin - yMinTightScaling * textRect.height;
                tightRect.yMax = textRect.yMax + yMaxTightScaling * textRect.height;
                float x0 = tightRect.x;
                float y0 = tightRect.y;
                int sign = Rand.ValueSeeded(3 * pawn.HashOffset() + 11 * i) < 0.5f ? 1 : -1;
                float alpha0 = 2 * Mathf.PI * Rand.ValueSeeded(pawn.HashOffset() + node.GetHashCode() + i);
                //for (float l = 0f; l < kSpiral; l += dl)
                //{
                //    if (RectDoesNotOverlapWordCloud(tightRect, cloudRect, forbiddenRect1, forbiddenRect2))
                //    {
                //        textRect.x = tightRect.x;
                //        textRect.y = tightRect.y - yMinTightScaling * textRect.height;
                //        CloudRects.Add(textRect);
                //        CloudTexts.Add(personalityText);
                //        CloudNodes.Add(node);
                //        CloudFontSizes.Add(style.fontSize);
                //        break;
                //    }
                //    float kt2 = 2f * kSpiral * l;
                //    float kt = Mathf.Sqrt(kt2);
                //    float ds = dl / Mathf.Sqrt(1f + kt2);
                //    float alpha = kt + alpha0;
                //    float cos = Mathf.Cos(alpha);
                //    float sin = Mathf.Sin(alpha);
                //    tightRect.x += lengthX * (cos - kt * sin) * ds;
                //    tightRect.y += sign * lengthY * (sin + kt * cos) * ds;
                //}
                for (float t = dt; t < 1.414; t += dt)
                {
                    if (RectDoesNotOverlapWordCloud(tightRect, cloudRect, forbiddenRect1, forbiddenRect2))
                    {
                        textRect.x = tightRect.x + xMinTightScaling * textRect.width;
                        textRect.y = tightRect.y + yMinTightScaling * textRect.height;
                        CloudTextRects.Add(textRect);
                        CloudTightRects.Add(tightRect);
                        CloudTexts.Add(personalityText);
                        CloudNodes.Add(node);
                        CloudFontSizes.Add(style.fontSize);
                        break;
                    }
                    tightRect.x = x0 + lengthX * t * Mathf.Cos(alpha0);
                    tightRect.y = y0 + lengthY * t * Mathf.Sin(alpha0) * sign;
                    alpha0 += dl;
                }
            }
            style.fontSize = OldMediumFontSize;
            Text.Font = GameFont.Small;
            DrawWordCloud();
        }

        private static void DrawWordCloud()
        {
            Text.Font = GameFont.Medium;
            GUIStyle style = Text.fontStyles[2];
            TextAnchor oldAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            for (int i = 0; i < CloudTextRects.Count(); i++)
            {
                style.fontSize = CloudFontSizes[i];
                Widgets.Label(CloudTextRects[i], CloudTexts[i]);
                DrawHighlightAndTooltip(CloudTightRects[i], CloudNodes[i], i);
            }
            style.fontSize = OldMediumFontSize;
            Text.Anchor = oldAnchor;
            Text.Font = GameFont.Small;
        }

        private static bool RectDoesNotOverlapWordCloud(Rect foundRectangle, Rect totalRect, Rect forbiddenRect1, Rect forbiddenRect2)
        {
            foreach (Rect rect in CloudTightRects)
            {
                if (foundRectangle.Overlaps(rect))
                {
                    return false;
                }
            }
            if (foundRectangle.Overlaps(forbiddenRect1) || foundRectangle.Overlaps(forbiddenRect2))
            {
                return false;
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
            TooltipHandler.TipRegion(highlightRect, delegate
            {
                string nodeName = node.def.descriptionLabel;
                string antonymLabel = node.def.antonymLabel;
                if (UseColorsBool)
                {
                    Color nodeColor = HSVtoColor(node.def.nodeHSV);
                    Color antoColor = HSVtoColor(node.def.antonymHSV);
                    nodeName = nodeName == "Creative" ? CreativeRainbow : nodeName.Colorize(nodeColor);
                    antonymLabel = antonymLabel.Colorize(antoColor);
                    //nodeName = nodeColor == Color.white ? "<b>" + nodeName + "</b>" : nodeName;
                    //antonymLabel = antoColor == Color.white ? "<b>" + antonymLabel + "</b>" : antonymLabel;
                    //nodeName = "<b>" + nodeName + "</b>";
                    //antonymLabel = "<b>" + antonymLabel + "</b>";
                }
                else
                {
                    nodeName = nodeName.Colorize(ColoredText.TipSectionTitleColor);
                    antonymLabel = antonymLabel.Colorize(ColoredText.TipSectionTitleColor);
                }
                string tooltipString = node.def.description.ReplaceFirst("{0}", nodeName) + " " + ((string)"AntonymColon".Translate()) + " " + antonymLabel + ".";

                List<string> convoTopics = node.def.conversationTopics;
                if (convoTopics != null)
                {
                    string convoString = "\n\n" + "ConvoTooltip".Translate(string.Join("PsycheComma".Translate(), convoTopics.Take(convoTopics.Count - 1).ToArray()), convoTopics.Last());
                    tooltipString += convoString;
                }
                if (Prefs.DevMode)
                {
                    string rawRating = node.rawRating.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                    string adjRating = node.AdjustedRating.ToString(RatingFormat, CultureInfo.InvariantCulture).Colorize(new Color(1f, 1f, 0f));
                    tooltipString += "\n\nRaw: " + rawRating + "  Adjusted: " + adjRating;
                }
                return tooltipString;
            }, 613261 + 612 * i);
        }

        public static Color HSVtoColor(Vector3 HSV)
        {
            float H = PsycheHelper.Mod(HSV.x, 360f);
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

        public static Vector2 RadialProjectionFromCenter(Vector2 center, float radius, float angle)
        {
            float x = center.x + radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            float y = center.y - radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            return new Vector2(x, y);
        }

        public static List<Vector2> GetPentagonVerticies(Vector2 center, float radius)
        {
            List<Vector2> verticies = new List<Vector2>();
            for (byte p = 0; p < 5; p++)
            {
                verticies.Add(RadialProjectionFromCenter(center, radius, 72 * p));
            }
            return verticies;
        }

        public static void PsycheCardDrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            float xDiff = end.x - start.x;
            float yDiff = end.y - start.y;
            float length = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
            if (length > 0.01f)
            {
                float z = Mathf.Atan2(yDiff, xDiff) * Mathf.Rad2Deg;
                Matrix4x4 m = Matrix4x4.TRS(start, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-start, Quaternion.identity, Vector3.one);
                Rect position = new Rect(start.x, start.y - 0.5f * width, length, width);
                GL.PushMatrix();
                GL.MultMatrix(m);
                GUI.DrawTexture(position, PsycheLineTex, ScaleMode.StretchToFill, alphaBlend: true, 0f, color, 0f, 0f);
                GL.PopMatrix();
            }
        }

        public static float[] BigFiveRatings(Pawn pawn)
        {
            float[] bigFiveNumerators = { 0f, 0f, 0f, 0f, 0f };
            float[] bigFiveDenominators = { 0f, 0f, 0f, 0f, 0f };
            foreach (PersonalityNode node in PsycheHelper.Comp(pawn).Psyche.PersonalityNodes)
            {
                List<float> bigFiveModifierList = node.def.bigFiveModifiers;
                float displacement = node.AdjustedRating - 0.5f;
                for (int p = 0; p < 5; p++)
                {
                    bigFiveNumerators[p] += bigFiveModifierList[p] * displacement;
                    bigFiveDenominators[p] += Mathf.Abs(bigFiveModifierList[p]);
                }
            }
            float[] bigFiveArray = { 0f, 0f, 0f, 0f, 0f };
            float[] bigFiveScalings = { 1.25f, 1.2f, 1.3f, 1.2f, 1.3f };
            for (int p = 0; p < 5; p++)
            {
                bigFiveArray[p] = Mathf.Clamp(0.5f + bigFiveScalings[p] * (bigFiveNumerators[p] / bigFiveDenominators[p]), 0f, 1f);
            }
            return bigFiveArray;
        }
    }
}