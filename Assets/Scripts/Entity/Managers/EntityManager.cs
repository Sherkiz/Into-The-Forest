using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    public class EntityManager : MonoBehaviour
    {
        static EntityManager instance;
        public static EntityManager Instance => instance;

        [Tooltip("The sub entity managers"), SerializeField]
        GameObject[] managers;
        Dictionary<Type, IEntityManager> subEntityManagers;

        #region public methods

        public static IEntityManager GetSubEntityManager<T>() where T : IEntityManager
        {
            if (Instance.subEntityManagers.TryGetValue(typeof(T), out IEntityManager manager))
            {
                return manager;
            }
            return null;
        }

        #endregion

        #region private methods

        private void Awake()
        {
            if (instance == null) instance = this;
            subEntityManagers = new Dictionary<Type, IEntityManager>();
            foreach (var manager in managers)
            {
                IEntityManager entityManager = manager.GetComponent<IEntityManager>();
                if (entityManager != null)
                    subEntityManagers.Add(entityManager.GetType(), entityManager);
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        #endregion
    }

}