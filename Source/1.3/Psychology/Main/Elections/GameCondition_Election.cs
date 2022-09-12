using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace Psychology
{
    public class GameCondition_Election : GameCondition
    {
        public List<Candidate> candidates = new List<Candidate>();

        public override void Init()
        {
            base.Init();
            // Make sure the election occurs during the day if possible.
            int plannedStart = GenDate.HourOfDay(this.Duration + Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(this.SingleMap.Tile).x);
            // Make election start at 7
            this.Duration += GenMath.PositiveMod(7 - plannedStart, 24) * GenDate.TicksPerHour;

            //if (plannedStart < 7)
            //{
            //    this.Duration += (7 - plannedStart) * GenDate.TicksPerHour;
            //}
            //else if (plannedStart > 18)
            //{
            //    this.Duration -= (plannedStart - 18) * GenDate.TicksPerHour;
            //}
            IEnumerable<Pawn> colonists = from p in this.SingleMap.mapPawns.FreeColonistsSpawned
                                          where PsycheHelper.PsychologyEnabled(p) && p.HomeFaction == Faction.OfPlayer // 1.3
                                          select p;

            int numColonists = colonists.Count();
            if (numColonists < 5)
            {
                return;
            }
            //float maxCandidatesThisColonySupports = Mathf.Clamp(0.3f * numColonists, 2f, 4f);
            IEnumerable<Pawn> eligibleColonists = from p in colonists
                                                  where p.ageTracker.AgeBiologicalYearsFloat >= PsychologySettings.mayorAge // 1.3
                                                  select p;
            float rowdiness = 1f;
            Dictionary<Pawn, float> likelihoodToRun = new Dictionary<Pawn, float>();
            foreach (Pawn p in eligibleColonists)
            {
                // 95% chance incumbent runs for re-election
                if (p.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor) && Rand.Value < 0.95f)
                {
                    List<PersonalityNodeDef> issues = GenerateIssues(p);
                    this.candidates.Add(new Candidate(p, issues));
                    eligibleColonists = eligibleColonists.Except(p);
                }
                else
                {
                    float confident = PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Confident);
                    float outspoken = PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Outspoken);
                    float ambitious = PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Ambitious);
                    float competitive = PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Competitive);
                    float extroverted = PsycheHelper.Comp(p).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted);
                    float score = confident + outspoken + ambitious + competitive + extroverted;
                    rowdiness += 0.2f * score;
                    likelihoodToRun.Add(p, score * score);
                }
            }
            float cutoff2 = 12f;
            float cutoff3 = 24f;
            float x = (0.5f * numColonists + rowdiness - cutoff2) / (cutoff3 - cutoff2);
            float p2 = x < 0 ? 1f : x < 2 ? 1f / (1f + x) : 1f / 3f;
            float p3 = Mathf.Clamp01(x) * p2;
            float rand = Rand.Value;
            int numCandidates = rand < p2 ? 2 : rand < p2 + p3 ? 3 : 4;
            if (eligibleColonists.Count() < numCandidates - this.candidates.Count)
            {
                return;
            }
            int tries = 0;
            while (this.candidates.Count < numCandidates && tries < 500)
            {
                Pawn candidatePawn = eligibleColonists.RandomElementByWeight(p => likelihoodToRun[p]);
                List<PersonalityNodeDef> issues = GenerateIssues(candidatePawn);
                this.candidates.Add(new Candidate(candidatePawn, issues));
                eligibleColonists = eligibleColonists.Except(candidatePawn);
                tries++;
            }
            if (candidates.Count == 0)
            {
                this.End();
                Log.Error("[Psychology] Tried to start election but could not find anyone to run.");
                return;
            }
            foreach (Candidate candidate in candidates)
            {
                StringBuilder issuesString = new StringBuilder();
                for (int i = 0; i < candidate.nodes.Count; i++)
                {
                    issuesString.AppendFormat("{0}) {1}{2}", i + 1, PsycheHelper.Comp(candidate.pawn).Psyche.GetPersonalityNodeOfDef(candidate.nodes[i]).PlatformIssue, (i != candidate.nodes.Count - 1 ? "\n" : ""));
                }
                Find.LetterStack.ReceiveLetter("LetterLabelElectionCandidate".Translate(candidate.pawn), "LetterElectionCandidate".Translate(candidate.pawn, Find.WorldObjects.ObjectsAt(candidate.pawn.Tile).OfType<Settlement>().First().Label, issuesString.ToString()), LetterDefOf.NeutralEvent, candidate.pawn, null);
            }
            candidates.RemoveDuplicates();
        }

        public List<PersonalityNodeDef> GenerateIssues(Pawn candidate)
        {
            List<PersonalityNodeDef> issues = new List<PersonalityNodeDef>();
            IEnumerable<PersonalityNodeDef> nodeDefs = from node in PsycheHelper.Comp(candidate).Psyche.PersonalityNodes
                                                       where node.HasPlatformIssue
                                                       select node.def;
            int tries2 = 0;
            while (issues.Count < 5 && tries2 < 500)
            {
                PersonalityNodeDef issue = nodeDefs.RandomElementByWeight(n => Mathf.Pow(PsycheHelper.Comp(candidate).Psyche.GetPersonalityRating(n) - 0.5f, 4) * Mathf.Pow(2, n.controversiality));
                issues.Add(issue);
                nodeDefs = nodeDefs.Except(issue);
                tries2++;
            }
            if (issues.Count < 5)
            {
                Log.Error("[Psychology] Could not find five unique issues for " + candidate.LabelShort + "'s platform.");
            }
            return issues;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.candidates, "candidates", LookMode.Deep);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            foreach (Candidate candidate in candidates)
            {
                if (candidate.pawn.Dead)
                {
                    candidates.Remove(candidate);
                }
            }
            if (candidates.Count == 0)
            {
                End();
            }
        }

        //[LogPerformance]
        public override void End()
        {
            base.End();
            if (candidates.Count == 0)
            {
                Log.Message("Psychology :: Tried to start election... but there were no candidates.");
                return;
            }
            IntVec3 intVec;
            Pawn organizer = candidates.RandomElement().pawn;
            string baseName = Find.WorldObjects.ObjectsAt(organizer.Map.Tile).OfType<Settlement>().First().Label;
            if (!TryFindElectionSpot(organizer, out intVec))
            {
                Messages.Message("MessageElectionCancelled".Translate(baseName), MessageTypeDefOf.NegativeEvent);
                return;
            }
            LordMaker.MakeNewLord(organizer.Faction, new LordJob_Joinable_Election(intVec, candidates, baseName, organizer.Map), organizer.Map, null);
            Find.LetterStack.ReceiveLetter("LetterLabelElectionHeld".Translate(baseName), "LetterElectionHeld".Translate(baseName), LetterDefOf.NeutralEvent, new TargetInfo(intVec, organizer.Map, false), null);
        }

        private static bool TryFindElectionSpot(Pawn organizer, out IntVec3 result)
        {
            bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer, null);
            Map map = organizer.Map;
            Predicate<IntVec3> baseValidator = delegate (IntVec3 cell)
            {
                if (!cell.Standable(map))
                {
                    return false;
                }
                if (cell.GetDangerFor(organizer, map) != Danger.None)
                {
                    return false;
                }
                if (!enjoyableOutside && !cell.Roofed(map))
                {
                    return false;
                }
                if (cell.IsForbidden(organizer))
                {
                    return false;
                }
                if (!organizer.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None, 1, -1, null, false))
                {
                    return false;
                }
                Room room = cell.GetRoom(map);
                return !room.IsPrisonCell;
            };
            if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.PartySpot)
                 where baseValidator(x.Position)
                 select x.Position).TryRandomElement(out result))
            {
                return true;
            }
            Predicate<IntVec3> noPartySpotValidator = delegate (IntVec3 cell)
            {
                Room room = cell.GetRoom(map);
                return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
            };
            foreach (CompGatherSpot current in map.gatherSpotLister.activeSpots.InRandomOrder(null))
            {
                for (int i = 0; i < 10; i++)
                {
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, current.parent.Map, 4, null);
                    if (baseValidator(intVec) && noPartySpotValidator(intVec))
                    {
                        result = intVec;
                        bool result2 = true;
                        return result2;
                    }
                }
            }
            if (CellFinder.TryFindRandomCellNear(organizer.Position, organizer.Map, 25, (IntVec3 cell) => baseValidator(cell) && noPartySpotValidator(cell), out result, -1))
            {
                return true;
            }
            result = IntVec3.Invalid;
            return false;
        }

        
    }
}
