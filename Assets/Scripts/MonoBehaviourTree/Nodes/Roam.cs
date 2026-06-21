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

        public float maxAngle = 360;
        public float minDistance = 3;
        public float maxDistance = 10;
        public Vector2 stopInterval = new(1, 3);

        private float stopTimer = 0;
        private Character character;
        private ResultPath path;
        private int pathIndex = 0;

        public override void OnEnter()
        {
            stopTimer = 0;
            if(character == null) character = host.GetVariable().GetComponent<Character>();
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
                    path = WorldManager.Map.FindPath(character.transform.position.RoundToVector2Int(), targetPosition.Value.ToVector2Int());
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
                float speed = character.CurrentState.GetAttribute(CharacterAttributeType.Speed);
                character.transform.position = Vector2.MoveTowards(character.transform.position, path.path[pathIndex], speed * Time.deltaTime);
                if((Vector2)character.transform.position == path.path[pathIndex])
                {
                    pathIndex++;
                }
            }

            return NodeResult.running;
        }

        Vector2 FindNextPosition()
        {
            var angle = Random.Range(0, maxAngle);
            Vector2 direction = host.GetVariable().transform.right;
            direction = StaticTools.RotateVector2(direction, angle);
            float distance = Random.Range(minDistance, maxDistance);
            Vector2 targetPosition = GetFarestPosition((Vector2)host.Value.transform.position, direction, distance);
            targetPosition = new((int)targetPosition.x + 0.5f, (int)targetPosition.y + 0.5f);
            return targetPosition;
        }

        Vector2 GetFarestPosition(Vector2 startPosition, Vector2 direction, float maxDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxDistance);
            Vector2 targetPosition;
            if (hit.collider != null)
            {
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = startPosition + direction * maxDistance;
            }

            Vector2Int tilePos = new((int)targetPosition.x, (int)targetPosition.y);
            while(Vector2.Distance(tilePos, startPosition) >= 1 && WorldManager.Map.GetTileOnPathfingTilemap(tilePos) != null)
            {
                targetPosition -= direction;
                tilePos = new((int)targetPosition.x, (int)targetPosition.y);
            }

            return targetPosition;
        }
    }

}