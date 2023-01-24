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
using System.Runtime.Remoting.Contexts;
using UnityEngine.UIElements;

namespace Psychology;

public class EntryControlIDTracker
{
  private int controlID = -1;
  private static int instanceCounter = 0;
  public int ControlID
  {
    get
    {
      if (controlID == -1)
      {
        controlID = NewControlID();
      }
      return controlID;
    }
  }
  private static int NewControlID()
  {
    instanceCounter++;
    return "Psychology".GetHashCode() + 204982 + 37 * instanceCounter;
  }
}

public abstract class EntryObject<T, T2>
{
  public int ticker = 0;
  public const int maxTicks = 3;
  private bool tickUp = false;
  private bool activeFlag = false;

  public const bool shiftTextFieldForArrows = false;

  public bool alwaysOpen;

  private T val = default(T);
  public T2 instance;
  private string fieldName;

  protected string BufferText = "";
  private EntryControlIDTracker controlIDTracker;

  public static readonly List<KeyCode> keyCodesToUnfocus = new List<KeyCode> { KeyCode.Escape, KeyCode.KeypadEnter, KeyCode.Return };
  private bool pressedUnfocusKey = false;

  //private static MethodInfo methodInfoTemp;

  //private string controlName;
  //private static int instanceCounter;
  //internal TextEditor textEditor;

  internal int ControlID => controlIDTracker.ControlID;

  //internal int ControlID;

  //internal string buffer
  //{
  //  get => textEditor.text;
  //  set => textEditor.text = value;
  //}

  //internal int controlID
  //{
  //  get => textEditor.controlID;
  //  set => textEditor.controlID = value;
  //}


  internal FieldInfo Field => fieldName.NullOrEmpty() ? null : AccessTools.Field(typeof(T2), fieldName);

  public T Value
  {
    get
    {
      FieldValueCheck();
      return val;
      //return fieldName.NullOrEmpty() ? val : Field != null ? (T)Field.GetValue(instance) : default(T);
    }
    set
    {
      val = value;
      if (!fieldName.NullOrEmpty())
      {
        Field.SetValue(instance, value);
      }
    }
  }

  // Idea: try ticking up and ticking down and use openFlag as well
  //public bool Active
  //{
  //  get => ticker > 0;
  //  set => ticker = value ? 5 : 0;
  //}
  public bool Active
  {
    get
    {
      if (ticker == maxTicks)
      {
        activeFlag = true;
        return true;
      }
      if (ticker == 0)
      {
        if (activeFlag)
        {
          SetBufferToValue();
        }
        activeFlag = false;
        return false;
      }
      return activeFlag;
    }
    //set
    //{
    //  tickUp = value;
    //  activeFlag = value;
    //  ticker = value ? maxTicks : 0;
    //}
  }

  //public bool CurrentlyFocused => GUI.GetNameOfFocusedControl() == controlName;

  public bool CurrentlyFocused => GUIUtility.keyboardControl == ControlID;

  //public void Focus()
  //{
  //  GUIUtility.hotControl = textEditor.controlID;
  //  GUIUtility.keyboardControl = textEditor.controlID;
  //  //editor.m_HasFocus = true;
  //  textEditor.SetMemberValue("m_HasFocus", true);
  //  textEditor.MoveCursorToPosition(Event.current.mousePosition);
  //}

  public virtual void Unfocus()
  {
    if (CurrentlyFocused)
    {
      //UI.UnfocusCurrentTextField();
      GUI.FocusControl(null);
      SetBufferToValue();
      tickUp = false;
    }
  }

  private void FieldValueCheck()
  {
    if (!fieldName.NullOrEmpty())
    {
      T fieldValue = (T)Field.GetValue(instance);
      if (!val.Equals(fieldValue))
      {
        val = fieldValue;
        SetBufferToValue();
      }
    }
  }

  internal abstract void TryParseBuffer(bool useBounds, T min, T max);

