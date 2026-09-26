using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

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
            this.requiredGroups = requiredGroups.OrderBy(profile => profile.priority).ToArray();
            currentCombatGroups = new();
            foreach(var group in requiredGroups) 
            { 
                CreateNewCombatGroup(group);
                currentCombatGroups[^1].SetCenterCell(GetRandomCell());
            }
        }

        public Character[] GetCharacters()
        {
            return currentCombatGroups.SelectMany(group => group.UnitsInGroup).ToArray();
        }

        public CombatRoleSO[] GetAllNeededClasses()
        {
            List<CombatRoleSO> res = new List<CombatRoleSO>();
            foreach (CombatGroup group in currentCombatGroups) 
            {
                if (group.IsComplete) continue;
                foreach (CombatRoleSO unitRole in group.MissingUnits.Keys) {
                    if (group.MissingUnits[unitRole] > 0) res.Add(unitRole);
                }
            }
            return res.ToArray();
        }

        public CombatRoleSO GetNextNeededClass()
        {
            return GetAllNeededClasses()[0];
        }

        public void ReceiveNewUnit(SkilledCharacter unit)
        {
            currentCombatGroups[0].AddUnitToGroup(unit);
            foreach(var group in currentCombatGroups)
            {
                if(group.IsComplete) continue;
                if (group.AddUnitToGroup(unit)) break;
            }
        }

        public void CreateNewCombatGroup(CombatGroupProfileSO combatGroupProfile)
        {
            if (!requiredGroups.Contains(combatGroupProfile)) return;
            CombatGroup combatGroup = new CombatGroup(Array.IndexOf(requiredGroups, combatGroupProfile), combatGroupProfile);
            currentCombatGroups.Add(combatGroup);
        }

        Vector2Int GetRandomCell()
        {
            return homeRange.min + new Vector2Int(Random.Range(0, homeRange.width), Random.Range(0, homeRange.height));
        }
    }
}
