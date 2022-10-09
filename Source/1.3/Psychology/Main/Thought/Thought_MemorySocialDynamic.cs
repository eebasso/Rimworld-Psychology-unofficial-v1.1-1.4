using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology
{
    public class Thought_MemorySocialDynamic : Thought_MemorySocial
    {
        public string topic;
        public string label;
        public float baseOpinionOffset;

        public Thought_MemorySocialDynamic()
        {
        }

        public override void ExposeData()
        {
            if (this.def != null)
            {
                this.def.defName = "DynamicSocial";
            }
            base.ExposeData();
            Scribe_Values.Look(ref this.topic, "topic", "DynamicSocial");
            Scribe_Values.Look(ref this.label, "label", "conversation");
            Scribe_Values.Look(ref this.baseOpinionOffset, "realOpinionOffset", 5);
            ThoughtDef def = new ThoughtDef();
            def.defName = this.topic;
            def.label = "conversation";
            //def.durationDays = 60f; 
            def.durationDays = PsychologySettings.conversationDuration; // ToDo: check if this change does anything
            def.thoughtClass = typeof(Thought_MemorySocialDynamic);
            ThoughtStage stage = new ThoughtStage();
            stage.label = this.label;
            //stage.baseOpinionOffset = this.baseOpinionOffset; 
            stage.baseOpinionOffset = opinionOffset; // ToDo: check if this change does anything
            def.stages.Add(stage);
            this.def = def;
        }

        public override void Init()
        {
            this.topic = def.defName;
            this.label = def.stages[0].label;
            this.baseOpinionOffset = def.stages[0].baseOpinionOffset;
            if (PsycheHelper.PsychologyEnabled(this.pawn))
            {
                PsycheHelper.Comp(this.pawn).Psyche.OpinionCacheDirty[this.otherPawn.ThingID] = true;
                Pair<string, string> disagreeKey = new Pair<string, string>(this.otherPawn.ThingID, label);
                PsycheHelper.Comp(this.pawn).Psyche.DisagreementCacheDirty[disagreeKey] = true;
            }
            base.Init();
        }
    }
}
