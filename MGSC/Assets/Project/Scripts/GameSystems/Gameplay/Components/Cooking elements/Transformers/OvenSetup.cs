using Features;
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
        [Tooltip("List of all the ovens that can get activated")]
        [SerializeField] private List<Oven> _ovens;

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
            if (_ovens == null || _ovens.Count == 0)
                Debug.LogError("[OvenSetup] : Please reference at least a few slots for the ovens to go in the inspector (" + name + ")");
        }

        public void Setup(int maxOvenCount)
        {
            // A refacto, To Do

            /*
            int ovenCount = Mathf.Min(maxOvenCount, _slots.Count);
            var ovens = Reference.GetAll<Oven>();
            for (int i = ovens.Count; i < ovenCount; i++)
            {
                GameObject go = Instantiate(_ovenPrefab);
                go.name = "Oven n°" + i + " (" + Time.time + ")";
                Oven oven = go.GetComponent<Oven>();
                _slots[i].Container.RequestGrab(null); // Replace null by oven
                Reference.TryAdd(oven);
            }
            */
        }

        private void SetupSlots()
        {
                /*
            foreach (var slot in _slots)
            {
                Conditions filter;
                if (slot.Conditions == null)
                    filter = slot.AddComponent<Conditions>();
                else
                    filter = slot.Conditions;
                filter.Clear();
                filter.AddFilter(EFilterCondition.MustBe, EItemType.Oven);
            }
                */
        }

        #endregion
    }
}