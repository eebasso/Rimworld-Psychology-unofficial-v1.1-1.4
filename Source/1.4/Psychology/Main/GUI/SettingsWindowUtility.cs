﻿using System;

namespace Psychology;

public static class SettingsWindowUtility
    //public static Rect WindowRect = new Rect(0f, 0f, 1f, 1f);
    public const float WindowWidth = 900f;
    public const float WindowHeight = 700f;
    //public static float InWidth = WindowWidth - 2f * Window.StandardMargin;
    //public static float InHeight = WindowHeight - 2f * Window.StandardMargin;


    public const float RowTopPadding = 3f;
    public static float UpperAreaHeight = 35f;
    public static float RowHeight = 33f;

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

    public static List<string> SpeciesTitleList = new List<string>() { "SpeciesSettingsTitle".Translate(), "EnablePsycheTitle".Translate(), "EnableAgeGapTitle".Translate(), "MinDatingAgeTitle".Translate(), "MinLovinAgeTitle".Translate() };

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
    public static float VerticalSliderHeight = 2.5f * RowHeight;

    public static Rect SpeciesRect;

    //public static string SaveButtonText = "Save".Translate();
    //public static Rect SaveButtonRect;

    public static string DefaultButtonText = "Default".Translate();
    public static Rect DefaultButtonRect;

    public static string UndoButtonText = "Cancel".Translate();
    public static Rect UndoButtonRect;

    //public static string romanceBySpeciesTitle = "RomanceBySpeciesTitle".Translate();

    public static KinseyMode kinseyFormulaCached;
    public static bool enableDateLettersBackup;

    //public static bool enableImprisonedDebuffCached;
    public static bool enableAnxietyCached;

    // Initialize after SpeciesHelper
    public static void Initialize()
        //Log.Message("SettingsWindowUtility.Initialize()");
        SetAllCachedToSettings();
        SaveBackupOfSettings();

        Text.Font = GameFont.Small;

        settingTitleList.Clear();
        settingTooltipList.Clear();
        TitleWidth = 0f;
        KinseyModeButtonWidth = 0f;
        SpeciesWidthList.Clear();
        SpeciesNameList.Clear();

        foreach (string name in SettingNameList)
            //settingSizeList.Add(size);
            //Log.Message("size.x = " + size.x);
            TitleWidth = Mathf.Max(size.x, TitleWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }
        //TitleWidth += 3f * BoundaryPadding;


        foreach (KeyValuePair<KinseyMode, string> kvp in KinseyModeTitleDict)
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }
        TitleWidth = Mathf.Max(KinseyModeButtonWidth - EntryWidth, TitleWidth);
        TitleWidth += 2f * HighlightPadding;

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
            string label = SpeciesHelper.registeredSpecies.First(x => x.defName == kvp.Key).label.CapitalizeFirst();
            if (labelList.Contains(label))
            {
                repeatList.AddDistinct(label);
            }
            labelList.Add(label);
        }

        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)
        {
            ThingDef def = SpeciesHelper.registeredSpecies.First(x => x.defName == kvp.Key);
            string label = def.label.CapitalizeFirst();
            if (repeatList.Contains(label))
            {
                SpeciesNameList.Add(new string[] { def.defName, label + " (" + def.defName + ")" });
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

        DefaultButtonRect = new Rect(spaceBetweenButtons, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
        UndoButtonRect = new Rect(WindowWidth - spaceBetweenButtons - Window.CloseButSize.x, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
    }
        Widgets.EndGroup();

        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

        Rect titleRect = new Rect(xMin + HighlightPadding, yMin, TitleWidth - 2f * HighlightPadding, RowHeight);
        entryRect.yMin += HighlightPadding;
        entryRect.yMax -= HighlightPadding;

        CheckboxEntry(ref titleRect, settingTitleList[0], ref entryRect, ref enableKinseyCached, settingTooltipList[0]);

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 1");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        if (enableKinseyCached)
            //Widgets.Label(titleRect, settingTitleList[1]);
            //Rect buttonRect = new Rect(entryRect.x, entryRect.y, KinseyModeButtonWidth, entryRect.height);
            Rect buttonRect = new Rect(xMin, titleRect.y, LeftColumnWidth, titleRect.height);

            if (Widgets.ButtonText(buttonRect, settingTitleList[1] + ": " + KinseyModeTitleDict[kinseyFormulaCached], drawBackground: true, doMouseoverSound: true, true))
                    SetCacheAndBufferBasedOnKinseyMode();
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 2");
            //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
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
            //if (buttonActivated)
            //{
            //    kinseyFormulaCached = delayedChoice;
            //}
            //if (kinseyFormulaCached != KinseyMode.Custom)
            //{
            //    kinseyWeightCustomCached.Clear();
            //    kinseyWeightCustomBuffer.Clear();
            //    foreach (float w in PsycheHelper.KinseyModeWeightDict[kinseyFormulaCached])
            //    {
            //        kinseyWeightCustomCached.Add(w);
            //        kinseyWeightCustomBuffer.Add(w.ToString());
            //    }
            //}

            Rect customWeightEntryRect = new Rect(xMin, titleRect.y, VerticalSliderWidth, VerticalSliderHeight);
            for (int i = 0; i <= 6; i++)
            {
                KinseyCustomEntry(i, ref customWeightEntryRect);
            }
            titleRect.y += customWeightEntryRect.height;
            entryRect.y += customWeightEntryRect.height;
        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 3");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

        CheckboxEntry(ref titleRect, settingTitleList[2], ref entryRect, ref enableEmpathyCached, settingTooltipList[2]);
        CheckboxEntry(ref titleRect, settingTitleList[6], ref entryRect, ref enableAnxietyCached, settingTooltipList[7]);
        NumericEntry(ref titleRect, settingTitleList[13], ref entryRect, ref personalityExtremenessCached, ref personalityExtremenessBuffer, settingTooltipList[13], 0f, 1f);
        NumericEntry(ref titleRect, settingTitleList[14], ref entryRect, ref ideoPsycheMultiplierCached, ref ideoPsycheMultiplierBuffer, settingTooltipList[14], 0f, 10f);

        Rect labelRect = new Rect(SpeciesX + HighlightPadding, yMin, SpeciesWidthList[0], RowHeight);

        Widgets.Label(labelRect, SpeciesTitleList[0]);

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 4");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

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

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5a");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        //Log.Message("SpeciesNameList.Count() = " + SpeciesNameList.Count());
        foreach (string[] namePair in SpeciesNameList)
            //GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(labelRect, label);
            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5b");
            //Log.Message("Retrieving " + defName + " from speciesDictCached");
            Widgets.Checkbox(psycheVec, ref speciesDictCached[defName].enablePsyche);
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5c");
            if (speciesDictCached[defName].enablePsyche)
                //Widgets.TextFieldNumeric(minDatingAgeRect, ref val, ref buffer, min: -1);
                UIAssets.TextFieldFloat(minDatingAgeRect, ref val, ref buffer, min: -1);
                //Widgets.TextFieldNumeric(minLovinAgeRect, ref val, ref buffer, min: -1);
                UIAssets.TextFieldFloat(minLovinAgeRect, ref val, ref buffer, min: -1);
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5d");
            //Rect rowHighlightRect = labelRect;
            //rowHighlightRect.xMax = minLovinAgeRect.xMax;
            //Widgets.DrawHighlightIfMouseover(rowHighlightRect);

            Widgets.DrawHighlightIfMouseover(testHighlightRect);

        if (Widgets.ButtonText(DefaultButtonRect, DefaultButtonText, true, true))
        {
            PsychologySettings.ResetAllSettings();
            SetAllCachedToSettings();
        }

        if (Widgets.ButtonText(UndoButtonRect, UndoButtonText, true, true))
        {
            RestoreSettingsFromBackup();
            SetAllCachedToSettings();
            //Find.WindowStack.Add(new Dialog_UpdateYesNo());
        }

        //UIAssets.DrawLineHorizontal(ResetButtonRect.x, ResetButtonRect.y, UpdateButtonRect.xMax - ResetButtonRect.x, UIAssets.ModEntryLineColor);
        //UIAssets.DrawLineHorizontal(ResetButtonRect.x, ResetButtonRect.yMax, UpdateButtonRect.xMax - ResetButtonRect.x, UIAssets.ModEntryLineColor);

        GenUI.ResetLabelAlign();
    {
        kinseyWeightCustomCached.Clear();
        if (kinseyFormulaCached != KinseyMode.Custom)
        {
            foreach (float w in PsycheHelper.KinseyModeWeightDict[kinseyFormulaCached])
            {
                float rounded = (float)Math.Round(w, 1);
                kinseyWeightCustomCached.Add(rounded);
                kinseyWeightCustomBuffer.Add(rounded.ToString());
            }
        }
        else
        {
            foreach (float w in PsychologySettings.kinseyWeightCustom)
            {
                kinseyWeightCustomCached.Add(w);
                kinseyWeightCustomBuffer.Add(w.ToString());
            }
        }
    }
        //numericBuffer = Widgets.TextField(entryRect, numericBuffer);
        //GUI.TextArea
        //Widgets.TextFieldNumeric<float>(entryRect, ref numericCached, ref numericBuffer, min, max);
        UIAssets.TextFieldFloat(entryRect, ref numericCached, ref numericBuffer, min, max);
        Rect highlightRect = titleRect;

    public static void KinseyCustomEntry(int i, ref Rect customWeightEntryRect)
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
        //if (kinseyFormulaCached != KinseyMode.Custom)
        //{
        //    val = (float)Math.Round(PsycheHelper.KinseyModeWeightDict[kinseyFormulaCached][i], 1);
        //    buffer = val.ToString();
        //    kinseyWeightCustomCached[i] = val;
        //    kinseyWeightCustomBuffer[i] = buffer;
        //}
        //Widgets.TextFieldNumeric(numberRect, ref val, ref buffer, 0f, 100f);
        UIAssets.TextFieldFloat(numberRect, ref val, ref buffer, 0f, 100f);
        kinseyWeightCustomBuffer[i] = buffer;
        //float valSlider = (float)Math.Round(GUI.VerticalSlider(SliderRect, val, 100f, 0f), 1);
        float valSlider = GUI.VerticalSlider(SliderRect, val, 100f, 0f);
        if (val != kinseyWeightCustomCached[i])
        {
            kinseyWeightCustomCached[i] = val;
            kinseyFormulaCached = KinseyMode.Custom;
        }
        else if (valSlider != kinseyWeightCustomCached[i])
        {
            kinseyWeightCustomCached[i] = (float)Math.Round(valSlider, 1);
            //kinseyWeightCustomBuffer[i] = Convert.ToString(valSlider);
            kinseyWeightCustomBuffer[i] = kinseyWeightCustomCached[i].ToString();
            kinseyFormulaCached = KinseyMode.Custom;
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
        //Log.Message("SetAllCachedToSettings(), step 0");
        enableKinseyCached = PsychologySettings.enableKinsey;
        SetCacheAndBufferBasedOnKinseyMode();
        //Log.Message("SetAllCachedToSettings(), step 2");
        enableEmpathyCached = PsychologySettings.enableEmpathy;
        enableAnxietyCached = PsychologySettings.enableAnxiety;

        //Log.Message("SetAllCachedToSettings(), step 3");
        imprisonedDebuffCached = PsychologySettings.imprisonedDebuff;
        imprisonedDebuffBuffer = imprisonedDebuffCached.ToString();

        conversationDurationCached = PsychologySettings.conversationDuration;

        personalityExtremenessCached = PsychologySettings.personalityExtremeness;
        personalityExtremenessBuffer = personalityExtremenessCached.ToString();

        ideoPsycheMultiplierCached = PsychologySettings.ideoPsycheMultiplier;
        ideoPsycheMultiplierBuffer = ideoPsycheMultiplierCached.ToString();

        //Log.Message("SetAllCachedToSettings(), step 4");
        speciesDictCached.Clear();
        //Log.Message("humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        //Log.Message("SetAllCachedToSettings(), step 5");
        foreach (ThingDef def in SpeciesHelper.registeredSpecies)
            //Log.Message("SetAllCachedToSettings(), step 5a");
            string defName = def.defName;
            //Log.Message("SetAllCachedToSettings(), step 5b");
            SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
            //Log.Message("SetAllCachedToSettings(), step 5c");
            speciesDictCached[defName] = settings;
            //Log.Message("SetAllCachedToSettings(), step 5d");
            speciesBuffer[defName] = new List<string> { settings.minDatingAge.ToString(), settings.minLovinAge.ToString() };
            //Log.Message("SetAllCachedToSettings(), step 5e");
        }
        //Log.Message("SetAllCachedToSettings(), step 6");
    }
        if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        {
            PsychologySettings.kinseyWeightCustom = kinseyWeightCustomCached.ListFullCopy();
        }
        //PsychologySettings.enableImprisonedDebuff = enableImprisonedDebuffCached;
        PsychologySettings.enableAnxiety = enableAnxietyCached;
        PsychologySettings.ideoPsycheMultiplier = ideoPsycheMultiplierCached;


    public static void SaveBackupOfSettings()
    {
        enableKinseyBackup = PsychologySettings.enableKinsey;
        kinseyFormulaBackup = PsychologySettings.kinseyFormula;
        kinseyWeightCustomBackup = PsychologySettings.kinseyWeightCustom.ListFullCopy();
        enableEmpathyBackup = PsychologySettings.enableEmpathy;
        enableIndividualityBackup = PsychologySettings.enableIndividuality;
        enableElectionsBackup = PsychologySettings.enableElections;
        enableDateLettersBackup = PsychologySettings.enableDateLetters;
        enableAnxietyBackup = PsychologySettings.enableAnxiety;

        imprisonedDebuffBackup = PsychologySettings.imprisonedDebuff;
        conversationDurationBackup = PsychologySettings.conversationDuration;
        romanceChanceMultiplierBackup = PsychologySettings.romanceChanceMultiplier;
        romanceOpinionThresholdBackup = PsychologySettings.romanceOpinionThreshold;
        mayorAgeBackup = PsychologySettings.mayorAge;
        traitOpinionMultiplierBackup = PsychologySettings.traitOpinionMultiplier;
        personalityExtremenessBackup = PsychologySettings.personalityExtremeness;
        ideoPsycheMultiplierBackup = PsychologySettings.ideoPsycheMultiplier;

        speciesDictBackup.Clear();
        foreach (ThingDef def in SpeciesHelper.registeredSpecies)
        {
            speciesDictBackup[def.defName] = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
        }
    }
    {
        PsychologySettings.enableKinsey = enableKinseyBackup;
        PsychologySettings.kinseyFormula = kinseyFormulaBackup;
        if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        {
            PsychologySettings.kinseyWeightCustom = kinseyWeightCustomBackup.ListFullCopy();
        }
        PsychologySettings.enableEmpathy = enableEmpathyBackup;
        PsychologySettings.enableIndividuality = enableIndividualityBackup;
        PsychologySettings.enableElections = enableElectionsBackup;
        PsychologySettings.enableDateLetters = enableDateLettersBackup;
        PsychologySettings.enableAnxiety = enableAnxietyBackup;

        PsychologySettings.imprisonedDebuff = imprisonedDebuffBackup;
        PsychologySettings.conversationDuration = conversationDurationBackup;
        PsychologySettings.romanceChanceMultiplier = romanceChanceMultiplierBackup;
        PsychologySettings.romanceOpinionThreshold = romanceOpinionThresholdBackup;
        PsychologySettings.mayorAge = mayorAgeBackup;
        PsychologySettings.traitOpinionMultiplier = traitOpinionMultiplierBackup;
        PsychologySettings.personalityExtremeness = personalityExtremenessBackup;
        PsychologySettings.ideoPsycheMultiplier = ideoPsycheMultiplierBackup;

        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictBackup)
        {
            PsychologySettings.speciesDict[kvp.Key] = kvp.Value;
        }
    }