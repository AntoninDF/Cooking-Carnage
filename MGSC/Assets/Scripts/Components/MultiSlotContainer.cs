using UnityEngine;
using Features;
public class MultiSlotContainer : MonoBehaviour, IFeatureCollection
{
    #region Properties
    public FeatureCollection Collection { get; set; } = new();
    #endregion


    private void Start()
    {
        
    }
}
