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

namespace Psychology;

// ToDo:
// - Add a "Electoral debate" ritual, to accelerate the vote
// - Make indoctrination texture with hands hovering over a brain
// - Make setting to control magnitude of the conversation opinion offsets
// - Make panic attack a mental break

public class PsychologyMod : Mod
{
    public static PsychologySettings settings;

    public static int lastDrawFrame = Time.frameCount;

    public override string SettingsCategory() => "Psychology";

    public PsychologyMod(ModContentPack content) : base(content)
    {
        settings = this.GetSettings<PsychologySettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        if (Time.frameCount - lastDrawFrame > 5)
        {
            Log.Message("DoSettingsWindowContents, SpeciesHelper.Initialize()");
            SettingsWindowUtility.Initialize();
        }
        SettingsWindowUtility.DrawSettingsWindow(inRect);
        lastDrawFrame = Time.frameCount;
    }
}
