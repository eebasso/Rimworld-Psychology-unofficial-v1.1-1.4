using UnityEngine;
using Verse;

namespace Psychology;

public class PsychColor
{
    public static Color ButtonDarkColor = new Color(0.623529f, 0.623529f, 0.623529f);
    public static Color ButtonLightColor = new Color(0.97647f, 0.97647f, 0.97647f);
    public static Color LineColor = new Color(1f, 1f, 1f, 0.5f);
    public static Color ModEntryLineColor = new Color(0.3f, 0.3f, 0.3f);

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

}

