using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Psychology
{

    public static class PolygonTooltipHandler
    {
        private static Dictionary<int, ActiveTip> activeTips = new Dictionary<int, ActiveTip>();

        private static int frame = 0;

        private static List<int> dyingTips = new List<int>(32);

        private static List<ActiveTip> drawingTips = new List<ActiveTip>();

        private static Func<ActiveTip, ActiveTip, int> compareTooltipsByPriorityCached = CompareTooltipsByPriority;

        public static void ClearTooltipsFrom(List<Vector2> vertices)
        {
            //Log.Message("Start ClearTooltipsFrom");
            if (Event.current.type != EventType.Repaint || !PolygonUtility.IsMouseoverPolygon(vertices))
            {
                return;
            }
            dyingTips.Clear();
            foreach (KeyValuePair<int, ActiveTip> activeTip in activeTips)
            {
                if (activeTip.Value.lastTriggerFrame == frame)
                {
                    dyingTips.Add(activeTip.Key);
                }
            }
            for (int i = 0; i < dyingTips.Count; i++)
            {
                activeTips.Remove(dyingTips[i]);
            }
        }

        public static void TipRegion(List<Vector2> vertices, Func<string> textGetter, int uniqueId)
        {
            //Log.Message("Start TipRegion(List<Vector2> vertices, Func<string> textGetter, int uniqueId)");
            TipRegion(vertices, new TipSignal(textGetter, uniqueId));
        }

        public static void TipRegionByKey(List<Vector2> vertices, string key)
        {
            if (PolygonUtility.IsMouseoverPolygon(vertices) || DebugViewSettings.drawTooltipEdges)
            {
                TipRegion(vertices, key.Translate());
            }
        }

        public static void TipRegionByKey(List<Vector2> vertices, string key, NamedArgument arg1)
        {
            if (PolygonUtility.IsMouseoverPolygon(vertices) || DebugViewSettings.drawTooltipEdges)
            {
                TipRegion(vertices, key.Translate(arg1));
            }
        }

        public static void TipRegionByKey(List<Vector2> vertices, string key, NamedArgument arg1, NamedArgument arg2)
        {
            if (PolygonUtility.IsMouseoverPolygon(vertices) || DebugViewSettings.drawTooltipEdges)
            {
                TipRegion(vertices, key.Translate(arg1, arg2));
            }
        }

        public static void TipRegionByKey(List<Vector2> vertices, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
        {
            if (PolygonUtility.IsMouseoverPolygon(vertices) || DebugViewSettings.drawTooltipEdges)
            {
                TipRegion(vertices, key.Translate(arg1, arg2, arg3));
            }
        }

        public static void TipRegion(List<Vector2> vertices, TipSignal tip)
        {
            //Log.Message("TipRegion: start");
            bool bool1 = Event.current.type == EventType.Repaint;
            bool bool2 = PolygonUtility.IsMouseoverPolygon(vertices);
            bool bool3 = DebugViewSettings.drawTooltipEdges;
            bool bool4 = tip.textGetter != null;
            bool bool5 = !tip.text.NullOrEmpty();
            bool bool6 = !SteamDeck.KeyboardShowing;
            //if (bool2)
            //{
            //    Log.Message("bools are: " + bool1 + ", {" + bool2 + ", " + bool3 + "}, {" + bool4 + ", " + bool5 + "}, " + bool6);
            //}
            if (Event.current.type == EventType.Repaint && (PolygonUtility.IsMouseoverPolygon(vertices) || DebugViewSettings.drawTooltipEdges) && (tip.textGetter != null || !tip.text.NullOrEmpty()) && !SteamDeck.KeyboardShowing)
            {
                //Log.Message("TipRegion: past 1st if conditions");
                //if (DebugViewSettings.drawTooltipEdges)
                //{
                //	Widgets.DrawBox(rect);
                //}
                //bool bool7 = activeTips.ContainsKey(tip.uniqueId);
                //Log.Message("activeTips.ContainsKey(tip.uniqueId): " + bool7 + " for tip.uniqueId = " + tip.uniqueId);
                if (!activeTips.ContainsKey(tip.uniqueId))
                {
                    Log.Message("TipRegion: past 2nd if conditions");
                    ActiveTip value = new ActiveTip(tip);
                    activeTips.Add(tip.uniqueId, value);
                    activeTips[tip.uniqueId].firstTriggerTime = Time.realtimeSinceStartup;
                    Log.Message("TipRegion: end of 2nd if");
                }
                //else
                //{
                //    Log.Message("activeTips[tip.uniqueId].lastTriggerFrame: " + activeTips[tip.uniqueId].lastTriggerFrame);
                //    Log.Message("activeTips[tip.uniqueId].signal.text: " + activeTips[tip.uniqueId].signal.text);
                //    Log.Message("activeTips[tip.uniqueId].signal.textGetter: " + activeTips[tip.uniqueId].signal.textGetter());
                //}
                activeTips[tip.uniqueId].lastTriggerFrame = frame;
                activeTips[tip.uniqueId].signal.text = tip.text;
                activeTips[tip.uniqueId].signal.textGetter = tip.textGetter;
                //Log.Message("TipRegion: end of 1st if");
            }
            //Log.Message("TipRegion: end");
        }

        public static void DoTooltipGUI()
        {
            //Log.Message("Start DoTooltipGUI()");
            DrawActiveTips();
            if (Event.current.type == EventType.Repaint)
            {
                CleanActiveTooltips();
                frame++;
            }
        }

        private static void DrawActiveTips()
        {
            //Log.Message("Start DrawActiveTips()");
            if (activeTips.Count == 0)
            {
                return;
            }
            drawingTips.Clear();
            foreach (ActiveTip value in activeTips.Values)
            {
                if ((double)Time.realtimeSinceStartup > value.firstTriggerTime + (double)value.signal.delay)
                {
                    drawingTips.Add(value);
                }
            }
            if (drawingTips.Any())
            {
                drawingTips.SortStable(compareTooltipsByPriorityCached);
                Vector2 pos = CalculateInitialTipPosition(drawingTips);
                for (int i = 0; i < drawingTips.Count; i++)
                {
                    pos.y += drawingTips[i].DrawTooltip(pos);
                    pos.y += 2f;
                }
                drawingTips.Clear();
            }
        }

        private static void CleanActiveTooltips()
        {
            //Log.Message("Start CleanActiveTooltips()");
            dyingTips.Clear();
            foreach (KeyValuePair<int, ActiveTip> activeTip in activeTips)
            {
                if (activeTip.Value.lastTriggerFrame != frame)
                {
                    dyingTips.Add(activeTip.Key);
                }
            }
            for (int i = 0; i < dyingTips.Count; i++)
            {
                activeTips.Remove(dyingTips[i]);
            }
        }

        private static Vector2 CalculateInitialTipPosition(List<ActiveTip> drawingTips)
        {
            //Log.Message("Start CalculateInitialTipPosition(List < ActiveTip > drawingTips)");
            float num = 0f;
            float num2 = 0f;
            for (int i = 0; i < drawingTips.Count; i++)
            {
                Rect tipRect = drawingTips[i].TipRect;
                num += tipRect.height;
                num2 = Mathf.Max(num2, tipRect.width);
                if (i != drawingTips.Count - 1)
                {
                    num += 2f;
                }
            }
            return GenUI.GetMouseAttachedWindowPos(num2, num);
        }

        private static int CompareTooltipsByPriority(ActiveTip A, ActiveTip B)
        {
            //Log.Message("Start CompareTooltipsByPriority(ActiveTip A, ActiveTip B)");
            int num = 0 - A.signal.priority;
            int value = 0 - B.signal.priority;
            return num.CompareTo(value);
        }
    }
}