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

namespace Psychology;

public class SpeciesSettings : IExposable
{
    public bool enablePsyche = true;
    public bool enableAgeGap = true;
    public float minDatingAge = 14f;
    public float minLovinAge = 16f;

    public SpeciesSettings()
    {
    }

    public SpeciesSettings(bool EnablePsyche = true, bool EnableAgeGap = true, float MinDatingAge = 14f, float MinLovinAge = 16f)
    {
        this.enablePsyche = EnablePsyche;
        this.enableAgeGap = EnableAgeGap;
        this.minDatingAge = MinDatingAge;
        this.minLovinAge = MinLovinAge;
    }

    public SpeciesSettings(SpeciesSettings settings)
    {
        this.enablePsyche = settings.enablePsyche;
        this.enableAgeGap = settings.enableAgeGap;
        this.minDatingAge = settings.minDatingAge;
        this.minLovinAge = settings.minLovinAge;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref enablePsyche, "enablePsyche", true, true);
        Scribe_Values.Look(ref enableAgeGap, "enableAgeGap", true, true);
        Scribe_Values.Look(ref minDatingAge, "minDatingAge", 14f, true);
        Scribe_Values.Look(ref minLovinAge, "minLovinAge", 16f, true);
    }
}

public class SpeciesSettingsDef : Def
{
    public ThingDef pawnDef;
    public bool enablePsyche = true;
    public bool enableAgeGap = true;
    public float minDatingAge = 14f;
    public float minLovinAge = 16f;
}
