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

        Character character;
        ResultPath path;
        int pathIndex;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            Vector2Int startCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
            path = WorldManager.Map.FindPath(startCell, targetCell.Value.ToVector2Int());
            pathIndex = 0;
        }

        public override NodeResult Execute()
        {
            if(path.path == null || pathIndex >= path.path.Count)
            {
                return NodeResult.From(Status.Failure);
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

            return NodeResult.From(Status.Running);
        }
    }

}