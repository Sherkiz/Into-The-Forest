using ITF.Skill.Passive;
using UnityEngine;

namespace ITF.Entity
{
    [CreateAssetMenu(fileName = "CombatGroup", menuName = "ITF/Entity/CombatGroupProfile")]
    public class CombatGroupProfileSO : ScriptableObject
    {
        public string groupName;
        public PassiveSkill[] neededSkills;
        public int NumberOfUnits => neededSkills.Length;
    }
}