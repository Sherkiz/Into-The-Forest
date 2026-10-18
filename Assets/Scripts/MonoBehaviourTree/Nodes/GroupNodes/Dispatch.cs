using ITF.Entity;
using ITF.World;
using ITF.WorldObjects;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Nodes
{
    [AddComponentMenu("")]
    [MBTNode(name = "ITF/Dispatch")]
    public class Dispatch : Leaf
    {
        public GameObjectReference host;

        CombatGroup group;
        public override void OnEnter()
        {
            
        }

        public override NodeResult Execute()
        {
            
            return NodeResult.From(Status.Success);
        }
    }

}