  internal virtual void Initialize(T2 instance, string fieldName, bool alwaysOpen = false, string controlName = null)
  {
    this.controlIDTracker = new EntryControlIDTracker();
    this.alwaysOpen = alwaysOpen;
    UpdateInstanceAndField(instance, fieldName);
    //instanceCounter++;
    //this.alwaysOpen = alwaysOpen;
    //this.Open = initialOpen;
    //this.controlName = !controlName.NullOrEmpty() ? controlName : "EntryObject_" + instanceCounter + (fieldName.NullOrEmpty() ? "" : "_" + fieldName);
    //int controlID = 138532 + 37 * instanceCounter;
    //textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID);
    //textEditor.SaveBackup();
    //this.ControlID = controlID;
    //UpdateInstanceAndField(instance, fieldName);
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

  internal virtual void SetBufferToValue() => BufferText = Value.ToString();

  //public void Focus() => GUI.FocusControl(controlName);

  internal virtual bool EntryObjectTextField(Rect rect, T min, T max, bool useArrows = true, bool useCycle = false)
  {
    return EntryObjectTextField(rect.x, rect.y, rect.width, rect.height, min, max, useArrows, useCycle);
  }

  internal virtual bool EntryObjectTextField(float x, float y, float w, float h, T min, T max, bool useArrows = true, bool useCycle = false)
  {
    bool result = false;
    Rect rectTotal = new Rect(x - (useArrows && !shiftTextFieldForArrows ? 20f : 0f), y, w + (useArrows ? 40f : 0f), h);
    ControlBoxIntro(rectTotal);
    if (Active || alwaysOpen)
    {
      Rect rectTextField = new Rect(x + (useArrows && shiftTextFieldForArrows ? 20f : 0f), y, w, h);
      //Rect rectTextField = new Rect(x, y, w, h);
      DrawArrows(x, y, w, h, min, max, useArrows, useCycle);
      result = DrawTextField(rectTextField, min, max);
      CurrentlyFocusedChecks(rectTotal);
    }
    ControlBoxOutro(rectTotal);
    return result;
  }

  internal virtual void ControlBoxIntro(Rect rect)
  {
    if (Mouse.IsOver(rect))
    {
      if (!pressedUnfocusKey)
      {
        tickUp = true;
      }
      //else if (OriginalEventUtility.EventType == EventType.MouseDown)
      else if (Event.current.type == EventType.MouseDown)
      {
        pressedUnfocusKey = false;
        tickUp = true;
        //activeFlag = true;
      }
    }
    else
    {
      pressedUnfocusKey = false;
    }
  }

  internal virtual void DrawArrows(float x, float y, float w, float h, T min, T max, bool useArrows = true, bool useCycle = false)
  {
  }

  internal bool DrawTextField(Rect rectTextField, T min = default(T), T max = default(T))
  {
    //GUI.SetNextControlName(controlName);
    //buffer = Widgets.TextField(rectTextField, buffer);
    //CustomTextField.TextField(rectTextField, textEditor);

    if (BufferText == null)
    {
      BufferText = "";
    }
    GUIContent content = (GUIContent)AccessTools.Method(typeof(GUIContent), "Temp", new Type[] { typeof(string) }).Invoke(null, new object[] { BufferText });
    Type[] types = new Type[] { typeof(Rect), typeof(int), typeof(GUIContent), typeof(bool), typeof(int), typeof(GUIStyle) };
    MethodInfo methodInfo = AccessTools.Method(typeof(GUI), "DoTextField", types);
    methodInfo.Invoke(null, new object[] { rectTextField, ControlID, content, false, -1, Text.CurTextFieldStyle });
    BufferText = content.text;
    T oldVal = Value;
    TryParseBuffer(true, min, max);
    return !oldVal.Equals(Value);
  }

  internal virtual void CurrentlyFocusedChecks(Rect rect)
  {
    if (CurrentlyFocused)
    {
      if (Event.current.type == EventType.KeyDown && keyCodesToUnfocus.Contains(Event.current.keyCode))
      {
        pressedUnfocusKey = true;
        Unfocus();
        Event.current.Use();
      }
      else if (!Mouse.IsOver(rect) && OriginalEventUtility.EventType == EventType.MouseDown)
      //else if (!Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown)
      {
        pressedUnfocusKey = false;
        Unfocus();
      }
    }
    else if (!Mouse.IsOver(rect))
    {
      tickUp = false;
    }
  }

  internal virtual void ControlBoxOutro(Rect rect)
  {
    FieldValueCheck();
    ticker = tickUp ? Math.Min(ticker + 1, maxTicks) : Math.Max(ticker - 1, 0);
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
    EntryObjectTextField(x, y, w, h, min, max, useArrows, useCycle);
  }

  internal override void DrawArrows(float x, float y, float w, float h, T min, T max, bool useArrows = true, bool useCycle = false)
  {
    base.DrawArrows(x, y, w, h, min, max, useArrows, useCycle);
    if (useArrows)
    {
      Rect rectBack = new Rect(x, y, 25f, h);
      Rect rectForw = new Rect(x + w + 15f, y, 25f, h);
      //Rect rectBack = new Rect(x - 20f, y, 25f, h);
      //Rect rectForw = new Rect(x + w - 5f, y, 25f, h);
      if (Widgets.ButtonImage(rectBack, ContentFinder<Texture2D>.Get("bbackward", true), true))
      {
        BackwardButtonAction(min, max, useCycle);
      }
      if (Widgets.ButtonImage(rectForw, ContentFinder<Texture2D>.Get("bforward", true), true))
      {
        ForwardButtonAction(min, max, useCycle);
      }
    }
  }

  public bool Slider(Rect rect, T min, T max, bool vertical = false, string label = null, int roundToDigit = -1)
  {
    float minSlider = FloatFromType(min);
    float maxSlider = FloatFromType(max);
    ////Log.Message("Value: " + Value);
    float clampedValue = Mathf.Clamp(FloatFromType(Value), minSlider, maxSlider);
    ////Log.Message("clampedValue: " + clampedValue);
    clampedValue = Round(clampedValue, roundToDigit);
    ////Log.Message("clampedValue rounded: " + clampedValue);
    float sliderValue = vertical ? GUI.VerticalSlider(rect, clampedValue, maxSlider, minSlider) : GUI.HorizontalSlider(rect, clampedValue, minSlider, maxSlider);
    ////Log.Message("sliderValue: " + sliderValue);
    sliderValue = Round(sliderValue, roundToDigit);
    ////Log.Message("sliderValue rounded: " + sliderValue);
    //if (roundTo > 0f)
    //{
    //  sliderValue = (float)Math.Round(clampedValue / roundTo) * roundTo;
    //}
    if (!TypeFromFloat(sliderValue).Equals(TypeFromFloat(clampedValue)))
    {
      ////Log.Message("UpdateValueAndBuffer");
      UpdateValueAndBuffer(TypeFromFloat(sliderValue));
      return true;
    }
    return false;
  }

  //internal static int SigDigits(float roundTo) => Mathf.CeilToInt(-Mathf.Log10(roundTo));

  internal static float Round(float value, int roundToDigits) => roundToDigits < 0 ? value : (float)Math.Round(value, Math.Min(roundToDigits, 15));

  internal void BackwardButtonAction(T min, T max, bool useCycle) => ShiftButtonAction(min, max, useCycle, false);

  internal void ForwardButtonAction(T min, T max, bool useCycle) => ShiftButtonAction(min, max, useCycle, true);

  internal void ShiftButtonAction(T min, T max, bool useCycle, bool forward)
  {
    float shiftedValue = FloatFromType(Value) + (forward ? 1f : -1f);
    if (forward)
    {
      Value = shiftedValue < FloatFromType(max) ? TypeFromFloat(shiftedValue) : useCycle ? min : max;
    }
    else
    {
      Value = FloatFromType(min) < shiftedValue ? TypeFromFloat(shiftedValue) : useCycle ? max : min;
    }
    SetBufferToValue();
  }

  internal abstract float FloatFromType(T value);
  internal abstract T TypeFromFloat(float floatVal);
}

public class EntryInt : EntryInt<object>
{
  public EntryInt(int initialValue = 0, bool alwaysOpen = false, string controlName = null)
  {
    Initialize(null, null, alwaysOpen, controlName);
    UpdateValueAndBuffer(initialValue);
  }
}

public class EntryInt<T2> : EntryNumeric<int, T2>
{
  internal EntryInt() { return; }

