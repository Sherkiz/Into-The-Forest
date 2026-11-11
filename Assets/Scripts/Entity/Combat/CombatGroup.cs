using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroup : MonoBehaviour
    {
        public int GroupID { get; private set; }
        [SerializeField] private CombatGroupProfileSO combatGroupProfileSO;
        public int GroupPriority { get => combatGroupProfileSO.priority; }
        public string Name { get => combatGroupProfileSO.groupName; }
        Dictionary<Character, CombatRoleSO> unitRoles = new Dictionary<Character, CombatRoleSO>();
        public List<Character> UnitsInGroup { get => unitRoles.Keys.ToList(); } 
        public bool IsComplete { get => MissingUnits.Values.Sum() == 0; }
        public Dictionary<CombatRoleSO, int> MissingUnits;

        private Vector2Int centerCell;
        public Vector2Int CenterCell { get => centerCell; }

        private void Awake()
        {
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
        public void CancelSlotSave(CombatRoleSO combatRole, Character unit)
        {
            MissingUnits[combatRole] = MissingUnits[combatRole] + 1;
            unit.OnDeinited.RemoveListener(OnUnitDeinited);
        }
        public void AddUnitToGroup(Character unit)
        {
            CombatRoleSO role = GetMissingRoleForUnitClass(unit.UnitClass, out int count);
            AddUnitToGroup(unit, role);
        }
        public void AddUnitToGroup(Character unit, CombatRoleSO combatRole)
        {
            unitRoles.Add(unit, combatRole);
            unit.transform.SetParent(transform, true);
            SetRallyPoint(centerCell, unit);
        }

        public void SetRallyPoint(Vector2Int centerCell, Character unit)
        {
            unit.SetTargetCell(centerCell, placeholder:true);
        }

        private void RemoveUnitFromGroup(Character unit) 
        {
            if (unitRoles.TryGetValue(unit, out var role))
            {
                unitRoles.Remove(unit);
                MissingUnits[role] = MissingUnits[role] + 1;
            }
            unit.OnDeinited.RemoveListener(OnUnitDeinited);
            unit.transform.SetParent(null);
        }

        void OnUnitDeinited(Character unit)
        {
            RemoveUnitFromGroup(unit);
        }
    }
}
