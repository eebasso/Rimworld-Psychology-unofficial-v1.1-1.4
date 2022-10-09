using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Grammar;
using System.Reflection;
using HarmonyLib;

namespace Psychology;

public class Hediff_Conversation : HediffWithComps
{
    public Pawn otherPawn;
    public PersonalityNodeDef topic;
    public string convoTopic;
    public bool waveGoodbye;
    public bool startedFight = false;
    public PlayLogEntry_InteractionConversation convoLog;

    public override void PostMake()
    {
        base.PostMake();
        if (!PsycheHelper.PsychologyEnabled(this.pawn))
        {
            this.pawn.health.RemoveHediff(this);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.otherPawn, "otherPawn");
        Scribe_Defs.Look(ref this.topic, "topic");
        Scribe_Values.Look(ref this.waveGoodbye, "waveGoodbye");
        Scribe_Values.Look(ref this.convoTopic, "convoTopic", "something");
    }

    public override void Tick()
    {
        base.Tick();
        if (this.otherPawn == null)
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (!this.otherPawn.Spawned || !this.pawn.Spawned || !InteractionUtility.CanReceiveInteraction(this.pawn) || !InteractionUtility.CanReceiveInteraction(this.otherPawn))
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (this.otherPawn.Dead || this.otherPawn.Downed || this.otherPawn.InAggroMentalState)
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (this.pawn.IsHashIntervalTick(200))
        {
            if (this.pawn.Map != null && this.otherPawn.Map != null && ((this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true)))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            if ((this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            /* When a conversation first starts, the mean time for it to last is 3 hours.
             * When it reaches half an hour, the mean time for it to continue is 2 hours.
             * When it reaches an hour, the mean time for it to continue is 1 hour.
             * When it surpasses 2 hours, it will on average last for half an hour more.
             * Conversations will thus usually not surpass 2 hours, and very rarely surpass 2 and a half hours, but are very likely to last up to an hour.
             */
            float mtb = 3f;
            if (this.ageTicks > 2f * GenDate.TicksPerHour)
            {
                mtb = 0.5f;
            }
            else if (this.ageTicks > GenDate.TicksPerHour)
            {
                mtb = 1f;
            }
            else if (this.ageTicks > 0.5f * GenDate.TicksPerHour)
            {
                mtb = 2f;
            }
            if (this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
            {
                mtb *= 2f;
            }
            if (this.otherPawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
            {
                mtb *= 2f;
            }
            if (Rand.MTBEventOccurs(mtb, GenDate.TicksPerHour, 200))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            else if (Rand.Value < 0.2f && this.pawn.Map != null)
            {
                MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOfPsychology.EndConversation.interactionMote, InteractionDefOfPsychology.EndConversation.GetSymbol()); // TEST in 1.3
            }
        }
    }

    public override void PostRemoved()
    {

        base.PostRemoved();
        if (this.pawn == null && this.otherPawn == null)
        {
            Log.Message("Hediff_Conversation.PostRemoved(), pawns were null");
            return;
        }
        if (this.pawn.Dead || this.otherPawn.Dead || !PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(otherPawn))
        {
            Log.Message("Hediff_Conversation.PostRemoved(), psychology not enabled");
            return;
        }
        //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 0");
        Hediff_Conversation otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h is Hediff_Conversation && ((Hediff_Conversation)h).otherPawn == this.pawn) as Hediff_Conversation;
        if (otherConvo != null)
        {
            this.otherPawn.health.RemoveHediff(otherConvo);
            this.startedFight = otherConvo.startedFight;
        }
        string talkDesc;
        if (this.ageTicks < 500)
        {
            int numShortTalks = int.Parse("NumberOfShortTalks".Translate());
            talkDesc = "ShortTalk" + Rand.RangeInclusive(1, numShortTalks);
        }
        else if (this.ageTicks < GenDate.TicksPerHour / 2)
        {
            int numNormalTalks = int.Parse("NumberOfNormalTalks".Translate());
            talkDesc = "NormalTalk" + Rand.RangeInclusive(1, numNormalTalks);
        }
        else if (this.ageTicks < GenDate.TicksPerHour * 2.5)
        {
            int numLongTalks = int.Parse("NumberOfLongTalks".Translate());
            talkDesc = "LongTalk" + Rand.RangeInclusive(1, numLongTalks);
        }
        else
        {
            int numEpicTalks = int.Parse("NumberOfEpicTalks".Translate());
            talkDesc = "EpicTalk" + Rand.RangeInclusive(1, numEpicTalks);
        }
        //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 1");
        float opinionMod;
        ThoughtDef def = CreateSocialThought(out opinionMod);
        bool mattered = TryGainThought(def, Mathf.RoundToInt(opinionMod));
        InteractionDef endConversation = new InteractionDef();
        endConversation.socialFightBaseChance = 0.2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) * PopulationModifier * Mathf.InverseLerp(0f, -80f, opinionMod);
        endConversation.defName = "EndConversation";
        endConversation.label = def.label;
        List<RulePackDef> socialFightPacks = new List<RulePackDef>();
        if (otherConvo != null && (startedFight || (mattered && this.pawn.interactions.CheckSocialFightStart(endConversation, otherPawn))))
        {
            if (startedFight)
            {
                socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoRecipientStarted);
            }
            else
            {
                socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoInitiatorStarted);
            }
            this.startedFight = true;
            if (!this.waveGoodbye && otherConvo.convoLog != null && !otherConvo.startedFight)
            {
                //The main conversation hediff was the other conversation, and didn't start a fight, so we have to add the extra sentence in after the fact.
                Traverse.Create(otherConvo.convoLog).Field("extraSentencePacks").GetValue<List<RulePackDef>>().AddRange(socialFightPacks);
            }
        }
        if (this.waveGoodbye && this.pawn.Map != null)
        {
            //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 2");
            RulePack goodbyeText = new RulePack();
            FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
            List<string> text = new List<string>(1);
            text.Add("r_logentry->" + talkDesc.Translate(convoTopic, pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")));
            RuleStrings.SetValue(goodbyeText, text);
            endConversation.logRulesInitiator = goodbyeText;
            FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
            Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOfPsychology.EndConversation));
            PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, pawn, this.otherPawn, socialFightPacks);
            Find.PlayLog.Add(log);
            convoLog = log;
            MoteMaker.MakeInteractionBubble(this.pawn, this.otherPawn, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.GetSymbol()); // 1.3
            //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 3");
        }
    }

    private ThoughtDef CreateSocialThought(out float opinionMod)
    {
        //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
        ThoughtDef def = new ThoughtDef();
        def.defName = this.pawn.GetHashCode() + "Conversation" + topic.defName;
        def.label = topic.defName;
        def.durationDays = PsychologySettings.conversationDuration;
        def.nullifyingTraits = new List<TraitDef>();
        def.nullifyingTraits.Add(TraitDefOf.Psychopath);
        def.thoughtClass = typeof(Thought_MemorySocialDynamic);
        ThoughtStage stage = new ThoughtStage();

        // NEW PSYCHOLOGY FORMULA
        float opin1 = Mathf.Clamp01(PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(topic));
        float opin2 = Mathf.Clamp01(PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(topic));

        float controversiality = topic.controversiality;
        // Baseline opinion modifier ranges from -1 to +1. It's expected value should be positive for low controversiality (less than 1), and negative otherwise
        float opinionModRaw = PsycheHelper.SaddleShapeFunction(opin1, opin2, 1f - 0.5f * controversiality, 4f * controversiality * controversiality);
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionModRaw = " + opinionModRaw.ToString());

        Pawn_PsycheTracker pawnPT = PsycheHelper.Comp(pawn).Psyche;
        float pawnJudgmental = -0.5f + pawnPT.GetPersonalityRating(PersonalityNodeDefOf.Judgmental);
        float pawnFriendly = -0.5f + pawnPT.GetPersonalityRating(PersonalityNodeDefOf.Friendly);
        float pawnTrusting = -0.5f + pawnPT.GetPersonalityRating(PersonalityNodeDefOf.Trusting);
        float pawnAggressive = -0.5f + pawnPT.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);

        Pawn_PsycheTracker otherPawnPT = PsycheHelper.Comp(otherPawn).Psyche;
        float otherPawnPassionate = -0.5f + otherPawnPT.GetPersonalityRating(PersonalityNodeDefOf.Passionate);
        float otherPawnFriendly = -0.5f + otherPawnPT.GetPersonalityRating(PersonalityNodeDefOf.Friendly);
        float otherPawnCool = -0.5f + otherPawnPT.GetPersonalityRating(PersonalityNodeDefOf.Cool);
        float otherPawnPolite = -0.5f + otherPawnPT.GetPersonalityRating(PersonalityNodeDefOf.Polite);

        // Initialize opinionMultiplier, which can be positive or negative. Represents % increase or decrease in opinionMod
        float opinionMultiplier = 0f;
        // All opinions are enhanced by how judgmental the pawn is
        opinionMultiplier += pawnJudgmental;
        // All conversations are affected by how passionate the other pawn is
        opinionMultiplier += 0.5f * otherPawnPassionate;

        bool opinionModRawPositive = opinionModRaw > 0f;
        // Positive opinions
        if (opinionModRawPositive)
        {
            // Positive opinions are enhanced by how Friendly the pawn is
            opinionMultiplier += 0.5f * pawnFriendly;
            // Positive opinions are enhanced by how Trusting the pawn is
            opinionMultiplier += pawnTrusting;
            // Positive opinions are enhanced by how Freindly the other pawn is
            opinionMultiplier += 0.5f * otherPawnFriendly;
            // Positive opinions are enhanced by how Cool the other pawn is
            opinionMultiplier += otherPawnCool;
            if (LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.otherPawn) && this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                //If it's a positive thought about their lover, Codependent pawns are always more affected by it.
                opinionMultiplier += 0.5f;
            }
        }
        // Negative opinions
        else
        {
            // Negative opinions are damped by how Friendly the pawn is
            opinionMultiplier += -0.5f * pawnFriendly;
            // Negative opinions are enhanced by how Aggressive the pawn is
            opinionMultiplier += pawnAggressive;
            // Negative opinions are damped by how Friendly the other pawn is
            opinionMultiplier += -0.5f * otherPawnFriendly;
            // Negative opinions are damped by how Polite the other pawn is
            opinionMultiplier += -otherPawnPolite;
        }

        // Controls how much personality affects the opinion. Should be a positive number.
        opinionMultiplier *= 1f;

        // Multiply by 1 + opinionMultiplier for positive multiplier
        // Divide by 1 - opinionMultiplier for negative multiplier. This ensures opinionMod will always remain positive.
        // For example, if opinionMultiplier = -2, i.e. a 200% decrease, opinionMod will be 1/3 of its original value. Note that a 200% increase of 1/3 is 1.
        float fm = opinionMultiplier > 0f ? 1f + opinionMultiplier : 1f / (1f - opinionMultiplier);

        float x = fm * (0.25f + controversiality * controversiality) * Mathf.Abs(opinionModRaw);
        float t = ((float)ageTicks) / ((float)GenDate.TicksPerHour);
        float r = Rand.ValueSeeded(PsycheHelper.PawnSeed(pawn) + PsycheHelper.PawnSeed(otherPawn) + topic.GetHashCode());

        x *= 1f;
        t *= 2f;
        float yr = 5f;
        float y0x = 10f;
        float y0t = 5f;
        float y1 = 10f;
        float y2 = 10f;

        // Added time minimum to baseline to opinionMod because conversations often end very quickly
        float y = yr * r + y0x * Func(x) + y0t * Func(t) + y1 * Func(x) * Func(t) + y2 * Func(x * (1f + t));

        // In low-population colonies, pawns will put aside their differences.
        opinionMod = opinionModRawPositive ? Mathf.Ceil(y) : -Mathf.Ceil(y * PopulationModifier);

        stage.label = "ConversationStage".Translate() + " " + convoTopic;
        stage.baseOpinionOffset = opinionMod;
        def.stages.Add(stage);
        return def;
    }

    public static float Func(float x)
    {
        return 1f - Mathf.Exp(-x);
    }

    private bool TryGainThought(ThoughtDef def, int opinionOffset)
    {
        ThoughtStage stage = def.stages.First();
        IEnumerable<Thought_MemorySocialDynamic> convoMemories;
        /* The more they know about someone, the less likely small thoughts are to have an impact on their opinion.
         * This helps declutter the Social card without preventing pawns from having conversations.
         * They just won't change their mind about the colonist as a result.
         */

        float totalThoughtOpinion = PsycheHelper.Comp(pawn).Psyche.TotalThoughtOpinion(this.otherPawn, out convoMemories);
        int maxConvoOpinions = 10;

        if (convoMemories.EnumerableCount() < maxConvoOpinions)
        {
            this.pawn.needs.mood.thoughts.memories.TryGainMemory(def, this.otherPawn);
            return true;
        }
        convoMemories.OrderByDescending(m => Mathf.Abs(m.OpinionOffset()));
        IEnumerable<Thought_MemorySocialDynamic> keptMemories = convoMemories.Take(maxConvoOpinions - 1);
        IEnumerable<Thought_MemorySocialDynamic> removedMemories = convoMemories.Except(keptMemories);

        Thought_MemorySocialDynamic lastMemory = removedMemories.MaxBy(m => Mathf.Abs(m.OpinionOffset()));
        if (Mathf.Abs(opinionOffset) < Mathf.Abs(lastMemory.OpinionOffset()))
        {
            return false;
        }
        this.pawn.needs.mood.thoughts.memories.TryGainMemory(def, this.otherPawn);
        foreach (Thought_MemorySocialDynamic m in removedMemories)
        {
            // Remove these memories by making them old
            m.age = m.DurationTicks + 300;
        }
        return true;
    }

    public float PopulationModifier
    {
        get
        {
            if (this.pawn.IsColonist && this.pawn.Map != null)
            {
                return Mathf.Clamp01(this.pawn.Map.mapPawns.FreeColonistsCount / 8f);
            }
            return 1f;
        }
    }


}
