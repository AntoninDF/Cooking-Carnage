

using GameSystem;
using UnityEngine;

namespace Cooking
{
    public class Oven2 : Oven
    {
        TimerManager _tm;
        string _cookingID;
        protected override void Start()
        {
            ReferenceManager.Get(out _tm);
        }
        protected override void Update()
        {

        }

        protected override void StopCooking()
        {
            _isCooking = false;
            OnStopCooking?.Invoke();
            _tm.StopTimer(_cookingID);
        }
        public override void StartCooking()
        {
            var plate = GetPlate();
            if (plate == null) return;
            var targetRecipe = GetTargetRecipe();
            if (targetRecipe == null) return;

            float targetDuration = targetRecipe.timeToCook;

            _isCooking = true;
            OnStartCooking?.Invoke();
            _cookingID = "Cooking (" + Time.time + ")";
            _tm.StartTimer(_cookingID, targetDuration * Strength, Cook);
        }

        protected override void Cook()
        {
            var plate = GetPlate();
            plate.Cook(GetTargetRecipe().timeToCook);
            _cookingID = "Overcooking (" + Time.time + ")";
            _tm.StartTimer(_cookingID, GetTargetRecipe().timeToOvercook, Overcook);
        }

        protected void Overcook()
        {

        }

        protected Plate GetPlate() => _slot.IsFree ? null : _slot.HeldItem as Plate;
        protected SO_Recipe GetTargetRecipe() => GetPlate()?.GetMatchingRecipe();
    }
}