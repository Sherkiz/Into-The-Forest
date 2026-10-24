using ITF.Entity;
using ITF.Navigation;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/MoveToTarget")]
    public class MoveToTarget : Leaf
    {
        public GameObjectReference host;
        public Vector2Reference targetCell;
        public bool updateSubarea = false;

        Character character;
        ResultPath path;
        int pathIndex;
        Vector2 lastPosition;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            Vector2Int startCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
            path = WorldManager.Map.FindPath(startCell, targetCell.Value.ToVector2Int(), true);
            pathIndex = 0;
            lastPosition = character.transform.position;
        }

        public override NodeResult Execute()
        {
            if(path.path == null || pathIndex >= path.path.Count)
            {
                return NodeResult.From(Status.Failure);
            }

            if (targetCell.Value != path.path[^1])
            {
                Vector2Int startCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
                path = WorldManager.Map.FindPath(startCell, targetCell.Value.ToVector2Int());
                pathIndex = 0;
                return NodeResult.From(Status.Running);
            }

            float speed = character.CurrentState.GetState(CharacterStateType.Speed);
            Vector3 targetPos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)path.path[pathIndex]);

            character.transform.position = Vector3.MoveTowards(character.transform.position, targetPos, speed * Time.deltaTime);

            if(Vector3.Distance(character.transform.position, targetPos) < 0.02f)
            {
                pathIndex++;
                if(pathIndex >= path.path.Count)
                {
                    return NodeResult.From(Status.Success);
                }
            }

            if(Vector2.Distance(character.transform.position, lastPosition) > .5f)
            {
                WorldManager.UpdateSubareaObject(character, lastPosition);
                lastPosition = character.transform.position;
            }

            return NodeResult.From(Status.Running);
        }

        public override void OnExit()
        {
            if(updateSubarea)
            {
                WorldManager.UpdateSubareaObject(character, lastPosition);
            }
        }

        private void OnDrawGizmos()
        {
            if(path.path != null)
            {
                Gizmos.color = Color.green;
                Vector3 startPos = host.Value.transform.position;
                for (int i = pathIndex; i < path.path.Count - 1; i++)
                {
                    Vector3 endPos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)path.path[i + 1]);
                    Gizmos.DrawLine(startPos, endPos);

                    startPos = endPos;
                }
            }
        }
    }

}