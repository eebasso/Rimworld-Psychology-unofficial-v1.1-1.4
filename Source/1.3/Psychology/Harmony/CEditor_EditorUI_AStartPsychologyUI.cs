//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using RimWorld;
//using Verse;
//using Verse.Sound;
//using HarmonyLib;
//using CharacterEditor;

//namespace Psychology.Harm
//{
//    [HarmonyPatch(typeof(CEditor.EditorUI), "AStartPsychologyUI")]
//    public class CEditor_EditorUI_AStartPsychology_Patch
//    {

//        [HarmonyPrefix]
//        public static bool AStartPsychologyUI()
//        {
//            Pawn pawn = CEditor.API.Pawn;
//            Find.WindowStack.Add(new Dialog_ViewPsyche(pawn, true));
//            Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
//            return false;
//        }
//    }
//}