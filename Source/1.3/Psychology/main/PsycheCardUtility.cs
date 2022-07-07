using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//using System.Text;
//using RimWorld.Planet;
using RimWorld;
using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.UI.Extensions;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace Psychology
{
    public class PsycheCardUtility
    {
        private static string[] BigFiveLetters = { "O", "C", "E", "A", "N" };
        private static Vector2[] BigFiveSpacings = { new Vector2(0f, -1f), new Vector2(1f, -0.32492f), new Vector2(0.726543f, 1f), new Vector2(-0.726543f, 1f), new Vector2(-1f, -0.32492f) };
        private static string[] NodeDescriptions = { "Not", "Slightly", "Less", "Somewhat", "More", "Very", "Extremely" };
        private static Color[] NodeColors = { new Color(1f, 0.2f, 0.2f, 0.8f), new Color(1f, 0.4f, 0.4f, 0.6f), new Color(1f, 0.6f, 0.6f, 0.4f), new Color(1f, 1f, 1f, 0.2f), new Color(0.6f, 1f, 0.6f, 0.4f), new Color(0.4f, 1f, 0.4f, 0.6f), new Color(0.2f, 1f, 0.2f, 0.8f) };
        private static Vector2 NodeScrollPosition = Vector2.zero;
        //private static List<Pair<string, int>> NodeStrings = new List<Pair<string, int>>();
        private static Texture2D PsycheLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
        private static Material PsycheYellowMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 0f, 0.3f));
        private const int Categories = 6;
        private const int RadialCategories = 4;
        private const float RowTopPadding = 3f;
        private const float BoundaryPadding = 7f;
        private const float HighlightPadding = 3f;
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
        private static Vector3[] CreativeHSVs = { new Vector3(0f, 0.9f, 1f), new Vector3(30f, 1f, 1f), new Vector3(60f, 1f, 0.9f), new Vector3(130f, 1f, 0.9f), new Vector3(195f, 1f, 1f), new Vector3(210f, 1f, 1f), new Vector3(280f, 0.9f, 1f), new Vector3(305f, 1f, 0.9f) };
        private static float sideScaling = 0.809017f;
        //private static Mesh m;

        //void Start()
        //{
        //    Color32 color32 = Color.red;


        //    using (var vh = new VertexHelper())
        //    {
        //        vh.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 0f));
        //        vh.AddVert(new Vector3(0, 100), color32, new Vector2(0f, 1f));
        //        vh.AddVert(new Vector3(100, 100), color32, new Vector2(1f, 1f));
        //        vh.AddVert(new Vector3(100, 0), color32, new Vector2(1f, 0f));

        //        vh.AddTriangle(0, 1, 2);
        //        vh.AddTriangle(2, 3, 0);
        //        vh.FillMesh(m);
        //    }
        //}



        public static void DrawPsycheCard(Rect totalRect, Pawn pawn, bool notOnMenu)
        {
            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                return;
            }
            GUI.BeginGroup(totalRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            // Kinsey rating text
            Vector2 kinseyTextSize = Text.CalcSize("KinseyRating".Translate() + " 0");
            // Sex and romantic drive text
            List<string> sexDriveText = new List<string>();
            List<Vector2> sexDriveSize = new List<Vector2>();
            List<string> romDriveText = new List<string>();
            List<Vector2> romDriveSize = new List<Vector2>();
            for (int i = 0; i < 5; i++)
            {
                sexDriveText.Add(("SexDrive" + i.ToString()).Translate());
                romDriveText.Add(("RomanticDrive" + i.ToString()).Translate());
                sexDriveSize.Add(Text.CalcSize(sexDriveText[i]));
                romDriveSize.Add(Text.CalcSize(romDriveText[i]));
            }
            float sexDriveWidth = sexDriveSize.Max(s => s.x);
            float sexDriveHeight = sexDriveSize.Max(s => s.y);
            float romDriveWidth = romDriveSize.Max(s => s.x);
            float romDriveHeight = romDriveSize.Max(s => s.y);
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

            float optionsWidth = 20f + ShowDistanceSize.Max(s => s.x);
            float optionsHeight = 5f + ShowDistanceSize.Max(s => s.y);

            string AlphabeticalText = "Alphabetical".Translate();
            Vector2 AlphabeticalSize = Text.CalcSize(AlphabeticalText);
            float alphabeticalWidth = AlphabeticalSize.x + 29f;
            float alphabeticalHeight = Mathf.Max(AlphabeticalSize.y, 24f);

            string UseAntonymsText = "UseAntonyms".Translate();
            Vector2 UseAntonymsSize = Text.CalcSize(UseAntonymsText);
            float useAntonymsWidth = UseAntonymsSize.x + 29f;
            float useAntonymsHeight = Mathf.Max(UseAntonymsSize.y, 24f);

            string EditPsycheText = "EditPsyche".Translate();
            Vector2 EditPsycheSize = Text.CalcSize(EditPsycheText);
            float editWidth = 20f + EditPsycheSize.x;
            float editHeight = 5f + EditPsycheSize.y;

            // Calculate rectangles for sexuality panel
            float sexualityWidth = 1.05f * Mathf.Max(kinseyTextSize.x, sexDriveWidth, romDriveWidth, optionsWidth, alphabeticalWidth, useAntonymsWidth);
            float kinseyWidth = sexualityWidth + BoundaryPadding - 27f;
            //float sexualityHeight = kinseyTextSize.y + sexDriveHeight + romanticDriveHeight + optionsHeight + 5f * RowTopPadding;
            Rect kinseyRect = new Rect(totalRect.xMax - sexualityWidth - BoundaryPadding, totalRect.y + BoundaryPadding, kinseyWidth, kinseyTextSize.y);
            Rect sexDriveRect = new Rect(kinseyRect.x, kinseyRect.yMax + RowTopPadding, sexualityWidth, sexDriveHeight);
            Rect romDriveRect = new Rect(kinseyRect.x, sexDriveRect.yMax + RowTopPadding, sexualityWidth, romDriveHeight);

            // Calculate rectanges for display options
            Rect optionsRect = new Rect(kinseyRect.x - HighlightPadding, romDriveRect.yMax + RowTopPadding, optionsWidth, optionsHeight);
            Rect alphabeticalRect = new Rect(kinseyRect.x, optionsRect.yMax + 10f, alphabeticalWidth, alphabeticalHeight);
            Rect useAntonymsRect = new Rect(kinseyRect.x, alphabeticalRect.yMax + 10f, useAntonymsWidth, useAntonymsHeight);
            Rect editPsycheRect = new Rect(totalRect.xMax - editWidth - BoundaryPadding, totalRect.yMax - editHeight - BoundaryPadding, editWidth, editHeight);

            //Calculate personality rectange
            Rect personalityRect = totalRect;
            personalityRect.xMax = kinseyRect.x - HighlightPadding;
            personalityRect = personalityRect.ContractedBy(BoundaryPadding);

            // Draw the widgets for sexuality panel
            if (PsychologyBase.ActivateKinsey())
            {
                string kinseyText = "KinseyRating".Translate() + " " + PsycheHelper.Comp(pawn).Sexuality.kinseyRating;

                float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
                float pawnRomDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
                int sexDriveInt = (pawnSexDrive > 0.9f) ? 4 : (pawnSexDrive > 0.63f) ? 3 : (pawnSexDrive > 0.37f) ? 2 : (pawnSexDrive > 0.1f) ? 1 : 0;
                int romDriveInt = (pawnRomDrive > 0.9f) ? 4 : (pawnRomDrive > 0.63f) ? 3 : (pawnRomDrive > 0.37f) ? 2 : (pawnRomDrive > 0.1f) ? 1 : 0;

                //Kinsey rating
                Widgets.Label(kinseyRect, kinseyText);
                kinseyRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(kinseyRect);
                Func<String> KinseyString = delegate
                {
                    return "KinseyDescription".Translate();
                };
                TooltipHandler.TipRegion(kinseyRect, KinseyString, 89140);

                //Sex drive
                Widgets.Label(sexDriveRect, sexDriveText[sexDriveInt]);
                sexDriveRect.xMin -= HighlightPadding;
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
                Widgets.Label(romDriveRect, romDriveText[romDriveInt]);
                romDriveRect.xMin -= HighlightPadding;
                Widgets.DrawHighlightIfMouseover(romDriveRect);
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
                TooltipHandler.TipRegion(romDriveRect, RomanticDriveString, 89142);

            }
            else if (notOnMenu)
            {
                Rect disabledRect = new Rect(kinseyRect.x, kinseyRect.y, sexualityWidth, romDriveRect.yMax - kinseyRect.y);
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

            Rect forbiddenRect = new Rect(personalityRect.xMax, 0f, totalRect.width - personalityRect.xMax, optionsRect.yMax + BoundaryPadding);
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
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.DrawLineVertical(forbiddenRect.x, totalRect.y, totalRect.height);
                GUI.color = Color.white;
                Widgets.CheckboxLabeled(alphabeticalRect, AlphabeticalText, ref AlphabeticalBool);
                Widgets.CheckboxLabeled(useAntonymsRect, UseAntonymsText, ref UseAntonymsBool);
                /* DRAW BIG FIVE PENTAGON */
                Vector2 pentaCenter = new Vector2(forbiddenRect.center.x, 0.5f * (useAntonymsRect.yMax + totalRect.yMax));
                float pentaRadius = 0.38f * forbiddenRect.width;
                for (int r = 1; r <= RadialCategories; r++)
                {
                    List<Vector2> pentaPoints = GetPentagonVerticies(pentaCenter, r * pentaRadius / RadialCategories);
                    if (r != RadialCategories)
                    {
                        for (int p = 0; p < 5; p++)
                        {

                            PsycheCardDrawLine(pentaPoints[p], pentaPoints[(p + 1) % 5], new Color(1f, 1f, 1f), 0.7f);
                        }
                    }
                    else
                    {
                        for (int p = 0; p < 5; p++)
                        {
                            PsycheCardDrawLine(pentaPoints[p], pentaPoints[(p + 1) % 5], new Color(1f, 1f, 1f), 1f);
                            PsycheCardDrawLine(pentaCenter, pentaPoints[p], new Color(1f, 1f, 1f), 1f);
                        }
                    }
                }

                // Draw yellow polygon
                float[] BigFiveList = { 0.8f, 0.3f, 0.5f, 0.1f, 0.6f };
                List<Vector2> yellowVerticies = new List<Vector2> { };
                List<int[]> yellowTriangles = new List<int[]> { };
                for (int p = 0; p < 5; p++)
                {
                    yellowVerticies.Add(RadialProjectionFromCenter(pentaCenter, BigFiveList[4 - p], 72f * (4 - p)));
                    yellowTriangles.Add(new int[] { p, (p + 1) % 5, 5 });
                }
                for (int p = 0; p < 5; p++)
                {
                    PsycheCardDrawLine(yellowVerticies[p], yellowVerticies[(p + 1) % 5], new Color(1f, 1f, 0.25f, 0.75f), 2.5f);
                }
                yellowVerticies.Add(pentaCenter);
                PolygonUtility.DrawPolygon(yellowVerticies, yellowTriangles, PsycheYellowMat);

                // Draw labels and highlights
                float highlightRadius = 0.513f * forbiddenRect.width;
                for (int p = 0; p < 5; p++)
                {
                    string letter = BigFiveLetters[p];
                    string letterBold = "<b>" + letter + "</b>";
                    Vector2 letterSize = Text.CalcSize(letterBold);
                    Vector2 tightSize = letterSize * new Vector2(1.25f, 0.8f);
                    float displacement = 0.5f * (tightSize * BigFiveSpacings[p]).magnitude;
                    Vector2 position = RadialProjectionFromCenter(pentaCenter, pentaRadius + displacement, 72 * p);
                    //Vector2 position = RadialProjectionFromCenter(pentaCenter, pentaRadius, 72 * p) + 0.5f * tightSize * BigFiveSpacings[p];
                    //float magnitude = tightSize.magnitude;
                    //Vector2 letterSquare = new Vector2(magnitude, magnitude);
                    Rect letterRect = new Rect(position.x - 0.5f * letterSize.x, position.y - 0.5f * letterSize.y, letterSize.x, letterSize.y);
                    //Rect highlightRect = new Rect(position - 0.5f * tightSize, tightSize);
                    //Rect highlightRect = letterRect;
                    Widgets.Label(letterRect, letterBold);
                    //Widgets.DrawHighlightIfMouseover(highlightRect);
                    //Log.Message("Draw tooltip for letter = " + letter);
                    //TooltipHandler.TipRegion(highlightRect, delegate
                    //{
                    //    string BigFiveDescription = ("BigFive" + letter + "Description").Translate();
                    //    string BigFiveTitle = ("BigFive" + letter + "Title").Translate().Colorize(ColoredText.TipSectionTitleColor);
                    //    return BigFiveDescription.ReplaceFirst("{0}", BigFiveTitle);
                    //}, 80381 + p * 137);
                    //Vector2 v1 = pentaCenter;
                    //Vector2 v2 = RadialProjectionFromCenter(pentaCenter, sideScaling * highlightRadius, 72f * (p - 0.5f));
                    //Vector2 v3 = RadialProjectionFromCenter(pentaCenter, highlightRadius, 72f * p);
                    //Vector2 v4 = RadialProjectionFromCenter(pentaCenter, sideScaling * highlightRadius, 72f * (p + 0.5f));
                    //List<Vector2> verticies = new List<Vector2> { v1, v2, v3, v4 };

                    List<Vector2> verticies = new List<Vector2> { };
                    verticies.Add(pentaCenter);
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, sideScaling * highlightRadius, 72f * (p - 0.5f)));
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, highlightRadius, 72f * p));
                    verticies.Add(RadialProjectionFromCenter(pentaCenter, sideScaling * highlightRadius, 72f * (p + 0.5f)));
                    List<int[]> triangles = new List<int[]> { };
                    Log.Message("verticies = " + verticies.ToString());
                    PolygonUtility.DrawPolygonHighlightIfMouseover(verticies, triangles);
                    PolygonTooltipHandler.TipRegion(verticies, delegate
                    {
                        string BigFiveDescription = ("BigFive" + letter + "Description").Translate();
                        string BigFiveTitle = ("BigFive" + letter + "Title").Translate().Colorize(ColoredText.TipSectionTitleColor);
                        return BigFiveDescription.ReplaceFirst("{0}", BigFiveTitle);
                    }, 80381 + p * 137);
                }
                /* Draw personality node list */
                DrawPersonalityNodes(personalityRect, pawn);

                //Vector2 guiV1 = new Vector2(totalRect.xMin, totalRect.yMin);
                //Vector2 guiV2 = new Vector2(0.5f * totalRect.xMax, totalRect.yMin);
                //Vector2 guiV3 = new Vector2(0.5f * totalRect.xMax, 0.5f * totalRect.yMax);
                //Vector2 guiV4 = new Vector2(totalRect.xMin, 0.5f * totalRect.yMax);
                //List<Vector2> verticies2 = new List<Vector2> { guiV1, guiV2, guiV3, guiV4 };
                List<Vector2> verticies2 = new List<Vector2> { };
                verticies2.Add(new Vector2(0, 0));
                verticies2.Add(new Vector2(200, 0));
                verticies2.Add(new Vector2(200, 200));
                verticies2.Add(new Vector2(0, 250));
                List<int[]> triangles2 = new List<int[]> { new int[] { 0, 1, 2 }, new int[] { 1, 2, 3 } };
                PolygonUtility.DrawPolygonHighlightIfMouseover(verticies2, triangles2);
                PolygonUtility.DrawPolygon(verticies2, triangles2, PsycheYellowMat);

            }

            //Matrix4x4 guiMatrix = GUI.matrix;
            //string logString = "START";
            //for (int i = 0; i < 4; i++)
            //{
            //    logString += "\n{ ";
            //    for (int j = 0; j < 3; j++)
            //    {
            //        logString += guiMatrix[i, j].ToString() + ", ";
            //    }
            //    logString += guiMatrix[i, 3].ToString() + " }";
            //}
            //logString += "\nEND";
            //Log.Message(logString);



            //Vector2 screenV1 = GUIUtility.GUIToScreenPoint(guiV1);
            //Vector2 screenV2 = GUIUtility.GUIToScreenPoint(guiV2);
            //Vector2 screenV3 = GUIUtility.GUIToScreenPoint(guiV3);
            //Vector2 screenV4 = GUIUtility.GUIToScreenPoint(guiV4);
            //Log.Message("V1: " + guiV1 + " -> " + screenV1);
            //Log.Message("V2: " + guiV2 + " -> " + screenV2);
            //Log.Message("V3: " + guiV3 + " -> " + screenV3);
            //Log.Message("V4: " + guiV4 + " -> " + screenV4);
            //Log.Message("Screen dimensions: (" + Screen.width + ", " + Screen.height + ")");

            //GL.PushMatrix();
            //PsycheMat.SetPass(0);

            //GL.LoadOrtho();
            //GL.LoadIdentity();
            //var proj = Matrix4x4.Ortho(0, 1, 0, 1, -1, 100);
            //GL.LoadProjectionMatrix(proj);
            //GL.LoadProjectionMatrix(Matrix4x4.identity);
            //GL.LoadProjectionMatrix(guiMatrix);

            //GL.MultMatrix(GUI.matrix.inverse);

            //GL.Begin(GL.QUADS);

            //GL.Vertex(new Vector2(0f, 0f));
            //GL.Vertex(new Vector2(0.5f * Screen.width, 0f));
            //GL.Vertex(new Vector2(0.25f * Screen.width, 0.25f * Screen.height));
            //GL.Vertex(new Vector2(0f, 0.25f * Screen.height));

            //GL.Vertex(guiV1);
            //GL.Vertex(guiV2);
            //GL.Vertex(guiV3);
            //GL.Vertex(guiV4);

            //GL.Vertex(screenV1);
            //GL.Vertex(screenV2);
            //GL.Vertex(screenV3);
            //GL.Vertex(screenV4);

            //GL.Vertex3(0.0f, 0.0f, 0.0f);
            //GL.Vertex3(50f, 100f, 0.0f);
            //GL.Vertex3(100f, 50f, 0.0f);
            //GL.Vertex3(50f, 0.0f, 0.0f);

            //GL.Vertex3(0f, 50.23f, 0f);
            //GL.Vertex3(50.23f, 0f, 0f);
            //GL.Vertex3(100.23f, 50.23f, 0f);
            //GL.Vertex3(50.23f, 100.23f, 0f);

            //GL.End();

            //GL.PopMatrix();

            // MESH TEST
            //Mesh meshtest = new Mesh();
            //meshtest.vertices = new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 200f, 0f), new Vector3(200f, 200f, 0f) };
            //meshtest.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            //meshtest.triangles = new int[] { 0, 1, 2 };
            //Graphics.DrawMesh(meshtest, new Vector3(50f, 50f, 0f), Quaternion.Euler(0f, 0f, 0f), PsycheMat, 0);
            //Graphics.DrawMesh(meshtest, new Vector3(50f, 50f, 0f), Quaternion.Euler(0f, 0f, 0f), PsycheMat, 10);
            //Graphics.DrawMesh(meshtest, new Vector3(50f, 50f, 0f), Quaternion.Euler(0f, 0f, 0f), PsycheMat, 20);
            //Graphics.DrawMeshNow(meshtest, new Vector3(50f, 50f, 0f), Quaternion.Euler(0f, 0f, 0f), 0);

            GUI.EndGroup();

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
                    labelNodeList.Add(new Tuple<string, float, PersonalityNode>(node.def.label, rating, node));
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
                //float yAxis = 0.5f - labelNodeList[i].Item2;
                //float weight = Mathf.Sqrt(2f * Mathf.Abs(yAxis));
                //int category = Mathf.RoundToInt(Categories * (0.5f - yAxis * weight));
                int category = Mathf.Clamp(Mathf.FloorToInt((Categories + 1) * labelNodeList[i].Item2), 0, Categories);
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

            Rect viewRect = new Rect(0f, 0f, personalityRect.width - 20f, categoryNodeHeight * viewRectHeight + 20f);
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
                //float categoryRectX = viewRect.xMin + BoundaryPadding;
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
            //Font oldFont = style.font;
            TextAnchor oldAlignment = style.alignment;
            int oldFontSize = style.fontSize;
            //style.font = ;
            style.alignment = TextAnchor.MiddleCenter;
            Rect textRect;
            List<float> CreativeLetterWidths = new List<float>();
            for (int i = 0; i < allNodes.Count(); i++)
            {
                PersonalityNode node = allNodes[i];
                float displacement = node.AdjustedRating - 0.5f;
                //style.fontSize = Mathf.FloorToInt(60f * Mathf.Abs(displacement)) + 5;
                //style.fontSize = Mathf.RoundToInt(Mathf.Clamp(35f * Mathf.Pow(Mathf.Abs(2 * displacement), 0.5f), 10f, 35f));
                //style.fontSize = Mathf.FloorToInt(30f * Mathf.Pow(Mathf.Abs(2 * displacement), 0.25f)) + 5;
                //style.fontSize = Mathf.RoundToInt(Mathf.Lerp(35f, 8f, i / 36f));
                style.fontSize = Mathf.RoundToInt(Mathf.Lerp(35f, 8f, Mathf.Pow(i / 36f, 1f)));
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
            TooltipHandler.TipRegion(highlightRect, delegate
            {
                string nodeName = "";
                Color nodeColor = HSVtoColor(node.def.nodeHSV);
                Color oppositeColor = HSVtoColor(node.def.oppositeHSV);
                if (node.def.label == "creative")
                {
                    for (int c = 0; c < CreativeLetters.Count(); c++)
                    {
                        nodeName += CreativeLetters[c].Colorize(HSVtoColor(CreativeHSVs[c]));
                    }
                }
                else
                {
                    nodeName = node.def.descriptionLabel.Colorize(nodeColor);
                }
                nodeName = "<b>" + nodeName + "</b>";
                string tooltipString = node.def.description.ReplaceFirst("{0}", nodeName);
                string antonymString = " " + "AntonymColon".Translate() + " ";
                tooltipString += antonymString + "<b>" + node.def.oppositeName.Colorize(oppositeColor) + "</b>";
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
            }, 613261 + i * 612);
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
            float x = Mathf.Round(center.x + radius * Mathf.Sin(Mathf.Deg2Rad * angle));
            float y = Mathf.Round(center.y - radius * Mathf.Cos(Mathf.Deg2Rad * angle));
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
            float length = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff) + width;
            if (length > 0.01f)
            {
                float z = Mathf.Atan2(yDiff, xDiff) * Mathf.Rad2Deg;
                Matrix4x4 m = Matrix4x4.TRS(start, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-start, Quaternion.identity, Vector3.one);
                Rect position = new Rect(start.x - 0.5f * width, start.y - 0.5f * width, length, width);
                GL.PushMatrix();
                GL.MultMatrix(m);
                GUI.DrawTexture(position, PsycheLineTex, ScaleMode.StretchToFill, alphaBlend: true, 0f, color, 0f, 0f);
                GL.PopMatrix();
            }
        }
    }
}