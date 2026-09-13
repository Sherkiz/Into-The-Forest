using ITF.Skill.Passive;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    public class CombatGroupManager : MonoBehaviour, IEntityManager
    {
        private CombatGroupProfileSO[] requiredGroups;
        private List<CombatGroup> currentCombatGroups;

        public Character[] GetCharacters()
        {
            return currentCombatGroups.SelectMany(group => group.UnitsInGroup).ToArray();
        }
        public PassiveSkill[] GetAllNeededSkills()
        {
            throw new System.NotImplementedException(); // Needs to go through requiredGroups, check if they are fullfilled and return all non-fullfilled requirements
        }
        public void ReceiveNewUnit(SkilledCharacter unit)
        {
            currentCombatGroups[0].AddUnitToGroup(unit);
        }
    }
}
