using ITF.Skill.Passive;
using ITF.World;
using ITF.WorldObjects;
using UnityEngine;

namespace ITF.Entity {
    public class TrainingManager : MonoBehaviour
    {

        private TrainingBuilding trainingBuilding => WorldManager.Map.TrainingBuilding;

        public void RegisterUnitForTraining(Character unit, TrainingAssignment assignment)
        {

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
