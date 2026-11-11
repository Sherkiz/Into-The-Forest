using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroupManager : MonoBehaviour, IEntityManager
    {
        [SerializeField] private CombatGroup[] combatGroups;

        private void OnValidate()
        {
            combatGroups = combatGroups.OrderBy(group => group.GroupPriority).ToArray();
        }

        public Character[] GetCharacters()
        {
            return combatGroups.SelectMany(group => group.UnitsInGroup).ToArray();
        }

        public CombatSlot[] GetAllNeededSlots()
        {
            List<CombatSlot> res = new List<CombatSlot>();
            foreach (CombatGroup group in combatGroups) 
            {
                if (group.IsComplete) continue;
                foreach (CombatRoleSO unitRole in group.MissingUnits.Keys) {
                    if (group.MissingUnits[unitRole] > 0) res.Add(new CombatSlot(group, unitRole));
                }
            }
            return res.ToArray();
        }
        public void ReceiveNewUnit(SkilledCharacter unit, CombatSlot combatSlot)
        {
            combatSlot.Group.AddUnitToGroup(unit, combatSlot.Role);
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
