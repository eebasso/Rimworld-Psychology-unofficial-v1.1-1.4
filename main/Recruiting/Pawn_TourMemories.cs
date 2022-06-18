using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public class Pawn_TourMemories : IExposable
    {
        public Pawn_TourMemories(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref avgBedroomImpressiveness, "bedrooms");
            Scribe_Values.Look(ref avgDiningRoomImpressiveness, "dining");
            Scribe_Values.Look(ref avgHospitalImpressiveness, "hospitals");
            Scribe_Values.Look(ref avgRecRoomImpressiveness, "recreation");
            Scribe_Values.Look(ref avgBaseCleanliness, "cleaning");
            Scribe_Values.Look(ref avgMedicalTreatment, "medicine");
            Scribe_Values.Look(ref avgFoodThoughts, "food");
        }

        public Pawn pawn;
        public Pair<float, int> avgBedroomImpressiveness = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgDiningRoomImpressiveness = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgHospitalImpressiveness = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgRecRoomImpressiveness = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgBaseCleanliness = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgMedicalTreatment = new Pair<float, int>(0f, 0);
        public Pair<float, int> avgFoodThoughts = new Pair<float, int>(0f, 0);
        public Room lastVisitedRoom;
        public bool allowedTours = false;

        public float ColonyImpressiveness
        {
            get
            {
                float bedroomFactor = 0.5f;
                if (avgBedroomImpressiveness.Second > 0)
                {
                    bedroomFactor = Mathf.InverseLerp(25f, 1000f, (avgBedroomImpressiveness.First / avgBedroomImpressiveness.Second)) + 0.7f;
                }
                float diningFactor = 0.5f;
                if (avgDiningRoomImpressiveness.Second > 0)
                {
                    diningFactor = Mathf.InverseLerp(25f, 2000f, (avgDiningRoomImpressiveness.First / avgDiningRoomImpressiveness.Second)) + 0.6f;
                }
                float hospitalFactor = 0.5f;
                if (avgHospitalImpressiveness.Second > 0)
                {
                    hospitalFactor = Mathf.InverseLerp(50f, 200f, (avgHospitalImpressiveness.First / avgHospitalImpressiveness.Second)) + 0.5f;
                }
                float recreationFactor = 1f;
                if (avgRecRoomImpressiveness.Second > 0)
                {
                    recreationFactor = Mathf.InverseLerp(0f, 5000f, (avgRecRoomImpressiveness.First / avgRecRoomImpressiveness.Second)) + 1f;
                }
                float cleanFactor = 1f;
                if (avgBaseCleanliness.Second > 0)
                {
                    cleanFactor = Mathf.InverseLerp(-1f, 0.6f, (avgBaseCleanliness.First / avgBaseCleanliness.Second)) * 1.25f;
                }
                float treatmentFactor = 0.75f;
                if (avgMedicalTreatment.Second > 0)
                {
                    treatmentFactor += Mathf.Lerp(0f, 0.35f, (avgMedicalTreatment.First / avgMedicalTreatment.Second));
                }
                float foodFactor = 1f;
                if (avgFoodThoughts.Second > 0)
                {
                    foodFactor += Mathf.InverseLerp(-12f, 10f, (avgFoodThoughts.First / avgFoodThoughts.Second)) - 0.5f;
                }
                return bedroomFactor * diningFactor * hospitalFactor * recreationFactor * cleanFactor * treatmentFactor * foodFactor;
            }
        }

        public void ObserveNewRoom(Room room)
        {
            lastVisitedRoom = room;
            float roomImpressiveness = room.GetStat(RoomStatDefOf.Impressiveness);
            if (!room.PsychologicallyOutdoors)
            {
                avgBaseCleanliness = new Pair<float, int>(avgBaseCleanliness.First + room.GetStat(RoomStatDefOf.Cleanliness), avgBaseCleanliness.Second + 1);
            }
            if (room.Role == RoomRoleDefOf.Bedroom)
            {
                avgBedroomImpressiveness = new Pair<float,int>(avgBedroomImpressiveness.First + roomImpressiveness, avgBedroomImpressiveness.Second + 1);
            }
            else if (room.Role == RoomRoleDefOf.DiningRoom)
            {
                avgDiningRoomImpressiveness = new Pair<float, int>(avgDiningRoomImpressiveness.First + roomImpressiveness, avgDiningRoomImpressiveness.Second + 1);
            }
            else if (room.Role == RoomRoleDefOf.Hospital)
            {
                avgHospitalImpressiveness = new Pair<float, int>(avgHospitalImpressiveness.First + roomImpressiveness, avgHospitalImpressiveness.Second + 1);
            }
            else if (room.Role == RoomRoleDefOf.RecRoom)
            {
                avgRecRoomImpressiveness = new Pair<float, int>(avgRecRoomImpressiveness.First + roomImpressiveness, avgRecRoomImpressiveness.Second + 1);
            }
        }
    }
}
