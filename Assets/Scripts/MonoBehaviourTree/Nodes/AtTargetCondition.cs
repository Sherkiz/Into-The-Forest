using ITF.Entity;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Conditions/AtTargetCondition")]
    public class AtTargetCondition : Condition
    {
        public Vector2Reference targetCell;
        public GameObjectReference host;

        public bool invert = false;

        public override bool Check()
        {
            Vector3 position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)targetCell.Value.ToVector2Int());
            bool result = Vector3.Distance(position, host.Value.transform.position) < 0.02f;
            return invert ? !result : result;
        }
    }

}