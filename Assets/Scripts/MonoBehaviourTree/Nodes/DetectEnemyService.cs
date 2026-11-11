using ITF.BehaviourTree.Vabriants;
using ITF.Entity;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Conditions/DetectWorkerCondition")]

    public class DetectEnemyService : Service
    {
        public float detectionInterval = .2f;
        public bool invert = false;
        GameObjectReference host;
        CharacterReference detectionTarget;
        Character character;
        float detectionTimer;
        bool detected = false;

        public override void OnEnter()
        {
            if(character == null) character = host.Value.GetComponent<Character>();
            detectionTimer = 0;
        }

        public override void Task()
        {
            if (detectionTimer <= Time.time - detectionInterval)
            {
                detectionTimer = Time.time;
                var target = Detect();
                if (target != null)
                {
                    detectionTarget.Value = target;
                    detected = true;
                }
                else
                {
                    detected = false;
                }
            }
        }

        private Enemy Detect()
        {
            var enemies = WorldManager.GetSubareaObjectWithin<Enemy>(character.transform.position, character.CurrentState.GetState(CharacterStateType.DetectionRange));
            foreach(var en in enemies)
            {
                if(en != null && en.gameObject.activeSelf)
                {
                    return (Enemy) en;
                }
            }
            return null;
        }
    }
}
