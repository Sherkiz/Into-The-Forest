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
        public CombatGroup(int groupID, CombatGroupProfileSO combatGroupProfileSO)
        {
            units = new List<Character>();
            GroupID = groupID;
            this.combatGroupProfileSO = combatGroupProfileSO;
            MissingUnits = combatGroupProfileSO.neededClasses;
        }
        public void AddUnitToGroup(Character unit)
        {
            units.Add(unit);
            MissingUnits[unit.UnitClass] = MissingUnits[unit.UnitClass] - 1;
            unit.OnDeinited.AddListener(RemoveUnitFromGroup);
        }
        private void RemoveUnitFromGroup(Character unit) { 
            units.Remove(unit);
            MissingUnits[unit.UnitClass] = MissingUnits[unit.UnitClass] + 1;
        }
    }
}
