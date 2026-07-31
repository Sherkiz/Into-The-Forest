using System;
using UnityEngine;

namespace ITF
{
    [DefaultExecutionOrder(-5)]
    public class TimeManager : MonoBehaviour
    {
        static TimeManager instance;

        [Tooltip("The time of day in second"), SerializeField]
        public float dayTime = 600f;
        [Tooltip("The time of night in second"), SerializeField]
        float nightTime = 600f;

        float totalGameTime;

        bool isDayTime = true;
        public static bool IsDayTime => Instance.isDayTime;
        public static bool IsNightTime => !Instance.isDayTime;

        public static TimeManager Instance => instance;

        Action onNightEnter;
        public static Action OnNightEnter => instance.onNightEnter;

        Action onDayEnter;
        public static Action OnDayEnter => instance.onDayEnter;

        #region public methods

        public static float GetTimeInDay()
        {
            return Instance.totalGameTime % (Instance.dayTime + Instance.nightTime);
        }

        #endregion

        #region privite methods

        private void Update()
        {
            totalGameTime += Time.deltaTime;
            bool isDayTimeNow = totalGameTime % (dayTime + nightTime) < dayTime;
            if (isDayTimeNow != isDayTime)
            {
                isDayTime = isDayTimeNow;
                if (isDayTime)
                {
                    onDayEnter?.Invoke();
                }
                else
                {
                    onNightEnter?.Invoke();
                }
            }
        }

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void OnDestroy()
        {
            if(instance == this) instance = null;
        }

        #endregion
    }

}