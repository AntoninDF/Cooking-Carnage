using System;
using Clients;
using UnityEngine;
using UnityEngine.Playables;
/// <summary>
/// Refs for the opening door events
/// callable from anywhere
/// </summary>
public class EntranceDoor : MonoBehaviour
{
        
        #region Fields

        [SerializeField] private PlayableDirector PlayableDirector;

        #endregion
    
        /// <summary>
        /// Binded to a EventDisplay
        /// </summary>
        public void PlayDirector()
        {
            Debug.Log("Open Door");
            PlayableDirector.Stop();
            PlayableDirector.Play();
        }
}