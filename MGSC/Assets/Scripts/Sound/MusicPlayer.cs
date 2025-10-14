using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public AudioClip music;
    [SerializeField] private float _volume = 0.5f;


    private AudioSource _source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _source = GetComponent<AudioSource>();
        _source.clip = music;
        _source.volume = _volume;
    }

    public void ToggleMusic()
    {
        if (_source.isPlaying)
        {
            _source.Stop();
        }
        else
        {
            _source.Play();
        }
    }

    public void AdjustVolume(float delta)
    {
        _volume += delta;
        _source.volume = _volume;
    }
}
