//using UnityEngine;//using RimWorld;//using Verse;//using Verse.Sound;//using System;//using System.Runtime.Remoting.Contexts;//using HarmonyLib;//using UnityEngine.UIElements.Experimental;//using System.Reflection;//using System.Collections.Generic;//using System.Reflection.Emit;//using System.Linq;
//using RimWorld.Planet;

//namespace Psychology.Harmony;
//public class VanillaPsycastersExpanded_Patches//{//    public static MethodInfo originalInfo;
//    public static HarmonyMethod harmonyMethod;//    public static void ManualPatches(HarmonyLib.Harmony harmonyInstance)//    {
//        originalInfo = AccessTools.Method(typeof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove), nameof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove.ValidateTarget));//        harmonyMethod = new HarmonyMethod(typeof(VanillaPsycastersExpanded_Patches).GetMethod(nameof(VanillaPsycastersExpanded_Patches.ValidateTarget_Prefix)));//        harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);//        //originalInfo = AccessTools.Method(typeof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove), nameof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove.ValidateTarget));//        //harmonyMethod = new HarmonyMethod(typeof(VanillaPsycastersExpanded_Patches).GetMethod(nameof(VanillaPsycastersExpanded_Patches.ValidateTarget_Transpiler)));//        //harmonyInstance.Patch(originalInfo, transpiler: harmonyMethod);//        originalInfo = AccessTools.Method(typeof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove), nameof(VanillaPsycastsExpanded.AbilityExtension_WordOfLove.Valid));//        harmonyMethod = new HarmonyMethod(typeof(VanillaPsycastersExpanded_Patches).GetMethod(nameof(VanillaPsycastersExpanded_Patches.Valid_Postfix)));//        harmonyInstance.Patch(originalInfo, prefix: harmonyMethod);//    }//    //public static bool ValidateTarget_Transpiler()
//    //{
//    //    var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
//    //    if (targets.Any())
//    //    {
//    //        Pawn inLovePawn = targets[0].Thing as Pawn;
//    //        Pawn lovedPawn = target.Pawn;
            
//    //    }
//    //    __result = true;
//    //    return false;
//    //}

//    //public static bool ValidateTargetKinseyEnabled(Pawn inLovePawn, Pawn lovedPawn)
//    //{
//    //    if (inLovePawn == lovedPawn)
//    //    {
//    //        return false;
//    //    }
//    //    if (PsycheHelper.PsychologyEnabled(inLovePawn) && PsycheHelper.PsychologyEnabled(lovedPawn))
//    //    {
//    //        Gender inLoveGender = inLovePawn.gender;
//    //        Gender lovedGender = lovedPawn.gender;
//    //        int pawnInLoveKinsey = PsycheHelper.Comp(inLovePawn).Sexuality.kinseyRating;
//    //        bool pawnInLoveCompat = inLoveGender == lovedGender ? pawnInLoveKinsey > 0 : pawnInLoveKinsey < 6;
//    //        if (!pawnInLoveCompat)
//    //        {
//    //            Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(inLovePawn, lovedPawn), inLovePawn, MessageTypeDefOf.RejectInput, historical: false);
//    //            __result = false;
//    //            return false;
//    //        }

//    //        int lovedKinsey = PsycheHelper.Comp(lovedPawn).Sexuality.kinseyRating;
//    //        bool pawnLovedCompat = inLoveGender == lovedGender ? lovedKinsey > 0 : lovedKinsey < 6;
//    //        if (!pawnLovedCompat)
//    //        {
//    //            Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(lovedPawn, inLovePawn), lovedPawn, MessageTypeDefOf.CautionInput, historical: false);
//    //        }
//    //    }
//    //    __result = true;
//    //    return false;
//    //}




//    public static bool ValidateTarget_Prefix(ref bool __result, LocalTargetInfo target, VFECore.Abilities.Ability ability)
//    {
//        if (!PsychologySettings.enableKinsey)
//        {
//            return true;
//        }
//        var targets = ability.currentTargets.Where(x => x.Thing != null).ToList();
//        if (targets.Any())
//        {
//            Pawn inLovePawn = targets[0].Thing as Pawn;
//            Pawn lovedPawn = target.Pawn;
//            if (inLovePawn == lovedPawn)
//            {
//                __result = false;
//                return false;
//            }
//            if (PsycheHelper.PsychologyEnabled(inLovePawn) && PsycheHelper.PsychologyEnabled(lovedPawn))
//            {
//                Gender inLoveGender = inLovePawn.gender;
//                Gender lovedGender = lovedPawn.gender;
//                int pawnInLoveKinsey = PsycheHelper.Comp(inLovePawn).Sexuality.kinseyRating;
//                bool pawnInLoveCompat = inLoveGender == lovedGender ? pawnInLoveKinsey > 0 : pawnInLoveKinsey < 6;
//                if (!pawnInLoveCompat)
//                {
//                    Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(inLovePawn, lovedPawn), inLovePawn, MessageTypeDefOf.RejectInput, historical: false);
//                    __result = false;
//                    return false;
//                }

//                int lovedKinsey = PsycheHelper.Comp(lovedPawn).Sexuality.kinseyRating;
//                bool pawnLovedCompat = inLoveGender == lovedGender ? lovedKinsey > 0 : lovedKinsey < 6;
//                if (!pawnLovedCompat)
//                {
//                    Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(lovedPawn, inLovePawn), lovedPawn, MessageTypeDefOf.CautionInput, historical: false);
//                }
//            }
//            __result = true;
//            return false;
//        }
//        __result = true;
//        return false;
//    }

//    public static void Valid_Postfix(ref bool __result, GlobalTargetInfo[] targets, bool throwMessages)
//    {
//        if (!PsychologySettings.enableKinsey)
//        {
//            return;
//        }
//        foreach (var target in targets)
//        {
//            Pawn pawn = target.Thing as Pawn;
//            if (!PsycheHelper.PsychologyEnabled(pawn))
//            {
//                continue;
//            }
//            if (PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive < 0.1f)
//            {
//                if (throwMessages)
//                {
//                    Messages.Message("AbilityCantApplyOnAsexual".Translate(pawn.def.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
//                }
//                __result = false;
//                return;
//            }
//        }

        
//    }//}