  public EntryInt(T2 instance, string fieldName, bool keepOpen = false, string controlName = null)
  {
    Initialize(instance, fieldName, keepOpen, controlName);
  }

  internal override void TryParseBuffer(bool useBounds, int min = 0, int max = 100)
  {
    if (float.TryParse(BufferText, out float parsedFloat))
    {
      Value = Mathf.RoundToInt(parsedFloat);
    }
    else if (int.TryParse(BufferText, out int parsedInt))
    {
      Value = parsedInt;
    }
    if (useBounds)
    {
      Value = Mathf.Clamp(Value, min, max);
    }
  }

  //internal override void BackwardButtonAction(int min, int max, bool useCycle)
  //{
  //  Value = Value - 1;
  //  if (Value < min)
  //  {
  //    Value = useCycle ? max : min;
  //  }
  //  SetBufferToValue();
  //}

  //internal override void ForwardButtonAction(int min, int max, bool useCycle)
  //{
  //  Value = Value + 1;
  //  if (Value > max)
  //  {
  //    Value = useCycle ? min : max;
  //  }
  //  SetBufferToValue();
  //}

  internal override float FloatFromType(int value) => (float)value;
  internal override int TypeFromFloat(float floatVal) => (int)floatVal;

}

public class EntryFloat : EntryFloat<object>
{
  public EntryFloat(float initialValue = 0f, bool alwaysOpen = false, string controlName = null, int roundToDigits = -1)
  {
    InitEntryFloat(null, null, alwaysOpen, controlName, roundToDigits);
    UpdateValueAndBuffer(initialValue);
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

  internal override void SetBufferToValue() => BufferText = roundToDigits < 0 ? Value.ToString() : Value.ToString(format);

  internal override void TryParseBuffer(bool useBounds, float min = 0f, float max = 100f)
  {
    if (float.TryParse(BufferText, out float parsedFloat))
    {
      SetValueRounded(parsedFloat);
    }
    if (useBounds)
    {
      Value = Mathf.Clamp(Value, min, max);
    }
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

  internal void SetValueRounded(float rawValue) => Value = Round(rawValue, roundToDigits);

  internal override float FloatFromType(float value) => value;
  internal override float TypeFromFloat(float floatVal) => floatVal;

}

public class EntryString : EntryString<object>
{
  public EntryString(string initialText = "", bool alwaysOpen = false, string controlName = null)
  {
    Initialize(null, null, alwaysOpen, controlName);
    UpdateValueAndBuffer(initialText);
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
    Value = BufferText.NullOrEmpty() ? " " : BufferText;
  }
}

