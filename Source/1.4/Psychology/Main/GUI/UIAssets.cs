using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using HarmonyLib;

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

  public static readonly Texture2D SliderRailAtlas = (Texture2D)AccessTools.Field(typeof(Widgets), "SliderRailAtlas").GetValue(null);
  public static readonly Texture2D SliderHandle = (Texture2D)AccessTools.Field(typeof(Widgets), "SliderHandle").GetValue(null);
  public static readonly Color RangeControlTextColor = (Color)AccessTools.Field(typeof(Widgets), "RangeControlTextColor").GetValue(null);

  public const float SliderHandleSize = 12f;
  public static int sliderDraggingID;

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

  public static void DrawTexture(Vector2 start, Vector2 end, float width, Color color, Texture2D texture)
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
      GUI.DrawTexture(position, texture, ScaleMode.StretchToFill, alphaBlend: true, 0f, color, 0f, 0f);
      GL.PopMatrix();
    }
  }

  public static void DrawLine(Vector2 start, Vector2 end, float width, Color color)
  {
    DrawTexture(start, end, width, color, PsycheLineTex);
  }

  public static float HorizontalSlider(Rect rect, float value, float min, float max)
  {
    Vector2 start = new Vector2(rect.xMin, rect.center.y);
    Vector2 end = new Vector2(rect.xMax, rect.center.y);
    return Slider(start, end, rect.height, value, min, max);
  }

  public static float VerticalSlider(Rect rect, float value, float min, float max)
  {
    Vector2 start = new Vector2(rect.center.x, rect.yMin);
    Vector2 end = new Vector2(rect.center.x, rect.yMax);
    return Slider(start, end, rect.width, value, min, max);
  }

  public static float Slider(Vector2 start, Vector2 end, float width, float value, float min, float max)
  {
    float num = value;

    Vector2 diff = end - start;
    float length = diff.magnitude;
    float altasWidth = 8f;
    if (length < SliderHandleSize)
    {
      return value;
    }
    float radians = Mathf.Atan2(diff.y, diff.x);
    float z = radians * Mathf.Rad2Deg;

    float c = diff.x / length;
    float s = diff.y / length;
    Vector2 unitVector = diff / length;


    Vector2 atlasStart = start + 0.5f * SliderHandleSize * unitVector;
    Vector2 atlasEnd = end - 0.5f * SliderHandleSize * unitVector;
    Vector2 atlasDiff = atlasEnd - atlasStart;
    float atlasMagnitude = atlasDiff.magnitude;



    Rect atlasRect = new Rect(start.x + 0.5f * SliderHandleSize, start.y - 0.5f * altasWidth, atlasMagnitude, altasWidth);

    float x = start.x + atlasMagnitude * Mathf.InverseLerp(min, max, num);

    Rect handleRect = new Rect(x, atlasRect.center.y - 0.5f * SliderHandleSize, SliderHandleSize, SliderHandleSize);

    Matrix4x4 m = Matrix4x4.TRS(start, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-start, Quaternion.identity, Vector3.one);
    GL.PushMatrix();
    GL.MultMatrix(m);
    GUI.color = RangeControlTextColor;
    Widgets.DrawAtlas(atlasRect, SliderRailAtlas);
    GUI.color = Color.white;
    GUI.DrawTexture(handleRect, SliderHandle);
    GL.PopMatrix();

    int hashCode = UI.GUIToScreenPoint(new Vector2(start.x, start.y)).GetHashCode();
    hashCode = Gen.HashCombine(hashCode, end.x);
    hashCode = Gen.HashCombine(hashCode, end.y);
    hashCode = Gen.HashCombine(hashCode, min);
    hashCode = Gen.HashCombine(hashCode, max);


    Vector2 orthoWidthVector = 0.5f * width * new Vector2(-s, c);
    Vector2 v1 = start + orthoWidthVector;
    Vector2 v2 = start - orthoWidthVector;
    Vector2 v3 = end - orthoWidthVector;
    Vector2 v4 = end + orthoWidthVector;
    List<Vector2> vertexList = new List<Vector2>() { v1, v2, v3, v4 };

    if (Event.current.type == EventType.MouseDown && PolygonUtility.IsMouseoverPolygon(vertexList) && sliderDraggingID != hashCode)
    {
      sliderDraggingID = hashCode;
      SoundDefOf.DragSlider.PlayOneShotOnCamera();
      Event.current.Use();
    }
    if (sliderDraggingID == hashCode && UnityGUIBugsFixer.MouseDrag())
    {
      Vector2 mousePositionShifted = Event.current.mousePosition - atlasStart;
      float val = (mousePositionShifted.x * atlasDiff.x + mousePositionShifted.y * atlasDiff.y) / atlasDiff.sqrMagnitude;
      num = Mathf.Clamp(val * (max - min) + min, min, max);
      if (Event.current.type == EventType.MouseDrag)
      {
        Event.current.Use();
      }
    }

    if (value != num)
    {
      AccessTools.Method(typeof(Widgets), "CheckPlayDragSliderSound").Invoke(null, new object[] { });
    }
    return num;
  }




  public static bool ButtonLabel(Rect rect, string label, bool doMouseOverSound = true)
  {
    Widgets.Label(rect, label);
    return Widgets.ButtonInvisible(rect, doMouseOverSound);
  }

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
