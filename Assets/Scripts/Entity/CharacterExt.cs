using MBT;
using UnityEngine;
using ITF.Utilities;
using ITF.BehaviourTree;
using ITF.World;

namespace ITF.Entity
{

    public static class CharacterExt
    {
        public static void SetTargetCell(this Character character, Vector2Int cell, bool placeholder = false)
        {
            if(character != null)
            {
                if(character.TryGetComponentInReferences<Blackboard>(GlobalConstants.BLACKBOARD, out var blackboard))
                {
                    blackboard.SetValue<Vector2Variable, Vector2>(GlobalConstants.TARGET_CELL, cell);
                    if(placeholder) WorldManager.Map.RegisterUnitPlaceholder(character, cell);
                }
            }
        }
    }

}