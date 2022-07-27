using System.Collections.Generic;
using Verse;

namespace Psychology
{
    public class MayorUtility : GameComponent
    {
        //public static List<Pawn> Mayors;
        public static Dictionary<int, Pair<Pawn, Hediff>> Mayors;

        public MayorUtility(Game game)
        {
        }
        public override void LoadedGame()
        {
            Mayors = new Dictionary<int, Pair<Pawn, Hediff>>();
            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def == HediffDefOfPsychology.Mayor)
                    {
                        int mapTile = (hediff as Hediff_Mayor).worldTileElectedOn;
                        if (!Mayors.ContainsKey(mapTile))
                        {
                            Mayors.Add(mapTile, new Pair<Pawn, Hediff>(pawn, hediff));
                        }
                        else
                        {
                            // There can only be one mayor per colony
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }
        }
        public static void RemoveMayorOfThisColony(int mapTile)
        {
            foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
            {
                if (mapTile == kvp.Key)
                {
                    RemoveMayor(kvp);
                }
            }
        }
        public static void RemoveAllMayorshipsFromPawn(Pawn pawn)
        {
            foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
            {
                if (pawn == kvp.Value.First)
                {
                    RemoveMayor(kvp);
                }
            }
        }
        public static void DeleteAllMayorHediffs()
        {
            foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
            {
                RemoveMayor(kvp);
            }
        }
        public static void RemoveMayor(KeyValuePair<int, Pair<Pawn, Hediff>> kvp)
        {
            kvp.Value.First.health.RemoveHediff(kvp.Value.Second);
            Mayors.Remove(kvp.Key);
        }
    }
}

