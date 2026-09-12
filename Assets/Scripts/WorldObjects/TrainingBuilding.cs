using ITF.Entity;
using UnityEngine;
using ITF.Utilities;
using System.Collections;
using ITF.Skill.Passive;
using System.Collections.Generic;

namespace ITF.WorldObjects
{
    public class TrainingBuilding : MapObject
    {
        [SerializeField] private int totalNumberOfTrainingSlots;
        [SerializeField] private float trainingTime;
        private int numberOfFreeTrainingSlots;
        public bool HasFreeSlot { get =>  numberOfFreeTrainingSlots > 0; }
        private List<Task> currentTrainings;
        public override void Deinit()
        {
            foreach (var task in currentTrainings) { task.Stop(); }
        }

        public override void Init()
        {
            numberOfFreeTrainingSlots = totalNumberOfTrainingSlots;
            currentTrainings = new();
        }
        
        public void AffectTrainingUnit(Character unit, TrainingAssignment assignment)
        {
            if (!HasFreeSlot)
            {
                Debug.LogWarning("TrainingBuilding does not have any free slot!");
                return;
            }
            numberOfFreeTrainingSlots--;
            unit.gameObject.SetActive(false);
            Task training = new Task(StartTraining(unit));
            training.Finished += (bool manual) =>
            {
                OnTrainingEnded(manual, unit, assignment.RequiredCombatRole);
                currentTrainings.Remove(training);
            };
            currentTrainings.Add(training);
        }
        private IEnumerator StartTraining(Character unit) 
        { 
            yield return new WaitForSeconds(trainingTime);
        }
        private void OnTrainingEnded(bool completed, Character unit, PassiveSkillAddor passiveSkillAddor)
        {
            if (!completed)
            {
                unit.gameObject.SetActive(true);
                return;
            }
            SkilledCharacter trainedUnit = unit.gameObject.AddComponent<SkilledCharacter>();
            Destroy(unit);
            passiveSkillAddor.AddPassiveSkill(trainedUnit);
        }
    }
}
