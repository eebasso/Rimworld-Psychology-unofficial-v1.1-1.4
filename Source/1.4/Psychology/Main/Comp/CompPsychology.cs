using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psychology;

public class CompPsychology : ThingComp
{
    private Pawn_SexualityTracker sexuality;
    private Pawn_PsycheTracker psyche;
    //public Pawn_TourMemories recruiting;
    private bool beenBuried = false;
    private int tickSinceLastSeenLover;

    public Pawn_SexualityTracker Sexuality
    {
        get
        {
            if (this.sexuality == null)
            {
                Pawn p = this.parent as Pawn;
                if (p != null)
                {
                    this.sexuality = new Pawn_SexualityTracker(p);
                    this.sexuality.GenerateSexuality();
                }
                else
                {
                    Log.Error("Psychology :: CompPsychology was added to " + this.parent.Label + " which cannot be cast to a Pawn.");
                }
            }
            return this.sexuality;
        }
        set
        {
            this.sexuality = value;
        }
    }

    public Pawn_PsycheTracker Psyche
    {
        get
        {
            if (this.psyche == null)
            {
                Pawn p = this.parent as Pawn;
                if (p != null)
                {
                    this.psyche = new Pawn_PsycheTracker(p);
                    this.psyche.Initialize();
                }
                else
                {
                    Log.Error("Psychology :: CompPsychology was added to " + this.parent.Label + " which cannot be cast to a Pawn.");
                }
            }
            return this.psyche;
        }
        set
        {
            this.psyche = value;
        }
    }

    public bool AlreadyBuried
    {
        get
        {
            return this.beenBuried;
        }
        set
        {
            this.beenBuried = value;
        }
    }

    public int LDRTick
    {
        get
        {
            return tickSinceLastSeenLover;
        }
        set
        {
            tickSinceLastSeenLover = value;
        }
    }

    public bool IsPsychologyPawn
    {
        get
        {
            return this.Psyche != null && this.Sexuality != null;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref this.sexuality, "sexuality", new object[]
        {
            this.parent as Pawn
        });
        Scribe_Deep.Look(ref this.psyche, "psyche", new object[]
        {
            this.parent as Pawn
        });
        /*Scribe_Deep.Look(ref this.recruiting, "recruiting", new object[]
        {
            this.parent as Pawn
        });*/
        Scribe_Values.Look(ref this.beenBuried, "beenBuried");
        Scribe_Values.Look(ref this.tickSinceLastSeenLover, "tickSinceLastSeenLover", Find.TickManager.TicksAbs);
    }

    public void DeepCopyFromOtherComp(CompPsychology otherComp)
    {
        Pawn pawn = this.parent as Pawn;
        this.sexuality = new Pawn_SexualityTracker(pawn);
        this.sexuality.DeepCopyFromOtherTracker(otherComp.Sexuality);
        this.psyche = new Pawn_PsycheTracker(pawn);
        this.psyche.DeepCopyFromOtherTracker(otherComp.Psyche);
        this.beenBuried = otherComp.AlreadyBuried;
        this.tickSinceLastSeenLover = otherComp.LDRTick;
        try
        {
            Pawn otherPawn = otherComp.parent as Pawn;
            //Log.Message("CompPsychology, deep copy complete for pawn = " + pawn + ", " + pawn.def.label + " from otherPawn = " + otherPawn.Label + ", " + otherPawn.def.label);
        }
        catch (Exception ex)
        {
            Log.Warning("CompPsychology, deep copy complete for pawn = " + pawn + ", " + pawn.def.label + ", cast of otherComp.parent failed because of " + ex);
        }
    }
}
