using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{
    public interface IReferencer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The unique name corresponding to the reference object</param>
        /// <returns>If the object corresponding to the name cannot be found, returns null</returns>
        public GameObject GetReference(string name);

        /// <summary>
        /// Sets or adds a reference object with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="reference"></param>
        public void SetOrAddReference(string name, GameObject reference);
    }

}