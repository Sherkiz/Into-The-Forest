using ITF.Entity;
using ITF.Navigation;
using ITF.Utilities;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "ITF/Roam")]
    public class Roam : Leaf
    {
        public GameObjectReference host;
        public Vector2Reference targetPosition;
        public Vector2Reference startCell;

        public float maxAngle = 360;
        public float minDistance = 3;
        public float maxDistance = 10;
        public Vector2 stopInterval = new(1, 3);
        public LayerMask layerMask;

        private float stopTimer = 0;
        private Character character;
        private ResultPath path;
        private int pathIndex = 0;

        public override void OnEnter()
        {
            stopTimer = 0;
            if(character == null) character = host.Value.GetComponent<Character>();
        }

        public override NodeResult Execute()
        {
            if(stopTimer > 0)
            {
                stopTimer -= Time.deltaTime;
                if(stopTimer <= 0)
                {
                    stopTimer = 0;
                    targetPosition.Value = FindNextPosition();
                    Vector2Int startCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
                    this.startCell.Value = startCell;
                    path = WorldManager.Map.FindPath(startCell, targetPosition.Value.ToVector2Int());
                    pathIndex = 0;
                }
            }
            else
            {
                if(path.path == null || pathIndex >= path.path.Count)
                {
                    stopTimer = Random.Range(stopInterval.x, stopInterval.y);
                    return NodeResult.running;
                }

                //move to target position
                float speed = character.CurrentState.GetState(CharacterStateType.Speed);
                Vector3 targetWorldPosition = WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)path.path[pathIndex]);
                targetWorldPosition += (Vector3)Vector2.one * 0.5f; //center of the tile
                character.transform.position = Vector2.MoveTowards(character.transform.position, targetWorldPosition, speed * Time.deltaTime);
                if (character.transform.position == targetWorldPosition)
                {
                    pathIndex++;
                }
            }

            return NodeResult.running;
        }

        Vector2Int FindNextPosition()
        {
            var angle = Random.Range(0, maxAngle);
            Vector2 direction = host.GetVariable().transform.right;
            direction = StaticTools.RotateVector2(direction, angle);
            float distance = Random.Range(minDistance, maxDistance);
            return GetFarestPosition((Vector2)host.Value.transform.position, direction, distance);
        }

        Vector2Int GetFarestPosition(Vector2 startPosition, Vector2 direction, float maxDistance)
        {
            Vector3Int startCell = WorldManager.Map.PathfindingTilemap.WorldToCell(new((int)startPosition.x, (int)startPosition.y, 0));
            Vector2Int farestCell = GetFarestPassableTile(new(startCell.x, startCell.y), direction, maxDistance);
            return farestCell;
        }

        static Vector2Int GetFarestPassableTile(Vector2Int startCell, Vector2 direction, float maxDistance)
        {
            Vector2 targetPosition = startCell + direction * maxDistance;
            Vector2Int lastCell = startCell;
            Vector2Int cell = new((int)targetPosition.x, (int)targetPosition.y);
            for(float distance = 0; distance < maxDistance && WorldManager.Map.IsPassable(cell); distance += 1f)
            {
                lastCell = cell;
                do
                {
                    targetPosition += direction;
                    cell = new((int)targetPosition.x, (int)targetPosition.y);
                } while (cell == lastCell);
            }
            return lastCell;
        }

        private void OnDrawGizmos()
        {
            if(path.path != null)
            {
                if(WorldManager.Instance == null || WorldManager.Map == null) return;

                Gizmos.color = Color.green;
                Vector2Int start = startCell.Value.ToVector2Int();
                for (int i = 0; i < path.path.Count; i++)
                {
                    Vector2Int end = path.path[i];
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)start) + Vector3.one * .5f, 
                        WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)end) + Vector3.one * .5f);
                    start = end;
                }

                Gizmos.color = Color.red;
                Gizmos.DrawLine(WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)startCell.Value.ToVector2Int()) + Vector3.one * .5f, 
                    WorldManager.Map.PathfindingTilemap.CellToWorld((Vector3Int)targetPosition.Value.ToVector2Int()) + Vector3.one * .5f);
            }
        }
    }

}