using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{
    [CreateAssetMenu(fileName = "CombatRole", menuName = "ITF/Entity/CombatRole")]
    public class CombatRoleSO : ScriptableObject
    {
        public string roleName;
        public UnitClass[] unitClassesInRole;

        public void OnValidate()
        {
            HashSet<UnitClass> selectedClasses = new HashSet<UnitClass>();
            for (int i = 0; i < unitClassesInRole.Length; i++)
            {
                if (!selectedClasses.Add(unitClassesInRole[i]))
                {
                    unitClassesInRole[i] = UnitClass.None;
                }
            }
        }
    }
}