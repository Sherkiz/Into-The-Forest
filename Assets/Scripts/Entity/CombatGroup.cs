using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroup
    {
        private List<SkilledCharacter> units;
        public List<SkilledCharacter> UnitsInGroup { get => units; } 
        public CombatGroup()
        {
            units = new List<SkilledCharacter>();
        }
        public CombatGroup(List<SkilledCharacter> units)
        {
            this.units = units;
        }
        public void AddUnitToGroup(SkilledCharacter unit)
        {
            units.Add(unit);
        }
    }
}
