using Clients;
using UnityEngine;

public class BloodTypeVFXSelector : MonoBehaviour
{
    [SerializeField] private OrderingArea _sittingCounter;
    [SerializeField] private ParticleSystem _bloodEffectA;
    [SerializeField] private ParticleSystem _bloodEffectB;
    [SerializeField] private ParticleSystem _bloodEffectC;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_sittingCounter == null)
        {
            Debug.LogWarning($"[BloodTypeVFXSelector] No SittingCounter assigned to {name}, disabling script");
            enabled = false;
            return;
        }
    }

    public void Play()
    {
        var type = _sittingCounter.LastRegisteredClientType;
#if DEMO
        type = EClientType.B; // Blood will always be red in the demo. 
#endif
        switch (type)
        {
            case EClientType.A:
                _bloodEffectA?.Play();
                break;
            case EClientType.B:
                _bloodEffectB?.Play();
                break;
            case EClientType.C:
                _bloodEffectC?.Play();
                break;
            default:
                Debug.LogWarning($"[BloodTypeVFXSelector] Unknown client type {type} for {name}");
                break;
        }
        Debug.Log("Playing blood FX for client type: " + type);
    }
}
