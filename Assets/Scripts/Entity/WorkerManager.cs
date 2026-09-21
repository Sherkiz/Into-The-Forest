using ITF.Spawners;
using ITF.Utilities;
using ITF.World;
using System.Collections.Generic;
using UnityEngine;

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

        #region public methods

        public Character[] GetCharacters()
        {
            return workers.ToArray();
        }

        #endregion

        void Start()
        {
            WorldManager.OnWorldGenerated.AddListener(OnWorldGenerated);
            trainingManager.Init();
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
                Character[] characters = spawnUnits[spawnUnitIndex].workerSpawner.SpawnCharacters();
                workers.AddRange(characters);
                foreach(var character in characters)
                {
                    character.Init();
                }
                Debug.Log($"The {spawnUnitIndex} wave of workers has been spawned.");

                spawnUnitIndex++;
                if (spawnUnitIndex < spawnUnits.Length)
                {
                    _.timer = 0;
                    _.interval = spawnUnits[spawnUnitIndex].delay;
                }
            });
            TimeManager.AddSimpleTimer(spawnTimer);
            WorldManager.Map.onBuilt -= OnMapBuilt;
        }

        void OnTrained(Character worker, Character specialist)
        {
            worker.Deinit();
            GameObjectPool.RecycleGameObject(worker.gameObject);
            specialists.Add(specialist);
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