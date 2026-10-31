using ITF.Entity;
using MBT;
using UnityEngine;

namespace ITF.BehaviourTree.Vabriants
{

    public class CharacterVariable : Variable<Character>
    {
        protected override bool ValueEquals(Character val1, Character val2)
        {
            return val1 == val2;
        }
    }

    public class CharacterReference : VariableReference<CharacterVariable, Character>
    {
        public CharacterReference(VarRefMode mode = VarRefMode.EnableConstant)
        {
            SetMode(mode);
        }

        public CharacterReference(Character defaultConstant)
        {
            useConstant = true;
            constantValue = defaultConstant;
        }

        public Character Value
        {
            get
            {
                return (useConstant) ? constantValue : this.GetVariable().Value;
            }
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                }
                else
                {
                    this.GetVariable().Value = value;
                }
            }
        }
    }

}