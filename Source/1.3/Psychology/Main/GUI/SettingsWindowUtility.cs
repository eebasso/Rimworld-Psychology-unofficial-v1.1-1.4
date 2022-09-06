using System;using System.Collections.Generic;using System.Linq;using System.Text;using System.Text.RegularExpressions;using System.Reflection;using RimWorld;using RimWorld.Planet;using Verse;using Verse.AI.Group;using Verse.Grammar;using UnityEngine;using Verse.Noise;using Unity;

namespace Psychology;public static class SettingsWindowUtility{
    //public static Rect WindowRect = new Rect(0f, 0f, 1f, 1f);
    public const float WindowWidth = 900f;
    public const float WindowHeight = 700f;
    //public static float InWidth = WindowWidth - 2f * Window.StandardMargin;
    //public static float InHeight = WindowHeight - 2f * Window.StandardMargin;


    public const float RowTopPadding = 3f;    public const float CheckboxSize = 24f;    public const float BoundaryPadding = PsycheCardUtility.BoundaryPadding;    public const float HighlightPadding = PsycheCardUtility.HighlightPadding;
    public static float UpperAreaHeight = 35f;    public static float LowerAreaHeight = 44f;
    public static float RowHeight = 34f;

    public static float SpeciesX;
    public static float SpeciesY;
    public static float SpeciesWidth;    public static float SpeciesHeight;    public static List<string> SettingNameList = new List<string> { "SexualityChanges", "KinseyMode", "EmpathyChanges", "Individuality", "Elections", "SendDateLetters", "Imprisoned", "AllowAnxiety", "Duration", "RomanceMultiplier", "RomanceChanceThreshold", "MayorAge", "TraitOpinionMultiplier" };    public static List<KinseyMode> KinseyModeList = new List<KinseyMode>() { KinseyMode.Realistic, KinseyMode.Uniform, KinseyMode.Invisible, KinseyMode.Gaypocalypse, KinseyMode.Custom };    public static Dictionary<KinseyMode, string> KinseyModeTitleDict = new Dictionary<KinseyMode, string>();
    public static List<string> SpeciesTitleList = new List<string>();
    public static List<float> SpeciesWidthList = new List<float>();
    public static List<string> SpeciesNameList = new List<string>();


    //public static List<Tuple<string, bool>> boolSettingsList;
    //public static List<Tuple<string, float, string, float, float>> floatSettingsList;

    public static List<string> settingTitleList = new List<string>();    public static List<string> settingTooltipList = new List<string>();
    //public static List<Vector2> settingSizeList = new List<Vector2>();


    //public static float settingTitleHeight = 34f;
    public static float TitleWidth = 0f;    public static float EntryWidth = Text.CalcSize("100.000").x;    public static float LeftColumnWidth;    //public static float MaxEntryWidth;


    public static float KinseyModeButtonWidth = 0f;
    //public static Vector2 customWeightTitleSize = Text.CalcSize("0");
    //public static float customWeightEntryWidth = entryWidth;
    //public static float customWeightTotalWidth = customWeightEntryWidth;
    public static float VerticalSliderWidth;
    public static float VerticalSliderHeight = 3f * RowHeight;

    public static Rect SpeciesRect;    public static Rect ViewRect;

    //public static string SaveButtonText = "Save".Translate();
    //public static Rect SaveButtonRect;

    public static string ResetButtonText = "Default".Translate();
    public static Rect ResetButtonRect;

    public static string UpdateButtonText = "ApplyUpdateButton".Translate();
    public static Rect UpdateButtonRect;    public static Vector2 NodeScrollPosition = Vector2.zero;

    //public static string romanceBySpeciesTitle = "RomanceBySpeciesTitle".Translate();

    public static KinseyMode kinseyFormulaCached;    public static List<float> kinseyWeightCustomCached = new List<float>();    public static List<string> kinseyWeightCustomBuffer = new List<string>();    public static bool enableKinseyCached;    public static bool enableEmpathyCached;    public static bool enableIndividualityCached;    public static bool enableElectionsCached;    public static bool enableDateLettersCached;    public static bool enableImprisonedDebuffCached;    public static bool enableAnxietyCached;    public static float conversationDurationCached;    public static string conversationDurationBuffer;    public static float romanceChanceMultiplierCached;    public static string romanceChanceMultiplierBuffer;    public static float romanceOpinionThresholdCached;    public static string romanceOpinionThresholdBuffer;    public static float mayorAgeCached;    public static string mayorAgeBuffer;    public static float traitOpinionMultiplierCached;    public static string traitOpinionMultiplierBuffer;    public static Dictionary<string, SpeciesSettings> speciesDictCached = new Dictionary<string, SpeciesSettings>();    public static Dictionary<string, List<string>> speciesBuffer = new Dictionary<string, List<string>>();

