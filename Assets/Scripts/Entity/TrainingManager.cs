using ITF.Skill.Passive;
using ITF.World;
using ITF.WorldObjects;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity {
    public class TrainingManager : MonoBehaviour, IEntityManager
    {
        [SerializeField] private CombatGroupManager combatGroupManager;
        private List<Character> trainingUnits;
        private TrainingBuilding trainingBuilding => WorldManager.Map.TrainingBuilding;

        public Character[] GetCharacters()
        {
            return trainingUnits.ToArray();
        }

        public void RegisterUnitForTraining(Character unit, TrainingAssignment assignment)
        {
            trainingUnits.Add(unit);
            trainingBuilding.AffectTrainingUnit(unit, assignment);
            //Need to remove unit from trainingUnits when training is completed
        }
    }
    public class TrainingAssignment
    {
        public int trainingAssignmentId;

        public PassiveSkillAddor RequiredCombatRole;
        public Faction TargetUnitFaction;
        public TrainingStatus status;
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
