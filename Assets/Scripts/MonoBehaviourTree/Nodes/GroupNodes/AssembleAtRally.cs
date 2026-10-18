using ITF.Entity;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "ITF/AssembleAtRally")]
    public class AssembleAtRally : Leaf
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