using ITF.Entity;
using ITF.Inputs;
using ITF.World;
using MBT;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace ITF.BehaviourTree.Nodes
{

    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/PlayerControll")]
    public class PlayerControll : Leaf
    {

        public GameObjectReference host;
        public Vector2Reference targetPosition;

        InputControls inputAction;
        Vector2 moveInput;

        Character character;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            
            inputAction ??= new InputControls();
            inputAction.Player.Move.performed += OnMovePerformed;
            inputAction.Player.Move.canceled += OnMoveCanceled;
            inputAction.Enable();

            targetPosition.Value = (Vector3)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);
        }

        public override void OnExit()
        {
            inputAction.Player.Move.performed -= OnMovePerformed;
            inputAction.Player.Move.canceled -= OnMoveCanceled;
        }

        public override NodeResult Execute()
        {
            UpdateTargetPos();

            //To move
            Vector3 pos = character.transform.position;
            Vector3 targetPos = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)targetPosition.Value.ToVector2Int());
            pos.z = targetPos.z = 0;
            float speed = character.CurrentState.GetState(CharacterStateType.Speed);
            character.transform.position = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);

            return NodeResult.running;
        }

        void UpdateTargetPos()
        {
            Vector2Int moveOffset = Vector2Int.zero;
            moveOffset.x = (int)(moveInput.x * 1.9f);
            moveOffset.y = (int)(moveInput.y * 1.9f);

            Vector2Int currentCell = (Vector2Int)WorldManager.Map.PathfindingTilemap.WorldToCell(character.transform.position);

            if (moveOffset.x != 0)
            {
                Vector2Int targetCell = currentCell + new Vector2Int(moveOffset.x, 0);
                if (WorldManager.Map.IsPassable(targetCell))
                {
                    targetPosition.Value = targetCell;
                    return;
                }
            }

            if (moveOffset.y != 0)
            {
                Vector2Int targetCell = currentCell + new Vector2Int(0, moveOffset.y);
                if (WorldManager.Map.IsPassable(targetCell))
                {
                    targetPosition.Value = targetCell;
                    return;
                }
            }
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

}