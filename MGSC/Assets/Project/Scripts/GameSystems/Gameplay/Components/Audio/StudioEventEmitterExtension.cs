
using UnityEngine;
using FMODUnity;
using FMOD;

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
    public void StopAll() => StopAll_Static();

    public static void StopAll_Static()
    {
        FMODUnity.RuntimeManager.StudioSystem.getBankList(out FMOD.Studio.Bank[] loadedBanks);
        foreach (FMOD.Studio.Bank bank in loadedBanks)
        {
            bank.getPath(out string path);
            RESULT busListOk = bank.getBusList(out FMOD.Studio.Bus[] myBuses);
            bank.getBusCount(out int busCount);
            if (busCount > 0)
            {
                foreach (var bus in myBuses)
                {
                    bus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }
        }
    }
    private void Get() => eventEmitter = GetComponent<StudioEventEmitter>();
    
    public void FMOD_SetParameterToMax(string parameterName)
    {
        eventEmitter.EventInstance.setParameterByName(parameterName, 1);
    }
        
    public void FMOD_SetParameterToMin(string parameterName)
    {
        eventEmitter.EventInstance.setParameterByName(parameterName, 0);
    }
}