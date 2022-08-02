using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(InteractionUtility), "CanReceiveRandomInteraction")]
    public static class InteractionUtility_CanReceive_Patch
    {
        [HarmonyPostfix]
        public static void PsychologyAddonsForCanReceive(ref bool __result, Pawn p)
        {
            bool bool2 = p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor) == null;
            bool bool3 = !p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor).ownedPawns.Contains(p);
            __result = __result && !p.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation) && (bool2 || bool3);
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), "CanInitiateRandomInteraction", new[] { typeof(Pawn) })]
    public static class InteractionUtility_CanInitiate_Patch
    {
        [HarmonyPostfix]
        public static void PsychologyAddonsForCanInitiate(ref bool __result, Pawn p)
        {
            __result = __result && p.health.hediffSet.HasHediff(HediffDefOfPsychology.HoldingConversation) && (p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor) == null || !p.Map.lordManager.lords.Find(l => l.LordJob is LordJob_VisitMayor).ownedPawns.Contains(p));
        }
    }

    [HarmonyPatch(typeof(InteractionUtility), nameof(InteractionUtility.TryGetRandomVerbForSocialFight))]
    public static class InteractionUtility_SocialFightVerb_Patch
    {
        [HarmonyPostfix]
        public static void RemoveBiting(ref Verb verb, Pawn p)
        {
            //Verb v = null;
            //(from x in p.verbTracker.AllVerbs
            // where x.IsMeleeAttack && x.IsStillUsableBy(p) && x.verbProps?.meleeDamageDef?.label != "bite"
            // select x).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p), out v);
            //if (v != null)
            //{
            //    verb = v;
            //}
            if (verb == null)
            {
                return;
            }
            if (verb.verbProps.meleeDamageDef.label != "bite")
            {
                return;
            }
            int iter = 0;
            while (iter < 20)
            {
                InteractionUtility.TryGetRandomVerbForSocialFight(p, out Verb v);
                if (v != null)
                {
                    if (v.verbProps?.meleeDamageDef?.label != "bite")
                    {
                        verb = v;
                        return;
                    }
                }
                iter++;
            }
        }
    }
}

