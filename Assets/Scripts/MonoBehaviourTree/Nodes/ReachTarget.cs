using ITF.Entity;
using ITF.Navigation;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "ITF/ReachTarget")]
    public class ReachTarget : Leaf
    {
        public GameObjectReference host;
        public Vector2Reference targetPosition;
        public Vector2Reference startCell;

        public float maxAngle = 360;
        public float minDistance = 3;
        public float maxDistance = 10;
        public Vector2 stopInterval = new(1, 3);
        public LayerMask layerMask;

        private Character character;
        private ResultPath path;
        private int pathIndex = 0;
        private Vector2Int currentCell { get => (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position); }

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            GetPath();
        }

        public override NodeResult Execute()
        {
            if (currentCell == targetPosition.Value)
            {
                return NodeResult.success;
            }
            if (path.path == null)
            {
                //move to target position
                float speed = character.CurrentState.GetState(CharacterStateType.Speed);
                Vector3 targetWorldPosition = WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)path.path[pathIndex]);
                targetWorldPosition += (Vector3)Vector2.one * 0.5f; //center of the tile
                character.transform.position = Vector2.MoveTowards(character.transform.position, targetWorldPosition, speed * Time.deltaTime);
                if (character.transform.position == targetWorldPosition)
                {
                    pathIndex++;
                }
                return NodeResult.running;
            }
            else
            {
                return NodeResult.failure;
            }
        }
        private ResultPath GetPath()
        {
            Vector2Int startCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
            this.startCell.Value = startCell;
            path = WorldManager.Map.FindPath(startCell, targetPosition.Value.ToVector2Int());
            pathIndex = 0;
            return path;
        }
    }

}