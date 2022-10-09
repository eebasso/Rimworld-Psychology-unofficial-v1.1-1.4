using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Psychology;

//[StaticConstructorOnStartup]
public class PolygonUtility
{
    //private static readonly Material PsycheHighlightMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.1f));

    public static float TriangleSign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public static bool IsPointInPolygon(Vector2 pt, List<Vector2> polyVertexList, float errorMargin = 0f)
    {
        int sides = polyVertexList.Count();
        bool has_neg = false;
        bool has_pos = false;
        for (int i = 0; i < sides; i++)
        {
            float d = TriangleSign(pt, polyVertexList[i], polyVertexList[(i + 1) % sides]);
            has_neg = has_neg || (d < -errorMargin);
            has_pos = has_pos || (d > errorMargin);
        }
        return !(has_neg && has_pos);
    }


    public static bool IsMouseoverPolygon(List<Vector2> polyVertexList, float errorMargin = 0f)
    {
        if (IsPointInPolygon(Event.current.mousePosition, polyVertexList, errorMargin) && !Mouse.IsInputBlockedNow)
        {
            return true;
        }
        return false;
    }

    public static void DrawPolygonHighlightIfMouseover(List<Vector2> polyVertexList, List<int[]> triangles, float errorMargin = 0f)
    {
        if (IsMouseoverPolygon(polyVertexList, errorMargin))
        {
            DrawPolygon(polyVertexList, triangles, UIAssets.PsycheHighlightMat);
        }
    }

    public static void DrawPolygon(List<Vector2> verticies, List<int[]> triangles, Material mat)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        int sides = verticies.Count();
        //GL.PushMatrix();
        mat.SetPass(0);
        //GL.LoadIdentity();
        if (sides == 3)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Vertex(verticies[0]);
            GL.Vertex(verticies[1]);
            GL.Vertex(verticies[2]);
            GL.End();
        }
        if (sides == 4)
        {
            GL.Begin(GL.QUADS);
            GL.Vertex(verticies[0]);
            GL.Vertex(verticies[1]);
            GL.Vertex(verticies[2]);
            GL.Vertex(verticies[3]);
            GL.End();
        }
        if (sides > 4)
        {
            //FILL IN THE BLANKS HERE BUT BASICALLY USE A BUNCH OF TRIANGLES AND/OR QUADS
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < triangles.Count(); i++)
            {
                GL.Vertex(verticies[triangles[i][0]]);
                GL.Vertex(verticies[triangles[i][1]]);
                GL.Vertex(verticies[triangles[i][2]]);
            }
            GL.End();
        }
        //GL.PopMatrix();
    }
}

