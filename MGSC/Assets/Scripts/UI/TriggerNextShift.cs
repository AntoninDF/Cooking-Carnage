using GameSystem;
using UnityEngine;

public class TriggerNextShift : MonoBehaviour
{
    public void RequestNextShift()
    {
        ReferenceManager.Instance.GetManager<ShiftManager>().NextShift();
    }
    public void Reset()
    {
        
        ReferenceManager.Instance.GetManager<DataManager>().ResetSave();
        ReferenceManager.Instance.GetManager<ShiftManager>().NextShift();
    }
}
