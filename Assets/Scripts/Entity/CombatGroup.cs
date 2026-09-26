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
        public Dictionary<CombatRoleSO, int> MissingUnits;

        private Vector2Int centerCell;

        public CombatGroup(int groupID, CombatGroupProfileSO combatGroupProfileSO)
        {
            units = new List<Character>();
            GroupID = groupID;
            this.combatGroupProfileSO = combatGroupProfileSO;
            MissingUnits = combatGroupProfileSO.neededRoles;
        }
        public CombatRoleSO GetMissingRoleForUnitClass(UnitClass unitClass, out int count)
        {
            CombatRoleSO missingRole = null;
            count = 0;
            foreach(var role in MissingUnits.Keys)
            {
                if (role.unitClassesInRole.Contains(unitClass))
                {
                    missingRole = role;
                    count += MissingUnits[role];
                }
            }
            return missingRole;
        }
        public bool AddUnitToGroup(Character unit)
        {
            CombatRoleSO role = GetMissingRoleForUnitClass(unit.UnitClass, out int count);
            if (role == null) return false;
            if (count <= 0) return false;

            units.Add(unit);
            SetCenterCell(centerCell, unit);
            MissingUnits[role] = count - 1;
            unit.OnDeinited.AddListener((unit) => RemoveUnitFromGroup(unit, role));

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

        private void RemoveUnitFromGroup(Character unit, CombatRoleSO role) { 
            units.Remove(unit);
            MissingUnits[role] = MissingUnits[role] + 1;
        }
    }
}
