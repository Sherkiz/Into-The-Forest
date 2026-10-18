using UnityEngine;

namespace ITF.Utilities
{
    [System.Serializable]
    public struct MinMaxFloat
    {
        [Tooltip("Min value (inclusive")]
        [SerializeField] private float min;
        [Tooltip("Max value (inclusive")]
        [SerializeField] private float max;
        private float _value;
        private bool isInit;
        public float Value 
        { 
            get 
            { 
                if (!isInit) 
                {
                    _value = GetOtherValue();
                    isInit = true;
                }
                return _value; 
            } 
        }
        public float GetOtherValue()
        {
            return Random.Range(min, max);
        }
    }
    [System.Serializable]
    public struct MinMaxInt
    {
        [Tooltip("Min value (inclusive")]
        [SerializeField] private int min;
        [Tooltip("Max value (exclusive")]
        [SerializeField] private int max;
        private int _value;
        private bool isInit;
        public int Value
        {
            get
            {
                if (!isInit)
                {
                    _value = GetOtherValue();
                    isInit = true;
                }
                return _value;
            }
        }
        public int GetOtherValue()
        {
            return Random.Range(min, max);
        }
    }
}
