using ITF.Entity;
using UnityEngine;
using ITF.Utilities;
using System.Collections;
using ITF.Skill.Passive;
using System.Collections.Generic;
using UnityEngine.Events;
using ITF.World;
using MBT;

namespace ITF.WorldObjects
{
    public class TrainingBuilding : Building
    {
        [SerializeField] private int totalNumberOfTrainingSlots;
        [SerializeField] private float trainingTime;
        [SerializeField] private RectInt waitingRange;
        [SerializeField] private Character[] specialistPrefabs;
        public UnitClass[] trainingClasses;
        public bool HasFreeSlot { get =>  currentTrainings.Count < totalNumberOfTrainingSlots; }
        private List<Task> currentTrainings;
        private List<TrainingAssignment> waitingUnits;
        public int WaitingCount => waitingUnits.Count;

        private Vector2Int entranceCell;
        private Vector3 entrancePos;

        public UnityEvent<TrainingBuilding, TrainingAssignment> onTrained;

        public override void Deinit()
        {
            foreach (var task in currentTrainings) { task.Stop(); }
        }

        public override void Init()
        {
            entranceCell = Range.position + (Vector2Int)EntranceOffset;
            entrancePos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)entranceCell);
            currentTrainings = new();
            waitingUnits = new();
        }
        
        public void AffectTrainingUnit(TrainingAssignment assignment)
        {
            if (!HasFreeSlot)
            {
                Waiting(assignment);
                return;
            }

            SetTargetCell(assignment.unit, entranceCell);
            Task training = new Task(StartTraining(assignment));
            training.Finished += (bool manual) =>
            {
                currentTrainings.Remove(training);
                if(waitingUnits.Count > 0)
                {
                    AffectTrainingUnit(waitingUnits[0]);
                    waitingUnits.RemoveAt(0);
                }

                if (manual)
                {
                    assignment.status = TrainingStatus.Cancelled;
                    return;
                }
                assignment.status = TrainingStatus.Completed;
                assignment.trainedUnit = SpawnSpecialist(assignment);
                onTrained?.Invoke(this, assignment);
            };
            currentTrainings.Add(training);
        }

        private void Waiting(TrainingAssignment trainingAssignment)
        {
            waitingUnits.Add(trainingAssignment);
            if (!WorldManager.Map.GetNearestEmptyCell(entranceCell, waitingRange, out var emptyCell)) emptyCell = entranceCell;
            SetTargetCell(trainingAssignment.unit, emptyCell);
        }

        private void SetTargetCell(Character unit, Vector2Int cell)
        {
            var blackboardGo = unit.GetReference("blackboard");
            if (blackboardGo)
            {
                var blackboard = blackboardGo.GetComponent<Blackboard>();
                var targetCell = blackboard.GetVariable<Vector2Variable>("target_cell");
                if (targetCell) targetCell.Value = cell;
            }
        }

        private Character SpawnSpecialist(TrainingAssignment trainingAssignment)
        {
            foreach(var prefab in specialistPrefabs)
            {
                if(trainingAssignment.unit.Faction == prefab.Faction && trainingAssignment.TargetCombatRole == prefab.UnitClass)
                {
                    var character = GameObjectPool.CreateGameObject(prefab.gameObject).GetComponent<Character>();
                    character.transform.position = entrancePos;
                    return character;
                }
            }
            return null;
        }

        private IEnumerator StartTraining(TrainingAssignment trainingAssignment) 
        { 
            while(Vector3.Distance(trainingAssignment.unit.transform.position, entrancePos) > .1f)
            {
                yield return null;
            }
            trainingAssignment.unit.gameObject.SetActive(false);
            yield return new WaitForSeconds(trainingTime);
        }
    }
}
