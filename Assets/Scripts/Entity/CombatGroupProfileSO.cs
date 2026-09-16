using AYellowpaper.SerializedCollections;
using System.Linq;
using UnityEngine;

namespace ITF.Entity
{
    [CreateAssetMenu(fileName = "CombatGroup", menuName = "ITF/Entity/CombatGroupProfile")]
    public class CombatGroupProfileSO : ScriptableObject
    {
        public string groupName;
        [Tooltip("Priority for training and replacement. Ranges from 1 (highest priority) to 100 (lowest priority)"), Range(1, 100)]
        public int priority;
        [SerializedDictionary("Unit Class", "Number")]
        public SerializedDictionary<UnitClass, int> neededClasses;
        public int NumberOfUnits => neededClasses.Values.Sum();
    }
}