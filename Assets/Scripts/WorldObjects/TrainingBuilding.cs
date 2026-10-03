using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using MBT;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
        private Dictionary<Character, Task> currentTrainings;
        private List<TrainingAssignment> waitingUnits;
        public int WaitingCount => waitingUnits.Count;

        private Vector2Int entranceCell;
        private Vector3 entrancePos;

        public UnityEvent<TrainingBuilding, TrainingAssignment> onTrained;

        public override void Deinit()
        {
            foreach (var task in currentTrainings.Values) { task.Stop(); }
        }

        public override void Init()
        {
            //entranceCell = Range.position + (Vector2Int)EntranceOffset;
            entranceCell = Range.min;   // Temp
            entrancePos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)entranceCell);
            currentTrainings = new();
            waitingUnits = new();
        }
        
        public void AffectTrainingUnit(TrainingAssignment assignment)
        {
            Character unit = assignment.unit;
            Debug.Log(unit + " has been affected to " + name + ".");
            if (!HasFreeSlot)
            {
                Waiting(assignment);
                return;
            }
            SetTargetCell(unit, entranceCell);
            Task training = new Task(StartTraining(assignment));
            unit.OnDeinited.AddListener(StopTraining);
            training.Finished += (bool manual) =>
            {
                currentTrainings.Remove(unit);
                TrainNextWaitingUnit();

                if (manual)
                {
                    assignment.status = TrainingStatus.Cancelled;
                }
                else
                {
                    assignment.status = TrainingStatus.Completed;
                    assignment.trainedUnit = SpawnSpecialist(assignment);
                }
                onTrained?.Invoke(this, assignment);
            };
            currentTrainings.Add(assignment.unit, training);
        }
        private void TrainNextWaitingUnit()
        {
            if (waitingUnits.Count > 0)
            {
                AffectTrainingUnit(waitingUnits[0]);
                waitingUnits.RemoveAt(0);
            }
        }
        private void StopTraining(Character unit)
        {
            if (currentTrainings.TryGetValue(unit, out var training)) training.Stop();
        }
        private void Waiting(TrainingAssignment trainingAssignment)
        {
            waitingUnits.Add(trainingAssignment);
            if (!WorldManager.Map.GetNearestEmptyCell(entranceCell, waitingRange, out var emptyCell)) emptyCell = entranceCell;
            WorldManager.Map.RegisterUnitPlaceholder(trainingAssignment.unit, emptyCell);
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
            Debug.LogWarning("No Specialist found for unit " + trainingAssignment.unit.name + " (target class was " + trainingAssignment.TargetCombatRole + ")");
            return null;
        }

        private IEnumerator StartTraining(TrainingAssignment trainingAssignment) 
        { 
            while(Vector3.Distance(trainingAssignment.unit.transform.position, entrancePos) > .1f)
            {
                yield return null;
            }
            trainingAssignment.unit.OnDeinited.RemoveListener(StopTraining);
            Debug.Log(trainingAssignment.unit + " started training at " + name);
            trainingAssignment.unit.gameObject.SetActive(false);
            yield return new WaitForSeconds(trainingTime);
            Debug.Log(trainingAssignment.unit + " finished training at " + name);
        }
        #region Debug Methods
        [ContextMenu("Debug Waiting queue")]
        public void DebugWaitingQueue()
        {
            foreach (var a in waitingUnits)
            {
                Debug.Log(a.unit);
            }
        }
        #endregion
    }
}
