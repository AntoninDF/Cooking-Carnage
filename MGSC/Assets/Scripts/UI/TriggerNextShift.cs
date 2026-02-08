using GameSystem;
using UnityEngine;

public class TriggerNextShift : MonoBehaviour
{
    public void RequestNextShift()
    {
        Reference.GetManager<ShiftManager>().NextShift();
    }
    public void Reset()
    {
        
        Reference.GetManager<DataManager>().ResetSave();
        Reference.GetManager<ShiftManager>().NextShift();
    }
}
