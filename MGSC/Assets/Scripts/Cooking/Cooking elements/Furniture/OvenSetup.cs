using GameSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Cooking
{
    public class OvenSetup : MonoBehaviour
    {
        #region Properties

        #endregion

        #region Fields
        [Header("OvenSetup references")]
        [Tooltip("The oven prefab that will get instantiated for each oven unlocked")]
        [SerializeField] private GameObject _ovenPrefab;
        [Tooltip("List of all the slots where new ovens can go")]
        [SerializeField] private List<SlotContainer> _slots;

        #endregion

        #region Methods

        private void Start()
        {
            EnforceReferences();
            SetupSlots();
            Setup(0);
        }

        private void EnforceReferences()
        {
            if (_ovenPrefab == null)
                Debug.LogError("[OvenSetup] : Please reference an oven prefab in the inspector");
            if (_slots == null)
                Debug.LogError("[OvenSetup] : Please reference at least a few slots for the ovens to go in the inspector");
        }

        public void Setup(int maxOvenCount)
        {
            int ovenCount = Mathf.Min(maxOvenCount, _slots.Count);
            var ovens = Reference.GetAll<Oven>();
            for (int i = ovens.Count; i < ovenCount; i++)
            {
                GameObject go = Instantiate(_ovenPrefab);
                go.name = "Oven n°" + i + " (" + Time.time + ")";
                Oven oven = go.GetComponent<Oven>();
                _slots[i].Grab(oven);
                Reference.TryAdd(oven);
            }
        }

        private void SetupSlots()
        {
            foreach (var slot in _slots)
            {
                ContainerFilter filter;
                if (slot.Filter == null)
                    filter = slot.AddComponent<ContainerFilter>();
                else
                    filter = slot.Filter;

                filter.Clear();
                filter.AddFilter(EFilterCondition.MustBe, EItemType.Oven);
            }
        }

        #endregion
    }
}