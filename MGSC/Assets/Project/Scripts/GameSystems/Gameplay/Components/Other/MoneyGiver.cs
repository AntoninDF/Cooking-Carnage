using System;
using GameSystem;
using UnityEngine;

namespace Project.Scripts.GameSystems.Gameplay.Components.Other
{
    public class MoneyGiver : MonoBehaviour
    {
        public void Start()
        {
            Reference.GetManager<MoneyManager>().AddMoney(100000);
        }
    }
}