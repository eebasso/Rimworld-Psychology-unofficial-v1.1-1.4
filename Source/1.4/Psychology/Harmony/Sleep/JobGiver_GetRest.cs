using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using System.Reflection;
using HarmonyLib;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(JobGiver_GetRest), nameof(JobGiver_GetRest.GetPriority))]
public static class JobGiver_GetRest_PriorityPatch
{
  [HarmonyTranspiler]
  public static IEnumerable<CodeInstruction> GetPriorityTranspiler(IEnumerable<CodeInstruction> codes)
  {
    Log.Message("GetPriorityTranspiler, start");
    bool failure = true;
    MethodInfo get_CurLevel_MethodInfo = AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevel));
    List<CodeInstruction> clist = codes.ToList();

    Dictionary<OpCode, OpCode> numberedOpCodeDict = new Dictionary<OpCode, OpCode> { { OpCodes.Stloc_0, OpCodes.Ldloc_0 }, { OpCodes.Stloc_1, OpCodes.Ldloc_1 }, { OpCodes.Stloc_2, OpCodes.Ldloc_2 }, { OpCodes.Stloc_3, OpCodes.Ldloc_3 } };
    Dictionary<OpCode, OpCode> operandOpCodeDict = new Dictionary<OpCode, OpCode> { { OpCodes.Stloc, OpCodes.Ldloc }, { OpCodes.Stloc_S, OpCodes.Ldloc_S } };

    bool isNumbered = false;
    OpCode loadCurLevelOpCode = OpCodes.Ldloc;
    object loadCurLevelOperand = null;

    for (int i = 0; i < clist.Count - 1; i++)
    {      
      if (clist[i].Calls(get_CurLevel_MethodInfo))
      {
        Log.Message("GetPriorityTranspiler: found instance of loading CurLevel");
        OpCode storeOpCode = clist[i + 1].opcode;
        if (numberedOpCodeDict.TryGetValue(storeOpCode, out OpCode numberedLoadOpCode))
        {
          isNumbered = true;
          loadCurLevelOpCode = numberedLoadOpCode;
          failure = false;
          break;
        }
        else if (operandOpCodeDict.TryGetValue(storeOpCode, out OpCode operandLoadOpCode))
        {
          isNumbered = false;
          loadCurLevelOpCode = operandLoadOpCode;
          loadCurLevelOperand = clist[i + 1].operand;
          failure = false;
          break;
        }
      }
    }

    if (failure)
    {
      Log.Error("GetPriorityTranspiler: could not find loading of CurLevel or storing of local variable");
      foreach (CodeInstruction c in codes)
      {
        yield return c;
      }
      yield break;
    }

    failure = true;
    for (int i = 0; i < clist.Count; i++)
    {
      yield return clist[i];
      if (clist[i].opcode == loadCurLevelOpCode)
      {
        if (!isNumbered && !clist[i].OperandIs(loadCurLevelOperand))
        {
          continue;
        }
        yield return new CodeInstruction(OpCodes.Ldarg_1);
        yield return CodeInstruction.Call(typeof(JobGiver_GetRest_PriorityPatch), nameof(JobGiver_GetRest_PriorityPatch.InsomniacCurLevelAdjustment));
        failure = false;
      }
    }
    if (failure)
    {
      Log.Error("GetPriorityTranspiler: could not find loading of local variable");
    }
  }


  /// <summary>
  /// Increases the effective CurLevel in the method GetPriority. This effective decreases the min level of increased priority, as intended.
  /// </summary>
  /// <param name="curLevel"></param>
  /// <param name="pawn"></param>
  /// <returns></returns>
  public static float InsomniacCurLevelAdjustment(float curLevel, Pawn pawn)
  {
    if (pawn.IsUntreatedInsomniac())
    {
      curLevel *= PsycheHelper.InsomniacRandRangeValueSeeded(pawn, 1f, 3f);
    }
    return curLevel;
  }


  [HarmonyPostfix]
  public static void GetPriorityPostfix(Pawn pawn, ref float __result)
  {
    if (__result != 0f && pawn.IsUntreatedInsomniac())
    {
      __result *= PsycheHelper.InsomniacRandRangeValueSeeded(pawn, 0.75f, 0f);
    }
  }

}
