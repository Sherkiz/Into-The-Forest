using ITF.BehaviourTree.Vabriants;
using ITF.Entity;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Conditions/DetectWorkerCondition")]

    public class DetectWorkerCondition : Condition, IMonoBehaviourTreeTickListener
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

        public override bool Check() => detected;

        public void OnBehaviourTreeTick()
        {
            if(detectionTimer <= Time.time - detectionInterval)
            {
                detectionTimer = Time.time;
                var target = Detect();
                if(target != null)
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

        Character Detect()
        {
            var characters = WorldManager.GetSubareaObjectWithin<Character>(character.transform.position, character.CurrentState.GetState(CharacterStateType.DetectionRange));
            foreach(var character in characters)
            {
                if(character != null && character is not Enemy && character.gameObject.activeSelf)
                {
                    return (Character)character;
                }
            }
            return null;
        }
    }
}
