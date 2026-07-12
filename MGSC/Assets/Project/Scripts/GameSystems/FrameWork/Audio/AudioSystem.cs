using System.Collections.Generic;
using FMOD;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public static class AudioSystem
{
    #region Properties
    
    public static AudioDatabase Database => audioDatabase;
    
    #endregion
    
    #region Fields
    
    private static Bus masterBus;
    private static Bus musicBus;
    private static Bus ambienceBus;
    private static Bus sfxBus;
    private static Bus dialogBus;

    private static bool isInitialized = false;
    
    private static Dictionary<string, EventInstance> activeAudios = new Dictionary<string, EventInstance>();
    private static AudioDatabase audioDatabase;
    
    #endregion
    
    #region Methods
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (isInitialized) return;
        
        try
        {
            masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
            musicBus = FMODUnity.RuntimeManager.GetBus("bus:/MusicBus"); 
            sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/AmbienceBus"); 
            ambienceBus = FMODUnity.RuntimeManager.GetBus("bus:/SFXBus"); 
            dialogBus = FMODUnity.RuntimeManager.GetBus("bus:/DialogBus"); 
        
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[AudioSystem] Init Error : {e.Message}");
        }
        
        audioDatabase  = Resources.Load<AudioDatabase>("AudioDatabase");

        if (audioDatabase == null)
        {
            UnityEngine.Debug.LogWarning("[AudioSystem] AudioDatabase not found");
        }

        isInitialized = true;
    }
    
    public static void SetMasterVolume(float volume) => masterBus.setVolume(Mathf.Clamp01(volume));
    public static void SetMusicVolume(float volume) => musicBus.setVolume(Mathf.Clamp01(volume));
    public static void SetAmbienceVolume(float volume) => ambienceBus.setVolume(Mathf.Clamp01(volume));
    public static void SetSFXVolume(float volume) => sfxBus.setVolume(Mathf.Clamp01(volume));
    public static void SetVoiceVolume(float volume) => dialogBus.setVolume(Mathf.Clamp01(volume));
    
    public static void PlayAudio(EventReference audioEvent)
    {
        if (audioEvent.IsNull)
        {
            UnityEngine.Debug.LogWarning("[AudioSystem] An audio can't be played because it is null");
            return;
        }
        
        string id = audioEvent.Guid.ToString();

        if (activeAudios.ContainsKey(id)) return;

        EventInstance newTrack = RuntimeManager.CreateInstance(audioEvent);
        newTrack.start();
        newTrack.release();
        
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[AudioSystem] is playing "+ audioEvent.Path);
#endif
        
        activeAudios.Add(id, newTrack);
    }
    
    public static void StopAudio(EventReference audioEvent,FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (audioEvent.IsNull) return;
        string id = audioEvent.Guid.ToString();

        if (activeAudios.TryGetValue(id, out EventInstance track))
        {
            track.stop(stopMode);
            track.clearHandle();
            activeAudios.Remove(id);
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[AudioSystem] stopped "+ audioEvent.Path);
#endif
        }
    }
    
    public static void SetGlobalParameter(string parameterName, float value)
    {
        RESULT result = RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        
        if (result != RESULT.OK)
        {
            UnityEngine.Debug.LogError("[AudioSystem] "+ parameterName + " not found and can't be modified.");
        }
    }
    
    public static void StopAllAudio(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        foreach (var track in activeAudios.Values)
        {
            track.stop(stopMode);
            track.clearHandle();
        }
        activeAudios.Clear();
        
        masterBus.stopAllEvents(stopMode);
    }
    
    #endregion
}
