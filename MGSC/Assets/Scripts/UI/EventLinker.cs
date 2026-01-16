using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using GameSystem;
using Cooking;
using Clients;
using System.Linq;

namespace UI
{

    public class EventLinker : MonoBehaviour
    {
        #region Fields
        [SerializeField] private UnityEvent _onStart;
        [SerializeField] private List<EventLink> _events;


        bool init = false;
        private Dictionary<string, EventLink> _eventsByName = new();
        #endregion

        #region Methods

        void Start()
        {
            _onStart?.Invoke();
        }
        private void Init()
        {
            foreach(var e in _events)
                _eventsByName[e.name] = e;
            init = true;
        }

        public void BindEvent(string eventName) => BindOrUnbindEvent(eventName, true);
        public void UnbindEvent(string eventName) => BindOrUnbindEvent(eventName, false);
        private void BindOrUnbindEvent(string eventName, bool bind)
        {
            if (!init) Init();
            if (!_eventsByName.ContainsKey(eventName)) return;
            var ev = _eventsByName[eventName];
            BindOrUnbindEvent(ev.TargetEvent, ev.OnCall, bind);
        }

        private void BindOrUnbindEvent(TargetEvent target, UnityEvent callback, bool bind)
        {
            var Ref = ReferenceManager.Instance;
            // Target event
            UnityEvent ev = null;
            switch(target)
            {
                case TargetEvent.PlateGrabbedFromRack:
                    foreach(var elm in Ref.GetElementsOfType<ItemSpawner>())
                    {
                        // Target event
                        ev = elm.OnSpawn;
                        if (bind)
                            ev.AddListener(callback.Invoke);
                        else
                            ev.RemoveListener(callback.Invoke);
                    }
                    break;
                case TargetEvent.IngredientCut:
                    foreach (var elm in Ref.GetElementsOfType<Blender>())
                    {
                        // Target event
                        ev = elm.OnItemCut;
                        if (bind)
                            ev.AddListener(callback.Invoke);
                        else
                            ev.RemoveListener(callback.Invoke);
                    }
                    break;
                case TargetEvent.MealCooked:
                    foreach (var elm in Ref.GetElementsOfType<Oven>().Concat(Ref.GetElementsOfType<Oven2>()))
                    {
                        // Target event
                        ev = elm.OnItemCooked;
                        if (bind)
                            ev.AddListener(callback.Invoke);
                        else
                            ev.RemoveListener(callback.Invoke);
                    }
                    break;
                case TargetEvent.MealGrabbedFromOven:
                    foreach (var elm in Ref.GetElementsOfType<Oven>())
                    {
                        // Target event
                        ev = elm.OnItemDropped;
                        if (bind)
                            ev.AddListener(callback.Invoke);
                        else
                            ev.RemoveListener(callback.Invoke);
                    }
                    break;
                case TargetEvent.ClientServed:
                    foreach (var elm in Ref.GetElementsOfType<SittingCounter>())
                    {
                        // Target event
                        ev = elm.OnServe;
                        if (bind)
                            ev.AddListener(callback.Invoke);
                        else
                            ev.RemoveListener(callback.Invoke);
                    }
                    break;
            }
        }

        public void UnBindAll()
        {
            foreach (var e in _events)
            {
                UnbindEvent(e.name);
            }
        }
        
        public void BindAll()
        {
            foreach (var e in _events)
            {
                BindEvent(e.name);
            }
        }

        #endregion
        
        

    }

    [System.Serializable]
    public struct EventLink
    {
        public string name;
        public TargetEvent TargetEvent;
        public UnityEvent OnCall;

    }

    public enum TargetEvent
    {
        None,
        PlateGrabbedFromRack,
        IngredientCut,
        MealCooked,
        MealGrabbedFromOven,
        ClientServed
    }

}