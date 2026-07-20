using ITF.Entity;
using ITF.Inputs;
using ITF.World;
using MBT;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "ITF/Roam")]
    public class PlayerControll : Leaf
    {

        public GameObjectReference host;
        public Vector2Reference targetPosition;

        InputControls inputAction;

        Character character;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            
            inputAction ??= new InputControls();
            inputAction.Player.Move.performed += OnMovePerformed;
            inputAction.Enable();
        }

        public override void OnExit()
        {
            inputAction.Player.Move.performed -= OnMovePerformed;
        }

        public override NodeResult Execute()
        {
            //To move

            return NodeResult.running;
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            Vector2Int targetCell = targetPosition.Value.ToVector2Int();
            Vector3 targetWorldPos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)targetCell);
            if(Vector3.Distance(targetWorldPos, character.transform.position) > 0.1f)
            {
                return;
            }

            Vector2 input = context.ReadValue<Vector2>();
            Vector2Int moveOffset = Vector2Int.zero;

            moveOffset.x = (int)(input.x * 1.9f);
            moveOffset.y = (int)(input.y * 1.9f);

            if(WorldManager.Map.IsPassable(targetCell + moveOffset))
            {
                targetPosition.Value = targetCell + moveOffset;
                return;
            }
            else
            {
                Vector2Int offset = new(0, moveOffset.y);
                if(WorldManager.Map.IsPassable(targetCell + offset))
                {
                    targetPosition.Value = targetCell + offset;
                    return;
                }
                else
                {
                    offset = new Vector2Int(moveOffset.x, 0);
                    if(WorldManager.Map.IsPassable(targetCell + offset))
                    {
                        targetPosition.Value = targetCell + offset;
                        return;
                    }
                }
            }
        }
    }

}