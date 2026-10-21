using System;
using UnityEngine;

namespace ITF.Entity
{

    public class SkeletonGroup : EnemyGroup
    {
        Guid guid;
        public override Guid Guid => guid;

        public SkeletonGroup()
        {
            guid = Guid.NewGuid();
        }

        public SkeletonGroup(Guid guid)
        {
            this.guid = guid;
        }

        public override void AddCharacter(Character character)
        {
            
        }

        public override void RemoveCharacter(Character character)
        {
            throw new NotImplementedException();
        }
    }

}