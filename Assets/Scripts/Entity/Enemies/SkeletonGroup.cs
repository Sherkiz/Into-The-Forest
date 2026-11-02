using ITF.BehaviourTree.Vabriants;
using ITF.Utilities;
using MBT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    public class SkeletonGroup : EnemyGroup
    {
        Guid guid;
        public override Guid Guid => guid;

        Dictionary<Character, CharacterUnit> characters;

        public SkeletonGroup()
        {
            guid = Guid.NewGuid();
            characters = new();
        }

        public SkeletonGroup(Guid guid)
        {
            this.guid = guid;
            characters = new();
        }

        public override void AddCharacter(Character character)
        {
            CharacterVariable detectionTarget = null;
            CharacterVariable attackTarget = null;
            if(character.TryGetComponentInReferences<Blackboard>(GlobalConstants.BLACKBOARD, out var blackboard))
            {
                detectionTarget = blackboard.GetVariable<CharacterVariable>(GlobalConstants.DETECTION_TARGET);
                detectionTarget.AddListener(OnDetectionTargetChanged);
                attackTarget = blackboard.GetVariable<CharacterVariable>(GlobalConstants.ATTACK_TARGET);
            }
            characters.Add(character, new CharacterUnit()
            {
                detectionTarget = detectionTarget,
                attackTarget = attackTarget
            });
            character.OnDeinited.AddListener(OnCharacterDeinit);
        }

        public override void RemoveCharacter(Character character)
        {
            if(characters.TryGetValue(character, out var characterUnit))
            {
                if(characterUnit.detectionTarget != null)
                {
                    characterUnit.detectionTarget.RemoveListener(OnDetectionTargetChanged);
                }
                characters.Remove(character);
                character.OnDeinited.RemoveListener(OnCharacterDeinit);
            }
        }

        void OnCharacterDeinit(Character character)
        {
            RemoveCharacter(character);
        }

        void OnDetectionTargetChanged(Character oldValue, Character newValue)
        {
            if(newValue != null)
            {
                foreach(var pair in characters)
                {
                    pair.Value.attackTarget.Value = newValue;
                }
            }
        }

        class CharacterUnit
        {
            public CharacterVariable detectionTarget;
            public CharacterVariable attackTarget;
        }
    }

}