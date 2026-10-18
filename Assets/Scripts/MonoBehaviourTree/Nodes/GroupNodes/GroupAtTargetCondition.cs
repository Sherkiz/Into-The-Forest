using ITF.Entity;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Conditions/GroupAtTargetCondition")]
    public class GroupAtTargetCondition : Condition
    {
        public Vector2Reference targetCell;
        public GameObjectReference host;

        public bool invert = false;
        private CombatGroup group;
        public override bool Check()
        {
            if (group == null) group = host.Value.GetComponent<CombatGroup>();
            Vector3 position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)targetCell.Value.ToVector2Int());
            bool result = true;
            foreach(Character character in group.UnitsInGroup)
            {
                if (Vector3.Distance(position, host.Value.transform.position) < 0.02f)
                {
                    result = false;
                    break;
                }
            }
            return invert ? !result : result;
        }
    }

}