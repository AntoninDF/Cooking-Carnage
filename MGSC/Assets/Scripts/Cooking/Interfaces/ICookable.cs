using System;
using UnityEngine;

public interface ICookable : IGeneric<ICookable>
{
    public float TargetCookDuration { get; set; } // In seconds
    public float CookDuration { get; set; }
    public float CookPercentage { get; } // 0 is raw, 1 is cooked, over 1 is Overcooked!(tm)
    public Action<ICookable> OnCook {  get; set; }
    public void Cook(float dT) { CookDuration += dT; OnCook?.Invoke(this); }

}
