using Cooking;
using System;
using UnityEngine;
using UnityEngine.Events;

public interface ICookable : IGeneric<ICookable>
{
    public float TargetCookDuration { get; set; } // In seconds
    public float CookDuration { get; set; }
    public float CookPercentage { get; } // 0 is raw, 1 is cooked, over 1 is Overcooked!(tm)
    public ECookStatus CookStatus { get;}
    public UnityEvent<ICookable> OnCook {  get; }
    public void Cook(float dT);
    public void StopCooking();

}
