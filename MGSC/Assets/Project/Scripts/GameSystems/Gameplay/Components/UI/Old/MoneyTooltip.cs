using GameSystem;
using UnityEngine;

namespace UI
{

    public class MoneyTooltip : MonoBehaviour
    {
        [SerializeField] private Tooltip _tooltip;

        private DataManager _dataManager;

        private void Start()
        {
            Reference.GetManager(out _dataManager);
        }

        private void Update()
        {
            _tooltip.SetText("Current balance : " + _dataManager.MoneyInfo);
        }
    }
}