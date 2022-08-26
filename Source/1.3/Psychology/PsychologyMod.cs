using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;
using HarmonyLib;

namespace Psychology
{
    public class PsychologyMod : Mod
    {
        public static PsychologySettings settings;

        public PsychologyMod(ModContentPack content) : base(content)
        {
            settings = this.GetSettings<PsychologySettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {

        }

    }
}
