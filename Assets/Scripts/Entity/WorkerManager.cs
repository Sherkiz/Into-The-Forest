using ITF.Spawners;
using ITF.World;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Entity
{

    public class WorkerManager : MonoBehaviour, IEntityManager
    {
        [SerializeField]
        SpawnUnit[] spawnUnits;

        int spawnUnitIndex;

        SimpleTimer spawnTimer;

        List<Character> workers = new();

        #region public methods

        public Character[] GetCharacters()
        {
            return workers.ToArray();
        }

        #endregion

        void Start()
        {
            WorldManager.OnWorldGenerated.AddListener(OnWorldGenerated);
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

        [System.Serializable]
        struct SpawnUnit
        {
            [Tooltip("The delay before the next unit is spawned.")]
            public float delay;
            public WorkerSpawner workerSpawner;
        }
    }

}