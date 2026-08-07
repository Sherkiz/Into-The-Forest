using System;
using System.Collections.Generic;
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

        HashSet<SimpleTimer> simpleTimers = new();
        HashSet<SimpleTimer> addingTimers = new();
        HashSet<SimpleTimer> removingTimers = new();

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

        public static void AddSimpleTimer(SimpleTimer simpleTimer)
        {
            Instance.addingTimers.Add(simpleTimer);
        }

        public static bool RemoveSimpleTimer(SimpleTimer simpleTimer)
        {
            bool suc = false;
            if (Instance.simpleTimers.Contains(simpleTimer))
            {
                Instance.removingTimers.Add(simpleTimer);
                suc = true;
            }
            if (Instance.addingTimers.Contains(simpleTimer))
            {
                Instance.addingTimers.Remove(simpleTimer);
                suc = true;
            }
            return suc;
        }


        /// <summary>
        /// If the specified timer has been added, return true; otherwise, return false.
        /// </summary>
        /// <param name="timer"></param>
        /// <returns></returns>
        public static bool Contains(SimpleTimer timer)
        {
            if (Instance.addingTimers.Contains(timer)) return true;

            return Instance.simpleTimers.Contains(timer) && !Instance.removingTimers.Contains(timer);
        }

        /// <summary>
        /// Remove all timers that contain the specified callback function.
        /// </summary>
        /// <param name="callBack">The callback function to match timers against.</param>
        /// <returns>The number of timers removed.</returns>
        public static int RemoveSimpleTimers(Action<SimpleTimer> callBack)
        {
            return Instance.simpleTimers.RemoveWhere(_ => _.callBack.Equals(callBack));
        }

        #endregion

        #region privite methods

        /// <summary>
        /// Apply the changes to the simple timers, adding new timers and removing finished ones.
        /// </summary>
        void UpdateSimpleTimers()
        {
            // Remove timers
            foreach (SimpleTimer timer in removingTimers) simpleTimers.Remove(timer);
            removingTimers.Clear();

            //Add timers
            foreach (SimpleTimer timer in addingTimers) simpleTimers.Add(timer);
            addingTimers.Clear();
        }

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

            UpdateSimpleTimers();
            foreach (SimpleTimer timer in simpleTimers)
            {
                timer.timer += Time.deltaTime;

                try
                {
                    if (timer.timer >= timer.interval) timer.callBack(timer);
                }
                catch (Exception e)
                {
                    timer.timer = timer.interval;
                    Debug.LogError(e);
                }
            }
            simpleTimers.RemoveWhere(_ => _.timer >= _.interval);
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

    public class SimpleTimer
    {
        public float interval;
        public float timer;
        public Action<SimpleTimer> callBack;

        public SimpleTimer(float interval, Action<SimpleTimer> callBack)
        {
            this.interval = interval;
            this.callBack = callBack;
        }
    }
}