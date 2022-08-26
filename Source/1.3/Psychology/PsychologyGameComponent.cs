using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology
{
    public class PsychologyGameComponent : GameComponent
    {
        public override void LoadedGame()
        {
            Log.Message("Loading game");
            MayorUtility.BuildMayorDictionary();
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
            {

            }

            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
            {
                if (pawn.story == null || !PsycheHelper.PsychologyEnabled(pawn))
                {
                    continue;
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    RemoveTrait(pawn, TraitDefOf.Gay);
                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 0f, 0f, 0f, 1f, 2f);
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                {
                    RemoveTrait(pawn, TraitDefOf.Bisexual);
                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 1f, 2f, 1f, 0f, 0f);
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                {
                    RemoveTrait(pawn, TraitDefOf.Asexual);
                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = 0.10f * Rand.ValueSeeded(11 * pawn.HashOffset() + 8);
                }
            }
        }





    }
}

