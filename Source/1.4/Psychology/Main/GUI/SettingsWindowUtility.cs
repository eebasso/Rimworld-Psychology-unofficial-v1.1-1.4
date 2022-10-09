using System;using System.Collections.Generic;using System.Linq;using System.Text;using System.Text.RegularExpressions;using System.Reflection;using RimWorld;using RimWorld.Planet;using Verse;using Verse.AI.Group;using Verse.Grammar;using UnityEngine;using Verse.Noise;using Unity;

namespace Psychology;

// ToDo: make a separate class for each setting to centralize the cache, buffer, 

public static class SettingsWindowUtility{
    //public static Rect WindowRect = new Rect(0f, 0f, 1f, 1f);
    public const float WindowWidth = 900f;
    public const float WindowHeight = 700f;

    //public static float InWidth = WindowWidth - 2f * Window.StandardMargin;
    //public static float InHeight = WindowHeight - 2f * Window.StandardMargin;


    public const float RowTopPadding = 3f;    public const float CheckboxSize = 24f;    public const float BoundaryPadding = PsycheCardUtility.BoundaryPadding;    public const float HighlightPadding = PsycheCardUtility.HighlightPadding;
    public const float UpperAreaHeight = 35f;    public const float LowerAreaHeight = 44f;
    public const float RowHeight = 34f;
    public const float xMin = Window.StandardMargin;
    public const float yMin = Window.StandardMargin + UpperAreaHeight;

    public static float SpeciesX;
    public static float SpeciesY;
    public static float SpeciesWidth;    public static float SpeciesHeight;
    public static Dictionary<string, string> TitleDict = new Dictionary<string, string>();
    public static Dictionary<string, string> TooltipDict = new Dictionary<string, string>();

    public static Dictionary<string, bool> BoolCachedDict = new Dictionary<string, bool>();
    public static Dictionary<string, bool> BoolBackupDict = new Dictionary<string, bool>();

    public static Dictionary<string, float> FloatCachedDict = new Dictionary<string, float>();
    public static Dictionary<string, float> FloatBackupDict = new Dictionary<string, float>();
    public static Dictionary<string, string> FloatBufferDict = new Dictionary<string, string>();

    public static string KinseyFormulaTitle = "KinseyFormulaTitle".Translate();
    public static Dictionary<KinseyMode, string> KinseyFormulaTitleDict = new Dictionary<KinseyMode, string>()
    {
        { KinseyMode.Realistic, KinseyFormulaTitle + ": " + "KinseyMode_Realistic".Translate() } ,
        { KinseyMode.Uniform, KinseyFormulaTitle + ": " + "KinseyMode_Uniform".Translate() },
        { KinseyMode.Invisible, KinseyFormulaTitle + ": " + "KinseyMode_Invisible".Translate() },
        { KinseyMode.Gaypocalypse, KinseyFormulaTitle + ": " + "KinseyMode_Gaypocalypse".Translate() },
        { KinseyMode.Custom, KinseyFormulaTitle + ": " + "KinseyMode_Custom".Translate() }
    };
    public static string KinseyFormulaTooltip = "KinseyFormulaTooltip".Translate();
    public static KinseyMode kinseyFormulaCached;    public static KinseyMode kinseyFormulaBackup;
    public static List<float> kinseyWeightCustomCached = new List<float>();    public static List<float> kinseyWeightCustomBackup = new List<float>();    public static List<string> kinseyWeightCustomBuffer = new List<string>();

    public static List<string> SpeciesTitleList = new List<string> { "SpeciesSettingsTitle".Translate(), "EnablePsycheTitle".Translate(), "EnableAgeGapTitle".Translate(), "MinDatingAgeTitle".Translate(), "MinLovinAgeTitle".Translate() };
    public static List<string> SpeciesTooltipList = new List<string> { "SpeciesSettingsTooltip".Translate(), "EnablePsycheTooltip".Translate(), "EnableAgeGapTooltip".Translate(), "MinDatingAgeTooltip".Translate("datingRomanceBehavior".Translate(), 14), "MinLovinAgeTooltip".Translate("lovinRomanceBehavior".Translate(), 16) };
    public static Dictionary<string, SpeciesSettings> speciesDictCached = new Dictionary<string, SpeciesSettings>();    public static Dictionary<string, SpeciesSettings> speciesDictBackup = new Dictionary<string, SpeciesSettings>();    public static Dictionary<string, List<string>> speciesBuffer = new Dictionary<string, List<string>>();
    public static List<float> SpeciesWidthList = new List<float>();
    public static List<string[]> SpeciesNameList = new List<string[]>();

    //public static List<Pair<string, string>> settingTitleTooltipList = new List<Pair<string, string>>();

    //public static float settingTitleHeight = 34f;
    public static float TitleWidth = 0f;    public static float EntryWidth = Text.CalcSize("100.000").x;    public static float LeftColumnWidth;    //public static float MaxEntryWidth;


