using System;
using UnityEngine;

namespace ITF.Entity
{

    public abstract class EnemyGroup
    {
        public abstract Guid Guid { get; }

        public abstract void AddCharacter(Character character);

        public abstract void RemoveCharacter(Character character);
    }

}