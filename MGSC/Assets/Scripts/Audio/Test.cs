using FMODUnity;
using UnityEngine;

namespace Audio
{

public class Test : MonoBehaviour
{
        [SerializeField] private EventReference _eventInstance;
        [SerializeField] private float _delay = 3f;

        private float _timeOfLastSoundEffect = float.MinValue;
        private void Update()
        {
            if (Time.time -  _timeOfLastSoundEffect > _delay)
            {
                _timeOfLastSoundEffect = Time.time;
                RuntimeManager.PlayOneShot(_eventInstance);
            }
        }
    }

}