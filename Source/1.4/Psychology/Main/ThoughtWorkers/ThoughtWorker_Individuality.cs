using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace Psychology;

public class ThoughtWorker_Individuality : ThoughtWorker
{
    Dictionary<Pawn, int[]> lastTick = new Dictionary<Pawn, int[]>();

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!PsychologySettings.enableIndividuality)
            return ThoughtState.Inactive;
        if (!p.Spawned)
            return ThoughtState.Inactive;
        if (p.Map == null)
            return ThoughtState.Inactive;
        if (!p.IsColonist)
            return ThoughtState.Inactive;
        if (p.apparel.PsychologicallyNude)
            return ThoughtState.Inactive;

        bool flag = false;
        if (!lastTick.ContainsKey(p))
        {
            lastTick.Add(p, new[] { p.GetHashCode() % 250, -1 });
            flag = true;
        }
        lastTick[p][0]--;
        if (lastTick[p][0] < 0)
        {
            lastTick[p][0] = 250;
            flag = true;
        }
        
        if (flag)
        {
            List<Thought> tmpThoughts = new List<Thought>();
            p.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);

            // Added fix to disable for low expectations
            
            IEnumerable<int> stagesList = from t in tmpThoughts
                                          where t.def.defName == "Expectations"
                                          select t.CurStageIndex;
            ////Log.Message(p.LabelShort + ": stagesList.Count() = " + stagesList.Count());
            if (stagesList.Count() > 0)
            {
                ////Log.Message(p.LabelShort + ": stagesList.First() = " + stagesList.First());
                if (stagesList.First() < 4)
                {
                    return ThoughtState.Inactive;
                }
            }
            //if (tmpThoughts.Find(t => t.def.defName == "LowExpectations") != null)
            //{
            //    return ThoughtState.Inactive;
            //}

            // Might be faster to find the opposite
            Func<Apparel, bool> identical = delegate (Apparel x)
            {
                foreach (Apparel a in p.apparel.WornApparel)
                {
                    if (a.def == x.def && a.Stuff == x.Stuff && a.DrawColor == x.DrawColor)
                        return true;
                }
                return false;
            };
            IEnumerable<Pawn> colonists = from c in p.Map.mapPawns.FreeColonistsSpawned
                                          where c != p
                                          select c;
            IEnumerable<Pawn> sameClothes = from c in colonists
                                            where (from x in c.apparel.WornApparel
                                                   where identical(x)
                                                   select x).Count() == p.apparel.WornApparelCount && p.apparel.WornApparelCount == c.apparel.WornApparelCount
                                            select c;
            int sameClothesCount = sameClothes.Count();
            int colonistsCount = colonists.Count();
            if (sameClothesCount == colonistsCount && colonistsCount > 5)
            {
                lastTick[p][1] = 3;
            }
            else if (sameClothesCount >= (colonistsCount / 2) && colonistsCount > 5)
            {
                lastTick[p][1] = 2;
            }
            else if (sameClothesCount > 1)
            {
                lastTick[p][1] = 1;
            }
            else if (sameClothesCount > 0)
            {
                lastTick[p][1] = 0;
            }
            else
            {
                lastTick[p][1] = -1;
            }
        }
        if (lastTick[p][1] >= 0)
        {
            return ThoughtState.ActiveAtStage(lastTick[p][1]);
        }
        return ThoughtState.Inactive;
    }


}
