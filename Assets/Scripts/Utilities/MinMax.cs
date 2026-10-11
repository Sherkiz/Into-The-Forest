using UnityEngine;

namespace ITF.Utilities
{
    public struct MinMaxFloat
    {
        [Tooltip("Min value (inclusive")]
        public float min;
        [Tooltip("Max value (exclusive")]
        public float max;

        public float GetValue()
        {
            return Random.Range(min, max);
        }
    }

    public struct MinMaxInt
    {
        [Tooltip("Min value (inclusive")]
        public int min;
        [Tooltip("Max value (exclusive")]
        public int max;

        public float GetValue()
        {
            return Random.Range(min, max);
        }
    }
}
