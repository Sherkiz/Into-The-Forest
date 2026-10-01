using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroupManager : IEntityManager
    {
        private CombatGroupProfileSO[] requiredGroups;
        private List<CombatGroup> currentCombatGroups;

        public RectInt homeRange;

        public CombatGroupManager(CombatGroupProfileSO[] requiredGroups, RectInt homeRange)
        {
            this.homeRange = homeRange;
            this.requiredGroups = requiredGroups;
            currentCombatGroups = new();
            foreach(var group in requiredGroups) 
            { 
                CreateNewCombatGroup(group);
            }
            currentCombatGroups = currentCombatGroups.OrderBy(group => group.GroupPriority).ToList();
        }

        public Character[] GetCharacters()
        {
            return currentCombatGroups.SelectMany(group => group.UnitsInGroup).ToArray();
        }

        public CombatSlot[] GetAllNeededSlots()
        {
            List<CombatSlot> res = new List<CombatSlot>();
            foreach (CombatGroup group in currentCombatGroups) 
            {
                if (group.IsComplete) continue;
                foreach (CombatRoleSO unitRole in group.MissingUnits.Keys) {
                    if (group.MissingUnits[unitRole] > 0) res.Add(new CombatSlot(group, unitRole));
                }
            }
            return res.ToArray();
        }
        public void ReceiveNewUnit(SkilledCharacter unit)
        {
            foreach(var group in currentCombatGroups)
            {
                if(group.IsComplete) continue;
                if (group.AddUnitToGroup(unit)) break;
                else Debug.LogWarning($"Unit {unit.name} could not be added to group {group.Name}");
            }
        }

        public void CreateNewCombatGroup(CombatGroupProfileSO combatGroupProfile)
        {
            if (!requiredGroups.Contains(combatGroupProfile)) return;
            CombatGroup combatGroup = new CombatGroup(Array.IndexOf(requiredGroups, combatGroupProfile), combatGroupProfile);
            currentCombatGroups.Add(combatGroup);
        }
    }
    public struct CombatSlot
    {
        public CombatGroup Group;
        public CombatRoleSO Role;
        
        public CombatSlot(CombatGroup group, CombatRoleSO role)
        {
            Group = group;
            Role = role;
        }
    }
}
