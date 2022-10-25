using System;using System.Collections.Generic;using System.Linq;using System.Text;using System.Text.RegularExpressions;using System.Reflection;using RimWorld;using RimWorld.Planet;using Verse;using Verse.AI.Group;using Verse.Grammar;using UnityEngine;using Verse.Noise;using Unity;
using HarmonyLib;

namespace Psychology;

// ToDo: make a separate class for each setting to centralize the cache, buffer, 
[StaticConstructorOnStartup]
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

    public static Dictionary<string, EntryFloat> FloatEntryDict = new Dictionary<string, EntryFloat>();
    public static Dictionary<string, float> FloatBackupDict = new Dictionary<string, float>();

    //public static Dictionary<string, float> FloatCachedDict = new Dictionary<string, float>();
    //public static Dictionary<string, string> FloatBufferDict = new Dictionary<string, string>();


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
    public static List<string> SpeciesTooltipList = new List<string> { "SpeciesSettingsTooltip".Translate(), "EnablePsycheTooltip".Translate(), "EnableAgeGapTooltip".Translate(), "MinDatingAgeTooltip".Translate("dating".Translate(), 14), "MinLovinAgeTooltip".Translate("lovin".Translate(), 16) };
    public static Dictionary<string, SpeciesSettings> speciesDictCached = new Dictionary<string, SpeciesSettings>();    public static Dictionary<string, EntryFloat> speciesMinDatingEntry = new Dictionary<string, EntryFloat>();    public static Dictionary<string, EntryFloat> speciesMinLovinEntry = new Dictionary<string, EntryFloat>();    public static Dictionary<string, SpeciesSettings> speciesDictBackup = new Dictionary<string, SpeciesSettings>();    //public static Dictionary<string, List<string>> speciesBuffer = new Dictionary<string, List<string>>();


    public static List<float> SpeciesWidthList = new List<float>();
    public static List<string[]> SpeciesNameList = new List<string[]>();

    //public static List<Pair<string, string>> settingTitleTooltipList = new List<Pair<string, string>>();

    //public static float settingTitleHeight = 34f;
    public static float TitleWidth = 0f;    public static float EntryWidth = Text.CalcSize("100.000").x;    public static float LeftColumnWidth;    //public static float MaxEntryWidth;

    public static float KinseyModeButtonWidth = 0f;
    public static float KinseyCustomEntryWidth;
    public const float KinseyCustomEntryHeight = 3f * RowHeight;

    public static Rect LeftColumnRect;
    public static Rect LeftColumnViewRect;

    public static Rect SpeciesRect;    public static Rect SpeciesViewRect;

    public static string DefaultButtonText = "Default".Translate();
    public static Rect DefaultButtonRect;

    public static string UndoButtonText = "Cancel".Translate();
    public static Rect UndoButtonRect;    public static Vector2 LeftScrollPosition = Vector2.zero;    public static Vector2 RightScrollPosition = Vector2.zero;

    public static Rect titleRect;
    public static Rect entryRect;
    public static Rect buttonRect;

    public static bool romanceOpen = false;
    public static bool electionsOpen = false;
    public static bool opinionChangesOpen = false;
    public static bool otherSettingsOpen = false;

    static SettingsWindowUtility()
    {
        SaveBackupOfSettings();
    }

    // Initialize after SpeciesHelper
    public static void Initialize()    {
        SetAllCachedToSettings();
        //Log.Message("SettingsWindowUtility.Initialize()");
        Text.Font = GameFont.Small;
        float leftColumnViewHeight = KinseyCustomEntryHeight;
        TitleWidth = 0f;
        foreach (string settingName in PsychologySettings.CombinedSettingNameList)        {            string title = (settingName.CapitalizeFirst() + "Title").Translate();            string tooltip = (settingName.CapitalizeFirst() + "Tooltip").Translate();
            Vector2 size = Text.CalcSize(title);
            TitleDict[settingName] = title;
            TooltipDict[settingName] = tooltip;
            TitleWidth = Mathf.Max(size.x, TitleWidth);            leftColumnViewHeight += RowHeight;
        }
        
        KinseyModeButtonWidth = 0f;
        foreach (KeyValuePair<KinseyMode, string> kvp in KinseyFormulaTitleDict)        {            Vector2 size = Text.CalcSize(kvp.Value);            KinseyModeButtonWidth = Mathf.Max(size.x, KinseyModeButtonWidth);
            //entryHeight = Mathf.Max(size.y, entryHeight);
        }        KinseyModeButtonWidth += 20f;
        TitleWidth = Mathf.Max(KinseyModeButtonWidth - EntryWidth, TitleWidth);
        TitleWidth += 2f * HighlightPadding;


        float leftColumnHeight = WindowHeight - Window.StandardMargin - LowerAreaHeight - yMin;
        float leftScrollBarWidth = leftColumnViewHeight < leftColumnHeight ? 0f : GenUI.ScrollBarWidth;

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

        List<string> labelList = new List<string>();        List<string> repeatList = new List<string>();        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)
        {
            string label = SpeciesHelper.registeredSpecies.First(x => x.defName == kvp.Key).label.CapitalizeFirst();
            if (labelList.Contains(label))
            {
                repeatList.AddDistinct(label);
            }
            labelList.Add(label);
        }
        SpeciesNameList.Clear();
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

        LeftColumnWidth = TitleWidth + EntryWidth + leftScrollBarWidth;        LeftColumnRect = new Rect(xMin, yMin, LeftColumnWidth, leftColumnHeight);
        LeftColumnViewRect = new Rect(0f, 0f, LeftColumnRect.width - leftScrollBarWidth, leftColumnViewHeight);        KinseyCustomEntryWidth = LeftColumnViewRect.width / 7f;        SpeciesX = Window.StandardMargin + LeftColumnWidth + BoundaryPadding;        SpeciesWidth = WindowWidth - Window.StandardMargin - SpeciesX;

        SpeciesRect = new Rect(SpeciesX, SpeciesY, SpeciesWidth, SpeciesHeight);
        SpeciesViewRect = new Rect(0f, 0f, SpeciesRect.width - rightScrollBarWidth, speciesViewHeight);

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
        LeftColumnViewRect.height = 0f;

        titleRect = new Rect(HighlightPadding, 0f, TitleWidth - 2f * HighlightPadding, RowHeight);        entryRect = new Rect(titleRect.xMax, titleRect.y + HighlightPadding, EntryWidth, titleRect.height - 2f * HighlightPadding);
        buttonRect = new Rect(titleRect.x - HighlightPadding, titleRect.y, LeftColumnViewRect.width, titleRect.height);

        if (Widgets.ButtonText(buttonRect, "Romance and sexuality"))
        {
            romanceOpen = !romanceOpen;
        }
        ShiftRectsDown(RowHeight);

        if (romanceOpen)
        {
            CheckboxEntry(nameof(PsychologySettings.enableKinsey));
            if (BoolCachedDict.TryGetValue(nameof(PsychologySettings.enableKinsey), out bool enableKinseyCached) && enableKinseyCached)
            {
                buttonRect.xMin += HighlightPadding;
                buttonRect.xMax -= HighlightPadding;
                if (UIAssets.ButtonLabel(buttonRect, KinseyFormulaTitleDict[kinseyFormulaCached]))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Realistic], delegate
                    {
                        kinseyFormulaCached = KinseyMode.Realistic;
                        SetCacheAndBufferBasedOnKinseyMode();
                        // ToDo: Add these as translations
                        //Log.Message("Set to Realistic");
                    }));
                    list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Uniform], delegate
                    {
                        kinseyFormulaCached = KinseyMode.Uniform;
                        SetCacheAndBufferBasedOnKinseyMode();
                        //Log.Message("Set to Uniform");
                    }));
                    list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Invisible], delegate
                    {
                        kinseyFormulaCached = KinseyMode.Invisible;
                        SetCacheAndBufferBasedOnKinseyMode();
                        //Log.Message("Set to Invisible");
                    }));
                    list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Gaypocalypse], delegate
                    {
                        kinseyFormulaCached = KinseyMode.Gaypocalypse;
                        SetCacheAndBufferBasedOnKinseyMode();
                        //Log.Message("Set to Gaypocalypse");
                    }));
                    list.Add(new FloatMenuOption(KinseyFormulaTitleDict[KinseyMode.Custom], delegate
                    {
                        kinseyFormulaCached = KinseyMode.Custom;
                        SetCacheAndBufferBasedOnKinseyMode();
                        //Log.Message("Set to Custom");
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                buttonRect.xMin -= HighlightPadding;
                buttonRect.xMax += HighlightPadding;
                Widgets.DrawHighlightIfMouseover(buttonRect);
                TooltipHandler.TipRegion(buttonRect, delegate
                {
                    return KinseyFormulaTooltip;
                }, KinseyFormulaTooltip.GetHashCode());
                ShiftRectsDown(RowHeight);

                Rect customWeightEntryRect = new Rect(buttonRect.x, titleRect.y, KinseyCustomEntryWidth, KinseyCustomEntryHeight);
                for (int i = 0; i <= 6; i++)
                {
                    KinseyCustomEntry(i, ref customWeightEntryRect);
                }
                ShiftRectsDown(customWeightEntryRect.height);
            }
            CheckboxEntry(nameof(PsychologySettings.enableDateLetters));
            FloatEntry(nameof(PsychologySettings.romanceChanceMultiplier), 0f, 1E+09f);
            FloatEntry(nameof(PsychologySettings.romanceOpinionThreshold), -99f, 99f);
        }

        if (Widgets.ButtonText(buttonRect, "Elections"))
        {
            electionsOpen = !electionsOpen;
        }
        ShiftRectsDown(RowHeight);

        if (electionsOpen)
        {
            CheckboxEntry(nameof(PsychologySettings.enableElections));
            if (BoolCachedDict.TryGetValue(nameof(PsychologySettings.enableElections)))
            {
                FloatEntry(nameof(PsychologySettings.mayorAge), 0f, 1E+09f);
                FloatEntry(nameof(PsychologySettings.visitMayorMtbHours), 0f, 1E+09f);
            }
        }

        if (Widgets.ButtonText(buttonRect, "Opinion changes"))
        {
            opinionChangesOpen = !opinionChangesOpen;
        }
        ShiftRectsDown(RowHeight);
        if (opinionChangesOpen)
        {
            FloatEntry(nameof(PsychologySettings.traitOpinionMultiplier), 0f, 2f);
            FloatEntry(nameof(PsychologySettings.imprisonedDebuff), 0f, 100f);
            FloatEntry(nameof(PsychologySettings.conversationDuration), 15f, 180f);
            FloatEntry(nameof(PsychologySettings.convoOpinionMultiplier), 0f, 3f);
        }

        if (Widgets.ButtonText(buttonRect, "Other settings"))
        {
            otherSettingsOpen = !otherSettingsOpen;
        }
        ShiftRectsDown(RowHeight);
        if (otherSettingsOpen)
        {
            CheckboxEntry(nameof(PsychologySettings.enableEmpathy));
            CheckboxEntry(nameof(PsychologySettings.enableIndividuality));
            FloatEntry(nameof(PsychologySettings.mentalBreakAnxietyChance), 0f, 2f);
            FloatEntry(nameof(PsychologySettings.personalityExtremeness), 0f, 1f);
            FloatEntry(nameof(PsychologySettings.ideoPsycheMultiplier), 0f, 10f);
        }

        //Log.Message("SettingsWindowUtility.DrawSettingsWindow step 3");
        //Log.Message("SpeciesHelper.humanlikeDefs.Count() = " + SpeciesHelper.humanlikeDefs.Count());

        Widgets.EndScrollView();

        Rect labelRect = new Rect(SpeciesX + HighlightPadding, yMin, SpeciesWidthList[0], RowHeight);        Rect psycheRect = new Rect(labelRect.xMax, labelRect.y, SpeciesWidthList[1], RowHeight);        Rect ageGapRect = new Rect(psycheRect.xMax, labelRect.y, SpeciesWidthList[2], RowHeight);        Rect minDatingAgeRect = new Rect(ageGapRect.xMax, labelRect.y, SpeciesWidthList[3], RowHeight);        Rect minLovinAgeRect = new Rect(minDatingAgeRect.xMax, labelRect.y, SpeciesWidthList[4], RowHeight);

        Widgets.Label(labelRect, SpeciesTitleList[0]);        Widgets.Label(psycheRect, SpeciesTitleList[1]);        Widgets.Label(ageGapRect, SpeciesTitleList[2]);        Widgets.Label(minDatingAgeRect, SpeciesTitleList[3]);        Widgets.Label(minLovinAgeRect, SpeciesTitleList[4]);

        float columnHeight = RowHeight + HighlightPadding + SpeciesHeight;
        ColumnHighlightAndTooltip(labelRect, columnHeight, SpeciesTooltipList[0]);        ColumnHighlightAndTooltip(psycheRect, columnHeight, SpeciesTooltipList[1]);        ColumnHighlightAndTooltip(ageGapRect, columnHeight, SpeciesTooltipList[2]);        ColumnHighlightAndTooltip(minDatingAgeRect, columnHeight, SpeciesTooltipList[3]);        ColumnHighlightAndTooltip(minLovinAgeRect, columnHeight, SpeciesTooltipList[4]);

        UIAssets.DrawLineHorizontal(SpeciesRect.x, labelRect.yMax, SpeciesRect.width, UIAssets.ModEntryLineColor);

        labelRect.y += HighlightPadding;        psycheRect.y += HighlightPadding;        ageGapRect.y += HighlightPadding;        minDatingAgeRect.y += HighlightPadding;        minLovinAgeRect.y += HighlightPadding;
        Rect testHighlightRect = new Rect(0f, 0f, minLovinAgeRect.xMax - labelRect.x, labelRect.height);        Widgets.BeginScrollView(SpeciesRect, ref RightScrollPosition, SpeciesViewRect);        labelRect.position = new Vector2(HighlightPadding, 0f);        psycheRect.position = new Vector2(labelRect.xMax, labelRect.y);        ageGapRect.position = new Vector2(psycheRect.xMax, labelRect.y);        minDatingAgeRect.position = new Vector2(ageGapRect.xMax, labelRect.y);
        minLovinAgeRect.position = new Vector2(minDatingAgeRect.xMax, labelRect.y);
        //testHighlightRect.position = new Vector2(0f, 0f);

        //Vector2 psycheVec = new Vector2(psycheRect.x, psycheRect.center.y - 0.5f * CheckboxSize);
        //Vector2 ageGapVec = new Vector2(ageGapRect.x, ageGapRect.center.y - 0.5f * CheckboxSize);
        Vector2 psycheVec = new Vector2(psycheRect.center.x - 0.5f * CheckboxSize, psycheRect.center.y - 0.5f * CheckboxSize);
        Vector2 ageGapVec = new Vector2(ageGapRect.center.x - 0.5f * CheckboxSize, ageGapRect.center.y - 0.5f * CheckboxSize);

        minDatingAgeRect.width = EntryWidth;
        minDatingAgeRect.yMin += HighlightPadding;
        minDatingAgeRect.yMax -= HighlightPadding;
        minLovinAgeRect.width = EntryWidth;
        minLovinAgeRect.yMin += HighlightPadding;
        minLovinAgeRect.yMax -= HighlightPadding;

        foreach (string[] namePair in SpeciesNameList)        {            string defName = namePair[0];            string label = namePair[1];            //float val;            //string buffer;
            //GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(labelRect, label);
            if (UIAssets.ButtonLabel(labelRect, label))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("Reset".Translate(), delegate
                {
                    foreach (ThingDef def in SpeciesHelper.registeredSpecies)
                    {
                        if (def.defName == defName)
                        {
                            speciesDictCached[defName] = SpeciesHelper.DefaultSettingsForSpeciesDef(def);
                            speciesMinDatingEntry[defName].UpdateValueAndBuffer(speciesDictCached[defName].minDatingAge);
                            speciesMinLovinEntry[defName].UpdateValueAndBuffer(speciesDictCached[defName].minLovinAge);
                            //speciesBuffer[defName][0] = speciesDictCached[defName].minDatingAge.ToString();
                            //speciesBuffer[defName][1] = speciesDictCached[defName].minLovinAge.ToString();
                            break;
                        }
                    }
                }));
                Find.WindowStack.Add(new FloatMenu(list));
            }

            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            Widgets.Checkbox(psycheVec, ref speciesDictCached[defName].enablePsyche);
            if (speciesDictCached[defName].enablePsyche)            {                Widgets.Checkbox(ageGapVec, ref speciesDictCached[defName].enableAgeGap);                speciesMinDatingEntry[defName].NumericTextField(minDatingAgeRect, -1, float.MaxValue, false);                speciesMinLovinEntry[defName].NumericTextField(minLovinAgeRect, -1, float.MaxValue, false);
            }
            speciesDictCached[defName].minDatingAge = speciesMinDatingEntry[defName].valFloat;
            speciesDictCached[defName].minLovinAge = speciesMinLovinEntry[defName].valFloat;

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

        GenUI.ResetLabelAlign();        SaveAllSettings();        Widgets.BeginGroup(totalRect);    }    public static void ShiftRectsDown(float Yshift)
    {
        titleRect.y += Yshift;
        entryRect.y += Yshift;
        buttonRect.y += Yshift;
        LeftColumnViewRect.height += Yshift;
    }    public static void SetCacheAndBufferBasedOnKinseyMode()
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


    public static void CheckboxEntry(string boolSettingName)
    {
        if (!BoolCachedDict.TryGetValue(boolSettingName, out bool cachedBool))
        {
            Log.Warning("SettingsWindowUtility, could not find " + boolSettingName + " in cached settings");
            return;
        }
        //Widgets.Label(titleRect, TitleDict[boolSettingName]);
        if (UIAssets.ButtonLabel(titleRect, TitleDict[boolSettingName], false))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("Reset".Translate(), delegate
            {
                FieldInfo fieldInfoDefault = AccessTools.Field(typeof(PsychologySettings), boolSettingName + "Default");
                cachedBool = (bool)fieldInfoDefault.GetValue(null);
                BoolCachedDict[boolSettingName] = cachedBool;
            }));
            Find.WindowStack.Add(new FloatMenu(list));
        }
        Widgets.Checkbox(entryRect.x, entryRect.center.y - 0.5f * CheckboxSize, ref cachedBool);
        Rect highlightRect = titleRect;
        highlightRect.xMin -= HighlightPadding;
        highlightRect.xMax = entryRect.xMax + HighlightPadding;
        Widgets.DrawHighlightIfMouseover(highlightRect);
        TooltipHandler.TipRegion(highlightRect, delegate
        {
            return TooltipDict[boolSettingName];
        }, TooltipDict[boolSettingName].GetHashCode());
        BoolCachedDict[boolSettingName] = cachedBool;
        ShiftRectsDown(RowHeight);
    }

    public static void FloatEntry(string floatSettingName, float min, float max)    {
        //if (FloatCachedDict.TryGetValue(floatSettingName, out float floatCached) != true)
        //{
        //    Log.Warning("SettingsWindowUtility, could not find " + floatSettingName + " in cached settings");
        //    return;
        //}
        //string floatBuffer = FloatBufferDict[floatSettingName];

        if (!FloatEntryDict.TryGetValue(floatSettingName, out EntryFloat entryFloat))
        {
            Log.Warning("SettingsWindowUtility, could not find " + floatSettingName + " in cached settings");
            return;
        }

        if (UIAssets.ButtonLabel(titleRect, TitleDict[floatSettingName], false))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("Reset".Translate(), delegate
            {
                FieldInfo fieldInfoDefault = AccessTools.Field(typeof(PsychologySettings), floatSettingName + "Default");
                entryFloat.UpdateValueAndBuffer((float)fieldInfoDefault.GetValue(null));
            }));
            Find.WindowStack.Add(new FloatMenu(list));
        }

        entryFloat.NumericTextField(entryRect, min, max, false);
        //UIAssets.TextFieldFloat(entryRect, ref floatCached, ref floatBuffer, min, max);

        Rect highlightRect = titleRect;        highlightRect.xMin -= HighlightPadding;        highlightRect.xMax = entryRect.xMax + HighlightPadding;        Widgets.DrawHighlightIfMouseover(highlightRect);        TooltipHandler.TipRegion(highlightRect, delegate        {            return TooltipDict[floatSettingName];        }, TooltipDict[floatSettingName].GetHashCode());
        //FloatCachedDict[floatSettingName] = floatCached;
        //FloatBufferDict[floatSettingName] = floatBuffer;
        ShiftRectsDown(RowHeight);    }

    public static void KinseyCustomEntry(int i, ref Rect customWeightEntryRect)
    {
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

        UIAssets.TextFieldFloat(numberRect, ref val, ref buffer, 0f, 100f);
        kinseyWeightCustomBuffer[i] = buffer;

        float valSlider = GUI.VerticalSlider(SliderRect, val, 100f, 0f);
        if (val != kinseyWeightCustomCached[i])
        {
            kinseyWeightCustomCached[i] = val;
            kinseyFormulaCached = KinseyMode.Custom;
        }
        else if (valSlider != kinseyWeightCustomCached[i])
        {
            kinseyWeightCustomCached[i] = (float)Math.Round(valSlider, 1);
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
                //Log.Message("Set cache of " + boolSettingName + " to a value of " + boolValue);
            }
            else
            {
                Log.Warning("SetAllCachedToSettings, " + boolSettingName + " not a valid bool setting name");
            }
        }        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            if (PsychologySettings.GetSettingFromName(floatSettingName) is float floatValue)
            {
                if (FloatEntryDict.ContainsKey(floatSettingName))
                {
                    FloatEntryDict[floatSettingName].UpdateValueAndBuffer(floatValue);
                }
                else
                {
                    FloatEntryDict[floatSettingName] = new EntryFloat(floatValue, null, true, true, -1);
                }
                //FloatCachedDict[floatSettingName] = floatValue;
                //FloatBufferDict[floatSettingName] = floatValue.ToString();
                //Log.Message("Set cache of " + floatSettingName + " to a value of " + floatValue);
            }
            else
            {
                Log.Warning("SetAllCachedToSettings, " + floatSettingName + " not a valid bool setting name");
            }
        }        kinseyFormulaCached = PsychologySettings.kinseyFormula;
        SetCacheAndBufferBasedOnKinseyMode();

        //speciesDictCached.Clear();
        //speciesMinDatingEntry.Clear();
        //speciesMinLovinEntry.Clear();
        //speciesBuffer.Clear();
        foreach (ThingDef def in SpeciesHelper.registeredSpecies)        {
            string defName = def.defName;
            SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(def);
            speciesDictCached[defName] = new SpeciesSettings(settings);

            if (!speciesMinDatingEntry.ContainsKey(defName))
            {
                speciesMinDatingEntry[defName] = new EntryFloat(settings.minDatingAge, null, true, true, -1);
            }
            speciesMinDatingEntry[defName].UpdateValueAndBuffer(settings.minDatingAge);

            if (!speciesMinLovinEntry.ContainsKey(defName))
            {
                speciesMinLovinEntry[defName] = new EntryFloat(settings.minLovinAge, null, true, true, -1);
            }
            speciesMinLovinEntry[defName].UpdateValueAndBuffer(settings.minLovinAge);
            //speciesBuffer[defName] = new List<string> { settings.minDatingAge.ToString(), settings.minLovinAge.ToString() };
        }
    }    public static void SaveAllSettings()    {        foreach (string boolSettingName in PsychologySettings.BoolSettingNameList)
        {
            PsychologySettings.SetSettingFromName(boolSettingName, BoolCachedDict[boolSettingName]);
        }
        foreach (string floatSettingName in PsychologySettings.FloatSettingNameList)
        {
            PsychologySettings.SetSettingFromName(floatSettingName, FloatEntryDict[floatSettingName].valFloat);
        }
        PsychologySettings.kinseyFormula = kinseyFormulaCached;        if (PsychologySettings.kinseyFormula == KinseyMode.Custom)
        {
            PsychologySettings.kinseyWeightCustom = kinseyWeightCustomCached.ListFullCopy();
        }        foreach (KeyValuePair<string, SpeciesSettings> kvp in speciesDictCached)        {            PsychologySettings.speciesDict[kvp.Key] = new SpeciesSettings(kvp.Value);        }
    }

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
    }}
