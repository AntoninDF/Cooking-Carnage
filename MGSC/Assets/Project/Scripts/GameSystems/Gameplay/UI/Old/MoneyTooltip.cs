using GameSystem;
using UnityEngine;

namespace UI
{

    public class MoneyTooltip : MonoBehaviour
    {
        [SerializeField] private Tooltip _tooltip;

        private MoneyManager _moneyManager;

        private void Start()
        {
            Reference.GetManager(out _moneyManager);
        }

        private void Update()
        {
            _tooltip.SetText("Current balance : " + _moneyManager.MoneyInfo);
        }
    }
}