    public static float KinseyModeButtonWidth = 0f;
    //public static Vector2 customWeightTitleSize = Text.CalcSize("0");
    //public static float customWeightEntryWidth = entryWidth;
    //public static float customWeightTotalWidth = customWeightEntryWidth;
    public static float KinseyCustomEntryWidth;
    public static float KinseyCustomEntryHeight = 3f * RowHeight;

    public static Rect LeftColumnRect;
    public static Rect LeftColumnViewRect;

    public static Rect SpeciesRect;    public static Rect SpeciesViewRect;

    public static string DefaultButtonText = "Default".Translate();
    public static Rect DefaultButtonRect;

    public static string UndoButtonText = "Cancel".Translate();
    public static Rect UndoButtonRect;    public static Vector2 LeftScrollPosition = Vector2.zero;    public static Vector2 RightScrollPosition = Vector2.zero;    //public static bool enableKinseyCached;    //public static bool enableKinseyBackup;    //public static bool enableEmpathyCached;    //public static bool enableEmpathyBackup;    //public static bool enableIndividualityCached;    //public static bool enableIndividualityBackup;    //public static bool enableElectionsCached;    //public static bool enableElectionsBackup;    //public static bool enableDateLettersCached;
    //public static bool enableDateLettersBackup;

    //public static bool enableImprisonedDebuffCached;
    //public static float mentalBreakAnxietyChanceCached;    //public static float mentalBreakAnxietyChanceBackup;    //public static string mentalBreakAnxietyChanceBuffer;    //public static float imprisonedDebuffCached;    //public static float imprisonedDebuffBackup;    //public static string imprisonedDebuffBuffer;    //public static float conversationDurationCached;    //public static float conversationDurationBackup;    //public static string conversationDurationBuffer;    //public static float conversationStrengthCached;    //public static float conversationStrengthBackup;    //public static string conversationStrengthBuffer;    //public static float romanceChanceMultiplierCached;    //public static float romanceChanceMultiplierBackup;    //public static string romanceChanceMultiplierBuffer;    //public static float romanceOpinionThresholdCached;    //public static float romanceOpinionThresholdBackup;    //public static string romanceOpinionThresholdBuffer;    //public static float mayorAgeCached;    //public static float mayorAgeBackup;    //public static string mayorAgeBuffer;    //public static float mayorVisitMtbHoursCached;    //public static float mayorVisitMtbHoursBackup;    //public static string mayorVisitMtbHoursBuffer;    //public static float traitOpinionMultiplierCached;    //public static float traitOpinionMultiplierBackup;    //public static string traitOpinionMultiplierBuffer;    //public static float personalityExtremenessCached;    //public static float personalityExtremenessBackup;    //public static string personalityExtremenessBuffer;    //public static float ideoPsycheMultiplierCached;    //public static float ideoPsycheMultiplierBackup;    //public static string ideoPsycheMultiplierBuffer;

    




