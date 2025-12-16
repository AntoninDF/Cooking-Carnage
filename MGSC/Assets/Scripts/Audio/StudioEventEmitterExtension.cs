
using UnityEngine;
using FMODUnity;

[AddComponentMenu("FMOD Studio/FMOD Studio Extended Event Emitter")]
[RequireComponent(typeof(StudioEventEmitter))]
public class StudioEventEmitterExtension : MonoBehaviour
{
    [HideInInspector] public StudioEventEmitter eventEmitter;
    public void Pause()
    {
        Get();
        eventEmitter.EventInstance.setPaused(true);
    }
    public void Resume()
    {
        Get();
        eventEmitter.EventInstance.setPaused(false);
    }
    public void SetVolume(float volume)
    {
        Get();
        eventEmitter.EventInstance.setVolume(volume);
    }
    public void Play()
    {
        Get();
        eventEmitter.Play();
    }
    public void Stop()
    {
        Get();
        eventEmitter.Stop();
    }
    private void Get() => eventEmitter = GetComponent<StudioEventEmitter>();
}