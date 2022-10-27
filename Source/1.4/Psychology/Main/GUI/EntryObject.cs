using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Psychology;

public abstract class EntryObject
{
    public string controlName;
    public int ticker = 0;

    private static int instanceCounter;
    internal string buffer = "";
    internal bool openFlag = false;
    public static readonly List<KeyCode> keyCodesToClose = new List<KeyCode> { KeyCode.Escape, KeyCode.KeypadEnter, KeyCode.Return };
    internal bool pressedClosedKey = false;
    internal bool alwaysOpen;

    public bool Open
    {
        get
        {
            if (alwaysOpen)
            {
                return true;
            }
            return ticker > 0;
        }
        set
        {
            if (alwaysOpen)
            {
                ticker = 5;
                return;
            }
            ticker = value ? 5 : 0;
        }
    }

    public bool CurrentlyFocused => GUI.GetNameOfFocusedControl() == controlName;

    public virtual void Unfocus()
    {
        if (CurrentlyFocused)
        {
            //Log.Message("Unfocus");
            UI.UnfocusCurrentControl();
        }
    }

    public void Focus()
    {
        GUI.FocusControl(controlName);
    }

    internal virtual void Initialize(string customControlName = null, bool initialOpen = false, bool keepOpen = false)
    {
        alwaysOpen = keepOpen;
        Open = initialOpen;
        controlName = customControlName == null ? $"EntryObject_{instanceCounter++}" : customControlName;
    }

    internal void ControlBoxIntro(Rect rect)
    {
        //Log.Message("ControlBoxIntro, start");
        if (CurrentlyFocused && Event.current.type == EventType.KeyDown && keyCodesToClose.Contains(Event.current.keyCode))
        {
            Open = false;
            pressedClosedKey = true;
            Unfocus();
            Event.current.Use();
        }
        if (Mouse.IsOver(rect))
        {
            //Log.Message("ControlBoxIntro, Mouse.IsOver");
            if (!pressedClosedKey)
            {
                Open = true;
            }
        }
        else
        {
            //Log.Message("ControlBoxIntro, !Mouse.IsOver");
            pressedClosedKey = false;
            if (OriginalEventUtility.EventType == EventType.MouseDown)
            {
                //Log.Message("MouseDown");
                Open = false;
                Unfocus();
            }
        }
    }

    public abstract class EntryNumeric : EntryObject
    {
        internal override void Initialize(string customControlName = null, bool initialOpen = false, bool alwaysOpen = false)
        {
            SetBufferToValue();
            base.Initialize(customControlName, initialOpen, alwaysOpen);
        }

        public override void Unfocus()
        {
            base.Unfocus();
            SetBufferToValue();
        }

        private bool NumbericControlBoxIntro(Rect rect, float min, float max)
        {
            //Log.Message("NumbericControlBoxIntro, start");
            ControlBoxIntro(rect);
            if (!Open)
            {
                if (openFlag)
                {
                    openFlag = false;
                    TryParseBuffer();
                    ClampValue(min, max);
                }
                SetBufferToValue();
                return false;
            }
            ticker--;
            return true;
        }

        public void NumericTextField(Rect rect, float min, float max, bool useArrows = false, bool useCycle = false)
        {
            NumericTextField(rect.x, rect.y, rect.width, rect.height, min, max, useArrows, useCycle);
        }

        public void NumericTextField(float x, float y, float w, float h, float min, float max, bool useArrows = true, bool useCycle = false)
        {
            //Log.Message("NumericTextField, start");
            Rect rectTotal = new Rect(x, y, w + (useArrows ? 40f : 0f), h);
            bool controlBoxIntro = NumbericControlBoxIntro(rectTotal, min, max);
            if (!controlBoxIntro && !alwaysOpen)
            {
                return;
            }
            Rect rectField = new Rect(x + (useArrows ? 20f : 0f), y, w, h);
            if (useArrows)
            {
                Rect rectBack = new Rect(x, y, 25f, h);
                Rect rectForw = new Rect(x + w + 15f, y, 25f, h);
                if (Widgets.ButtonImage(rectBack, ContentFinder<Texture2D>.Get("bbackward", true), true))
                {
                    //Log.Message("NumericTextField, back button pressed");
                    BackwardButtonAction(min, max, useCycle);
                }
                if (Widgets.ButtonImage(rectForw, ContentFinder<Texture2D>.Get("bforward", true), true))
                {
                    //Log.Message("NumericTextField, back button pressed");
                    ForwardButtonAction(min, max, useCycle);
                }
            }
            EntryObjectTextField(rectField, min, max);
        }

        private void EntryObjectTextField(Rect rectTextField, float min, float max)
        {
            if (!openFlag)
            {
                openFlag = true;
                ClampValue(min, max);
                SetBufferToValue();
            }
            //string controlName = "TextFieldInt" + "_x_" + rect.x.ToString("F0") + "_y_" + rect.y.ToString("F0");
            GUI.SetNextControlName(controlName);
            buffer = Widgets.TextField(rectTextField, buffer);
            if (CurrentlyFocused)
            {
                Open = true;
                //ticker = maxTicker;
            }
            TryParseBuffer();
            ClampValue(min, max);
            //if (justOpened)
            //{
            //  Focus();
            //}
        }

        public abstract void SetBufferToValue();

