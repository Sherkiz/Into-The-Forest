using ITF.Entity;
using ITF.Utilities;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "ITF/Assemble")]
    public class Assemble : Leaf
    {
        public GameObjectReference host;
        public Vector2Reference rallyCell;

        CombatGroup group;
        private Vector3 targetWorldPosition;
        public override void OnEnter()
        {
            if(group == null) group = host.Value.GetComponent<CombatGroup>();
            foreach (Character character in group.UnitsInGroup)
            {
                character.SetTargetCell(rallyCell.Value.ToVector2Int(), placeholder: true); 
                character.TryGetComponentInReferences<Blackboard>(GlobalConstants.BLACKBOARD, out var blackboard);
                blackboard.SetValue<BoolVariable, bool>(GlobalConstants.RESTING, false);
            }
            targetWorldPosition = WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)rallyCell.Value.ToVector2Int());
            targetWorldPosition += (Vector3)Vector2.one * 0.5f;
        }

        public override NodeResult Execute()
        {
            foreach(Character character in group.UnitsInGroup)
            {
                if (character.transform.position != targetWorldPosition)
                return NodeResult.From(Status.Running);
            }
            return NodeResult.From(Status.Success);
        }
    }

}