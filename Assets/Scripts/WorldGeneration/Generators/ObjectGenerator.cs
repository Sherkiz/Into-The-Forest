using UnityEngine;
using ITF.CustomTiles;

namespace ITF.WorldGeneration
{

    //[CreateAssetMenu(fileName = "ObjectGenerator", menuName = "ITF/ObjectGenerator")]
    public abstract class ObjectGenerator : ScriptableObject
    {
        public class GenerateStatus
        {
            public bool finished;
            /// <summary>
            /// true if the generation failed or stopped
            /// </summary>
            public bool failed;
            /// <summary>
            /// The progress of the generation, should be between 0 and 1
            /// </summary>
            public float progress;

            public GenerateStatus()
            {
                finished = false;
                failed = false;
                progress = 0;
            }
        }

        [Tooltip("Used to determine the seed of random numbers"), SerializeField]
        protected new string name;

        public abstract int Seed { get; set; }

        /// <summary>
        /// To generate tiles on the tilemap, it may finished after multiple frames
        /// </summary>
        /// <param name="tilemap"></param>
        /// <returns></returns>
        public abstract GenerateStatus Generate(TilemapManager tilemap);

        /// <summary>
        /// Stop all the generating tasks
        /// </summary>
        public abstract void StopAllGeneration();
    }

}