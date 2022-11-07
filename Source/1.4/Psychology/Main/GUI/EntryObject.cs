using System;
using System.Runtime;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using HarmonyLib;
using System.Reflection;

namespace Psychology;

public abstract class EntryObject<T, T2>
{
  private string controlName;
  public int ticker = 0;
  private static int instanceCounter;

  internal string buffer = "";
  internal bool openFlag = false;
  public static readonly List<KeyCode> keyCodesToUnfocus = new List<KeyCode> { KeyCode.Escape, KeyCode.KeypadEnter, KeyCode.Return };
  internal bool pressedClosedKey = false;
  internal bool alwaysOpen;
  internal T val;
  internal T2 instance;
  internal string fieldName;
  internal FieldInfo Field => AccessTools.Field(typeof(T2), nameof(fieldName));

  public T Value
  {
    get
    {
      return fieldName.NullOrEmpty() ? val : Field != null ? (T)Field.GetValue(instance) : default(T);
    }
    set
    {
      if (fieldName.NullOrEmpty())
      {
        val = value;
        return;
      }
      if (Field != null)
      {
        Field.SetValue(instance, value);
      }
      else
      {
        Log.Error("Psychology.EntryObject: FieldInfo is null");
      }
    }
  }

  public bool Active
  {
    get
    {
      return ticker > 0;
    }
    set
    {
      ticker = value ? 5 : 0;
    }
  }

  public bool CurrentlyFocused => GUI.GetNameOfFocusedControl() == controlName;

  internal abstract void TryParseBuffer(bool useBounds, T min, T max);

  internal virtual void Initialize(T2 instance, string fieldName, bool alwaysOpen = false, string controlName = null)
  {
    this.alwaysOpen = alwaysOpen;
    //this.Open = initialOpen;
    this.controlName = controlName.NullOrEmpty() ? $"EntryObject_{instanceCounter++}" : controlName;
    UpdateInstanceAndField(instance, fieldName);
  }

  public virtual void UpdateValueAndBuffer(T value)
  {
    Value = value;
    SetBufferToValue();
  }

  public virtual void UpdateInstanceAndField(T2 instance, string fieldName)
  {
    this.instance = instance;
    this.fieldName = fieldName;
    SetBufferToValue();
  }

  internal virtual void SetBufferToValue() => buffer = Value.ToString();

  public void Focus() => GUI.FocusControl(controlName);

  public virtual void Unfocus()
  {
    if (CurrentlyFocused)
    {
      UI.UnfocusCurrentControl();
    }
  }

  internal virtual bool ControlBoxIntro(Rect rect, T min = default(T), T max = default(T))
  {
    bool result;
    if (CurrentlyFocused && Event.current.type == EventType.KeyDown && keyCodesToUnfocus.Contains(Event.current.keyCode))
    {
      Active = false;
      pressedClosedKey = true;
      Unfocus();
      Event.current.Use();
    }
    if (Mouse.IsOver(rect))
    {
      if (!pressedClosedKey)
      {
        Active = true;
      }
      else if (OriginalEventUtility.EventType == EventType.MouseDown)
      {
        pressedClosedKey = false;
        Active = true;
      }
    }
    else
    {
      pressedClosedKey = false;
      if (OriginalEventUtility.EventType == EventType.MouseDown)
      {
        Active = false;
        Unfocus();
      }
    }
    if (Active)
    {
      ticker--;
      if (!openFlag)
      {
        openFlag = true;
        SetBufferToValue();
      }
      result = true;
    }
    else
    {
      if (openFlag)
      {
        openFlag = false;
        TryParseBuffer(true, min, max);
      }
      SetBufferToValue();
      result = alwaysOpen;
    }
    return result;
  }

  internal void EntryObjectTextField(Rect rectTextField, T min = default(T), T max = default(T))
  {
    //if (!openFlag)
    //{
    //  openFlag = true;
    //  SetBufferToValue();
    //}
    GUI.SetNextControlName(controlName);
    buffer = Widgets.TextField(rectTextField, buffer);
    TryParseBuffer(true, min, max);
    if (CurrentlyFocused)
    {
      Active = true;
    }
  }
}

public abstract class EntryNumeric<T, T2> : EntryObject<T, T2> where T : IConvertible
{
  public void NumericTextField(Rect rect, T min, T max, bool useArrows = true, bool useCycle = false)
  {
    NumericTextField(rect.x, rect.y, rect.width, rect.height, min, max, useArrows, useCycle);
  }

