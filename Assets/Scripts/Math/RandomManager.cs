using System.Collections.Generic;
using UnityEngine;

namespace ITF.Math
{

    [DefaultExecutionOrder(-5)]
    public class RandomManager : MonoBehaviour
    {
        static RandomManager instance;
        public static RandomManager Instance => instance;

        [SerializeField]
        int seed;
        public static int Seed
        {
            get => instance.seed;
        }

        //Storage the diffrent seed for different module, the key is the module name, the value is the seed
        Dictionary<string, int> seedVariants;

        #region 公共方法

        public static void Init(int seed)
        {
            instance.seed = seed;
            instance.seedVariants = new();
        }

        /// <summary>
        /// Get the seed for the module
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static int GetSeedFor(string moduleName)
        {
            if (!instance.seedVariants.ContainsKey(moduleName))
            {
                // FNV-1a Hash Algorithm
                int variant = Seed;
                foreach (char c in moduleName)
                {
                    variant ^= c;
                    variant *= 16777619;
                }
                instance.seedVariants[moduleName] = Mathf.Abs(variant);
            }

            return instance.seedVariants[moduleName];
        }

        /// <summary>
        /// Init with a random seed
        /// </summary>
        public void RandomSeed()
        {
            Init(Random.Range(0, 999_999_999));
        }

        #endregion

        #region 内部方法

        private void Awake()
        {
            if(instance == null) instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        #endregion
    }

    /// <summary>
    /// XorShift Random Number Generator
    /// </summary>
    public class XorShiftRandom
    {
        uint state;
        public uint State => state;

        public XorShiftRandom(uint seed)
        {
            //avalanche
            uint hash = seed;
            hash ^= hash >> 16;
            hash *= 0x85EBCA77;
            hash ^= hash >> 13;
            hash *= 0xC2B2AE35;
            hash ^= hash >> 16;

            state = hash == 0 ? 123456789 : hash;
        }

        public uint Next()
        {
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        /// <summary>
        /// Generate the next random number in the range [min, max)
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive)</param>
        /// <returns></returns>
        public uint Range(uint min, uint max)
        {
            return Next() % (max - min) + min;
        }

        /// <summary>
        /// Generate the next random number in the range [0, 1)
        /// </summary>
        /// <returns></returns>
        public float Range01()
        {
            return Next() / (float)uint.MaxValue;
        }

        /// <summary>
        /// Generate the next random number in the range [min, max)
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive)</param>
        /// <returns></returns>
        public float Range(float min, float max)
        {
            return Range01() * (max - min) + min;
        }
    }

}