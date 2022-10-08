using System;
using Verse;
using RimWorld;
using Verse.Sound;

namespace Psychology;

public class CompProperties_AbilityIndoctrinate : CompProperties_AbilityEffect
{
    [MustTranslate]
    public string successMessage;

    public float baseDailyCertaintyChangeIncrease = 0f;

    public CompProperties_AbilityIndoctrinate()
    {
        this.compClass = typeof(CompAbilityEffect_Indoctrinate);
    }
}

public class CompAbilityEffect_Indoctrinate : CompAbilityEffect
{
    public static readonly SimpleCurve SocialSkillFactor = new SimpleCurve
    {
        new CurvePoint(0f, 0.1f),
        new CurvePoint(5f, 0.7f),
        new CurvePoint(10f, 1.0f),
        new CurvePoint(20f, 1.4f)
    };

    public static readonly SimpleCurve OpinionFactor = new SimpleCurve
    {
        new CurvePoint(-100f, 0f),
        new CurvePoint(0f, 1f),
        new CurvePoint(100f, 1.3f)
    };

    public new CompProperties_AbilityIndoctrinate Props => (CompProperties_AbilityIndoctrinate)(this.props);

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        if (target == null)
        {
            return false;
        }
        Pawn pawn = target.Pawn;
        if (pawn == null)
        {
            return false;
        }
        if (!AbilityUtility.ValidateMustBeHuman(pawn, throwMessages))
        {
            return false;
        }
        if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
        {
            return false;
        }
        if (!AbilityUtility.ValidateIsAwake(pawn, throwMessages))
        {
            return false;
        }
        if (!AbilityUtility.ValidateSameIdeo(parent.pawn, pawn, throwMessages))
        {
            return false;
        }
        return true;
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        if (ModLister.CheckIdeology("Ideoligion certainty") != true)
        {
            return;
        }
        Pawn initiator = parent.pawn;
        Pawn recipient = target.Pawn;
        Pawn_PsycheTracker pt = PsycheHelper.Comp(recipient).Psyche;
        float oldDailyChange = pt.CalculateCertaintyChangePerDay(recipient.Ideo, false);

        float multiplier = AbilityMultiplier(initiator, recipient);
        pt.BoostRatingsTowardsIdeo(recipient.Ideo, multiplier, randomize: true);
        float newDailyChange = pt.CalculateCertaintyChangePerDay(recipient.Ideo, false);

        NamedArgument text1 = initiator.Named("INITIATOR");
        NamedArgument text2 = recipient.Named("RECIPIENT");
        NamedArgument text3 = oldDailyChange.ToStringPercent().Named("OLDDAILYCHANGE");
        NamedArgument text4 = newDailyChange.ToStringPercent().Named("NEWDAILYCHANGE");
        NamedArgument text5 = initiator.Ideo.name.Named("IDEO");
        TaggedString message = Props.successMessage.Formatted(text1, text2, text3, text4, text5);
        Messages.Message(message, new LookTargets(new Pawn[2] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
        PlayLogEntry_Interaction entry = new PlayLogEntry_Interaction(InteractionDefOfPsychology.Indoctrinate, initiator, recipient, null);
        Find.PlayLog.Add(entry);
        if (Props.sound != null)
        {
            Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
        }
    }

    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        if (target == null || !Valid(target))
        {
            return null;
        }
        Pawn recipient = target.Pawn;
        Pawn initiator = parent.pawn;
        float multiplier = AbilityMultiplier(initiator, recipient);
        return PsycheHelper.Comp(recipient).Psyche.IdeoAbilityEffectOnPsycheTooltip(multiplier);
    }

    public float AbilityMultiplier(Pawn initiator, Pawn recipient)
    {
        float multiplier = Props.baseDailyCertaintyChangeIncrease;
        multiplier *= initiator.GetStatValue(StatDefOf.ConversionPower);
        multiplier *= SocialSkillFactor.Evaluate((float)initiator.skills.GetSkill(SkillDefOf.Social).levelInt);
        multiplier *= OpinionFactor.Evaluate((float)recipient.relations.OpinionOf(initiator));
        if (Props.baseDailyCertaintyChangeIncrease == 0f)
        {
            Log.Error("Ability multiplier is 0");
        }
        return multiplier;
    }

}

