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
        [Tooltip("The interval(s) of the workers to shift"), SerializeField]
        float shiftInterval = 60f;

        Character[] workers;
        Character[] shiftAWorkers;
        Character[] shiftBWorkers;
        bool isAWorking;

        SimpleTimer shiftATimer;
        SimpleTimer shiftBTimer;

        void Start()
        {
            shiftATimer = new(shiftInterval, _ =>
            {
                ShiftWorkers(shiftAWorkers, shiftBWorkers);
                isAWorking = true;
                shiftBTimer.timer = 0;
                TimeManager.AddSimpleTimer(shiftBTimer);
            });
            shiftBTimer = new(shiftInterval, _=>
            {
                ShiftWorkers(shiftBWorkers, shiftAWorkers);
                isAWorking = false;
                shiftATimer.timer = 0;
                TimeManager.AddSimpleTimer(shiftATimer);
            });

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
                int aWorkerCount = workerCount / 2;
                int bWorkerCount = workerCount - aWorkerCount;

                workers = new Character[workerCount];
                shiftAWorkers = new Character[aWorkerCount];
                shiftBWorkers = new Character[bWorkerCount];
                for (int i = 0; i < workerCount; i++)
                {
                    Character worker = GameObjectPool.CreateGameObject(workerPrefab.gameObject).GetComponent<Character>();
                    Vector2Int randomCell = new(Random.Range(spawnRange.xMin, spawnRange.xMax), Random.Range(spawnRange.yMin, spawnRange.yMax));
                    WorldManager.Map.GetNearestPassableCell(randomCell, spawnRange, out Vector2Int spawnCell);
                    GameObject blackboardGo = worker.GetReference("blackboard");
                    Blackboard blackboard = null;
                    if (blackboardGo) blackboard = blackboardGo.GetComponent<Blackboard>();
                    if(blackboard != null) blackboard.GetVariable<Vector2Variable>("spawn_cell").Value = spawnCell;

                    worker.name = $"Worker_{i}";
                    workers[i] = worker;
                    worker.transform.position = WorldManager.Map.PathfindingTilemap.GetCellCenterWorld((Vector3Int)spawnCell);
                    worker.Init();

                    if(i < aWorkerCount)
                    {
                        shiftAWorkers[i] = worker;
                    }
                    else
                    {
                        int index = i - aWorkerCount; 
                        shiftBWorkers[index] = worker;
                    }
                }
                ShiftWorkers(shiftAWorkers, shiftBWorkers);
            }
            isAWorking = true;
            shiftBTimer.timer = 0;
            TimeManager.AddSimpleTimer(shiftBTimer);
            WorldManager.Map.onBuilt -= OnMapBuilt;
        }

        void ShiftWorkers(Character[] workingWorkers, Character[] restWorkers)
        {
            foreach (var worker in restWorkers)
            {
                Blackboard blackboard = worker.GetReference("blackboard")?.GetComponent<Blackboard>();
                if(blackboard != null)
                {
                    blackboard.GetVariable<Vector2Variable>("target_cell").Value = blackboard.GetVariable<Vector2Variable>("spawn_cell").Value;
                    blackboard.GetVariable<BoolVariable>("resting").Value = true;
                }
            }

            MapObject[] goldenStones = WorldManager.Map.GetMapObjectsByName("gold_stones");
            if(goldenStones.Length > 0) {                 
                Vector2 targetPosition = goldenStones[0].range.min;
                for (int i = 0; i < workingWorkers.Length; i++)
                {
                    var worker = workingWorkers[i];
                    Blackboard blackboard = worker.GetReference("blackboard")?.GetComponent<Blackboard>();
                    if(blackboard != null)
                    {
                        targetPosition += i switch
                        {
                            0 => new Vector2(0, -1),
                            1 => new Vector2(-1, 0),
                            2 => new Vector2(0, 2),
                            3 => new Vector2(2, 0),
                            _ => new Vector2(0, -1),
                        };
                        blackboard.GetVariable<Vector2Variable>("target_cell").Value = targetPosition;
                        blackboard.GetVariable<BoolVariable>("resting").Value = false;
                    }
                }
            }
        }
    }

}