    // Initialize after SpeciesHelper
    public static void Initialize()    {
        SetAllCachedToSettings();
        Text.Font = GameFont.Small;
        foreach (string name in SettingNameList)        {            string title = (name + "Title").Translate();            string tooltip = (name + "Tooltip").Translate();            Vector2 size = Text.CalcSize(title);            settingTitleList.Add(title);            settingTooltipList.Add(tooltip);
            //settingSizeList.Add(size);
            //Log.Message("size.x = " + size.x);
            TitleWidth = Mathf.Max(size.x, TitleWidth);            //Log.Message("titleWidth = " + TitleWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }
        //TitleWidth += 3f * BoundaryPadding;

        KinseyModeTitleDict.Add(KinseyMode.Realistic, "KinseyMode_Realistic".Translate());        KinseyModeTitleDict.Add(KinseyMode.Uniform, "KinseyMode_Uniform".Translate());        KinseyModeTitleDict.Add(KinseyMode.Invisible, "KinseyMode_Invisible".Translate());        KinseyModeTitleDict.Add(KinseyMode.Gaypocalypse, "KinseyMode_Gaypocalypse".Translate());        KinseyModeTitleDict.Add(KinseyMode.Custom, "KinseyMode_Custom".Translate());        foreach (KeyValuePair<KinseyMode, string> kvp in KinseyModeTitleDict)        {            Vector2 size = Text.CalcSize(settingTitleList[1] + ": " + kvp.Value);            KinseyModeButtonWidth = Mathf.Max(size.x, KinseyModeButtonWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }        KinseyModeButtonWidth += 20f;
        TitleWidth = Mathf.Max(KinseyModeButtonWidth - EntryWidth, TitleWidth);
        TitleWidth += 2f * HighlightPadding;

        SpeciesTitleList.Add("SpeciesSettingsTitle".Translate());
        SpeciesTitleList.Add("EnablePsycheTitle".Translate());
        SpeciesTitleList.Add("EnableAgeGapTitle".Translate());
        SpeciesTitleList.Add("MinDatingAgeTitle".Translate());
        SpeciesTitleList.Add("MinLovinAgeTitle".Translate());

        foreach (string title in SpeciesTitleList)
        {
            SpeciesWidthList.Add(Text.CalcSize(title).x);
        }
        float entryMaxWidth = Mathf.Max(SpeciesWidthList[1], SpeciesWidthList[2], SpeciesWidthList[3], SpeciesWidthList[4], EntryWidth);
        for (int i = 1; i < SpeciesTitleList.Count(); i++)
        {
            SpeciesWidthList[i] = entryMaxWidth + 2f * HighlightPadding;
        }

        //Log.Message("test1");
        List<string> labelList = new List<string>();        List<string> repeatList = new List<string>();        foreach (ThingDef def in SpeciesHelper.humanlikeDefs)
        {
            string label = def.label;
            if (labelList.Contains(label))
            {
                repeatList.AddDistinct(label);
            }
            labelList.Add(label);
        }
        foreach (ThingDef def in SpeciesHelper.humanlikeDefs)
        {
            string label = def.label;
            if (repeatList.Contains(label))
            {
                SpeciesNameList.Add(label + " (" + def.defName + ")");
                continue;
            }
            SpeciesNameList.Add(label);
        }
        SpeciesNameList.Sort();
        float viewHeight = 0f;
        foreach (string name in SpeciesNameList)        {            SpeciesWidthList[0] = Mathf.Max(Text.CalcSize(name).x, SpeciesWidthList[0]);            viewHeight += RowHeight;        }
        SpeciesY = Window.StandardMargin + UpperAreaHeight + RowHeight + HighlightPadding;        SpeciesHeight = WindowHeight - Window.StandardMargin - LowerAreaHeight - SpeciesY;

        float scrollBarWidth = GenUI.ScrollBarWidth;
        if (SpeciesHeight > viewHeight)
        {
            scrollBarWidth = 0f;
            SpeciesHeight = viewHeight;
        }

        SpeciesWidthList[0] = Mathf.Max(EntryWidth, SpeciesWidthList[0]);
        SpeciesWidthList[0] += 2f * HighlightPadding;

        float shiftFactor = (WindowWidth - 2f * Window.StandardMargin - TitleWidth - EntryWidth - BoundaryPadding - SpeciesWidthList.Sum() - scrollBarWidth) / 2f;        TitleWidth += shiftFactor;
        SpeciesWidthList[0] += shiftFactor;

        LeftColumnWidth = TitleWidth + EntryWidth;        VerticalSliderWidth = LeftColumnWidth / 7f;        SpeciesX = Window.StandardMargin +  LeftColumnWidth + BoundaryPadding;        SpeciesWidth = WindowWidth - Window.StandardMargin - SpeciesX;

        SpeciesRect = new Rect(SpeciesX, SpeciesY, SpeciesWidth, SpeciesHeight);
        ViewRect = new Rect(0f, 0f, SpeciesWidth - scrollBarWidth, SpeciesHeight);


        //float recalcWidth = Mathf.Max(Window.CloseButSize.x, Text.CalcSize(UpdateButtonText).x + 20f);

        float numButtons = 3f;
        float spaceBetweenButtons = (WindowWidth - numButtons * Window.CloseButSize.x) / (1f + numButtons);
        float buttonY = WindowHeight - Window.FooterRowHeight;

        ResetButtonRect = new Rect(spaceBetweenButtons, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
        UpdateButtonRect = new Rect(WindowWidth - spaceBetweenButtons - Window.CloseButSize.x, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
    }    public static void DrawSettingsWindow(Rect totalRect)    {
        //Widgets.DrawLineHorizontal(totalRect.center.x - 1000f, totalRect.center.y, 2000f);
        //Widgets.DrawLineVertical(totalRect.center.x, totalRect.center.y - 1000f, 2000f);
        Widgets.EndGroup();

        //GUI.color = Color.blue;
        //Widgets.DrawLineHorizontal(totalRect.center.x + Window.StandardMargin - 1000f, totalRect.center.y, 2000f);
        //Widgets.DrawLineVertical(totalRect.center.x + Window.StandardMargin, totalRect.center.y - 1000f, 2000f);
        //GUI.color = Color.white;

        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);        Text.Font = GameFont.Small;        float xMin = Window.StandardMargin;        float yMin = Window.StandardMargin + UpperAreaHeight;
        //float xMin = Window.StandardMargin;

        //float yMin = Window.StandardMargin;
        Rect titleRect = new Rect(xMin + HighlightPadding, yMin, TitleWidth - HighlightPadding, RowHeight);        Rect entryRect = new Rect(titleRect.xMax, titleRect.y, EntryWidth, titleRect.height);
        entryRect.yMin += HighlightPadding;
        entryRect.yMax -= HighlightPadding;

        CheckboxEntry(ref titleRect, settingTitleList[0], ref entryRect, ref enableKinseyCached, settingTooltipList[0]);        if (enableKinseyCached)        {
            //Widgets.Label(titleRect, settingTitleList[1]);
            //Rect buttonRect = new Rect(entryRect.x, entryRect.y, KinseyModeButtonWidth, entryRect.height);
            Rect buttonRect = new Rect(xMin, titleRect.y, LeftColumnWidth, titleRect.height);            if (Widgets.ButtonText(buttonRect, settingTitleList[1] + ": " + KinseyModeTitleDict[kinseyFormulaCached], drawBackground: true, doMouseoverSound: true, true))            {                List<FloatMenuOption> list = new List<FloatMenuOption>();                list.Add(new FloatMenuOption(KinseyModeTitleDict[KinseyMode.Realistic], delegate                {                    kinseyFormulaCached = KinseyMode.Realistic;                }));                list.Add(new FloatMenuOption(KinseyModeTitleDict[KinseyMode.Uniform], delegate                {                    kinseyFormulaCached = KinseyMode.Uniform;                }));                list.Add(new FloatMenuOption(KinseyModeTitleDict[KinseyMode.Invisible], delegate                {                    kinseyFormulaCached = KinseyMode.Invisible;                }));                list.Add(new FloatMenuOption(KinseyModeTitleDict[KinseyMode.Gaypocalypse], delegate                {                    kinseyFormulaCached = KinseyMode.Gaypocalypse;                }));                list.Add(new FloatMenuOption(KinseyModeTitleDict[KinseyMode.Custom], delegate                {                    kinseyFormulaCached = KinseyMode.Custom;                }));                Find.WindowStack.Add(new FloatMenu(list));            }

            //Rect highlightRect = titleRect;
            //highlightRect.xMin -= HighlightPadding;
            //highlightRect.xMax = entryRect.xMax + HighlightPadding;
            //Widgets.DrawHighlightIfMouseover(highlightRect);
            TooltipHandler.TipRegion(buttonRect, delegate
            {
                return settingTooltipList[1];
            }, settingTooltipList[1].GetHashCode());            titleRect.y += RowHeight;            entryRect.y += RowHeight;

            //if (kinseyFormulaCached == KinseyMode.Custom)
            //{
            //    Rect customWeightEntryRect = new Rect(titleRect.x, titleRect.y, VerticalSliderWidth, VerticalSliderHeight);
            //    for (int i = 0; i <= 6; i++)
            //    {
            //        KinseyCustomVerticalSlider(i, ref customWeightEntryRect);
            //    }
            //    titleRect.y += customWeightEntryRect.height;
            //    entryRect.y += customWeightEntryRect.height;
            //}

            Rect customWeightEntryRect = new Rect(xMin, titleRect.y, VerticalSliderWidth, VerticalSliderHeight);
            for (int i = 0; i <= 6; i++)
            {
                KinseyCustomVerticalSlider(i, ref customWeightEntryRect);
            }
            titleRect.y += customWeightEntryRect.height;
            entryRect.y += customWeightEntryRect.height;        }        CheckboxEntry(ref titleRect, settingTitleList[2], ref entryRect, ref enableEmpathyCached, settingTooltipList[2]);        CheckboxEntry(ref titleRect, settingTitleList[3], ref entryRect, ref enableIndividualityCached, settingTooltipList[3]);        CheckboxEntry(ref titleRect, settingTitleList[4], ref entryRect, ref enableElectionsCached, settingTooltipList[4]);        CheckboxEntry(ref titleRect, settingTitleList[5], ref entryRect, ref enableDateLettersCached, settingTooltipList[5]);        CheckboxEntry(ref titleRect, settingTitleList[6], ref entryRect, ref enableImprisonedDebuffCached, settingTooltipList[6]);        CheckboxEntry(ref titleRect, settingTitleList[7], ref entryRect, ref enableAnxietyCached, settingTooltipList[7]);        NumericEntry(ref titleRect, settingTitleList[8], ref entryRect, ref conversationDurationCached, ref conversationDurationBuffer, settingTooltipList[8], 0f, 180f);        NumericEntry(ref titleRect, settingTitleList[9], ref entryRect, ref romanceChanceMultiplierCached, ref romanceChanceMultiplierBuffer, settingTooltipList[9], 0f);        NumericEntry(ref titleRect, settingTitleList[10], ref entryRect, ref romanceOpinionThresholdCached, ref romanceOpinionThresholdBuffer, settingTooltipList[10], -99f, 99f);        NumericEntry(ref titleRect, settingTitleList[11], ref entryRect, ref mayorAgeCached, ref mayorAgeBuffer, settingTooltipList[11], 0f);        NumericEntry(ref titleRect, settingTitleList[12], ref entryRect, ref traitOpinionMultiplierCached, ref traitOpinionMultiplierBuffer, settingTooltipList[12], 0f, 2f);
        //float checkboxSize = 24f;
        //float HandleEntryPadding = 3f;
        //Dictionary<string, SpeciesSettings> selection = new Dictionary<string, SpeciesSettings>();
        //Dictionary<string, List<string>> speciesBuffer = new Dictionary<string, List<string>>();

        //string buffer;

        Rect labelRect = new Rect(SpeciesX + HighlightPadding, yMin, SpeciesWidthList[0], RowHeight);        Rect psycheRect = new Rect(labelRect.xMax, labelRect.y, SpeciesWidthList[1], RowHeight);        Rect ageGapRect = new Rect(psycheRect.xMax, labelRect.y, SpeciesWidthList[2], RowHeight);        Rect minDatingAgeRect = new Rect(ageGapRect.xMax, labelRect.y, SpeciesWidthList[3], RowHeight);        Rect minLovinAgeRect = new Rect(minDatingAgeRect.xMax, labelRect.y, SpeciesWidthList[4], RowHeight);

        Widgets.Label(labelRect, SpeciesTitleList[0]);        Widgets.Label(psycheRect, SpeciesTitleList[1]);        Widgets.Label(ageGapRect, SpeciesTitleList[2]);        Widgets.Label(minDatingAgeRect, SpeciesTitleList[3]);        Widgets.Label(minLovinAgeRect, SpeciesTitleList[4]);

        float columnHeight = RowHeight + HighlightPadding + SpeciesHeight;
        ColumnHighlightAndTooltip(labelRect, columnHeight, "SpeciesSettingsTooltip".Translate());        ColumnHighlightAndTooltip(psycheRect, columnHeight, "EnablePsycheTooltip".Translate());        ColumnHighlightAndTooltip(ageGapRect, columnHeight, "EnableAgeGapTooltip".Translate());        ColumnHighlightAndTooltip(minDatingAgeRect, columnHeight, "MinDatingAgeTooltip".Translate());        ColumnHighlightAndTooltip(minLovinAgeRect, columnHeight, "MinLovinAgeTooltip".Translate());

        UIAssets.DrawLineHorizontal(SpeciesRect.x, labelRect.yMax, SpeciesRect.width, UIAssets.ModEntryLineColor);

        labelRect.y += HighlightPadding;        psycheRect.y += HighlightPadding;        ageGapRect.y += HighlightPadding;        minDatingAgeRect.y += HighlightPadding;        minLovinAgeRect.y += HighlightPadding;
        Rect testHighlightRect = new Rect(0f, 0f, minLovinAgeRect.xMax - labelRect.x, labelRect.height);        Widgets.BeginScrollView(SpeciesRect, ref NodeScrollPosition, ViewRect);        labelRect.position = new Vector2(HighlightPadding, 0f);        psycheRect.position = new Vector2(labelRect.xMax, labelRect.y);        ageGapRect.position = new Vector2(psycheRect.xMax, labelRect.y);        minDatingAgeRect.position = new Vector2(ageGapRect.xMax, labelRect.y);
        minLovinAgeRect.position = new Vector2(minDatingAgeRect.xMax, labelRect.y);
        //testHighlightRect.position = new Vector2(0f, 0f);

        Vector2 psycheVec = new Vector2(psycheRect.x, psycheRect.center.y - 0.5f * CheckboxSize);
        Vector2 ageGapVec = new Vector2(ageGapRect.x, ageGapRect.center.y - 0.5f * CheckboxSize);

        minDatingAgeRect.width = EntryWidth;
        minDatingAgeRect.yMin += HighlightPadding;
        minDatingAgeRect.yMax -= HighlightPadding;
        minLovinAgeRect.width = EntryWidth;
        minLovinAgeRect.yMin += HighlightPadding;
        minLovinAgeRect.yMax -= HighlightPadding;

        //Rect rowHighlightRect = labelRect;
        //rowHighlightRect.xMin = ViewRect.xMin;
        //rowHighlightRect.xMax = ViewRect.xMax;


        foreach (ThingDef def in SpeciesHelper.humanlikeDefs)        {            string defName = def.defName;            string label = def.label.CapitalizeFirst();            float val;            string buffer;
            //GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(labelRect, label);
            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            Widgets.Checkbox(psycheVec, ref speciesDictCached[defName].enablePsyche);            if (speciesDictCached[defName].enablePsyche)            {                Widgets.Checkbox(ageGapVec, ref speciesDictCached[defName].enableAgeGap);                val = speciesDictCached[defName].minDatingAge;                buffer = speciesBuffer[defName][0];                Widgets.TextFieldNumeric(minDatingAgeRect, ref val, ref buffer, min: -1);                speciesDictCached[defName].minDatingAge = val;                speciesBuffer[defName][0] = buffer;                val = speciesDictCached[defName].minLovinAge;                buffer = speciesBuffer[defName][1];                Widgets.TextFieldNumeric(minLovinAgeRect, ref val, ref buffer, min: -1);                speciesDictCached[defName].minLovinAge = val;                speciesBuffer[defName][1] = buffer;            }

            //Rect rowHighlightRect = labelRect;
            //rowHighlightRect.xMax = minLovinAgeRect.xMax;
            //Widgets.DrawHighlightIfMouseover(rowHighlightRect);

            Widgets.DrawHighlightIfMouseover(testHighlightRect);            labelRect.y += RowHeight;            psycheVec.y += RowHeight;            ageGapVec.y += RowHeight;            minDatingAgeRect.y += RowHeight;            minLovinAgeRect.y += RowHeight;            testHighlightRect.y += RowHeight;        }        Widgets.EndScrollView();        UIAssets.DrawLineHorizontal(SpeciesRect.x, labelRect.y + HighlightPadding, SpeciesRect.width, UIAssets.ModEntryLineColor);

        if (Widgets.ButtonText(ResetButtonRect, ResetButtonText, true, true))
        {
            PsychologySettings.ResetAllSettings();
            SetAllCachedToSettings();
        }

        if (Widgets.ButtonText(UpdateButtonRect, UpdateButtonText, true, true))
        {
            Find.WindowStack.Add(new Dialog_UpdateYesNo());
        }

        //UIAssets.DrawLineHorizontal(ResetButtonRect.x, ResetButtonRect.y, UpdateButtonRect.xMax - ResetButtonRect.x, UIAssets.ModEntryLineColor);
        //UIAssets.DrawLineHorizontal(ResetButtonRect.x, ResetButtonRect.yMax, UpdateButtonRect.xMax - ResetButtonRect.x, UIAssets.ModEntryLineColor);

        GenUI.ResetLabelAlign();        SaveAllSettings();        Widgets.BeginGroup(totalRect);    }    public static void CheckboxEntry(ref Rect titleRect, string title, ref Rect entryRect, ref bool enableCached, string tooltip)    {        Widgets.Label(titleRect, title);        Widgets.Checkbox(entryRect.x, entryRect.center.y - 0.5f * CheckboxSize, ref enableCached);        Rect highlightRect = titleRect;        highlightRect.xMin -= HighlightPadding;        highlightRect.xMax = entryRect.xMax + HighlightPadding;        Widgets.DrawHighlightIfMouseover(highlightRect);        TooltipHandler.TipRegion(highlightRect, delegate        {            return tooltip;        }, tooltip.GetHashCode());        titleRect.y += RowHeight;        entryRect.y += RowHeight;    }    public static void NumericEntry(ref Rect titleRect, string title, ref Rect entryRect, ref float numericCached, ref string numericBuffer, string tooltip, float min = 0f, float max = 1E+09f)    {        Widgets.Label(titleRect, title);
        //numericBuffer = Widgets.TextField(entryRect, numericBuffer);
        //GUI.TextArea
        Widgets.TextFieldNumeric<float>(entryRect, ref numericCached, ref numericBuffer, min, max);
        Rect highlightRect = titleRect;        highlightRect.xMin -= HighlightPadding;        highlightRect.xMax = entryRect.xMax + HighlightPadding;        Widgets.DrawHighlightIfMouseover(highlightRect);        TooltipHandler.TipRegion(highlightRect, delegate        {            return tooltip;        }, tooltip.GetHashCode());        titleRect.y += RowHeight;        entryRect.y += RowHeight;    }

    public static void KinseyCustomVerticalSlider(int i, ref Rect customWeightEntryRect)
    {
        //Rect numberRect = customWeightEntryRect;
        //numberRect.height = RowHeight;
        //Vector2 center = numberRect.center;
        //numberRect.width -= HighlightPadding;
        //numberRect.height -= 2f * HighlightPadding;
        //numberRect.center = center;

        //Rect SliderRect = customWeightEntryRect;
        //SliderRect.x = customWeightEntryRect.center.x - 5f;
        //SliderRect.yMin = numberRect.yMax + HighlightPadding;
        //SliderRect.yMax -= HighlightPadding;

        Rect numberRect = customWeightEntryRect;
        numberRect.yMin = customWeightEntryRect.yMax - RowHeight;
        Vector2 center = numberRect.center;
        numberRect.width -= HighlightPadding;
        numberRect.height -= 2f * HighlightPadding;
        numberRect.center = center;

        Rect SliderRect = customWeightEntryRect;
        SliderRect.x = customWeightEntryRect.center.x - 5f;
        SliderRect.yMin += 1.5f * HighlightPadding;
        SliderRect.yMax -= RowHeight + 0.5f * HighlightPadding;

        float val = kinseyWeightCustomCached[i];
        string buffer = kinseyWeightCustomBuffer[i];
        if (kinseyFormulaCached != KinseyMode.Custom)
        {
            val = (float)Math.Round(PsycheHelper.KinseyModeWeightDict[kinseyFormulaCached][i], 1);
            buffer = val.ToString();
        }

        Widgets.TextFieldNumeric(numberRect, ref val, ref buffer, 0f, 100f);
        float valSlider = (float)Math.Round(GUI.VerticalSlider(SliderRect, val, 100f, 0f), 1);

        if (kinseyFormulaCached == KinseyMode.Custom)
        {
            kinseyWeightCustomBuffer[i] = buffer;
            if (val != kinseyWeightCustomCached[i])
            {
                kinseyWeightCustomCached[i] = val;
            }
            else if (valSlider != kinseyWeightCustomCached[i])
            {
                kinseyWeightCustomCached[i] = valSlider;
                kinseyWeightCustomBuffer[i] = Convert.ToString(valSlider);
            }
        }

        Widgets.DrawHighlightIfMouseover(customWeightEntryRect);
        TooltipHandler.TipRegion(customWeightEntryRect, delegate
        {
            return ((string)"KWTooltip".Translate()).ReplaceFirst("{0}", i.ToString().Colorize(UIAssets.TitleColor));
        }, ("KWTooltip".Translate(i)).GetHashCode() + 10 * i);
        customWeightEntryRect.x += customWeightEntryRect.width;
    }

    public static void ColumnHighlightAndTooltip(Rect titleRect, float columnHeight, string tooltip)
    {
        if (Mouse.IsOver(titleRect))
        {
            Widgets.DrawHighlight(new Rect(titleRect.x - HighlightPadding, titleRect.y, titleRect.width, columnHeight));
        }
        TooltipHandler.TipRegion(titleRect, delegate
        {
            return tooltip;
        }, tooltip.GetHashCode());
    }

    public static void SetAllCachedToSettings()    {        enableKinseyCached = PsychologySettings.enableKinsey;        kinseyFormulaCached = PsychologySettings.kinseyFormula;        kinseyWeightCustomCached.Clear();        kinseyWeightCustomBuffer.Clear();        foreach (float w in PsychologySettings.kinseyWeightCustom)        {            kinseyWeightCustomCached.Add(w);            kinseyWeightCustomBuffer.Add(w.ToString());        }        enableEmpathyCached = PsychologySettings.enableEmpathy;        enableIndividualityCached = PsychologySettings.enableIndividuality;        enableElectionsCached = PsychologySettings.enableElections;        enableDateLettersCached = PsychologySettings.enableDateLetters;        enableImprisonedDebuffCached = PsychologySettings.enableImprisonedDebuff;        enableAnxietyCached = PsychologySettings.enableAnxiety;        conversationDurationCached = PsychologySettings.conversationDuration;        conversationDurationBuffer = conversationDurationCached.ToString();        romanceChanceMultiplierCached = PsychologySettings.romanceChanceMultiplier;        romanceChanceMultiplierBuffer = romanceChanceMultiplierCached.ToString();        romanceOpinionThresholdCached = PsychologySettings.romanceOpinionThreshold;        romanceOpinionThresholdBuffer = romanceOpinionThresholdCached.ToString();        mayorAgeCached = PsychologySettings.mayorAge;        mayorAgeBuffer = mayorAgeCached.ToString();        traitOpinionMultiplierCached = PsychologySettings.traitOpinionMultiplier;        traitOpinionMultiplierBuffer = traitOpinionMultiplierCached.ToString();        speciesDictCached.Clear();        speciesBuffer.Clear();        foreach (ThingDef def in SpeciesHelper.humanlikeDefs)        {            string defName = def.defName;            SpeciesSettings settings = PsychologySettings.speciesDict[defName];            speciesDictCached.Add(defName, settings);            speciesBuffer.Add(defName, new List<string> { settings.minDatingAge.ToString(), settings.minLovinAge.ToString() });        }    }    public static void SaveAllSettings()    {        PsychologySettings.enableKinsey = enableKinseyCached;        PsychologySettings.kinseyFormula = kinseyFormulaCached;
        PsychologySettings.kinseyWeightCustom.Clear();        foreach (float w in kinseyWeightCustomCached)        {            PsychologySettings.kinseyWeightCustom.Add(w);        }        PsychologySettings.enableEmpathy = enableEmpathyCached;        PsychologySettings.enableIndividuality = enableIndividualityCached;        PsychologySettings.enableElections = enableElectionsCached;        PsychologySettings.enableDateLetters = enableDateLettersCached;        PsychologySettings.enableImprisonedDebuff = enableImprisonedDebuffCached;        PsychologySettings.enableAnxiety = enableAnxietyCached;        PsychologySettings.conversationDuration = Mathf.Max(conversationDurationCached, 15f);        PsychologySettings.romanceChanceMultiplier = romanceChanceMultiplierCached;        PsychologySettings.romanceOpinionThreshold = romanceOpinionThresholdCached;        PsychologySettings.mayorAge = mayorAgeCached;        PsychologySettings.traitOpinionMultiplier = traitOpinionMultiplierCached;        PsychologySettings.speciesDict.Clear();        foreach (ThingDef def in SpeciesHelper.humanlikeDefs)        {            PsychologySettings.speciesDict.Add(def.defName, speciesDictCached[def.defName]);        }    }}

//public static string enableKinseyTitle = "SexualityChangesTitle".Translate();
//public static string enableKinseyTooltip = "SexualityChangesTooltip".Translate();
//public static string kinseyModeTitle = "KinseyModeTitle".Translate();
//public static string kinseyModeTooltip = "KinseyModeTooltip".Translate();
//public static string kinseyCustomTitle;
//public static string kinseyCustomTooltip;
//public static string enableEmpathyTitle = "EmpathyChangesTitle".Translate();
//public static string enableEmpathyTooltip = "EmpathyChangesTooltip".Translate();
//public static string enableIndividualityTitle = "IndividualityTitle".Translate();
//public static string enableIndividualityTooltip = "IndividualityTooltip".Translate();
//public static string enableElectionsTitle = "ElectionsTitle".Translate();
//public static string enableElectionsTooltip = "ElectionsTooltip".Translate();
//public static string conversationDurationTitle = "DurationTitle".Translate();
//public static string conversationDurationTooltip = "DurationTooltip".Translate();
//public static string enableDateLettersTitle = "SendDateLettersTitle".Translate();
//public static string enableDateLettersTooltip = "SendDateLettersTooltip".Translate();
//public static string enableImprisonedTitle = ;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;
//public static string Title;
//public static string Tooltip;

//public static PsychologySettings.KinseyMode kinseyFormulaCached;
//public static List<float> kinseyWeightCustomCached;
//public static bool enableEmpathyCached;
//public static bool enableIndividualityCached;
//public static bool enableElectionsCached;
//public static bool enableDateLettersCached;
//public static bool enableImprisonedDebuffCached;
//public static bool enableAnxietyCached;
//public static float conversationDurationCached;
//public static float romanceChanceMultiplierCached;
//public static float romanceOpinionThresholdCached;W
//public static float mayorAgeCached;
//public static float traitOpinionMultiplierCached;
//public static byte displayOptionCached;
//public static bool useColorsCached;
//public static bool listAlphabeticalCached;
//public static bool useAntonymsCached;
//public static Dictionary<string, SpeciesSettings> speciesDictCached;

//kinseyFormulaCached = PsychologySettings.kinseyFormula;
//kinseyWeightCustomCached = PsychologySettings.kinseyWeightCustom;
//enableEmpathyCached = PsychologySettings.enableEmpathy;
//enableIndividualityCached = PsychologySettings.enableIndividuality;
//enableElectionsCached = PsychologySettings.enableElections;
//enableDateLettersCached = PsychologySettings.enableDateLetters;
//enableImprisonedDebuffCached = PsychologySettings.enableImprisonedDebuff;
//enableAnxietyCached = PsychologySettings.enableAnxiety;
//conversationDurationCached = PsychologySettings.conversationDuration;
//romanceChanceMultiplierCached = PsychologySettings.romanceChanceMultiplier;
//romanceOpinionThresholdCached = PsychologySettings.romanceOpinionThreshold;
//mayorAgeCached = PsychologySettings.mayorAge;
//traitOpinionMultiplierCached = PsychologySettings.traitOpinionMultiplier;
//displayOptionCached = PsychologySettings.displayOption;
//useColorsCached = PsychologySettings.useColors;
//listAlphabeticalCached = PsychologySettings.listAlphabetical;
//useAntonymsCached = PsychologySettings.useAntonyms;
//speciesDictCached = PsychologySettings.speciesDict;

//public static float kinseyWeight0Cached;
//public static float kinseyWeight1Cached;
//public static float kinseyWeight2Cached;
//public static float kinseyWeight3Cached;
//public static float kinseyWeight4Cached;
//public static float kinseyWeight5Cached;
//public static float kinseyWeight6Cached;
//public static string kinseyWeight0Buffer;
//public static string kinseyWeight1Buffer;
//public static string kinseyWeight2Buffer;
//public static string kinseyWeight3Buffer;
//public static string kinseyWeight4Buffer;
//public static string kinseyWeight5Buffer;
//public static string kinseyWeight6Buffer;

//kinseyWeight0Cached = PsychologySettings.kinseyWeightCustom[0];
//kinseyWeight1Cached = PsychologySettings.kinseyWeightCustom[1];
//kinseyWeight2Cached = PsychologySettings.kinseyWeightCustom[2];
//kinseyWeight3Cached = PsychologySettings.kinseyWeightCustom[3];
//kinseyWeight4Cached = PsychologySettings.kinseyWeightCustom[4];
//kinseyWeight5Cached = PsychologySettings.kinseyWeightCustom[5];
//kinseyWeight6Cached = PsychologySettings.kinseyWeightCustom[6];
//kinseyWeight0Buffer = kinseyWeight0Cached.ToString();
//kinseyWeight1Buffer = kinseyWeight1Cached.ToString();
//kinseyWeight2Buffer = kinseyWeight2Cached.ToString();
//kinseyWeight3Buffer = kinseyWeight3Cached.ToString();
//kinseyWeight4Buffer = kinseyWeight4Cached.ToString();
//kinseyWeight5Buffer = kinseyWeight5Cached.ToString();
//kinseyWeight6Buffer = kinseyWeight6Cached.ToString();

//KinseyCustomNumericEntry(ref customWeightTitleRect, "0:", ref customWeightEntryRect, ref kinseyWeight0Cached, ref kinseyWeight0Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "1:", ref customWeightEntryRect, ref kinseyWeight1Cached, ref kinseyWeight1Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "2:", ref customWeightEntryRect, ref kinseyWeight2Cached, ref kinseyWeight2Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "3:", ref customWeightEntryRect, ref kinseyWeight3Cached, ref kinseyWeight3Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "4:", ref customWeightEntryRect, ref kinseyWeight4Cached, ref kinseyWeight4Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "5:", ref customWeightEntryRect, ref kinseyWeight5Cached, ref kinseyWeight5Buffer);
//KinseyCustomNumericEntry(ref customWeightTitleRect, "6:", ref customWeightEntryRect, ref kinseyWeight6Cached, ref kinseyWeight6Buffer);

//public static List<string> kinseyModeTitleList = new List<string>() { "KinseyMode_Realistic".Translate(), "KinseyMode_Uniform".Translate(), "KinseyMode_Invisible".Translate(), "KinseyMode_Gaypocalypse".Translate(), "KinseyMode_Custom".Translate() };



//public static bool needToSetCachedAndBuffer = true;

//if (needToSetCachedAndBuffer)
//{
//    SetAllCachedToSettings();
//    needToSetCachedAndBuffer = false;
//}

