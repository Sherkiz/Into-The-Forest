using ITF.Utilities;
using ITF.World;
using MBT;
using UnityEngine;

namespace ITF.Entity
{

    public class WorkerManager : MonoBehaviour, IEntityManager
    {
        [Tooltip("The worker prefab"), SerializeField]
        Character workerPrefab;
        [Tooltip("The count of workers to spawn"), SerializeField]
        int workerCount = 8;
        [Tooltip("The range of the workers to spawn"), SerializeField]
        RectInt spawnRange = new RectInt(0, 0, 30, 30);

        Character[] workers;

        void Start()
        {
            WorldManager.OnWorldGenerated.AddListener(OnWorldGenerated);
        }

        void OnWorldGenerated()
        {
            WorldManager.Map.onBuilt += OnMapBuilt;
        }

        void OnMapBuilt(Map map)
        {
            if (workers != null)
            {
                foreach (var worker in workers)
                {
                    if (worker != null)
                    {
                        GameObjectPool.RecycleGameObject(worker.gameObject);
                    }
                }
            }
            else workers = new Character[workerCount];

            if (workerCount > 0 && workerPrefab != null)
            {
                workers = new Character[workerCount];
                for (int i = 0; i < workerCount; i++)
                {
                    Character worker = GameObjectPool.CreateGameObject(workerPrefab.gameObject).GetComponent<Character>();
                    Vector2Int randomCell = new(Random.Range(spawnRange.xMin, spawnRange.xMax), Random.Range(spawnRange.yMin, spawnRange.yMax));
                    WorldManager.Map.GetNearestPassableCell(randomCell, spawnRange, out Vector2Int spawnCell);
                    worker.name = $"Worker_{i}";
                    workers[i] = worker;
                    worker.transform.position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)spawnCell);
                    worker.Init();

                    if(i < 4)
                    {
                        GameObject blackboardGo = worker.GetReference("blackboard");
                        if(blackboardGo != null)
                        {
                            Blackboard blackboard = blackboardGo.GetComponent<Blackboard>();
                            MapObject[] goldenStones = WorldManager.Map.GetMapObjectsByName("gold_stones");
                            Debug.Log($"gold stone count: {goldenStones.Length}");
                            if (goldenStones.Length > 0)
                            {
                                Vector2 targetPosition = goldenStones[0].range.min;
                                targetPosition += i switch
                                {
                                    0 => new Vector2(0, -1),
                                    1 => new Vector2(-1, 0),
                                    2 => new Vector2(0, 2),
                                    3 => new Vector2(2, 0),
                                    _ => new Vector2(0, -1),
                                };
                                Vector2Variable targetCell = blackboard.GetVariable<Vector2Variable>("target_cell");
                                if (targetCell != null)
                                {
                                    targetCell.Value = targetPosition;
                                }
                            }
                        }
                    }
                }
            }
            WorldManager.Map.onBuilt -= OnMapBuilt;
        }
    }

}