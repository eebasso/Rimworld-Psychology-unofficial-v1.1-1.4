using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Diagnostics;
using UnityEngine;

namespace Psychology;

public class TreatmentRecipeDef : RecipeDef
{
    public HediffDef hediff;
    public TaleDef tale;
    public string illnessName;
    public List<Treatment> treatments;
}

public class Treatment
{
    public TraitDef trait;
    public int degree;
    public float easeOfTreatment;
}