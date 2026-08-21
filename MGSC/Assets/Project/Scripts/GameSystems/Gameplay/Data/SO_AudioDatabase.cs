using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioDatabase", fileName = "AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
    public EventReference mainMenuMusic;
    public EventReference beforeShiftMusic;
    public EventReference upgradeMusic;
    public EventReference ambience;
    public EventReference shiftEndSFX;
    public EventReference rentPaid;
    public EventReference rentIncrease;
    public EventReference rentGameOver;
}

