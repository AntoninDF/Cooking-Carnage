using UnityEngine;

/// <summary>
/// This class is dedicated to fixing a bug where the overflow VFX plays at times it's not supposed to.
/// It's just a bandaid, nothing clean.
/// </summary>
public class VFX_Overflow_Hotfix : MonoBehaviour
{
    [SerializeField] DrinkStation _correspondingStation;
    [SerializeField] ParticleSystem _overflowVFX;

    void Reset() => _overflowVFX = GetComponent<ParticleSystem>();
    // Update is called once per frame
    void Update()
    {
        if (_correspondingStation == null) return;
        if (_correspondingStation.IsServing) return;
        _overflowVFX?.Stop();
    }
}
