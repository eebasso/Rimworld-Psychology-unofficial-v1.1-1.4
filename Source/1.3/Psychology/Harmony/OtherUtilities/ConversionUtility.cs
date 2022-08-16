using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Text;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ConversionUtility), nameof(ConversionUtility.ConversionPowerFactor_MemesVsTraits))]
    public static class ConversionUtility_ConversionPowerFactor_MemesVsTraits_Patch
    {
        // The value of sb gets changed
        [HarmonyPostfix]
        public static void ConversionPowerFactor_MemesVsTraits(ref float __result, Pawn initiator, Pawn recipient, StringBuilder sb)
        {
            float reciWithInitIdeo = 0.05f * PsycheHelper.Comp(recipient).Psyche.CompatibilityWithIdeo(initiator.Ideo);
            float reciWithReciIdeo = 0.05f * PsycheHelper.Comp(recipient).Psyche.CompatibilityWithIdeo(recipient.Ideo);
            float initWithInitIdeo = 0.025f * PsycheHelper.Comp(initiator).Psyche.CompatibilityWithIdeo(initiator.Ideo);
            float initWithReciIdeo = 0.025f * PsycheHelper.Comp(initiator).Psyche.CompatibilityWithIdeo(recipient.Ideo);
            float additiveFactor = reciWithInitIdeo - reciWithReciIdeo + initWithInitIdeo - initWithReciIdeo;
            float multiplicativeFactor = additiveFactor > 0f ? 1f + additiveFactor : 1f / (1f - additiveFactor);

            string text = string.Empty;
            Log.Message("String builder before = \n" + sb.ToString());
            text += "   -  test";
            Log.Message("String builder middle = \n" + sb.ToString());


            if (sb != null)
            {
                NamedArgument initName = initiator.Named("PAWN");
                NamedArgument reciName = recipient.Named("PAWN");
                NamedArgument initIdeo = initiator.Ideo.Named("IDEO");
                NamedArgument reciIdeo = recipient.Ideo.Named("IDEO");
                
                //text += PawnCompatWithIdeoText(reciName, initIdeo, reciWithInitIdeo, true);
                //text += PawnCompatWithIdeoText(reciName, reciIdeo, reciWithReciIdeo, false);
                //text += PawnCompatWithIdeoText(initName, initIdeo, initWithInitIdeo, true);
                //text += PawnCompatWithIdeoText(initName, reciIdeo, initWithReciIdeo, false);
                sb.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownPsychologyEffects".Translate() + ": " + multiplicativeFactor.ToStringPercent() + text);
                Log.Message("String builder after = \n" + sb.ToString());
            }
            __result *= multiplicativeFactor;

        }
        //public static string PawnCompatWithIdeoText(NamedArgument pawnName, NamedArgument ideoName, float compat, bool isInitIdeo)
        //{
        //    string compatText = compat > 0 ? "AbilityIdeoConvertBreakdownPawnCompatWithIdeo" : "AbilityIdeoConvertBreakdownPawnIncompatWithIdeo";
        //    float compatSigned = isInitIdeo ? compat : -compat;
        //    string compatPercent = compatSigned > 0 ? "+" + compatSigned.ToStringPercent() : compatSigned.ToStringPercent();
        //    return "\n   -  " + compatText.Translate(pawnName, ideoName) + ": " + compatPercent;
        //}
    }
}