  public void NumericTextField(float x, float y, float w, float h, T min, T max, bool useArrows = true, bool useCycle = false)
  {
    //Log.Message("NumericTextField, start");
    //Rect rectTotal = new Rect(x, y, w + (useArrows ? 40f : 0f), h);
    Rect rectTotal = new Rect(x - (useArrows ? 20f : 0f), y, w + (useArrows ? 40f : 0f), h);
    bool controlBoxIntro = ControlBoxIntro(rectTotal, min, max);
    if (!controlBoxIntro && !alwaysOpen)
    {
      return;
    }
    //Rect rectField = new Rect(x + (useArrows ? 20f : 0f), y, w, h);
    Rect rectField = new Rect(x, y, w, h);
    if (useArrows)
    {
      //Rect rectBack = new Rect(x, y, 25f, h);
      //Rect rectForw = new Rect(x + w + 15f, y, 25f, h);
      Rect rectBack = new Rect(x - 20f, y, 25f, h);
      Rect rectForw = new Rect(x + w - 5f, y, 25f, h);
      if (Widgets.ButtonImage(rectBack, ContentFinder<Texture2D>.Get("bbackward", true), true))
      {
        BackwardButtonAction(min, max, useCycle);
      }
      if (Widgets.ButtonImage(rectForw, ContentFinder<Texture2D>.Get("bforward", true), true))
      {
        ForwardButtonAction(min, max, useCycle);
      }
    }
    EntryObjectTextField(rectField, min, max);
  }

  public bool Slider(Rect rect, T min, T max, bool vertical = false, int roundToDigit = -1)
  {
    float minSlider = FloatFromType(min);
    float maxSlider = FloatFromType(max);
    //Log.Message("Value: " + Value);
    float clampedValue = Mathf.Clamp(FloatFromType(Value), minSlider, maxSlider);
    //Log.Message("clampedValue: " + clampedValue);
    clampedValue = Round(clampedValue, roundToDigit);
    //Log.Message("clampedValue rounded: " + clampedValue);
    float sliderValue = vertical ? GUI.VerticalSlider(rect, clampedValue, maxSlider, minSlider) : Widgets.HorizontalSlider(rect, clampedValue, minSlider, maxSlider);
    //Log.Message("sliderValue: " + sliderValue);
    sliderValue = Round(sliderValue, roundToDigit);
    //Log.Message("sliderValue rounded: " + sliderValue);
    //if (roundTo > 0f)
    //{
    //  sliderValue = (float)Math.Round(clampedValue / roundTo) * roundTo;
    //}
    if (!TypeFromFloat(sliderValue).Equals(TypeFromFloat(clampedValue)))
    {
      //Log.Message("UpdateValueAndBuffer");
      UpdateValueAndBuffer(TypeFromFloat(sliderValue));
      return true;
    }
    return false;
  }

  //internal static int SigDigits(float roundTo) => Mathf.CeilToInt(-Mathf.Log10(roundTo));

  internal static float Round(float value, int roundToDigits)
  {
    return (float)Math.Round(value, roundToDigits);
    //if (roundTo < 0f)
    //{
    //  return value;
    //}
    //int digits = SigDigits(roundTo);

    //return roundTo > 0f ? (float)(Math.Round(value / roundTo, 1) * roundTo) : value;
  }

  internal abstract void BackwardButtonAction(T min, T max, bool useCycle);
  internal abstract void ForwardButtonAction(T min, T max, bool useCycle);
  internal abstract float FloatFromType(T value);
  internal abstract T TypeFromFloat(float floatVal);

}

public class EntryInt : EntryInt<object>
{
  public EntryInt(int initialValue = 0, bool alwaysOpen = false, string controlName = null)
  {
    InitEntryInt(initialValue, alwaysOpen, controlName);
  }
}

public class EntryInt<T2> : EntryNumeric<int, T2>
{
  internal EntryInt() { return; }

  public EntryInt(T2 instance, string fieldName, bool keepOpen = false, string controlName = null)
  {
    Initialize(instance, fieldName, keepOpen, controlName);
  }

  internal void InitEntryInt(int initialValue = 0, bool alwaysOpen = false, string controlName = null)
  {
    Initialize(default(T2), null, alwaysOpen, controlName);
    Value = initialValue;
  }

  internal override void TryParseBuffer(bool useBounds = true, int min = 0, int max = 100)
  {
    if (float.TryParse(buffer, out float parsedFloat))
    {
      Value = Mathf.RoundToInt(parsedFloat);
    }
    else if (int.TryParse(buffer, out int parsedInt))
    {
      Value = parsedInt;
    }
    if (useBounds)
    {
      Value = Mathf.Clamp(Value, min, max);
    }
  }

