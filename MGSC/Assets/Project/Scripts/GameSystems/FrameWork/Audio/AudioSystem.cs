using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

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
            sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFXBus"); 
            ambienceBus = FMODUnity.RuntimeManager.GetBus("bus:/AmbienceBus"); 
            dialogBus = FMODUnity.RuntimeManager.GetBus("bus:/DialogBus");

            Globals.Audio.MasterVolume.OnValueChange    += volume => masterBus.setVolume(Mathf.Clamp01(volume));
            Globals.Audio.MusicVolume.OnValueChange     += volume => musicBus.setVolume(Mathf.Clamp01(volume));
            Globals.Audio.AmbienceVolume.OnValueChange  += volume => ambienceBus.setVolume(Mathf.Clamp01(volume));
            Globals.Audio.SFXVolume.OnValueChange       += volume => sfxBus.setVolume(Mathf.Clamp01(volume));
            Globals.Audio.DialogVolume.OnValueChange    += volume => dialogBus.setVolume(Mathf.Clamp01(volume));

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
        
        SetMasterVolume(1);
        SetMusicVolume(1);
        SetAmbienceVolume(1);
        SetSFXVolume(1);
        SetVoiceVolume(1);

        isInitialized = true;
    }
    
    public static void SetMasterVolume(float volume) => Globals.Audio.MasterVolume.Set(volume);
    public static void SetMusicVolume(float volume) => Globals.Audio.MusicVolume.Set(volume);
    public static void SetAmbienceVolume(float volume) => Globals.Audio.AmbienceVolume.Set(volume);
    public static void SetSFXVolume(float volume) => Globals.Audio.SFXVolume.Set(volume);
    public static void SetVoiceVolume(float volume) => Globals.Audio.DialogVolume.Set(volume);
    public static void StopAudioForBus_Master() => StopAllAudioForBus(masterBus);
    public static void StopAudioForBus_Music() => StopAllAudioForBus(musicBus);
    public static void StopAudioForBus_Ambience() => StopAllAudioForBus(ambienceBus);
    public static void StopAudioForBus_SFX() => StopAllAudioForBus(sfxBus);
    public static void StopAudioForBus_Dialog() => StopAllAudioForBus(dialogBus);
    
    public static EventInstance PlayAudio(EventReference audioEvent, bool stopOtherAudios = false, bool forceRestart = true)
    {
        if (stopOtherAudios)
        {
            StopOtherAudios(audioEvent);
        }
        if (forceRestart)
        {
            StopAudio(audioEvent);
        }
        
        if (audioEvent.IsNull)
        {
            UnityEngine.Debug.LogWarning("[AudioSystem] An audio can't be played because it is null");
            return new EventInstance();
        }
        
        string id = audioEvent.Guid.ToString();

        if (activeAudios.ContainsKey(id)) return new EventInstance();

        EventInstance newTrack = RuntimeManager.CreateInstance(audioEvent);
        newTrack.start();
        newTrack.release();
        
#if UNITY_EDITOR
       // UnityEngine.Debug.Log("[AudioSystem] is playing "+ audioEvent.Path);
#endif
        
        activeAudios.Add(id, newTrack);
        return newTrack;
    }

    public static void StopOtherAudios(EventReference audioEvent, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        foreach (var track in activeAudios)
        {
            if (track.Key != audioEvent.Guid.ToString())
            {
                track.Value.stop(stopMode);
                track.Value.clearHandle();
            }
        }
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
            //UnityEngine.Debug.Log("[AudioSystem] stopped "+ audioEvent.Path);
#endif
        }
    }
    public static void StopAllAudioForBus(Bus bus, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (!bus.isValid()) return;
        bus.stopAllEvents(stopMode);

        // This should clear the stopped events from activeAudios, but idk how to
    }
    public static void SetGlobalParameter(string parameterName, float value)
    {
        RESULT result = RuntimeManager.StudioSystem.setParameterByName(parameterName, Mathf.Clamp01(value));
        
        if (result != RESULT.OK)
        {
            UnityEngine.Debug.LogError("[AudioSystem] "+ parameterName + " not found and can't be modified.");
        }
    }
    public static void SetEventParameter(EventInstance eventInst, string parameterName, float value)
    {
        eventInst.setParameterByName(parameterName, value);
    }
    public static void SetEventParameter(EventReference eventRef, string parameterName, float value)
    {
        if (!activeAudios.ContainsKey(eventRef.Guid.ToString())) return;
        EventInstance eventInst = activeAudios[eventRef.Guid.ToString()];
        eventInst.setParameterByName(parameterName, value);
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

    public static void LogActiveAudios()
    {
        string debug = "[AudioSystem] All active audios : ";
        foreach (var kvp in activeAudios)
        {
            debug += "\n" + kvp.Key + " : ";

            RESULT res = RESULT.OK;

            // Retrieve event name
            res = res | kvp.Value.getDescription(out EventDescription desc);
            res = res | desc.getPath(out string path);
            debug += "\t\tPath is ";
            if (res == RESULT.OK)
                debug += path;
            else
                debug += "Invalid";

            // Retrieve playback state
            res = res | kvp.Value.getPlaybackState(out PLAYBACK_STATE playbackState);
            debug += "\t\t" + "Playback state is ";
            if (res == RESULT.OK)
                debug += playbackState.ToString();
            else
                debug += "Invalid";

            // Retrieve volume
            res = res | kvp.Value.getVolume(out float volume);
            debug += "\t\tVolume is ";
            if (res == RESULT.OK)
                debug += volume;
            else
                debug += "Invalid";
        }

        UnityEngine.Debug.Log(debug);
    }
    
    #endregion
}
