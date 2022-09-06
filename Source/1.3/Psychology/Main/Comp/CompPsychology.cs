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

    public override void PostExposeData()
    {
        Log.Message("CompPsychology.PostExposeData() step 0");
        base.PostExposeData();
        Log.Message("CompPsychology.PostExposeData() step 1");
        Scribe_Deep.Look(ref this.sexuality, "sexuality", new object[]
        {
            this.parent as Pawn
        });
        Log.Message("CompPsychology.PostExposeData() step 2");
        Scribe_Deep.Look(ref this.psyche, "psyche", new object[]
        {
            this.parent as Pawn
        });
        Log.Message("CompPsychology.PostExposeData() step 3");
        Scribe_Values.Look(ref this.beenBuried, "beenBuried");
        Log.Message("CompPsychology.PostExposeData() step 4");
        Scribe_Values.Look(ref this.tickSinceLastSeenLover, "tickSinceLastSeenLover", GenTicks.TicksAbs);
        Log.Message("CompPsychology.PostExposeData() step 5");
    }

    public Pawn_SexualityTracker Sexuality
    {
        get
        {
            //Log.Message("Get Sexuality for " + this.parent.LabelShort);
            if(this.sexuality == null)
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
            //Log.Message("Return Sexuality for " + this.parent.LabelShort);
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
            //Log.Message("Get Psyche for " + this.parent.LabelShort);
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
            //Log.Message("Return Psyche for " + this.parent.LabelShort);
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
    
    
}
