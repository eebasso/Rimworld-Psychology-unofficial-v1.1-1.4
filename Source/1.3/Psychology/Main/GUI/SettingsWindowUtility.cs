﻿using System;

namespace Psychology;
    //public static Rect WindowRect = new Rect(0f, 0f, 1f, 1f);
    public const float WindowWidth = 900f;
    public const float WindowHeight = 700f;
    //public static float InWidth = WindowWidth - 2f * Window.StandardMargin;
    //public static float InHeight = WindowHeight - 2f * Window.StandardMargin;


    public const float RowTopPadding = 3f;
    public static float UpperAreaHeight = 35f;
    public static float RowHeight = 34f;

    public static float SpeciesX;
    public static float SpeciesY;
    public static float SpeciesWidth;

    public static Dictionary<KinseyMode, string> KinseyModeTitleDict = new Dictionary<KinseyMode, string>()
    {
        { KinseyMode.Realistic, "KinseyMode_Realistic".Translate() } ,
        { KinseyMode.Uniform, "KinseyMode_Uniform".Translate() },
        { KinseyMode.Invisible, "KinseyMode_Invisible".Translate() },
        { KinseyMode.Gaypocalypse, "KinseyMode_Gaypocalypse".Translate() },
        { KinseyMode.Custom, "KinseyMode_Custom".Translate() }
    };

    public static List<string> SpeciesTitleList = new List<string>() { "SpeciesSettingsTitle".Translate(), "EnablePsycheTitle".Translate(), "EnableAgeGapTitle".Translate() , "MinDatingAgeTitle".Translate(), "MinLovinAgeTitle".Translate() };

    public static List<float> SpeciesWidthList = new List<float>();
    public static List<string[]> SpeciesNameList = new List<string[]>();

    //public static List<Tuple<string, bool>> boolSettingsList;
    //public static List<Tuple<string, float, string, float, float>> floatSettingsList;

    public static List<string> settingTitleList = new List<string>();
    //public static List<Vector2> settingSizeList = new List<Vector2>();


    //public static float settingTitleHeight = 34f;
    public static float TitleWidth = 0f;


    public static float KinseyModeButtonWidth = 0f;
    //public static Vector2 customWeightTitleSize = Text.CalcSize("0");
    //public static float customWeightEntryWidth = entryWidth;
    //public static float customWeightTotalWidth = customWeightEntryWidth;
    public static float VerticalSliderWidth;
    public static float VerticalSliderHeight = 3f * RowHeight;

    public static Rect SpeciesRect;

    //public static string SaveButtonText = "Save".Translate();
    //public static Rect SaveButtonRect;

    public static string ResetButtonText = "Default".Translate();
    public static Rect ResetButtonRect;

    public static string UpdateButtonText = "ApplyUpdateButton".Translate();
    public static Rect UpdateButtonRect;

    //public static string romanceBySpeciesTitle = "RomanceBySpeciesTitle".Translate();

    public static KinseyMode kinseyFormulaCached;

    // Initialize after SpeciesHelper
    public static void Initialize()
        Log.Message("SettingsWindowUtility.Initialize()");

        SetAllCachedToSettings();
        Text.Font = GameFont.Small;
        settingTitleList.Clear();
        settingTooltipList.Clear();
        foreach (string name in SettingNameList)
            //settingSizeList.Add(size);
            //Log.Message("size.x = " + size.x);
            TitleWidth = Mathf.Max(size.x, TitleWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }
        //TitleWidth += 3f * BoundaryPadding;

            //entryHeight = Mathf.Max(size.y, entryHeight);
        }
        TitleWidth = Mathf.Max(KinseyModeButtonWidth - EntryWidth, TitleWidth);
        TitleWidth += 2f * HighlightPadding;

        SpeciesWidthList.Clear();
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
        List<string> labelList = new List<string>();
        {
            string label = SpeciesHelper.humanlikeDefs.First(x => x.defName == kvp.Key).label.CapitalizeFirst();
            if (labelList.Contains(label))
            {
                repeatList.AddDistinct(label);
            }
            labelList.Add(label);
        }

        SpeciesNameList.Clear();
        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)
        {
            ThingDef def = SpeciesHelper.humanlikeDefs.First(x => x.defName == kvp.Key);
            string label = def.label.CapitalizeFirst();
            if (repeatList.Contains(label))
            {
                SpeciesNameList.Add(new string[]{ def.defName, label + " (" + def.defName + ")"});
                continue;
            }
            SpeciesNameList.Add(new string[] { def.defName, label });
        }
        SpeciesNameList.SortBy(x => x[1]);

        float viewHeight = 0f;
        foreach (string[] namePair in SpeciesNameList)
        SpeciesY = Window.StandardMargin + UpperAreaHeight + RowHeight + HighlightPadding;

        float scrollBarWidth = GenUI.ScrollBarWidth;
        if (SpeciesHeight > viewHeight)
        {
            scrollBarWidth = 0f;
            SpeciesHeight = viewHeight;
        }

        SpeciesWidthList[0] = Mathf.Max(EntryWidth, SpeciesWidthList[0]);
        SpeciesWidthList[0] += 2f * HighlightPadding;

        float shiftFactor = (WindowWidth - 2f * Window.StandardMargin - TitleWidth - EntryWidth - BoundaryPadding - SpeciesWidthList.Sum() - scrollBarWidth) / 2f;
        SpeciesWidthList[0] += shiftFactor;

        LeftColumnWidth = TitleWidth + EntryWidth;

        SpeciesRect = new Rect(SpeciesX, SpeciesY, SpeciesWidth, SpeciesHeight);
        ViewRect = new Rect(0f, 0f, SpeciesWidth - scrollBarWidth, viewHeight);


        //float recalcWidth = Mathf.Max(Window.CloseButSize.x, Text.CalcSize(UpdateButtonText).x + 20f);

        float numButtons = 3f;
        float spaceBetweenButtons = (WindowWidth - numButtons * Window.CloseButSize.x) / (1f + numButtons);
        float buttonY = WindowHeight - Window.FooterRowHeight;

        ResetButtonRect = new Rect(spaceBetweenButtons, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
        UpdateButtonRect = new Rect(WindowWidth - spaceBetweenButtons - Window.CloseButSize.x, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
    }
        Log.Message("SettingsWindowUtility.DrawSettingsWindow step 0");
        Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        //Widgets.DrawLineHorizontal(totalRect.center.x - 1000f, totalRect.center.y, 2000f);
        //Widgets.DrawLineVertical(totalRect.center.x, totalRect.center.y - 1000f, 2000f);
        Widgets.EndGroup();

        //GUI.color = Color.blue;
        //Widgets.DrawLineHorizontal(totalRect.center.x + Window.StandardMargin - 1000f, totalRect.center.y, 2000f);
        //Widgets.DrawLineVertical(totalRect.center.x + Window.StandardMargin, totalRect.center.y - 1000f, 2000f);
        //GUI.color = Color.white;

        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
        //float xMin = Window.StandardMargin;

        //float yMin = Window.StandardMargin;
        Rect titleRect = new Rect(xMin + HighlightPadding, yMin, TitleWidth - HighlightPadding, RowHeight);
        entryRect.yMin += HighlightPadding;
        entryRect.yMax -= HighlightPadding;

        CheckboxEntry(ref titleRect, settingTitleList[0], ref entryRect, ref enableKinseyCached, settingTooltipList[0]);
            //Widgets.Label(titleRect, settingTitleList[1]);
            //Rect buttonRect = new Rect(entryRect.x, entryRect.y, KinseyModeButtonWidth, entryRect.height);
            Rect buttonRect = new Rect(xMin, titleRect.y, LeftColumnWidth, titleRect.height);
            Log.Message("SettingsWindowUtility.DrawSettingsWindow step 2");
            Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
            //Rect highlightRect = titleRect;
            //highlightRect.xMin -= HighlightPadding;
            //highlightRect.xMax = entryRect.xMax + HighlightPadding;
            //Widgets.DrawHighlightIfMouseover(highlightRect);
            TooltipHandler.TipRegion(buttonRect, delegate
            {
                return settingTooltipList[1];
            }, settingTooltipList[1].GetHashCode());

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
            entryRect.y += customWeightEntryRect.height;
        NumericEntry(ref titleRect, settingTitleList[12], ref entryRect, ref upbringingEffectCached, ref upbringingEffectBuffer, settingTooltipList[13], 0f, 1f);

        //float checkboxSize = 24f;
        //float HandleEntryPadding = 3f;
        //Dictionary<string, SpeciesSettings> selection = new Dictionary<string, SpeciesSettings>();
        //Dictionary<string, List<string>> speciesBuffer = new Dictionary<string, List<string>>();

        //string buffer;

        Rect labelRect = new Rect(SpeciesX + HighlightPadding, yMin, SpeciesWidthList[0], RowHeight);

        Widgets.Label(labelRect, SpeciesTitleList[0]);

        Log.Message("SettingsWindowUtility.DrawSettingsWindow step 4");
        Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

        float columnHeight = RowHeight + HighlightPadding + SpeciesHeight;
        ColumnHighlightAndTooltip(labelRect, columnHeight, "SpeciesSettingsTooltip".Translate());

        UIAssets.DrawLineHorizontal(SpeciesRect.x, labelRect.yMax, SpeciesRect.width, UIAssets.ModEntryLineColor);

        labelRect.y += HighlightPadding;

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

        Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5a");
        Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        Log.Message("SpeciesNameList.Count() = " + SpeciesNameList.Count());
        foreach (string[] namePair in SpeciesNameList)
            //GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(labelRect, label);
            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5b");
            Log.Message("Retrieving " + defName + " from speciesDictCached");
            Widgets.Checkbox(psycheVec, ref speciesDictCached[defName].enablePsyche);
            Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5d");
            //Rect rowHighlightRect = labelRect;
            //rowHighlightRect.xMax = minLovinAgeRect.xMax;
            //Widgets.DrawHighlightIfMouseover(rowHighlightRect);

            Widgets.DrawHighlightIfMouseover(testHighlightRect);

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

        GenUI.ResetLabelAlign();
        //numericBuffer = Widgets.TextField(entryRect, numericBuffer);
        //GUI.TextArea
        Widgets.TextFieldNumeric<float>(entryRect, ref numericCached, ref numericBuffer, min, max);
        Rect highlightRect = titleRect;

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

    public static void SetAllCachedToSettings()
        PsychologySettings.kinseyWeightCustom.Clear();

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
