using ITF.BehaviourTree.Vabriants;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "Services/PurseCharacter")]
    public class PursueCharacter : Service
    {
        public float updateInterval = .2f;
        public CharacterReference attackTarget;
        public Vector2Reference targetCell;

        float updateTimer;

        public override void Task()
        {
            if (updateTimer > Time.time - updateInterval || attackTarget.Value == null || !attackTarget.Value.Inited) return;
            updateTimer = Time.time;

            Vector2Int cell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(attackTarget.Value.transform.position);
            WorldManager.Map.GetNearestEmptyCell(cell, out cell);
            targetCell.Value = cell;
        }
    }

}