using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/RandomWait")]

    public class RandomWait : Leaf
    {
        public float minWaitTime = 1f;
        public float maxWaitTime = 5f;

        float interval;
        float timer = 0;

        public override void OnEnter()
        {
            interval = Random.Range(minWaitTime, maxWaitTime);
            timer = 0;
        }

        public override NodeResult Execute()
        {
            timer += Time.deltaTime;
            if (timer > interval) return NodeResult.success;
            else return NodeResult.running;
        }
    }
}
