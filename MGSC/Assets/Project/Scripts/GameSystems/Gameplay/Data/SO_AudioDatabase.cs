using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioDatabase", fileName = "AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
    public EventReference mainMenuMusic;
    public EventReference ingameMusic;
    public EventReference beforeShiftMusic;
    public EventReference upgradeMusic;
    public EventReference fliesAmbience;
    public EventReference kitchenAmbience;
    public EventReference shiftNoonSFX;
    public EventReference shiftEndSFX;
}

