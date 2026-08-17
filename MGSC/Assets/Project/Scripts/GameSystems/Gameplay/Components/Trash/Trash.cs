using System;
using System.Collections;
using System.Collections.Generic;
using Features;
using UnityEngine;
using UnityEngine.Events;

namespace Trash
{
    public class Trash : MonoBehaviour, IFeatureCollection
    {
        #region Properties

        public UnityEvent OnStartEating;
        public UnityEvent OnFinishEating;
        public FeatureCollection Collection { get; }
        public F_SoloContainer Container { get; set; }
        
        #endregion
        
        #region Fields
        [SerializeField] private float eatTime;
        [SerializeField] private float _animationDuration;
        [SerializeField] private CustomSlot _slotContainer;
        private F_GoToAnimation _objectAnimation;
        private bool _canEat;
        private Dictionary<int, int> _objectsTrashedPerShift = new();
        #endregion
        
        private void OnEnable()
        {
           GameLoop.OnReset += ResetThis; 
        }
        private void OnDisable()
        {
            GameLoop.OnReset -= ResetThis;
        }
        void ResetThis()
        {
            Container?.Empty();
            _objectAnimation = null;
            _canEat = true;
        }

        
        void Start()
        {
            _canEat = true; 
            Container = _slotContainer.Container;
            AddContainerCondition();
            BindExternalEvents();
        }

        void BindExternalEvents()
        {
            Container.OnGrab += TryDestroy;
        }

        void AddContainerCondition()
        {
            Container.Filter.AddCondition(  obj =>
                {
                    if(!_canEat)
                    {
                        return false;
                    }
                    if (!Globals.Authorizations.AllowTrashingStuff) return false;
                    if (obj is not Feature _feature) return false;
                    if(_feature.Owner is not Item _item) return false;
                    if (_item is not Plate) return false;
                    _objectAnimation = _item.Anim;
                    return true;
                }
            );
            
        }

        void TryDestroy(F_Grabbable obj)
        {
            
            if(!_canEat)return;
            if (!Globals.Authorizations.AllowTrashingStuff) return;
            
            if (_objectAnimation != null) 
                _objectAnimation.OnAnimationStop += (() =>
                {
                    StartCoroutine(EatCoroutine(obj.GetParentTransform().gameObject));
                    Achievements.Unlock("ACH_A05");

                    int sn = Globals.Gameplay.ShiftNumber;
                    if (!_objectsTrashedPerShift.ContainsKey(sn)) _objectsTrashedPerShift[sn] = 0;
                    _objectsTrashedPerShift[sn]++;
                    if (_objectsTrashedPerShift[sn] == 10) Achievements.Unlock("ACH_B14");
                });
        }
        
        private IEnumerator EatCoroutine(GameObject obj)
        {
            OnStartEating?.Invoke();
            Container.IsLocked = true;
            _canEat = false;
            yield return new WaitForSeconds(eatTime);
            DestroyItem(obj);
            yield return new WaitForSeconds(_animationDuration - eatTime);
            Container.IsLocked = false;
            OnFinishEating?.Invoke();
            ResetThis();

        }
        
        private void DestroyItem(GameObject go)
        {
            Destroy(go);
        }
        
    }
}