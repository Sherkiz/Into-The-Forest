using ITF.Utilities;
using ITF.World;
using UnityEngine;

namespace ITF.Entity
{

    public class WorkerManager : MonoBehaviour, IEntityManager
    {
        [Tooltip("The worker prefab"), SerializeField]
        Character workerPrefab;
        [Tooltip("The count of workers to spawn"), SerializeField]
        int workerCount = 8;

        Character[] workers;

        void Start()
        {
            WorldManager.Map.onBuilt += OnMapBuilt;
        }

        void OnDestroy()
        {
            WorldManager.Map.onBuilt -= OnMapBuilt;
        }

        void OnMapBuilt(Map map)
        {
            if (workers != null)
            {
                if(workers != null)
                {
                    foreach (var worker in workers)
                    {
                        if (worker != null)
                        {
                            GameObjectPool.RecycleGameObject(worker.gameObject);
                        }
                    }
                }

                if (workerCount > 0 && workerPrefab != null)
                {
                    workers = new Character[workerCount];
                    for (int i = 0; i < workerCount; i++)
                    {
                        Character worker = GameObjectPool.CreateGameObject(workerPrefab.gameObject).GetComponent<Character>();
                        worker.name = $"Worker_{i}";
                        workers[i] = worker;
                    }
                }
            }
        }
    }

}