using ITF.Entity;
using ITF.Utilities;
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
        public override NodeResult Execute()
        {
            if (group == null) group = host.Value.GetComponent<CombatGroup>();
            foreach (Character character in group.UnitsInGroup)
            {
                character.TryGetComponentInReferences<Blackboard>(GlobalConstants.BLACKBOARD, out var blackboard);
                blackboard.SetValue<BoolVariable, bool>(GlobalConstants.RESTING, true);
            }
            return NodeResult.From(Status.Success);
        }
    }

}