        public abstract void ClampValue(float min, float max);

        public abstract void TryParseBuffer();

        public abstract void BackwardButtonAction(float min, float max, bool useCycle);

        public abstract void ForwardButtonAction(float min, float max, bool useCycle);
    }
}

public class EntryInt : EntryObject.EntryNumeric
{
    public int valInt;

    public EntryInt(int initialVal = 0, string customControlName = null, bool initialOpen = false, bool keepOpen = false)
    {
        valInt = initialVal;
        Initialize(customControlName, initialOpen, keepOpen);
    }

    public override void SetBufferToValue()
    {
        buffer = valInt.ToString();
    }

    public override void ClampValue(float min, float max)
    {
        valInt = (int)Mathf.Clamp(valInt, min, max);
    }

    public override void TryParseBuffer()
    {
        if (float.TryParse(buffer, out float parsedFloat))
        {
            valInt = Mathf.RoundToInt(parsedFloat);
        }
        else if (int.TryParse(buffer, out int parsedInt))
        {
            valInt = parsedInt;
        }
    }

    public override void BackwardButtonAction(float min, float max, bool useCycle)
    {
        valInt--;
        if (valInt < min)
        {
            valInt = useCycle ? (int)max : (int)min;
        }
        SetBufferToValue();
    }

    public override void ForwardButtonAction(float min, float max, bool useCycle)
    {
        valInt++;
        if (valInt > max)
        {
            valInt = useCycle ? (int)min : (int)max;
        }
        SetBufferToValue();
    }

    //public static EntryInt GetEntryForScenPart(ScenPart part, int intialialValue)
    //{
    //  if (!ScenPartDict.TryGetValue(part, out EntryInt entryInt))
    //  {
    //    //Log.Message("GetEntryForScenPart, initializing " + part);
    //    entryInt = new EntryInt(intialialValue);
    //    ScenPartDict[part] = entryInt;
    //  }
    //  return entryInt;
    //}

    public void UpdateValueAndBuffer(int updatedVal)
    {
        valInt = updatedVal;
        SetBufferToValue();
    }

    public void HorizontalSlider(Rect rect, int min = 0, int max = 100, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
    {
        int valIntClamped = Mathf.Clamp(valInt, min, max);
        int sliderInt = (int)Widgets.HorizontalSlider(rect, valIntClamped, min, max, middleAlignment, label, leftAlignedLabel, rightAlignedLabel, roundTo);
        if (sliderInt != valIntClamped)
        {
            UpdateValueAndBuffer(sliderInt);
        }
    }
}

public class EntryFloat : EntryObject.EntryNumeric
{
    public float valFloat;
    public int roundToDigit;
    private string format;

    public EntryFloat(float initialVal = 0f, string customControlName = null, bool initialOpen = false, bool alwaysOpen = false, int roundToDigit = -1)
    {
        this.roundToDigit = roundToDigit;
        SetValueRounded(initialVal);
        Initialize(customControlName, initialOpen, alwaysOpen);
        format = "0.";
        for (int i = 0; i < roundToDigit; i++)
        {
            format += "0";
        }
        //Log.Message("EntryFloat, roundToDigit = " + this.roundToDigit);
    }

    public override void SetBufferToValue()
    {
        SetValueRounded(valFloat);
        buffer = roundToDigit < 0 ? valFloat.ToString() : valFloat.ToString(format);
    }

    public override void ClampValue(float min, float max)
    {
        SetValueRounded(Mathf.Clamp(valFloat, min, max));
    }

    public override void TryParseBuffer()
    {
        if (float.TryParse(buffer, out float parsedFloat))
        {
            SetValueRounded(parsedFloat);
        }
    }

    public override void BackwardButtonAction(float min, float max, bool useCycle)
    {
        valFloat = valFloat - 1f;
        if (useCycle && valFloat < min)
        {
            valFloat = max;
        }
        SetBufferToValue();
    }

    public override void ForwardButtonAction(float min, float max, bool useCycle)
    {
        valFloat = valFloat + 1f;
        if (useCycle && valFloat > max)
        {
            valFloat = min;
        }
        SetBufferToValue();
    }

    public void UpdateValueAndBuffer(float updatedVal)
    {
        SetValueRounded(updatedVal);
        SetBufferToValue();
    }

    public void SetValueRounded(float rawValue)
    {
        if (roundToDigit < 0)
        {
            valFloat = rawValue;
            return;
        }
        valFloat = (float)Math.Round(rawValue, roundToDigit);
    }

    public bool Slider(Rect rect, float min = 0, float max = 100, bool vertical = false, int roundToDigit = 1)
    {
        float valFloatClamped = Mathf.Clamp(valFloat, min, max);
        if (roundToDigit >= 0)
        {
            valFloatClamped = (float)Math.Round(valFloatClamped, roundToDigit);
        }
        float sliderFloat = vertical ? GUI.VerticalSlider(rect, valFloatClamped, max, min) : Widgets.HorizontalSlider(rect, valFloatClamped, min, max, false);
        if (roundToDigit >= 0)
        {
            sliderFloat = (float)Math.Round(sliderFloat, roundToDigit);
        }
        if (sliderFloat != valFloatClamped)
        {
            UpdateValueAndBuffer(sliderFloat);
            return true;
        }
        return false;
    }
}

public class EntryString
{

}
