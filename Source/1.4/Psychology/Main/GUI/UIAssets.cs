using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology;

[StaticConstructorOnStartup]
public static class UIAssets
{
    public static readonly Color ButtonDarkColor = new Color(0.623529f, 0.623529f, 0.623529f);
    public static readonly Color ButtonLightColor = new Color(0.97647f, 0.97647f, 0.97647f);
    public static readonly Color LineColor = new Color(1f, 1f, 1f, 0.5f);
    public static readonly Color ModEntryLineColor = new Color(0.3f, 0.3f, 0.3f);
    public static readonly Color TitleColor = ColoredText.TipSectionTitleColor;
    public static readonly Color RatingColor = Color.yellow;

    public static readonly Texture2D OfficeTable = ContentFinder<Texture2D>.Get("UI/Commands/MayoralTable", true);
    public static readonly Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/ButtonPsyche", true);
    public static readonly Texture2D PsycheLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
    public static readonly Material PsycheYellowMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 0f, 0.5f));
    public static readonly Material PsycheHighlightMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.1f));

    public static readonly KeyCode[] unfocusKeyCodes = new KeyCode[] { KeyCode.Escape, KeyCode.KeypadEnter, KeyCode.Return };

    public static void DrawLineHorizontal(float x, float y, float length, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        Widgets.DrawLineHorizontal(x, y, length);
        GUI.color = oldColor;
    }

    public static void DrawLineVertical(float x, float y, float height, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        Widgets.DrawLineVertical(x, y, height);
        GUI.color = oldColor;
    }

    public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
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

    public static float TextCalcHeight(string text, float width)
    {
        Vector2 size = Text.CalcSize(text);
        float multiplier = Mathf.Ceil(size.x / width);
        return multiplier * size.y;
    }

    public static void TextFieldFloat(Rect rect, ref float val, ref string buffer, float min = 0f, float max = 1E+09f, string customControlName = null)
    {
        if (buffer == null)
        {
            buffer = val.ToString();
        }
        buffer = TextFieldCommon(rect, buffer, customControlName, "Float");
        if (float.TryParse(buffer, out float parsedValue))
        {
            val = Mathf.Clamp(parsedValue, min, max);
        }
    }

    public static void TextFieldInt(Rect rect, ref int val, ref string buffer, int min = 0, int max = 1000000000, string customControlName = null)
    {
        if (buffer == null)
        {
            buffer = val.ToString();
        }
        buffer = TextFieldCommon(rect, buffer, customControlName, "Int");
        if (int.TryParse(buffer, out int parsedInt))
        {
            val = Mathf.Clamp(parsedInt, min, max);
        }
        else if (float.TryParse(buffer, out float parsedFloat))
        {
            val = Mathf.Clamp(Mathf.RoundToInt(parsedFloat), min, max);
        }
    }

    public static string TextFieldCommon(Rect rect, string buffer, string customControlName, string valueTypeString)
    {
        string controlName = "TextField" + valueTypeString + "_" + (customControlName.NullOrEmpty() ? rect.x.ToString("F0") + "_" + rect.y.ToString("F0") : customControlName);
        GUI.SetNextControlName(controlName);
        buffer = Widgets.TextField(rect, buffer);
        if (GUI.GetNameOfFocusedControl() == controlName && Event.current.type == EventType.KeyDown && unfocusKeyCodes.Contains(Event.current.keyCode))
        {
            UI.UnfocusCurrentControl();
            Event.current.Use();
        }
        if (GUI.GetNameOfFocusedControl() == controlName && OriginalEventUtility.EventType == EventType.MouseDown && !Mouse.IsOver(rect.ExpandedBy(2f)))
        {
            Log.Message("TextFieldCommon, unfocus");
            UI.UnfocusCurrentControl();
        }
        return buffer;
    }

    public static bool ButtonLabel(Rect rect, string label, bool doMouseOverSound = true)
    {
        Widgets.Label(rect, label);
        return Widgets.ButtonInvisible(rect, doMouseOverSound);
    }

    //public static void DrawSlider(Vector2 start, Vector2 end, float width, ref float sliderVal)
    //{
    //    //Log.Message("DrawSlider, start");
    //    float xDiff = end.x - start.x;
    //    float yDiff = end.y - start.y;
    //    float length = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
    //    if (length > 0.01f)
    //    {
    //        //Log.Message("DrawSlider, length > 0.01f");
    //        float z = Mathf.Atan2(yDiff, xDiff) * Mathf.Rad2Deg;
    //        Matrix4x4 m = Matrix4x4.TRS(start, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-start, Quaternion.identity, Vector3.one);
    //        Rect rect = new Rect(start.x, start.y - 0.5f * width, length, width);
    //        GL.PushMatrix();
    //        GL.MultMatrix(m);
    //        //Log.Message("DrawSlider, draw horizontal slider");
    //        sliderVal = Widgets.HorizontalSlider(rect, sliderVal, 0f, 1f);
    //        GL.PopMatrix();
    //    }
    //    //Log.Message("DrawSlider, end");
    //}


}

//class ColoredHexagon
//{
//    public static readonly Texture2D WhiteHexagon = ContentFinder<Texture2D>.Get("UI/WhiteHexagon", true);

//    public static Color GetColorFromTexture(Texture2D biomeTexture)
//    {
//        Color[] colorArray = biomeTexture.GetPixels();
//        return colorArray[colorArray.Count() / 2];
//    }

//    public static void DrawColoredHexagon(Rect hexagonRect, Texture2D biomeTexture)
//    {
//        GUI.color = GetColorFromTexture(biomeTexture);
//        GUI.DrawTexture(hexagonRect, WhiteHexagon);
//        GUI.color = Color.white;
//    }

//    public static void ExampleCode()
//    {
//        Texture2D biomeTexture = ContentFinder<Texture2D>.Get("Path/To/BiomeTexture", true);
//        Rect hexagonRect = new Rect(...);
//        DrawColoredHexagon(hexagonRect, biomeTexture);
//    }
//}

//class ColoredHexagon2
//{
//    public static Color32 invisibleColor = new Color32(255, 0, 0, 255);

//    public static Texture2D GetHexagonTexture(Texture2D biomeTexture)
//    {
//        Texture2D hexagonTexture = biomeTexture;
//        int width = biomeTexture.width;
//        int height = biomeTexture.height;
//        double a = 0.25 * Math.Sqrt(3);
//        int index = 0;
//        var biomeData = biomeTexture.GetRawTextureData<Color32>();
//        for (double y = 0.5; y < height; y++)
//        {
//            double dy = Math.Abs(y / height - 0.5);
//            for (double x = 0.5; x < width; x++)
//            {
//                double dx = Math.Abs(x / width - 0.5);
//                if ((dy > a) || (a * dx + 0.25 * dy) > 0.5 * a)
//                {
//                    biomeData[index] = invisibleColor;
//                }
//                index++;
//            }
//        }
//        hexagonTexture.LoadRawTextureData(biomeData);
//        return hexagonTexture;
//    }
//}
