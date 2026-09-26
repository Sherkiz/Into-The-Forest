using ITF.World;
using ITF.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Entity {
    [System.Serializable]
    public class TrainingManager 
    {
        [SerializeField, Tooltip("The traning programs that can be assigned")]
        TrainingProgram[] allowedTrainingPrograms;

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

            reserveUnits = new();
            trainingAssignments = new();
        }
        private TrainingProgram SelectTrainingProgram(Character unit, CombatRoleSO[] requiredCombatRoles)
        {
            // Check if this accounts for group priority
            //foreach(var role in requiredCombatRoles) Debug.Log(role.roleName);
            return allowedTrainingPrograms.FirstOrDefault(program => program.faction == unit.Faction && requiredCombatRoles.Any(role => role.unitClassesInRole.Contains(program.targetClass)));
        }
        private bool RegisterUnitForTraining(Character unit, TrainingProgram trainingProgram)
        {
            var trainingAssigment = TryTraining(unit, trainingProgram);
            if (trainingAssigment != null)
            {
                var trainingBuilding = SelectTrainingBuilding(trainingAssigment.TargetCombatRole);
                if (trainingBuilding != null)
                {
                    trainingBuilding.AffectTrainingUnit(trainingAssigment);
                    trainingBuilding.onTrained.AddListener(OnTrained);
                    trainingAssignments.Add(trainingAssigment);
                    return true;
                }
            }

            return false;
        }

        public void OnUnitSpawned(Character unit, CombatRoleSO[] requiredCombatRoles)
        {
            TrainingProgram trainingProgram = SelectTrainingProgram(unit, requiredCombatRoles);
            if (trainingProgram != null)
            {
                if (RegisterUnitForTraining(unit, trainingProgram)) return;
            }
            reserveUnits.Add(unit);
        }

        private TrainingBuilding SelectTrainingBuilding(UnitClass targetClass)
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

        private TrainingAssignment TryTraining(Character unit, TrainingProgram trainingProgram)
        {
            if (unit == null || trainingProgram == null) return null;
            Debug.Log(trainingProgram.targetClass + " (" + unit.Faction + ")");
            if(trainingProgram.faction == Faction.Unknow || trainingProgram.faction == unit.Faction) // Shiuld not be necessary since trainigProgram is now selected
            {
                return new(unit, trainingProgram.targetClass);
            }
            return null;
        }

        void OnTrained(TrainingBuilding trainingBuilding, TrainingAssignment trainingAssignment)
        {
            trainingAssignments.Remove(trainingAssignment);
            onTrained?.Invoke(trainingAssignment.unit, trainingAssignment.trainedUnit);
            
            /*
            //Train the reserve units
            for(int i = 0; i < reserveUnits.Count; i++)
            {
                if (RegisterUnitForTraining(reserveUnits[i]))
                {
                    reserveUnits.RemoveAt(i);
                    break;
                }
            }
            */
        }
    }

    [System.Serializable]
    class TrainingProgram
    {
        [Tooltip("Faction of training unit, Unknow is not limited")]
        public Faction faction;
        public UnitClass targetClass;
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
