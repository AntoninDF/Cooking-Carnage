using System;
using Features;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.Trash
{
    public class Trash : MonoBehaviour, IFeatureCollection
    {
        #region Properties
        public Action OnObjectDestroyed { get; set; }
        public FeatureCollection Collection { get; }
        public F_SoloContainer Container { get; set; }
        
        #endregion
        
        #region Fields
        [SerializeField] private SlotContainer _slotContainer;
        private F_GoToAnimation _objectAnimation;
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
        }
        
        void Start()
        {
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
            if (_objectAnimation != null) 
                _objectAnimation.OnAnimationStop += (() => DestroyItem(obj.GetParentTransform().gameObject));
            else Debug.Log($"[Trash] can't trow {obj.GetParentTransform().name} into trash");
        }

        private void DestroyItem(GameObject go)
        {
            Debug.Log($"[Trash] Destroy : {go.name}");
            Destroy(go);
            OnObjectDestroyed?.Invoke();
            ResetThis();
        }
        
    }
}