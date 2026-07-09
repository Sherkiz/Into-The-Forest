using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ITF.Utilities
{
    [DefaultExecutionOrder(-15)]
    public class GameObjectPool : MonoBehaviour
    {
        static GameObjectPool instance;
        public static GameObjectPool Instance => instance;

        //Store all unused objects, key is the prefab instance id, value is a queue of unused objects
        Dictionary<int, Queue<GameObject>> unuses = new Dictionary<int, Queue<GameObject>>();

        //Store all used objects, key is the object instance id, value is the object itself
        Dictionary<int, GameObject> usings = new Dictionary<int, GameObject>();

        //Record the prefab instance id of each used object, key is the object instance id, value is the prefab instance id
        Dictionary<int, int> usingIDs = new Dictionary<int, int>();

        //Parent transform for all unused objects
        Transform tsUnuses;

        private void Awake()
        {
            if(instance == null) instance = this;

            tsUnuses = transform.Find("Unuses");
            if (tsUnuses == null)
            {
                tsUnuses = new GameObject("Unuses").transform;
                tsUnuses.parent = transform;
                tsUnuses.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            //Destroy all objects
            foreach (var go in usings)
            {
                Destroy(go.Value);
            }
        }

        public static GameObject CreateGameObject(GameObject prefab, string name = null)
        {
            int id = prefab.GetInstanceID();
            var unuses = Instance.unuses;
            if (!unuses.ContainsKey(id))
            {
                unuses.Add(id, new Queue<GameObject>());
            }

            GameObject go;
            if(unuses[id].Count == 0) go = Instantiate(prefab);
            else go = unuses[id].Dequeue();

            if (name != null) go.name = name;
            if (go.scene != instance.gameObject.scene) SceneManager.MoveGameObjectToScene(go, instance.gameObject.scene);
            go.transform.SetParent(null);
            instance.usings.Add(go.GetInstanceID(), go);
            instance.usingIDs.Add(go.GetInstanceID(), id);

            return go;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Return true if recycle success</returns>
        public static bool RecycleGameObject(GameObject target)
        {
            int id = target.GetInstanceID();
            if(instance.usings.TryGetValue(id, out GameObject go))
            {
                go.SetActive(false);
                target.transform.SetParent(instance.tsUnuses);

                instance.usings.Remove(id);
                instance.unuses[instance.usingIDs[id]].Enqueue(target);
                instance.usingIDs.Remove(id);

                return true;
            }

            return false;
        }
    }

}