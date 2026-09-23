using ITF.World;
using ITF.WorldObjects;
using System.Collections.Generic;
using UnityEngine;
using ITF.WorldObjects;
using UnityEngine.Events;

namespace ITF.Entity {
    [System.Serializable]
    public class TrainingManager 
    {
        [SerializeField, Tooltip("The traning program needs to be arranged in order")]
        TrainingProgram[] trainingPrograms;

        private List<TrainingProgram> remainPrograms;
        private List<TrainingAssignment> trainingAssignments;
        private List<Character> reserveUnits;
        private Dictionary<UnitClass, List<TrainingBuilding>> trainingBuildings;

        [Tooltip("The first character is old. The second character is specialist.")]
        public UnityEvent<Character, Character> onTrained = new();

        public void Init()
        {
            var buildings = WorldManager.Map.GetMapObjects<TrainingBuilding>();
            trainingBuildings = new();
            foreach (var building in buildings)
            {
                foreach (var c in building.trainingClasses)
                {
                    if (!trainingBuildings.TryGetValue(c, out var list))
                    {
                        list = new();
                        trainingBuildings[c] = list;
                    }
                    list.Add(building);
                }
            }

            remainPrograms = new();
            foreach (var program in trainingPrograms)
            {
                for (int i = 0; i < program.count; i++)
                {
                    remainPrograms.Add(program);
                }
            }

            reserveUnits = new();
            trainingAssignments = new();
        }

        public bool RegisterUnitForTraining(Character unit)
        {
            var trainingAssigment = TryTraining(unit);
            if (trainingAssigment != null)
            {
                var trainingBuilding = SelectTranningBuilding(trainingAssigment.TargetCombatRole);
                if (trainingBuilding != null)
                {
                    trainingBuilding.AffectTrainingUnit(trainingAssigment);
                    trainingBuilding.onTrained.AddListener(OnTrained);
                    trainingAssignments.Add(trainingAssigment);
                    remainPrograms.RemoveAt(0);
                    return true;
                }
            }

            return false;
        }

        public void OnUnitSpawned(Character unit)
        {
            if(!RegisterUnitForTraining(unit)) reserveUnits.Add(unit);
        }

        private TrainingBuilding SelectTranningBuilding(UnitClass targetClass)
        {
            TrainingBuilding trainingBuilding = null;
            if (trainingBuildings.TryGetValue(targetClass, out var list))
            {
                int waitingCount = int.MaxValue;
                foreach(var building in list)
                {
                    if (building.HasFreeSlot) return building;
                    if(waitingCount > building.WaitingCount)
                    {
                        waitingCount = building.WaitingCount;
                        trainingBuilding = building;
                    }
                }
            }
            return trainingBuilding;
        }

        private TrainingAssignment TryTraining(Character unit)
        {
            if (unit == null || remainPrograms.Count == 0) return null;
            var trainingProgram = remainPrograms[0];
            Debug.Log(trainingProgram.targetClass);
            if(trainingProgram.faction == Faction.Unknow || trainingProgram.faction == unit.Faction)
            {
                return new(unit, trainingProgram.targetClass);
            }
            return null;
        }

        void OnTrained(TrainingBuilding trainingBuilding, TrainingAssignment trainingAssignment)
        {
            trainingAssignments.Remove(trainingAssignment);
            onTrained?.Invoke(trainingAssignment.unit, trainingAssignment.trainedUnit);

            //Train the reserve units
            for(int i = 0; i < reserveUnits.Count; i++)
            {
                if (RegisterUnitForTraining(reserveUnits[i]))
                {
                    reserveUnits.RemoveAt(i);
                    break;
                }
            }
        }
    }

    [System.Serializable]
    class TrainingProgram
    {
        [Tooltip("Faction of training unit, Unknow is not limited")]
        public Faction faction;
        public UnitClass targetClass;
        public int count = 1;
    }

    public class TrainingAssignment
    {
        public Character unit;
        public Character trainedUnit;

        public UnitClass TargetCombatRole;
        public TrainingStatus status;

        public TrainingAssignment(Character unit, UnitClass targetCombatRole)
        {
            this.unit = unit;
            TargetCombatRole = targetCombatRole;
            status = TrainingStatus.Waiting;
        }
    }

    public enum TrainingStatus 
    {
        Reserved, 
        MovingToWaiting, 
        Waiting, 
        MovingToSlot,
        Training, 
        Completed, 
        Cancelled
    }
}
