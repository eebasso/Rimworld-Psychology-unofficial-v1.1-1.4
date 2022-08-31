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

namespace Psychology;

public class PsychologyGameComponent : GameComponent
{
    public static int constituentTick = 156;
    public static int electionTick = 823;
    public bool firstTimeWithUpdate = true;

    public PsychologyGameComponent(Game game)
    {
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref this.firstTimeWithUpdate, "Psychology_FirstTimeWithUpdate", true);
    }

    public override void LoadedGame()
    {
        Log.Message("Psychology: loading game");
        MayorUtility.BuildMayorDictionary();
        ImplementSexualOrientation();
        FirstTimeLoadingNewPsychology();
    }

    public override void StartedNewGame()
    {
        Log.Message("Psychology: started new game");
        this.firstTimeWithUpdate = false;
    }

    public override void GameComponentTick()
    {
        if (PsychologySettings.enableElections)
        {
            ConstituentTick();
            ElectionTick();
        }
    }

    public void ImplementSexualOrientation()
    {
        if (!PsychologySettings.kinseySettingChanged)
        {
            return;
        }
        if (PsychologySettings.enableKinsey)
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                CorrectTraitsForPawnKinseyEnabled(pawn);
            }
        }
        else
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                CorrectTraitsForPawnKinseyDisabled(pawn);
            }
        }
        PsychologySettings.kinseySettingChanged = false;
    }

    public static void CorrectTraitsForPawnKinseyEnabled(Pawn pawn)
    {
        if (pawn.story == null || !PsycheHelper.PsychologyEnabled(pawn))
        {
            return;
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            RemoveTrait(pawn, TraitDefOf.Asexual);
            PsycheHelper.Comp(pawn).Sexuality.sexDrive = 0.10f * Rand.ValueSeeded(11 * pawn.HashOffset() + 8);
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
        {
            RemoveTrait(pawn, TraitDefOf.Bisexual);
            PsycheHelper.Comp(pawn).Sexuality.GenerateKinsey(0f, 0f, 1f, 2f, 1f, 0f, 0f);
        }
        if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
        {
            RemoveTrait(pawn, TraitDefOf.Gay);
            PsycheHelper.Comp(pawn).Sexuality.GenerateKinsey(0f, 0f, 0f, 0f, 0f, 1f, 2f);
        }
    }

    public static void CorrectTraitsForPawnKinseyDisabled(Pawn pawn)
    {
        if (pawn.story == null || !PsycheHelper.PsychologyEnabled(pawn))
        {
            return;
        }
        int kinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
        if (PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f)
        {
            TryGainTrait(pawn, TraitDefOf.Asexual);
        }
        if (kinseyRating < 2)
        {
            // If pawn is mostly heterosexual
            TryRemoveTrait(pawn, TraitDefOf.Bisexual);
            TryRemoveTrait(pawn, TraitDefOf.Gay);
        }
        else if (kinseyRating < 5)
        {
            // If pawn is mostly bisexual
            TryGainTrait(pawn, TraitDefOf.Bisexual);
            TryRemoveTrait(pawn, TraitDefOf.Gay);
        }
        else
        {
            // If pawn is mostly homosexual
            TryGainTrait(pawn, TraitDefOf.Gay);
            TryRemoveTrait(pawn, TraitDefOf.Bisexual);
        }
    }

    public static void TryGainTrait(Pawn pawn, TraitDef traitDef)
    {
        if (!pawn.story.traits.HasTrait(traitDef))
        {
            pawn.story.traits.GainTrait(new Trait(traitDef));
        }
    }

    public static void TryRemoveTrait(Pawn pawn, TraitDef traitDef)
    {
        if (pawn.story.traits.HasTrait(traitDef))
        {
            RemoveTrait(pawn, traitDef);
        }
    }

    public static void RemoveTrait(Pawn pawn, TraitDef traitDef)
    {
        pawn.story.traits.allTraits.RemoveAll(t => t.def == traitDef);
    }

    public void FirstTimeLoadingNewPsychology()
    {
        if (!this.firstTimeWithUpdate)
        {
            return;
        }
        Find.WindowStack.Add(new Dialog_PsychologyUpdatePrompt());
        //RandomizeUpbringingAndRatingsForAllPawns();
        this.firstTimeWithUpdate = false;
    }

    public static void RandomizeUpbringingAndRatingsForAllPawns()
    {
        foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead)
        {
            pawn.GetComp<CompPsychology>().Psyche.RandomizeUpbringingAndRatings();
        }
    }

    public static void ConstituentTick()
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
            if (!MayorUtility.Mayors.ContainsKey(settlement.Map.Tile))
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
            Pawn mayor = MayorUtility.Mayors[settlement.Map.Tile].First;
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

    public static void ElectionTick()
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
            if (MayorUtility.Mayors.ContainsKey(settlementTile))
            {
                // Don't start another election if the mayor was elected this year
                int yearDiff = GenLocalDate.Year(settlementTile) - (MayorUtility.Mayors[settlementTile].Second as Hediff_Mayor).yearElected;
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

