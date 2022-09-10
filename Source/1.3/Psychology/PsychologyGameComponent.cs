using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;
using UnityEngine;
using System.Diagnostics;

namespace Psychology;

public class PsychologyGameComponent : GameComponent
{
    public bool firstTimeWithUpdate = true;
    public bool taraiSiblingsGenerated = false;
    public Dictionary<int, float> CachedCertaintyChangePerDayDict = new Dictionary<int, float>();
    public Dictionary<int, Pair<Pawn, Hediff>> Mayors;
    public static int constituentTick = 156;
    public static int electionTick = 823;

    public PsychologyGameComponent(Game game)
    {
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref this.firstTimeWithUpdate, "Psychology_FirstTimeWithUpdate", true);
        Scribe_Values.Look(ref taraiSiblingsGenerated, "Psychology_TaraiSiblingsGenerated", false);
    }

    public override void LoadedGame()
    {
        Log.Message("Psychology: loading game");
        InitializeCachedIdeoCertaintyChange();
        BuildMayorDictionary();
        ImplementSexualOrientation();
        //FirstTimeLoadingNewPsychology();
    }

    public override void StartedNewGame()
    {
        Log.Message("Psychology: started new game");
        this.firstTimeWithUpdate = false;
        InitializeCachedIdeoCertaintyChange();
    }

    public override void GameComponentTick()
    {
        if (PsychologySettings.enableElections)
        {
            ConstituentTick();
            ElectionTick();
        }
    }

    //public virtual void FirstTimeLoadingNewPsychology()
    //{
    //    if (!this.firstTimeWithUpdate)
    //    {
    //        return;
    //    }
    //    Find.WindowStack.Add(new Dialog_UpdateIntro());
    //    this.firstTimeWithUpdate = false;
    //}

    public virtual void InitializeCachedIdeoCertaintyChange()
    {
        foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
        {
            AddPawnToCachedIdeoCertaityChange(pawn);
        }
    }

    public virtual void AddPawnToCachedIdeoCertaityChange(Pawn pawn)
    {
        if (PsycheHelper.PsychologyEnabled(pawn))
        {
            float ideoCertaintyChange = PsycheHelper.Comp(pawn).Psyche.CalculateCertaintyChangePerDay();
            CachedCertaintyChangePerDayDict.AddDistinct(pawn.thingIDNumber, ideoCertaintyChange);
        }
    }

    public virtual void BuildMayorDictionary()
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
                        // There can only be one mayor per map tile
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }
        if (!PsychologySettings.enableElections)
        {
            DeleteAllMayorHediffs();
        }
    }

    public virtual void RemoveMayorOfThisColony(int mapTile)
    {
        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
        {
            if (mapTile == kvp.Key)
            {
                RemoveMayor(kvp);
            }
        }
    }

    public virtual void RemoveAllMayorshipsFromPawn(Pawn pawn)
    {
        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
        {
            if (pawn == kvp.Value.First)
            {
                RemoveMayor(kvp);
            }
        }
    }

    public virtual void DeleteAllMayorHediffs()
    {
        foreach (KeyValuePair<int, Pair<Pawn, Hediff>> kvp in Mayors)
        {
            RemoveMayor(kvp);
        }
    }

    public virtual void RemoveMayor(KeyValuePair<int, Pair<Pawn, Hediff>> kvp)
    {
        kvp.Value.First.health.RemoveHediff(kvp.Value.Second);
        Mayors.Remove(kvp.Key);
    }

    public virtual void InitializeIdeoCertaintyChange()
    {
        foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
        {
            if (!CachedCertaintyChangePerDayDict.ContainsKey(pawn.thingIDNumber))
            {
                CachedCertaintyChangePerDayDict.Add(pawn.thingIDNumber, PsycheHelper.Comp(pawn).Psyche.CalculateCertaintyChangePerDay());
            }
        }
    }

    public virtual void ImplementSexualOrientation()
    {
        if (PsychologySettings.enableKinsey)
        {
            foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
            {
                PsycheHelper.CorrectTraitsForPawnKinseyEnabled(pawn);
            }
        }
        else
        {
            foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
            {
                PsycheHelper.CorrectTraitsForPawnKinseyDisabled(pawn);
            }
        }
    }

    public virtual void RandomizeRatingsForAllPawns()
    {
        foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
        {
            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                continue;
            }
            PsycheHelper.Comp(pawn).Psyche.RandomizeRatings();
        }
    }

    public virtual void ConstituentTick()
    {
        if (constituentTick < 2 * GenDate.TicksPerHour)
        {
            constituentTick++;
            return;
        }
        constituentTick = 0;

        //List<Settlement> playerSettlements = Find.WorldObjects.Settlements.FindAll(b => b.Faction.IsPlayer);
        List<Settlement> playerSettlements = Find.WorldObjects.SettlementBases.FindAll(b => b.Faction.IsPlayer);
        foreach (Settlement settlement in playerSettlements)
        {
            if (!Mayors.ContainsKey(settlement.Map.Tile))
            {
                continue;
            }
            IEnumerable<Pawn> constituents = from p in settlement.Map.mapPawns.FreeColonistsSpawned
                                             where !p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor)
                                             && p.GetLord() == null && p.GetTimeAssignment() != TimeAssignmentDefOf.Work
                                             && p.Awake() && !p.Downed && !p.Drafted && p.health.summaryHealth.SummaryHealthPercent >= 1f
                                             && PsycheHelper.PsychologyEnabled(p)
                                             select p;
            if (constituents.Count() == 0)
            {
                continue;
            }
            Pawn potentialConstituent = constituents.RandomElementByWeight(p => 0.0001f + Mathf.Pow(Mathf.Abs(0.7f - p.needs.mood.CurLevel), 2));
            if (potentialConstituent == null)
            {
                continue;
            }
            Pawn mayor = Mayors[settlement.Map.Tile].First;
            TimeAssignmentDef timeAssDef = mayor.GetTimeAssignment();
            bool mayorAvailable = timeAssDef != TimeAssignmentDefOf.Sleep && mayor.GetLord() == null && mayor.Tile == settlement.Map.Tile
                                  && mayor.Awake() && !mayor.Drafted && !mayor.Downed && mayor.health.summaryHealth.SummaryHealthPercent >= 1f
                                  && (mayor.CurJob == null || mayor.CurJob.def != JobDefOf.TendPatient || mayor.CurJob.RecipeDef.workerClass.IsAssignableFrom(typeof(Recipe_Surgery)));
            if (!mayorAvailable)
            {
                continue;
            }
            IntVec3 gather = default(IntVec3);
            string found = null;
            if (mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable != null)
            {
                gather = mayor.Map.GetComponent<OfficeTableMapComponent>().officeTable.parent.Position;
                found = "office";
            }
            else if (mayor.ownership != null && mayor.ownership.OwnedBed != null)
            {
                gather = mayor.ownership.OwnedBed.Position;
                found = "bed";
            }
            //if (PsycheHelper.PsychologyEnabled(potentialConstituent) && Rand.Chance((1f - PsycheHelper.Comp(potentialConstituent).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent)) / 5f) && (found != null || RCellFinder.TryFindGatheringSpot(mayor, GatheringDefOf.Party, true, out gather)) // 1.3 - added ignoreRequiredColonistCount = true
            //    && (!mayor.Drafted && !mayor.Downed && mayor.health.summaryHealth.SummaryHealthPercent >= 1f && mayor.GetTimeAssignment() != TimeAssignmentDefOf.Work && (mayor.CurJob == null || mayor.CurJob.def != JobDefOf.TendPatient || mayor.CurJob.RecipeDef.workerClass.IsAssignableFrom(typeof(Recipe_Surgery)))))
            float independent = PsycheHelper.Comp(potentialConstituent).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Independent);
            if (Rand.Chance((1f - independent) * 0.2f) && (found != null || RCellFinder.TryFindGatheringSpot(mayor, GatheringDefOf.Party, true, out gather)))
            {
                List<Pawn> pawns = new List<Pawn>();
                pawns.Add(mayor);
                pawns.Add(potentialConstituent);
                Lord meeting = LordMaker.MakeNewLord(mayor.Faction, new LordJob_VisitMayor(gather, potentialConstituent, mayor, potentialConstituent.needs.mood.CurLevel < 1.25f * potentialConstituent.mindState.mentalBreaker.BreakThresholdMinor), mayor.Map, pawns);
                mayor.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                potentialConstituent.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                if (found == "bed")
                {
                    mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoOffice);
                }
                else if (found == null)
                {
                    mayor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.MayorNoBedroom);
                }
            }

        }
    }

    public virtual void ElectionTick()
    {
        if (electionTick < 7 * GenDate.TicksPerHour)
        {
            electionTick++;
            return;
        }
        electionTick = 0;

        List<Settlement> eligibleSettlements = new List<Settlement>();
        foreach (Settlement settlement in Find.WorldObjects.Settlements)
        {
            //If the base isn't owned or named by the player, no election can be held.
            if (!settlement.Faction.IsPlayer || !settlement.namedByPlayer)
            {
                continue;
            }
            ////If the base is not at least a year old, no election will be held.
            //if ((Find.TickManager.TicksGame - settlement.creationGameTicks) < GenDate.TicksPerYear)
            //{
            //    continue;
            //}
            //A base must have at least 7 people in it to hold an election.
            if (settlement.Map.mapPawns.FreeColonistsSpawnedCount < 7)
            {
                continue;
            }
            //If an election is already being held, don't start a new one.
            if (settlement.Map.gameConditionManager.ConditionIsActive(GameConditionDefOfPsychology.Election) || settlement.Map.lordManager.lords.Find(l => l.LordJob is LordJob_Joinable_Election) != null)
            {
                continue;
            }

            int settlementTile = settlement.Tile;
            if (Mayors.ContainsKey(settlementTile))
            {
                // Don't start another election if the mayor was elected this year
                int yearDiff = GenLocalDate.Year(settlementTile) - (Mayors[settlementTile].Second as Hediff_Mayor).yearElected;
                if (yearDiff <= 0)
                {
                    continue;
                }
                //Elections are held starting in Septober (because I guess some maps don't have fall?)
                if (yearDiff == 1 && GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(settlementTile).x) < Quadrum.Septober)
                {
                    continue;
                }
            }
            else if (GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(settlementTile).x) < Quadrum.Septober)
            {
                continue;
            }
            // Start elections during the day
            if (GenLocalDate.HourOfDay(settlement.Map) < 7 || GenLocalDate.HourOfDay(settlement.Map) > 20)
            {
                continue;
            }
            eligibleSettlements.Add(settlement);
        }
        if (eligibleSettlements.Count() > 0)
        {
            // Only pick one election at random to happen per 7 hour tick so they don't all proc at once.
            Settlement settlement = eligibleSettlements.RandomElement();
            IncidentParms parms = new IncidentParms();
            parms.target = settlement.Map;
            parms.faction = settlement.Faction;
            FiringIncident fi = new FiringIncident(IncidentDefOfPsychology.Election, null, parms);
            Find.Storyteller.TryFire(fi);
        }
    }

}
