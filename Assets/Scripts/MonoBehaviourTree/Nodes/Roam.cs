using ITF.Entity;
using ITF.Navigation;
using ITF.Utilities;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/Roam")]
    public class Roam : Leaf
    {
        public GameObjectReference host;
        public Vector2Reference targetCell;
        public Vector2Reference startCell;

        public float maxAngle = 360;
        public float minDistance = 3;
        public float maxDistance = 10;
        public bool limitRange = true;
        [Tooltip("The range within which the character is allowed to roam. \n Only used if Limit Range is enabled.")]
        public RectInt roamRange = new(5, 5, 30, 30);

        private Character character;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
        }

        public override NodeResult Execute()
        {
            targetCell.Value = FindNextPosition();

            return NodeResult.success;
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
            Vector3Int startCell = WorldManager.Map.PathfindingTilemap.WorldToCell(startPosition);
            RectInt range;
            if(limitRange)
            {
                range = roamRange;
            }
            else
            {
                var bounds = WorldManager.Map.PathfindingTilemap.cellBounds;
                range = new RectInt(bounds.xMin, bounds.yMin, bounds.size.x, bounds.size.y);
            }
            Vector2Int farestCell = GetFarestPassableTile(new(startCell.x, startCell.y), direction, maxDistance, range);
            return farestCell;
        }

        static Vector2Int GetFarestPassableTile(Vector2Int startCell, Vector2 direction, float maxDistance, RectInt range)
        {
            Vector2 targetPosition = startCell;
            Vector2Int lastCell = startCell;
            Vector2Int cell = new((int)targetPosition.x, (int)targetPosition.y);
            for(float distance = 0; distance < maxDistance && WorldManager.Map.IsPassable(cell); distance += 1f)
            {
                if(!range.Contains(cell)) break;
                lastCell = cell;
                do
                {
                    targetPosition += direction;
                    cell = new((int)targetPosition.x, (int)targetPosition.y);
                } while (cell == lastCell);
            }
            return lastCell;
        }
    }

}