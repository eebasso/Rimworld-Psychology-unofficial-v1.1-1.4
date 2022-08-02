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
        // Note here that the value of sb gets changed afterwards
        [HarmonyPostfix]
        public static void ConversionPowerFactor_MemesVsTraits(ref float __result, Pawn initiator, Pawn recipient, StringBuilder sb)
        {
            float initiatorCompatWithInitiatorIdeo = 0.025f * PsycheHelper.Comp(initiator).Psyche.CompatibilityWithIdeo(initiator.Ideo);
            float initiatorCompatWithRecipientIdeo = -0.025f * PsycheHelper.Comp(initiator).Psyche.CompatibilityWithIdeo(recipient.Ideo);
            float recipientCompatWithInitiatorIdeo = 0.025f * PsycheHelper.Comp(recipient).Psyche.CompatibilityWithIdeo(initiator.Ideo);
            float recipientCompatWithRecipientIdeo = -0.025f * PsycheHelper.Comp(recipient).Psyche.CompatibilityWithIdeo(recipient.Ideo);
            float additiveFactor = initiatorCompatWithInitiatorIdeo + initiatorCompatWithRecipientIdeo + recipientCompatWithInitiatorIdeo + recipientCompatWithRecipientIdeo;
            if (sb != null)
            {
                NamedArgument initName = initiator.Named("PAWN");
                NamedArgument reciName = recipient.Named("PAWN");
                string text = string.Empty;
                text += Pawn1sCompatWithPawn2sIdeo(initName, initName, initiatorCompatWithInitiatorIdeo);
                text += Pawn1sCompatWithPawn2sIdeo(initName, reciName, initiatorCompatWithRecipientIdeo);
                text += Pawn1sCompatWithPawn2sIdeo(reciName, initName, recipientCompatWithInitiatorIdeo);
                text += Pawn1sCompatWithPawn2sIdeo(reciName, reciName, recipientCompatWithRecipientIdeo);
                sb.AppendInNewLine(" - " + "AbilityIdeoConvertBreakdownPsychologyEffects".Translate() + ": " + text);
            }
            //__result *= Mathf.Exp(additiveFactor);
            __result *= additiveFactor > 0f ? 1f + additiveFactor : 1f / (1f - additiveFactor);

        }
        public static string Pawn1sCompatWithPawn2sIdeo(NamedArgument name1, NamedArgument name2, float offset)
        {
            //string offsetText = offset > 0f ? "+" + offset.ToStringPercent() : offset.ToStringPercent();
            string offsetText = (1f + offset).ToStringPercent();
            return "\n   - " + "AbilityIdeoConvertBreakdownAsCompatWithBsIdeo".Translate(name1, name2) + ": " + offsetText;
        }

    }
}