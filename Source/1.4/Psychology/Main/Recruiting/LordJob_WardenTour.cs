/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Psychology
{
    public class LordJob_WardenTour : LordJob
    {
        public LordJob_WardenTour(Pawn warden, Pawn prisoner)
        {
            this.recruiter = warden;
            this.prisoner = prisoner;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_WardenTour lordToil_WardenTour = new LordToil_WardenTour(new Pawn[] { this.recruiter, this.prisoner });
            stateGraph.AddToil(lordToil_WardenTour);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_WardenTour, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => this.recruiter.Drafted));
            transition.AddTrigger(new Trigger_TickCondition(() => !this.prisoner.recruiting.allowedTours));
            transition.AddTrigger(new Trigger_TickCondition(() => this.prisoner.health.summaryHealth.SummaryHealthPercent < 0.8f));
            transition.AddTrigger(new Trigger_TickCondition(() => this.recruiter.health.summaryHealth.SummaryHealthPercent < 0.8f));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                this.Finished();
            }));
            stateGraph.AddTransition(transition);
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(GenDate.TicksPerHour * 4, GenDate.TicksPerHour * 8));
            Transition transition2 = new Transition(lordToil_WardenTour, lordToil_End);
            transition2.AddTrigger(this.timeoutTrigger);
            transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
            {
                this.Finished();
            }));
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        public void Finished()
        {
            float recruitChance = RecruitingUtility.Amenability(this.prisoner, this.recruiter);
            List<RulePackDef> extraSentencePacks = new List<RulePackDef>();
            if (Rand.Chance(recruitChance))
            {
                InteractionWorker_RecruitAttempt.DoRecruit(this.recruiter, this.prisoner, recruitChance, true);
                extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
            }
            else
            {
                string text = "TextMote_RecruitFail".Translate(new object[]
                {
                    recruitChance.ToStringPercent()
                });
                MoteMaker.ThrowText((this.recruiter.DrawPos + this.prisoner.DrawPos) / 2f, this.recruiter.Map, text, 8f);
                extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
            }
            Find.PlayLog.Add(new PlayLogEntry_Interaction(InteractionDefOfPsychology.TourFinished, this.recruiter, this.prisoner, extraSentencePacks));
        }

        public Map map;
        public Pawn prisoner;
        public Pawn recruiter;
        private Trigger_TicksPassed timeoutTrigger;
    }
}*/