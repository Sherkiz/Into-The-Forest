using ITF.Spawners;
using ITF.Utilities;
using ITF.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.Entity
{

    public class WorkerManager : MonoBehaviour, IEntityManager
    {
        [SerializeField]
        SpawnUnit[] spawnUnits;

        [SerializeField]
        private CombatGroupProfileSO[] requiredCombatGroups;

        int spawnUnitIndex;

        SimpleTimer spawnTimer;

        List<Character> workers = new();
        List<Character> specialists = new();

        // Sub-Managers
        [SerializeField]
        private TrainingManager trainingManager;
        private CombatGroupManager combatGroupManager;

        #region events

        public UnityEvent<Character> onUnitSpawned;

        #endregion

        #region public methods

        public Character[] GetCharacters()
        {
            return workers.ToArray();
        }

        #endregion

        void Start()
        {
            WorldManager.OnWorldGenerated.AddListener(OnWorldGenerated);
            combatGroupManager = new CombatGroupManager(requiredCombatGroups);
        }

        void OnWorldGenerated()
        {
            WorldManager.Map.onBuilt += OnMapBuilt;
            foreach(var character in workers)
            {
                character.Deinit();
            }
            spawnUnitIndex = 0;
            workers = new List<Character>();
        }

        void OnMapBuilt(Map map)
        {
            if(spawnTimer != null) TimeManager.RemoveSimpleTimer(spawnTimer);
            spawnTimer = new SimpleTimer(spawnUnits[spawnUnitIndex].delay, _ =>
            {
                WorkerSpawner spawner = Instantiate(spawnUnits[spawnUnitIndex].workerSpawner);
                Character[] characters = spawner.SpawnCharacters();
                Destroy(spawner);
                workers.AddRange(characters);
                foreach(var character in characters)
                {
                    character.Init();
                    onUnitSpawned.Invoke(character);
                }

                spawnUnitIndex++;
                if (spawnUnitIndex < spawnUnits.Length)
                {
                    _.timer = 0;
                    _.interval = spawnUnits[spawnUnitIndex].delay;
                }
            });
            TimeManager.AddSimpleTimer(spawnTimer);

            trainingManager.Init();
            onUnitSpawned?.AddListener(trainingManager.OnUnitSpawned);
            trainingManager.onTrained.AddListener(OnTrained);

            WorldManager.Map.onBuilt -= OnMapBuilt;
        }

        void OnTrained(Character worker, Character specialist)
        {
            worker.Deinit();
            GameObjectPool.RecycleGameObject(worker.gameObject);
            specialists.Add(specialist);
            specialist.Init();
        }

        [System.Serializable]
        struct SpawnUnit
        {
            [Tooltip("The delay before the next unit is spawned.")]
            public float delay;
            public WorkerSpawner workerSpawner;
        }
    }

}