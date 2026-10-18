using ITF.Entity;
using ITF.World;
using ITF.WorldObjects;
using MBT;
using System.Linq;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "ITF/FindSpawner")]
    public class FindSpawner : Leaf
    {
        public GameObjectReference host;
        public FloatReference searchTime;
        public Vector2Reference targetPosition;

        CombatGroup group;
        private SpawnerMapObject spawner;
        private Vector3 targetWorldPosition;
        private float remainingSearchTime;
        public override void OnEnter()
        {
            SpawnerMapObject[] spawners = WorldManager.Map.GetMapObjects<SpawnerMapObject>();
            spawner = spawners.OrderBy(s => (s.EntrancePosition - group.CenterCell).magnitude).FirstOrDefault();
            targetPosition.Value = spawner.EntrancePosition;
        }
        public override NodeResult Execute()
        {
            
            return NodeResult.From(Status.Success);
        }
    }

}