using Controller;
using UnityEngine;

public interface IGeneric<T>
{
    public PlayerInterface Player { get; set; }
    public Transform transform { get;}
}