    // Initialize after SpeciesHelper
    public static void Initialize()    {
        //Log.Message("SettingsWindowUtility.Initialize()");
        SetAllCachedToSettings();
        SaveBackupOfSettings();

        Text.Font = GameFont.Small;

        //settingTitleList.Clear();
        //settingTooltipList.Clear();
        //settingTitleTooltipList.Clear();
        TitleDict.Clear();
        TooltipDict.Clear();

        TitleWidth = 0f;
        KinseyModeButtonWidth = 0f;
        SpeciesWidthList.Clear();
        SpeciesNameList.Clear();

        float leftColumnViewHeight = KinseyCustomEntryHeight;
        List<string> settingNameList = PsychologySettings.FloatSettingNameList;
        settingNameList.AddRange(PsychologySettings.BoolSettingNameList);
        foreach (string settingName in settingNameList)        {            string title = (settingName + "Title").Translate();            string tooltip = (settingName + "Tooltip").Translate();            Vector2 size = Text.CalcSize(title);
            TitleDict[settingName] = title;
            TooltipDict[settingName] = tooltip;
            TitleWidth = Mathf.Max(size.x, TitleWidth);            leftColumnViewHeight += RowHeight;
        }
        float leftColumnY = yMin;
        float leftColumnHeight = WindowHeight - Window.StandardMargin - LowerAreaHeight - leftColumnY;
        //TitleWidth += 3f * BoundaryPadding;


        foreach (KeyValuePair<KinseyMode, string> kvp in KinseyFormulaTitleDict)        {            Vector2 size = Text.CalcSize(kvp.Value);            KinseyModeButtonWidth = Mathf.Max(size.x, KinseyModeButtonWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }        KinseyModeButtonWidth += 20f;
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
        List<string> labelList = new List<string>();        List<string> repeatList = new List<string>();        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)
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

        float speciesViewHeight = 0f;
        foreach (string[] namePair in SpeciesNameList)        {            SpeciesWidthList[0] = Mathf.Max(Text.CalcSize(namePair[1]).x, SpeciesWidthList[0]);            speciesViewHeight += RowHeight;        }
        SpeciesY = yMin + RowHeight + HighlightPadding;        SpeciesHeight = WindowHeight - Window.StandardMargin - LowerAreaHeight - SpeciesY;

        float leftScrollBarWidth = leftColumnViewHeight < leftColumnHeight ? 0f : GenUI.ScrollBarWidth;
        float rightScrollBarWidth = GenUI.ScrollBarWidth;
        if (speciesViewHeight < SpeciesHeight)
        {
            rightScrollBarWidth = 0f;
            SpeciesHeight = speciesViewHeight;
        }

        SpeciesWidthList[0] = Mathf.Max(EntryWidth, SpeciesWidthList[0]);
        SpeciesWidthList[0] += 2f * HighlightPadding;

        float shiftFactor = (WindowWidth - Window.StandardMargin - TitleWidth - EntryWidth - leftScrollBarWidth - BoundaryPadding - SpeciesWidthList.Sum() - rightScrollBarWidth - Window.StandardMargin) / 2f;        TitleWidth += shiftFactor;
        SpeciesWidthList[0] += shiftFactor;

        LeftColumnWidth = TitleWidth + EntryWidth + leftScrollBarWidth;        KinseyCustomEntryWidth = LeftColumnWidth / 7f;        SpeciesX = Window.StandardMargin + LeftColumnWidth + BoundaryPadding;        SpeciesWidth = WindowWidth - Window.StandardMargin - SpeciesX;

        SpeciesRect = new Rect(SpeciesX, SpeciesY, SpeciesWidth, SpeciesHeight);
        SpeciesViewRect = new Rect(0f, 0f, SpeciesRect.width - rightScrollBarWidth, speciesViewHeight);

        LeftColumnRect = new Rect(xMin, yMin, LeftColumnWidth, leftColumnHeight);
        LeftColumnViewRect = new Rect(0f, 0f, LeftColumnRect.width - leftScrollBarWidth, leftColumnViewHeight);

        //float recalcWidth = Mathf.Max(Window.CloseButSize.x, Text.CalcSize(UpdateButtonText).x + 20f);

        float numButtons = 3f;
        float spaceBetweenButtons = (WindowWidth - numButtons * Window.CloseButSize.x) / (1f + numButtons);
        float buttonY = WindowHeight - Window.FooterRowHeight;

        DefaultButtonRect = new Rect(spaceBetweenButtons, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
        UndoButtonRect = new Rect(WindowWidth - spaceBetweenButtons - Window.CloseButSize.x, buttonY, Window.CloseButSize.x, Window.CloseButSize.y);
    }    public static void DrawSettingsWindow(Rect totalRect)    {
        Widgets.EndGroup();

        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);        Text.Font = GameFont.Small;


        //Rect titleRect = new Rect(xMin + HighlightPadding, yMin, TitleWidth - 2f * HighlightPadding, RowHeight);        //Rect entryRect = new Rect(titleRect.xMax, titleRect.y, EntryWidth, titleRect.height);

        Widgets.BeginScrollView(LeftColumnRect, ref LeftScrollPosition, LeftColumnViewRect);

        Rect titleRect = new Rect(HighlightPadding, 0f, TitleWidth - 2f * HighlightPadding, RowHeight);        Rect entryRect = new Rect(titleRect.xMax, titleRect.y, EntryWidth, titleRect.height);
        entryRect.yMin += HighlightPadding;
        entryRect.yMax -= HighlightPadding;

        CheckboxEntry("enableKinsey", ref titleRect, ref entryRect);

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 1");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        if (BoolCachedDict.TryGetValue("enableKinsey", out bool enableKinseyCached) && enableKinseyCached)        {
            //Widgets.Label(titleRect, settingTitleList[1]);
            //Rect buttonRect = new Rect(entryRect.x, entryRect.y, KinseyModeButtonWidth, entryRect.height);
            Rect buttonRect = new Rect(titleRect.x - HighlightPadding, titleRect.y, LeftColumnWidth, titleRect.height);            //KinseyMode delayedChoice = kinseyFormulaCached;

            if (Widgets.ButtonText(buttonRect, KinseyFormulaTitleDict[kinseyFormulaCached], drawBackground: true, doMouseoverSound: true, true))            {                List<FloatMenuOption> list = new List<FloatMenuOption>();                list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Realistic], delegate                {                    kinseyFormulaCached = KinseyMode.Realistic;
                    SetCacheAndBufferBasedOnKinseyMode();                    Log.Message("Set to Realistic");                }));                list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Uniform], delegate                {                    kinseyFormulaCached = KinseyMode.Uniform;                    SetCacheAndBufferBasedOnKinseyMode();                    Log.Message("Set to Uniform");                }));                list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Invisible], delegate                {                    kinseyFormulaCached = KinseyMode.Invisible;                    SetCacheAndBufferBasedOnKinseyMode();                    Log.Message("Set to Invisible");                }));                list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Gaypocalypse], delegate                {                    kinseyFormulaCached = KinseyMode.Gaypocalypse;                    SetCacheAndBufferBasedOnKinseyMode();                    Log.Message("Set to Gaypocalypse");                }));                list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Custom], delegate                {                    kinseyFormulaCached = KinseyMode.Custom;                    SetCacheAndBufferBasedOnKinseyMode();                    Log.Message("Set to Custom");                }));                Find.WindowStack.Add(new FloatMenu(list));            }
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 2");
            //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
            //Rect highlightRect = titleRect;
            //highlightRect.xMin -= HighlightPadding;
            //highlightRect.xMax = entryRect.xMax + HighlightPadding;
            //Widgets.DrawHighlightIfMouseover(highlightRect);
            TooltipHandler.TipRegion(buttonRect, delegate
            {
                return KinseyFormulaTooltip;
            }, KinseyFormulaTooltip.GetHashCode());            titleRect.y += RowHeight;            entryRect.y += RowHeight;

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

            Rect customWeightEntryRect = new Rect(buttonRect.x, titleRect.y, KinseyCustomEntryWidth, KinseyCustomEntryHeight);
            for (int i = 0; i <= 6; i++)
            {
                KinseyCustomEntry(i, ref customWeightEntryRect);
            }
            titleRect.y += customWeightEntryRect.height;
            entryRect.y += customWeightEntryRect.height;        }
        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 3");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

        CheckboxEntry("enableEmpathy", ref titleRect, ref entryRect);        CheckboxEntry("enableIndividuality", ref titleRect, ref entryRect);        CheckboxEntry("enableDateLetters", ref titleRect, ref entryRect);        CheckboxEntry("enableElections", ref titleRect, ref entryRect);        if (BoolCachedDict.TryGetValue("enableElections", out bool value) && value)
        {
            NumericEntry("mayorAge", 0f, 1E+09f, ref titleRect, ref entryRect);
            NumericEntry("mayorVisitMtbHours", 0f, 1E+09f, ref titleRect, ref entryRect);
        }
        NumericEntry("mentalBreakAnxietyChance", 0f, 2f, ref titleRect,  ref entryRect);        NumericEntry("imprisonedDebuff", 0f, 100f, ref titleRect, ref entryRect);        NumericEntry("conversationDuration", 15f, 180f, ref titleRect, ref entryRect);        NumericEntry("romanceChanceMultiplier", 0f, 1E+09f, ref titleRect, ref entryRect);        NumericEntry("romanceOpinionThreshold", -99f, 99f, ref titleRect, ref entryRect);                NumericEntry("traitOpinionMultiplier", 0f, 2f, ref titleRect, ref entryRect);
        NumericEntry("personalityExtremeness", 0f, 1f, ref titleRect, ref entryRect);
        NumericEntry("ideoPsycheMultiplier", 0f, 10f, ref titleRect, ref entryRect);

        //CheckboxEntry(ref titleRect, settingTitleTooltipList[2], ref entryRect, ref enableEmpathyCached);        //CheckboxEntry(ref titleRect, settingTitleTooltipList[3], ref entryRect, ref enableIndividualityCached);        //CheckboxEntry(ref titleRect, settingTitleTooltipList[4], ref entryRect, ref enableElectionsCached);        //CheckboxEntry(ref titleRect, settingTitleTooltipList[5], ref entryRect, ref enableDateLettersCached);

        //NumericEntry(ref titleRect, settingTitleTooltipList[6], ref entryRect, ref mentalBreakAnxietyChanceCached, ref mentalBreakAnxietyChanceBuffer, 0f, 2f);        //NumericEntry(ref titleRect, settingTitleTooltipList[7], ref entryRect, ref imprisonedDebuffCached, ref imprisonedDebuffBuffer, 0f, 100f);        //NumericEntry(ref titleRect, settingTitleTooltipList[8], ref entryRect, ref conversationDurationCached, ref conversationDurationBuffer, 15f, 180f);        //NumericEntry(ref titleRect, settingTitleTooltipList[9], ref entryRect, ref romanceChanceMultiplierCached, ref romanceChanceMultiplierBuffer, 0f);        //NumericEntry(ref titleRect, settingTitleTooltipList[10], ref entryRect, ref romanceOpinionThresholdCached, ref romanceOpinionThresholdBuffer, -99f, 99f);        //NumericEntry(ref titleRect, settingTitleTooltipList[11], ref entryRect, ref mayorAgeCached, ref mayorAgeBuffer, 0f);        //NumericEntry(ref titleRect, settingTitleTooltipList[12], ref entryRect, ref traitOpinionMultiplierCached, ref traitOpinionMultiplierBuffer, 0f, 2f);
        //NumericEntry(ref titleRect, settingTitleTooltipList[13], ref entryRect, ref personalityExtremenessCached, ref personalityExtremenessBuffer, 0f, 1f);
        //NumericEntry(ref titleRect, settingTitleTooltipList[14], ref entryRect, ref ideoPsycheMultiplierCached, ref ideoPsycheMultiplierBuffer, 0f, 10f);

        Widgets.EndScrollView();

        Rect labelRect = new Rect(SpeciesX + HighlightPadding, yMin, SpeciesWidthList[0], RowHeight);        Rect psycheRect = new Rect(labelRect.xMax, labelRect.y, SpeciesWidthList[1], RowHeight);        Rect ageGapRect = new Rect(psycheRect.xMax, labelRect.y, SpeciesWidthList[2], RowHeight);        Rect minDatingAgeRect = new Rect(ageGapRect.xMax, labelRect.y, SpeciesWidthList[3], RowHeight);        Rect minLovinAgeRect = new Rect(minDatingAgeRect.xMax, labelRect.y, SpeciesWidthList[4], RowHeight);

        Widgets.Label(labelRect, SpeciesTitleList[0]);        Widgets.Label(psycheRect, SpeciesTitleList[1]);        Widgets.Label(ageGapRect, SpeciesTitleList[2]);        Widgets.Label(minDatingAgeRect, SpeciesTitleList[3]);        Widgets.Label(minLovinAgeRect, SpeciesTitleList[4]);

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 4");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

        float columnHeight = RowHeight + HighlightPadding + SpeciesHeight;
        ColumnHighlightAndTooltip(labelRect, columnHeight, "SpeciesSettingsTooltip".Translate());        ColumnHighlightAndTooltip(psycheRect, columnHeight, "EnablePsycheTooltip".Translate());        ColumnHighlightAndTooltip(ageGapRect, columnHeight, "EnableAgeGapTooltip".Translate());        ColumnHighlightAndTooltip(minDatingAgeRect, columnHeight, "MinDatingAgeTooltip".Translate());        ColumnHighlightAndTooltip(minLovinAgeRect, columnHeight, "MinLovinAgeTooltip".Translate());

        UIAssets.DrawLineHorizontal(SpeciesRect.x, labelRect.yMax, SpeciesRect.width, UIAssets.ModEntryLineColor);

        labelRect.y += HighlightPadding;        psycheRect.y += HighlightPadding;        ageGapRect.y += HighlightPadding;        minDatingAgeRect.y += HighlightPadding;        minLovinAgeRect.y += HighlightPadding;
        Rect testHighlightRect = new Rect(0f, 0f, minLovinAgeRect.xMax - labelRect.x, labelRect.height);        Widgets.BeginScrollView(SpeciesRect, ref RightScrollPosition, SpeciesViewRect);        labelRect.position = new Vector2(HighlightPadding, 0f);        psycheRect.position = new Vector2(labelRect.xMax, labelRect.y);        ageGapRect.position = new Vector2(psycheRect.xMax, labelRect.y);        minDatingAgeRect.position = new Vector2(ageGapRect.xMax, labelRect.y);
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
        foreach (string[] namePair in SpeciesNameList)        {            string defName = namePair[0];            string label = namePair[1];            float val;            string buffer;
            //GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(labelRect, label);
            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5b");
            //Log.Message("Retrieving " + defName + " from speciesDictCached");
            Widgets.Checkbox(psycheVec, ref speciesDictCached[defName].enablePsyche);
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5c");
            if (speciesDictCached[defName].enablePsyche)            {                Widgets.Checkbox(ageGapVec, ref speciesDictCached[defName].enableAgeGap);                val = speciesDictCached[defName].minDatingAge;                buffer = speciesBuffer[defName][0];
                //Widgets.TextFieldNumeric(minDatingAgeRect, ref val, ref buffer, min: -1);
                UIAssets.TextFieldFloat(minDatingAgeRect, ref val, ref buffer, min: -1);                speciesDictCached[defName].minDatingAge = val;                speciesBuffer[defName][0] = buffer;                val = speciesDictCached[defName].minLovinAge;                buffer = speciesBuffer[defName][1];
                //Widgets.TextFieldNumeric(minLovinAgeRect, ref val, ref buffer, min: -1);
                UIAssets.TextFieldFloat(minLovinAgeRect, ref val, ref buffer, min: -1);                speciesDictCached[defName].minLovinAge = val;                speciesBuffer[defName][1] = buffer;            }
            //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 5d");
            //Rect rowHighlightRect = labelRect;
            //rowHighlightRect.xMax = minLovinAgeRect.xMax;
            //Widgets.DrawHighlightIfMouseover(rowHighlightRect);

            Widgets.DrawHighlightIfMouseover(testHighlightRect);            labelRect.y += RowHeight;            psycheVec.y += RowHeight;            ageGapVec.y += RowHeight;            minDatingAgeRect.y += RowHeight;            minLovinAgeRect.y += RowHeight;            testHighlightRect.y += RowHeight;        }        Widgets.EndScrollView();        UIAssets.DrawLineHorizontal(SpeciesRect.x, SpeciesRect.yMax + HighlightPadding, SpeciesRect.width, UIAssets.ModEntryLineColor);

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

        GenUI.ResetLabelAlign();        SaveAllSettings();        Widgets.BeginGroup(totalRect);    }    public static void SetCacheAndBufferBasedOnKinseyMode()
    {
        kinseyWeightCustomCached.Clear();        kinseyWeightCustomBuffer.Clear();
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


    public static void CheckboxEntry(string boolSettingName, ref Rect titleRect,  ref Rect entryRect)
    {
        if (BoolCachedDict.TryGetValue(boolSettingName, out bool cachedBool) != true)
        {
            Log.Warning("SettingsWindowUtility, could not find " + boolSettingName + " in cached settings");
            return;
        }
        Widgets.Checkbox(entryRect.x, entryRect.center.y - 0.5f * CheckboxSize, ref cachedBool);
        BoolCachedDict[boolSettingName] = cachedBool;
        //if (cached is bool cachedBool)
        //{
        //}
        //else
        //{
        //    Log.Warning("SettingsWindowUtility, could not cast " + boolSettingName + " as a bool.");
        //    return;
        //}
        Widgets.Label(titleRect, TitleDict[boolSettingName]);
        Rect highlightRect = titleRect;
        highlightRect.xMin -= HighlightPadding;
        highlightRect.xMax = entryRect.xMax + HighlightPadding;
        Widgets.DrawHighlightIfMouseover(highlightRect);
        TooltipHandler.TipRegion(highlightRect, delegate
        {
            return TooltipDict[boolSettingName];
        }, TooltipDict[boolSettingName].GetHashCode());
        titleRect.y += RowHeight;
        entryRect.y += RowHeight;
    }

    //public static void CheckboxEntry(ref Rect titleRect, Pair<string, string> titleTooltip, ref Rect entryRect, ref bool enableCached)
    //{
    //    Widgets.Label(titleRect, titleTooltip.First);
    //    Widgets.Checkbox(entryRect.x, entryRect.center.y - 0.5f * CheckboxSize, ref enableCached);
    //    Rect highlightRect = titleRect;
    //    highlightRect.xMin -= HighlightPadding;
    //    highlightRect.xMax = entryRect.xMax + HighlightPadding;
    //    Widgets.DrawHighlightIfMouseover(highlightRect);
    //    TooltipHandler.TipRegion(highlightRect, delegate
    //    {
    //        return titleTooltip.Second;
    //    }, titleTooltip.Second.GetHashCode());

    //    titleRect.y += RowHeight;
    //    entryRect.y += RowHeight;
    //}

    public static void NumericEntry(string floatSettingName, float min, float max, ref Rect titleRect, ref Rect entryRect)    {
        if (FloatCachedDict.TryGetValue(floatSettingName, out float numericCached) != true)
        {
            Log.Warning("SettingsWindowUtility, could not find " + floatSettingName + " in cached settings");
            return;
        }
        string numericBuffer = FloatBufferDict[floatSettingName];
        UIAssets.TextFieldFloat(entryRect, ref numericCached, ref numericBuffer, min, max);
        FloatCachedDict[floatSettingName] = numericCached;
        FloatBufferDict[floatSettingName] = numericBuffer;
        //if (cached is float numericCached)
        //{
        //}
        //else
        //{
        //    Log.Warning("SettingsWindowUtility, could not cast " + floatSettingName + " as a float.");
        //    return;
        //}
        Widgets.Label(titleRect, TitleDict[floatSettingName]);
        Rect highlightRect = titleRect;        highlightRect.xMin -= HighlightPadding;        highlightRect.xMax = entryRect.xMax + HighlightPadding;        Widgets.DrawHighlightIfMouseover(highlightRect);        TooltipHandler.TipRegion(highlightRect, delegate        {            return TooltipDict[floatSettingName];        }, TooltipDict[floatSettingName].GetHashCode());        titleRect.y += RowHeight;        entryRect.y += RowHeight;    }

    //public static void NumericEntry(ref Rect titleRect, Pair<string, string> titleTooltip, ref Rect entryRect, ref float numericCached, ref string numericBuffer, float min = 0f, float max = 1E+09f)    //{    //    Widgets.Label(titleRect, titleTooltip.First);
    //    UIAssets.TextFieldFloat(entryRect, ref numericCached, ref numericBuffer, min, max);
    //    Rect highlightRect = titleRect;    //    highlightRect.xMin -= HighlightPadding;    //    highlightRect.xMax = entryRect.xMax + HighlightPadding;    //    Widgets.DrawHighlightIfMouseover(highlightRect);    //    TooltipHandler.TipRegion(highlightRect, delegate    //    {    //        return titleTooltip.Second;    //    }, titleTooltip.Second.GetHashCode());    //    titleRect.y += RowHeight;    //    entryRect.y += RowHeight;    //}

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

    public static void SetAllCachedToSettings()    {
       
        foreach (string boolSettingName in PsychologySettings.BoolSettingNameList)
        {
            if (PsychologySettings.GetSettingFromName(boolSettingName) is bool boolValue)
            {
                BoolCachedDict[boolSettingName] = boolValue;
            }
            else
            {
                Log.Warning("SetAllCachedToSettings, " + boolSettingName + " not a valid bool setting name");
            }
        }        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            if (PsychologySettings.GetSettingFromName(floatSettingName) is float floatValue)
            {
                FloatCachedDict[floatSettingName] = floatValue;
                FloatBufferDict[floatSettingName] = floatValue.ToString();
            }
            else
            {
                Log.Warning("SetAllCachedToSettings, " + floatSettingName + " not a valid bool setting name");
            }
        }        kinseyFormulaCached = PsychologySettings.kinseyFormula;
        SetCacheAndBufferBasedOnKinseyMode();

        speciesDictCached.Clear();        speciesBuffer.Clear();
        foreach (ThingDef def in SpeciesHelper.registeredSpecies)        {
            string defName = def.defName;
            SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
            speciesDictCached[defName] = new SpeciesSettings(settings);
            speciesBuffer[defName] = new List<string> { settings.minDatingAge.ToString(), settings.minLovinAge.ToString() };
        }

        //enableKinseyCached = PsychologySettings.enableKinsey;
        //enableEmpathyCached = PsychologySettings.enableEmpathy;
        //enableIndividualityCached = PsychologySettings.enableIndividuality;
        //enableElectionsCached = PsychologySettings.enableElections;
        //enableDateLettersCached = PsychologySettings.enableDateLetters;

        //mentalBreakAnxietyChanceCached = PsychologySettings.mentalBreakAnxietyChance;
        //mentalBreakAnxietyChanceBuffer = mentalBreakAnxietyChanceCached.ToString();

        //imprisonedDebuffCached = PsychologySettings.imprisonedDebuff;
        //imprisonedDebuffBuffer = imprisonedDebuffCached.ToString();

        //conversationDurationCached = PsychologySettings.conversationDuration;
        //conversationDurationBuffer = conversationDurationCached.ToString();

        //romanceChanceMultiplierCached = PsychologySettings.romanceChanceMultiplier;
        //romanceChanceMultiplierBuffer = romanceChanceMultiplierCached.ToString();

        //romanceOpinionThresholdCached = PsychologySettings.romanceOpinionThreshold;
        //romanceOpinionThresholdBuffer = romanceOpinionThresholdCached.ToString();

        //mayorAgeCached = PsychologySettings.mayorAge;
        //mayorAgeBuffer = mayorAgeCached.ToString();

        //traitOpinionMultiplierCached = PsychologySettings.traitOpinionMultiplier;
        //traitOpinionMultiplierBuffer = traitOpinionMultiplierCached.ToString();

        //personalityExtremenessCached = PsychologySettings.personalityExtremeness;
        //personalityExtremenessBuffer = personalityExtremenessCached.ToString();

        //ideoPsycheMultiplierCached = PsychologySettings.ideoPsycheMultiplier;
        //ideoPsycheMultiplierBuffer = ideoPsycheMultiplierCached.ToString();

        ////Log.Message("SetAllCachedToSettings(), step 4");
        //speciesDictCached.Clear();
        //speciesBuffer.Clear();
        ////Log.Message("humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());
        ////Log.Message("SetAllCachedToSettings(), step 5");
        //foreach (ThingDef def in SpeciesHelper.registeredSpecies)
        //{
        //    //Log.Message("SetAllCachedToSettings(), step 5a");
        //    string defName = def.defName;
        //    //Log.Message("SetAllCachedToSettings(), step 5b");
        //    SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
        //    //Log.Message("SetAllCachedToSettings(), step 5c");
        //    speciesDictCached[defName] = settings;
        //    //Log.Message("SetAllCachedToSettings(), step 5d");
        //    speciesBuffer[defName] = new List<string> { settings.minDatingAge.ToString(), settings.minLovinAge.ToString() };
        //    //Log.Message("SetAllCachedToSettings(), step 5e");
        //}
        ////Log.Message("SetAllCachedToSettings(), step 6");
    }    public static void SaveAllSettings()    {        foreach (string boolSettingName in PsychologySettings.BoolSettingNameList)
        {
            PsychologySettings.SetSettingFromName(boolSettingName, BoolCachedDict[boolSettingName]);
        }        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            PsychologySettings.SetSettingFromName(floatSettingName, FloatCachedDict[floatSettingName]);
        }        PsychologySettings.kinseyFormula = kinseyFormulaCached;        if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        {
            PsychologySettings.kinseyWeightCustom = kinseyWeightCustomCached.ListFullCopy();
        }        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)        {            PsychologySettings.speciesDict[kvp.Key] = new SpeciesSettings(kvp.Value);        }        //PsychologySettings.enableKinsey = enableKinseyCached;        //PsychologySettings.kinseyFormula = kinseyFormulaCached;
        //if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        //{
        //    PsychologySettings.kinseyWeightCustom = kinseyWeightCustomCached.ListFullCopy();
        //}        //PsychologySettings.enableEmpathy = enableEmpathyCached;        //PsychologySettings.enableIndividuality = enableIndividualityCached;        //PsychologySettings.enableElections = enableElectionsCached;        //PsychologySettings.enableDateLetters = enableDateLettersCached;
        //PsychologySettings.mentalBreakAnxietyChance = mentalBreakAnxietyChanceCached;        //PsychologySettings.imprisonedDebuff = imprisonedDebuffCached;        //PsychologySettings.conversationDuration = Mathf.Max(conversationDurationCached, 15f);        //PsychologySettings.romanceChanceMultiplier = romanceChanceMultiplierCached;        //PsychologySettings.romanceOpinionThreshold = romanceOpinionThresholdCached;        //PsychologySettings.mayorAge = mayorAgeCached;        //PsychologySettings.traitOpinionMultiplier = traitOpinionMultiplierCached;        //PsychologySettings.personalityExtremeness = personalityExtremenessCached;
        //PsychologySettings.ideoPsycheMultiplier = ideoPsycheMultiplierCached;
        //foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)        //{        //    PsychologySettings.speciesDict[kvp.Key] = kvp.Value;        //}    }

    public static void SaveBackupOfSettings()
    {
        foreach (string boolSettingName in PsychologySettings.BoolSettingNameList)
        {
            BoolBackupDict[boolSettingName] = (bool)PsychologySettings.GetSettingFromName(boolSettingName);
        }        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            FloatBackupDict[floatSettingName] = (float)PsychologySettings.GetSettingFromName(floatSettingName);
        }
        kinseyFormulaBackup = PsychologySettings.kinseyFormula;
        kinseyWeightCustomBackup = PsychologySettings.kinseyWeightCustom.ListFullCopy();
        speciesDictBackup.Clear();
        foreach (ThingDef def in SpeciesHelper.registeredSpecies)
        {
            speciesDictBackup[def.defName] = new SpeciesSettings(SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def));
        }

        //enableKinseyBackup = PsychologySettings.enableKinsey;
        //kinseyFormulaBackup = PsychologySettings.kinseyFormula;
        //kinseyWeightCustomBackup = PsychologySettings.kinseyWeightCustom.ListFullCopy();
        //enableEmpathyBackup = PsychologySettings.enableEmpathy;
        //enableIndividualityBackup = PsychologySettings.enableIndividuality;
        //enableElectionsBackup = PsychologySettings.enableElections;
        //enableDateLettersBackup = PsychologySettings.enableDateLetters;
        //mentalBreakAnxietyChanceBackup = PsychologySettings.mentalBreakAnxietyChance;

        //imprisonedDebuffBackup = PsychologySettings.imprisonedDebuff;
        //conversationDurationBackup = PsychologySettings.conversationDuration;
        //romanceChanceMultiplierBackup = PsychologySettings.romanceChanceMultiplier;
        //romanceOpinionThresholdBackup = PsychologySettings.romanceOpinionThreshold;
        //mayorAgeBackup = PsychologySettings.mayorAge;
        //traitOpinionMultiplierBackup = PsychologySettings.traitOpinionMultiplier;
        //personalityExtremenessBackup = PsychologySettings.personalityExtremeness;
        //ideoPsycheMultiplierBackup = PsychologySettings.ideoPsycheMultiplier;

        //speciesDictBackup.Clear();
        //foreach (ThingDef def in SpeciesHelper.registeredSpecies)
        //{
        //    speciesDictBackup[def.defName] = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
        //}
    }    public static void RestoreSettingsFromBackup()
    {
        foreach (string boolSettingName in PsychologySettings.BoolSettingNameList)
        {
            PsychologySettings.SetSettingFromName(boolSettingName, BoolBackupDict[boolSettingName]);
        }        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            PsychologySettings.SetSettingFromName(floatSettingName, FloatBackupDict[floatSettingName]);
        }
        PsychologySettings.kinseyFormula = kinseyFormulaBackup;
        PsychologySettings.kinseyWeightCustom = kinseyWeightCustomBackup.ListFullCopy();
        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictBackup)
        {
            PsychologySettings.speciesDict[kvp.Key] = new SpeciesSettings(kvp.Value);
        }

        //PsychologySettings.enableKinsey = enableKinseyBackup;
        //PsychologySettings.kinseyFormula = kinseyFormulaBackup;
        //if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        //{
        //    PsychologySettings.kinseyWeightCustom = kinseyWeightCustomBackup.ListFullCopy();
        //}
        //PsychologySettings.enableEmpathy = enableEmpathyBackup;
        //PsychologySettings.enableIndividuality = enableIndividualityBackup;
        //PsychologySettings.enableElections = enableElectionsBackup;
        //PsychologySettings.enableDateLetters = enableDateLettersBackup;
        //PsychologySettings.mentalBreakAnxietyChance = mentalBreakAnxietyChanceBackup;

        //PsychologySettings.imprisonedDebuff = imprisonedDebuffBackup;
        //PsychologySettings.conversationDuration = conversationDurationBackup;
        //PsychologySettings.romanceChanceMultiplier = romanceChanceMultiplierBackup;
        //PsychologySettings.romanceOpinionThreshold = romanceOpinionThresholdBackup;
        //PsychologySettings.mayorAge = mayorAgeBackup;
        //PsychologySettings.traitOpinionMultiplier = traitOpinionMultiplierBackup;
        //PsychologySettings.personalityExtremeness = personalityExtremenessBackup;
        //PsychologySettings.ideoPsycheMultiplier = ideoPsycheMultiplierBackup;

        //foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictBackup)
        //{
        //    PsychologySettings.speciesDict[kvp.Key] = kvp.Value;
        //}
    }}
