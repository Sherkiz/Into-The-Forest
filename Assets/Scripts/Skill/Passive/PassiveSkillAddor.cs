using ITF.StateMachine;
using UnityEngine;

namespace ITF.Skill.Passive
{

    //[CreateAssetMenu(fileName = "PassiveSkill", menuName = "ITF/Skill/Passive/PassiveSkill")]
    public abstract class PassiveSkillAddor : StateAddor
    {
        public abstract PassiveSkill PassiveSkill { get; }

        public sealed override void AddStates(IStateContainer stateContainer)
        {
            
        }

        public abstract PassiveSkill AddPassiveSkill(IStateContainer stateContainer);
    }

    public abstract class  PassiveSkill : SimpleStateUnit
    {
        public string ID { get; }
        public string Name { get; set; }
    }

}