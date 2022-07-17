using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    [StaticConstructorOnStartup]
    class PsychologyTexCommand
    {

        public static readonly Texture2D OfficeTable = ContentFinder<Texture2D>.Get("UI/Commands/MayoralTable", true);
        public static readonly Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/ButtonPsyche", true);
        //public static readonly Texture2D TestTexture = ContentFinder<Texture2D>.Get("UI/RedSquare", true);
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
}
