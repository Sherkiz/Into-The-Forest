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

        public bool inverterCondition = false;

        public override bool Check()
        {
            Vector3 position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)targetCell.Value.ToVector2Int());
            bool result = Vector3.Distance(position, transform.position) < 0.02f;
            return inverterCondition ? !result : result;
        }
    }

}