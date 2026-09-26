using ITF.World;
using MBT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroup
    {
        public int GroupID { get; private set; }
        private CombatGroupProfileSO combatGroupProfileSO;
        private List<Character> units;
        public List<Character> UnitsInGroup { get => units; } 
        public bool IsComplete { get => units.Count == combatGroupProfileSO.NumberOfUnits; }
        public Dictionary<UnitClass, int> MissingUnits;

        private Vector2Int centerCell;

        public CombatGroup(int groupID, CombatGroupProfileSO combatGroupProfileSO)
        {
            units = new List<Character>();
            GroupID = groupID;
            this.combatGroupProfileSO = combatGroupProfileSO;
            MissingUnits = combatGroupProfileSO.neededClasses;
        }

        public bool AddUnitToGroup(Character unit)
        {
            if (!MissingUnits.TryGetValue(unit.UnitClass, out int count)) return false;
            if (count <= 0) return false;

            units.Add(unit);
            SetCenterCell(centerCell, unit);
            MissingUnits[unit.UnitClass] = count - 1;
            unit.OnDeinited.AddListener(RemoveUnitFromGroup);

            return true;
        }

        public void SetCenterCell(Vector2Int centerCell)
        {
            this.centerCell = centerCell;
            foreach (var unit in units)
            {
                var blackboardGo = unit.GetReference("blackboard");
                if (blackboardGo == null) continue;
                var blackboard = blackboardGo.GetComponent<Blackboard>();
                if (blackboard == null) continue;
                var targetCell = blackboard.GetVariable<Vector2Variable>("target_cell");
                if (!WorldManager.Map.GetNearestEmptyCell(centerCell, out var emptyCell)) emptyCell = centerCell;
                targetCell.Value = emptyCell;
            }
        }

        private void SetCenterCell(Vector2Int centerCell, Character unit)
        {
            var blackboardGo = unit.GetReference("blackboard");
            if (blackboardGo == null) return;
            var blackboard = blackboardGo.GetComponent<Blackboard>();
            if (blackboard == null) return;
            var targetCell = blackboard.GetVariable<Vector2Variable>("target_cell");
            if (!WorldManager.Map.GetNearestEmptyCell(centerCell, out var emptyCell)) emptyCell = centerCell;
            targetCell.Value = emptyCell;
        }

        private void RemoveUnitFromGroup(Character unit) { 
            units.Remove(unit);
            MissingUnits[unit.UnitClass] = MissingUnits[unit.UnitClass] + 1;
        }
    }
}
