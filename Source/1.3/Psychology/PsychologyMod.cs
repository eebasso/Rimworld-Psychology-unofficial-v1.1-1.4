using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;
using Verse.Noise;
using Unity;
using static EdB.PrepareCarefully.ProviderTitles;

namespace Psychology;

// ToDo:
// - Optimize individuality alerts
// - Create Prude vs Nudist opinion modifier
// - Optimize anxiety ticker

public class PsychologyMod : Mod
{
    public static PsychologySettings settings;

    public override string SettingsCategory() => "Psychology";

    public PsychologyMod(ModContentPack content) : base(content)
    {
        settings = this.GetSettings<PsychologySettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        SettingsWindowUtility.DrawSettingsWindow(inRect);
    }
}

//base.DoSettingsWindowContents(inRect);
//GUI.EndGroup();
//inRect = SettingsWindowUtility.WindowRect;
//Find.WindowStack.currentlyDrawnWindow.windowRect = inRect;
//Find.WindowStack.currentlyDrawnWindow.windowRect.center = 0.5f * new Vector2(UI.screenWidth, UI.screenHeight);
//GUI.BeginGroup(inRect);