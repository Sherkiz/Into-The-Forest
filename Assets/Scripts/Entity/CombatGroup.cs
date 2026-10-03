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
        public int GroupPriority { get => combatGroupProfileSO.priority; }
        public string Name { get => combatGroupProfileSO.groupName; }
        Dictionary<Character, CombatRoleSO> unitRoles = new Dictionary<Character, CombatRoleSO>();
        public List<Character> UnitsInGroup { get => unitRoles.Keys.ToList(); } 
        public bool IsComplete { get => MissingUnits.Values.Sum() == 0; }
        public Dictionary<CombatRoleSO, int> MissingUnits;

        private Vector2Int centerCell;

        public CombatGroup(int groupID, CombatGroupProfileSO combatGroupProfileSO)
        {
            GroupID = groupID;
            this.combatGroupProfileSO = combatGroupProfileSO;
            MissingUnits = combatGroupProfileSO.neededRoles;
            centerCell = combatGroupProfileSO.rallyRange.min + new Vector2Int(Random.Range(0, combatGroupProfileSO.rallyRange.width), Random.Range(0, combatGroupProfileSO.rallyRange.height));
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
        public void SaveSlotForRole(CombatRoleSO combatRole, Character unit)
        {
            MissingUnits[combatRole] = MissingUnits[combatRole] - 1;
            unit.OnDeinited.AddListener(OnUnitDeinited);
            if (IsComplete) Debug.Log(Name + " is complete !");
        }
        public void AddUnitToGroup(Character unit)
        {
            CombatRoleSO role = GetMissingRoleForUnitClass(unit.UnitClass, out int count);
            AddUnitToGroup(unit, role);
        }
        public void AddUnitToGroup(Character unit, CombatRoleSO combatRole)
        {
            unitRoles.Add(unit, combatRole);
            SetRallyPoint(centerCell, unit);
        }

        private void SetRallyPoint(Vector2Int centerCell, Character unit)
        {
            var blackboardGo = unit.GetReference("blackboard");
            if (blackboardGo == null) return;
            var blackboard = blackboardGo.GetComponent<Blackboard>();
            if (blackboard == null) return;
            var targetCell = blackboard.GetVariable<Vector2Variable>("target_cell");
            if (!WorldManager.Map.GetNearestEmptyCell(centerCell, out var emptyCell)) emptyCell = centerCell;
            WorldManager.Map.RegisterUnitPlaceholder(unit, emptyCell);
            targetCell.Value = emptyCell;
        }

        private void RemoveUnitFromGroup(Character unit) {
            if (unitRoles.TryGetValue(unit, out var role))
            {
                unitRoles.Remove(unit);
                MissingUnits[role] = MissingUnits[role] + 1;
            }
            unit.OnDeinited.RemoveListener(OnUnitDeinited);
        }

        void OnUnitDeinited(Character unit)
        {
            RemoveUnitFromGroup(unit);
        }
    }
}
