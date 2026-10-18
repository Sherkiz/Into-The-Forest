using ITF.Entity;
using ITF.World;
using ITF.WorldObjects;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "ITF/FindSpawner")]
    public class FindSpawner : Leaf
    {
        public GameObjectReference host;
        public FloatReference searchTime;

        CombatGroup group;
        private SpawnerMapObject spawner;
        private Vector3 targetWorldPosition;
        private float remainingSearchTime;
        public override void OnEnter()
        {
            
        }

        public override NodeResult Execute()
        {
            
            return NodeResult.From(Status.Success);
        }
    }

}