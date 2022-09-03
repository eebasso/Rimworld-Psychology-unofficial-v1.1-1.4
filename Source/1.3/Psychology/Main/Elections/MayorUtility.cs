//using System.Collections.Generic;
//using Verse;

//namespace Psychology;

//public class MayorUtility
//{
//    public Dictionary<int, Pair<Pawn, Hediff>> Mayors => Current.Game.GetComponent<PsychologyGameComponent>().mayors;

//    public void BuildMayorDictionary()
//    {
//        Mayors = new Dictionary<int, Pair<Pawn, Hediff>>();
//        foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
//        {
//            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
//            {
//                if (hediff.def == HediffDefOfPsychology.Mayor)
//                {
//                    int mapTile = (hediff as Hediff_Mayor).worldTileElectedOn;
//                    if (!Mayors.ContainsKey(mapTile))
//                    {
//                        Mayors.Add(mapTile, new Pair<Pawn, Hediff>(pawn, hediff));
//                    }
//                    else
//                    {
//                        // There can only be one mayor per colony
//                        pawn.health.RemoveHediff(hediff);
//                    }
//                }
//            }
//        }
//        if (!PsychologySettings.enableElections)
//        {
//            DeleteAllMayorHediffs();
//        }
//    }

//    public void RemoveMayorOfThisColony(int mapTile)
//    {
//        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
//        {
//            if (mapTile == kvp.Key)
//            {
//                RemoveMayor(kvp);
//            }
//        }
//    }
//    public void RemoveAllMayorshipsFromPawn(Pawn pawn)
//    {
//        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
//        {
//            if (pawn == kvp.Value.First)
//            {
//                RemoveMayor(kvp);
//            }
//        }
//    }
//    public void DeleteAllMayorHediffs()
//    {
//        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
//        {
//            RemoveMayor(kvp);
//        }
//    }
//    public void RemoveMayor(KeyValuePair<int, Pair<Pawn, Hediff>> kvp)
//    {
//        kvp.Value.First.health.RemoveHediff(kvp.Value.Second);
//        Mayors.Remove(kvp.Key);
//    }

//    // Need to add case when pawn is banished
//}

