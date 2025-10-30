using Controller;
using GameSystem;
using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class SnapToPlayer : MonoBehaviour
    {

        private Transform _player;
        void Start()
        {
            _player = ReferenceManager.Instance.GetElementOfType<PlayerInterface>().transform;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = _player.position;
            transform.rotation = _player.rotation;
            transform.localScale = _player.localScale;
        }
    }
}