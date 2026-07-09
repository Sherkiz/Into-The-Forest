//#define EVENT_DEBUG

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ITF.EventChannels
{
    /// <summary>
    /// Call events with priority, the larger the priority value, the earlier it will be executed
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        protected List<GameEventData> eventDatas = new();

        protected bool sorted = false;
        //Temp data for binary search
        protected GameEventData locateData = new();

        public int BinarySearch(List<GameEventData> list, int priority, bool order)
        {
            locateData.priority = priority;
            if (order) return list.BinarySearch(locateData);
            else return list.BinarySearch(locateData, Comparer<GameEventData>.Create((a, b) => b.priority - a.priority));
        }

        /// <summary>
        /// Events will be sorted in descending order of priority after calling this method
        /// </summary>
        public void Resort()
        {
            eventDatas.Sort((a, b) => b.priority - a.priority);
            sorted = true;
        }

        public void Invoke()
        {
            foreach (GameEventData eventData in eventDatas)
            {
                try
                {
                    eventData.unityEvent?.Invoke();
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void AddListener(int priory, UnityAction action)
        {
            if (!sorted) Resort();
            //locate the position to add the event using binary search
            int index = BinarySearch(eventDatas, priory, false);
            if(index > 0)
            {
                GameEventData data = eventDatas[index]; 
                data.unityEvent.AddListener(action);
                return;
            }
            eventDatas.Insert(index, new(priory, action));
        }

        public void RemoveListener(int priory, UnityAction action)
        {
            if (!sorted) Resort();
            int index = BinarySearch(eventDatas, priory, false);
            while (index > 0)
            {
                GameEventData data = eventDatas[index];
                data.unityEvent.RemoveListener(action);
            }
        }

        /// <summary>
        /// This method will traverse all events of all priorities to remove the listener, it is not recommended to use this method when there are many events and you know the priority of the listener
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction action)
        {
            foreach (var data in eventDatas) data.unityEvent.RemoveListener(action);
        }

        public void RemoveAllListeners() => eventDatas.Clear();

        public int GetListernerCount()
        {
            int c = 0;
            foreach (var data in eventDatas) c += data.unityEvent.GetPersistentEventCount();
            return c;
        }
    }

    [System.Serializable]
    public class GameEvent<T>
    {
        protected List<GameEventData<T>> eventDatas = new();

        protected bool sorted = false;
        protected GameEventData<T> locateData = new();

        public int BinarySearch(List<GameEventData<T>> list, int priority, bool order)
        {
            locateData.priority = priority;
            if (order) return list.BinarySearch(locateData);
            else return list.BinarySearch(locateData, Comparer<GameEventData<T>>.Create((a, b) => b.priority - a.priority));
        }

        /// <summary>
        /// Events will be sorted in descending order of priority after calling this method
        /// </summary>
        public void Resort()
        {
            eventDatas.Sort((a, b) => b.priority - a.priority);
            sorted = true;
        }

        public void Invoke(T param)
        {
            foreach (GameEventData<T> eventData in eventDatas)
            {
                try
                {
                    eventData.unityEvent?.Invoke(param);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void AddListener(int priory, UnityAction<T> action)
        {
            if (!sorted) Resort();
            int index = BinarySearch(eventDatas, priory, false);
            if (index > 0)
            {
                GameEventData<T> data = eventDatas[index];
                data.unityEvent.AddListener(action);
                return;
            }
            eventDatas.Insert(index, new(priory, action));
        }

        public void RemoveListener(int priory, UnityAction<T> action)
        {
            if (!sorted) Resort();
            int index = BinarySearch(eventDatas, priory, false);
            while (index > 0)
            {
                GameEventData<T> data = eventDatas[index];
                data.unityEvent.RemoveListener(action);
            }
        }

        /// <summary>
        /// This method will traverse all events of all priorities to remove the listener, it is not recommended to use this method when there are many events and you know the priority of the listener
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction<T> action)
        {
            foreach (var data in eventDatas) data.unityEvent.RemoveListener(action);
        }

        public void RemoveAllListeners() => eventDatas.Clear();
    }

    [System.Serializable]
    public class GameEvent<T0, T1>
    {
        protected List<GameEventData<T0, T1>> eventDatas = new();

        protected bool sorted = false;
        protected GameEventData<T0, T1> locateData = new();

#if EVENT_DEBUG

        //Store all listeners for debugging purposes, the key is the priority of the listener
        Dictionary<int, List<UnityAction<T0, T1>>> unityActions = new();

#endif
        public int BinarySearch(List<GameEventData<T0, T1>> list, int priority, bool order)
        {
            locateData.priority = priority;
            if (order) return list.BinarySearch(locateData);
            else return list.BinarySearch(locateData, Comparer<GameEventData<T0, T1>>.Create((a, b) => b.priority - a.priority));
        }

        /// <summary>
        /// Resort the events, they will be sorted in descending order of priority
        /// </summary>
        public void Resort()
        {
            eventDatas.Sort((a, b) => b.priority - a.priority);
            sorted = true;
        }

        public void Invoke(T0 param1, T1 param2)
        {
            foreach (GameEventData<T0, T1> eventData in eventDatas)
            {
                try
                {
                    eventData.unityEvent?.Invoke(param1, param2);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void AddListener(int priory, UnityAction<T0, T1> action)
        {
            if (!sorted) Resort();
            int index = BinarySearch(eventDatas, priory, false);
            if (index > 0)
            {
                GameEventData<T0, T1> data = eventDatas[index];
                data.unityEvent.AddListener(action);
                return;
            }
            eventDatas.Insert(index, new(priory, action));
        }

        public void RemoveListener(int priory, UnityAction<T0, T1> action)
        {
            if (!sorted) Resort();
            int index = BinarySearch(eventDatas, priory, false);
            while (index > 0)
            {
                GameEventData<T0, T1> data = eventDatas[index];
                data.unityEvent.RemoveListener(action);
            }
        }

        /// <summary>
        /// This method will traverse all events of all priorities to remove the listener, it is not recommended to use this method when there are many events and you know the priority of the listener
        /// </summary>
        /// <param name="action"></param>
        public void RemoveListener(UnityAction<T0, T1> action)
        {
            foreach (var data in eventDatas) data.unityEvent.RemoveListener(action);
        }

        public void RemoveAllListeners() => eventDatas.Clear();

#if EVENT_DEBUG

        public void PrintAllEvents()
        {
            foreach(var pair in unityActions)
            {
                foreach(UnityAction<T0, T1> action in pair.Value)
                {
                    System.Delegate del = action as System.Delegate;
                    string className;
                    if(del.Target != null)
                    {
                        className = del.Target.GetType().FullName;
                    }
                    else
                    {
                        className = del.Method.DeclaringType?.FullName ?? "Unkonw";
                    }

                    Debug.Log($"priority:{pair.Key}: {className}.{del.Method.Name}");
                }
            }
        }

#endif
    }

    [System.Serializable]
    public struct GameEventData : IComparer<GameEventData>
    {
        [Tooltip("The larger the value, the earlier it will be executed")]
        public int priority;
        public UnityEvent unityEvent;

        public GameEventData(int priority, UnityAction action)
        {
            this.priority = priority;
            unityEvent = new();
            unityEvent.AddListener(action);
        }

        public int Compare(GameEventData x, GameEventData y)
        {
            return x.priority - y.priority;
        }
    }

    [System.Serializable]
    public struct GameEventData<T> : IComparer<GameEventData<T>>
    {
        [Tooltip("The larger the value, the earlier it will be executed")]
        public int priority;
        public UnityEvent<T> unityEvent;

        public GameEventData(int priority, UnityAction<T> action)
        {
            this.priority = priority;
            unityEvent = new();
            unityEvent.AddListener(action);
        }

        public int Compare(GameEventData<T> x, GameEventData<T> y)
        {
            return x.priority - y.priority;
        }
    }

    [System.Serializable]
    public struct GameEventData<T0, T1> : IComparer<GameEventData<T0, T1>>
    {
        [Tooltip("The larger the value, the earlier it will be executed")]
        public int priority;
        public UnityEvent<T0, T1> unityEvent;

        public GameEventData(int priority, UnityAction<T0, T1> action)
        {
            this.priority = priority;
            unityEvent = new();
            unityEvent.AddListener(action);
        }

        public int Compare(GameEventData<T0, T1> x, GameEventData<T0, T1> y)
        {
            return x.priority - y.priority;
        }
    }

    public enum EventPriority : int
    {
        First = 500,
        High = 200,
        Normal = 100,
        Low = 50,
        Last = 0
    }

}