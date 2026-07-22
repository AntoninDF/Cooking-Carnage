using System;
using System.Collections;
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
                        Debug.Log($"[Trash] Can't throw the object because glooby is already eating something 😞");
                        return false;
                    }
                    if (obj is not Feature _feature) return false;
                    if(_feature.Owner is not Item _item) return false;
                    _objectAnimation = _item.Anim;
                    return true;
                }
            );
            
        }

        void TryDestroy(F_Grabbable obj)
        {
            Debug.Log($"[Trash] Try to throw : {obj.GetParentTransform().name}");
            
            if(!_canEat)return;
            
            if (_objectAnimation != null) 
                _objectAnimation.OnAnimationStop += (() =>
                {
                    StartCoroutine(EatCoroutine(obj.GetParentTransform().gameObject));
                });
            else 
                Debug.Log($"[Trash] can't trow {obj.GetParentTransform().name} into trash");
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
            Debug.Log($"[Trash] Destroy : {go.name}");
            Destroy(go);
        }

        private void Update()
        {
            Debug.Log($"[Trash] Update : {Container}");
        }
    }
}