  internal override void BackwardButtonAction(int min, int max, bool useCycle)
  {
    Value = Value - 1;
    if (Value < min)
    {
      Value = useCycle ? max : min;
    }
    SetBufferToValue();
  }

  internal override void ForwardButtonAction(int min, int max, bool useCycle)
  {
    Value = Value + 1;
    if (Value > max)
    {
      Value = useCycle ? min : max;
    }
    SetBufferToValue();
  }

  internal override float FloatFromType(int value)
  {
    return (float)value;
  }

  internal override int TypeFromFloat(float floatVal)
  {
    return (int)floatVal;
  }

}

public class EntryFloat : EntryFloat<object>
{
  public EntryFloat(float initialValue = 0f, bool alwaysOpen = false, string controlName = null, int roundToDigits = -1)
  {
    InitEntryFloat(null, null, alwaysOpen, controlName, roundToDigits);
    Value = initialValue;
  }
}

public class EntryFloat<T2> : EntryNumeric<float, T2>
{
  public int roundToDigits;
  private string format;

  internal EntryFloat() { return; }

  public EntryFloat(T2 instance, string fieldName, bool alwaysOpen = false, string controlName = null, int roundToDigits = -1)
  {
    InitEntryFloat(instance, fieldName, alwaysOpen, controlName, roundToDigits);
  }

  internal void InitEntryFloat(T2 instance, string fieldName, bool alwaysOpen = false, string controlName = null, int roundToDigits = -1)
  {
    this.roundToDigits = roundToDigits;
    Initialize(instance, fieldName, alwaysOpen, controlName);
    if (roundToDigits == 0)
    {
      format = "F";
    }
    else if (roundToDigits > 0)
    {
      format = "F" + roundToDigits.ToString();
    }
    //if (roundTo > 0f)
    //{
    //  int digits = Mathf.CeilToInt(-Mathf.Log10(roundTo));
    //  for (int i = 0; i < digits; i++)
    //  {
    //    format += "0";
    //  }
    //}
  }

  internal override void SetBufferToValue() => buffer = roundToDigits < 0 ? Value.ToString() : Value.ToString(format);

  internal override void TryParseBuffer(bool useBounds, float min, float max)
  {
    if (float.TryParse(buffer, out float parsedFloat))
    {
      SetValueRounded(parsedFloat);
    }
    if (useBounds)
    {
      Value = Mathf.Clamp(Value, min, max);
    }
  }

  internal override void BackwardButtonAction(float min, float max, bool useCycle)
  {
    Value = Value - 1f;
    if (useCycle && Value < min)
    {
      Value = max;
    }
    SetBufferToValue();
  }

  internal override void ForwardButtonAction(float min, float max, bool useCycle)
  {
    Value = Value + 1f;
    if (useCycle && Value > max)
    {
      Value = min;
    }
    SetBufferToValue();
  }

  public override void UpdateValueAndBuffer(float updatedVal)
  {
    SetValueRounded(updatedVal);
    SetBufferToValue();
  }

  //internal void SetValueRounded(float rawValue)
  //{
  //  Value = roundToDigits > 0f ? (float)Math.Round(rawValue / roundToDigits) * roundToDigits : rawValue;
  //}

  internal void SetValueRounded(float rawValue)
  {
    Value = roundToDigits < 0 ? rawValue : (float)Math.Round(rawValue, roundToDigits);
  }

  internal override float TypeFromFloat(float floatVal) => floatVal;
  internal override float FloatFromType(float value) => value;


}

public class EntryString : EntryString<object>
{
  public EntryString(string initialText = "", bool alwaysOpen = false, string controlName = null)
  {
    Initialize(null, null, alwaysOpen, controlName);
    Value = initialText;
  }
}

public class EntryString<T2> : EntryObject<string, T2>
{
  internal EntryString() { return; }

  public EntryString(T2 instance, string fieldName, bool alwaysOpen = false, string controlName = null)
  {
    Initialize(instance, fieldName, alwaysOpen, controlName);
  }

  internal override void TryParseBuffer(bool useBounds, string min, string max)
  {
    Value = buffer.NullOrEmpty() ? " " : buffer;
  }

  public void TextField(float x, float y, float w, float h)
  {
    TextField(new Rect(x, y, w, h));
  }

  public void TextField(Rect rect)
  {
    bool controlBoxIntro = ControlBoxIntro(rect);
    if (!controlBoxIntro && !alwaysOpen)
    {
      return;
    }
    EntryObjectTextField(rect);
  }
}
