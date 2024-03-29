﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using HarmonyLib;
using UnityEngine;


namespace Psychology.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "AddPawnsToTransferables")]
public static class Dialog_FormCaravan_AddPawnsToTransferables_Patch
{

    [HarmonyPrefix]
    public static bool DoWindowContentsDisbandCaravans(Map ___map)
    {
        /* Get rid of hanging out Lords so that those pawns can be sent on caravans easily */
        //Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
        Lord[] lords = (from l in ___map.lordManager.lords
                        where (l.LordJob is LordJob_HangOut || l.LordJob is LordJob_Date)
                        select l).ToArray();
        foreach (Lord l2 in lords)
        {
            ___map.lordManager.RemoveLord(l2);
        }
        //foreach (Lord l in ___map.lordManager.lords)
        //{
        //    if (l.LordJob is LordJob_HangOut || l.LordJob is LordJob_Date)
        //    {
        //        ___map.lordManager.RemoveLord(l);
        //    }
        //}
        return true;
    }
}
