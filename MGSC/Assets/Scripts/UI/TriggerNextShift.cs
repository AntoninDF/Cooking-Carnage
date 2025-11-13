using GameSystem;
using UnityEngine;

public class TriggerNextShift : MonoBehaviour
{
    public void RequestNextShift()
    {
        ReferenceManager.Instance.GetManager<ShiftManager>().NextShift();
    }
}
