using System;
using System.Collections.Generic;
using System.Linq;

namespace ITF.Entity
{
    public class CombatGroupManager : IEntityManager
    {
        private CombatGroupProfileSO[] requiredGroups;
        private List<CombatGroup> currentCombatGroups;

        public CombatGroupManager(CombatGroupProfileSO[] requiredGroups)
        {
            this.requiredGroups = requiredGroups.OrderBy(profile => profile.priority).ToArray();
        }
        public Character[] GetCharacters()
        {
            return currentCombatGroups.SelectMany(group => group.UnitsInGroup).ToArray();
        }
        public UnitClass[] GetAllNeededClasses()
        {
            List<UnitClass> res = new List<UnitClass>();
            foreach (CombatGroup group in currentCombatGroups) 
            {
                if (group.IsComplete) continue;
                foreach (UnitClass unitClass in group.MissingUnits.Keys) {
                    if (group.MissingUnits[unitClass] > 0) res.Add(unitClass);
                }
            }
            return res.ToArray();
        }
        public UnitClass GetNextNeededClass()
        {
            return GetAllNeededClasses()[0];
        }
        public void ReceiveNewUnit(SkilledCharacter unit)
        {
            currentCombatGroups[0].AddUnitToGroup(unit);
        }
        public void CreateNewCombatGroup(CombatGroupProfileSO combatGroupProfile)
        {
            if (!requiredGroups.Contains(combatGroupProfile)) return;
            CombatGroup combatGroup = new CombatGroup(Array.IndexOf(requiredGroups, combatGroupProfile), combatGroupProfile);
            currentCombatGroups.Add(combatGroup);
        }
    }
}
