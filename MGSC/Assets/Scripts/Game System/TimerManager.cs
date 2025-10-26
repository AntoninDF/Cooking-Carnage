using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// A Gamesystem used to manage all the timers throughout the game
    /// </summary>
    public class TimerManager : UpdatableBase, IManager
    {
        #region Properties
        
        
        
        #endregion

        #region Fields
        
        private readonly Dictionary<string, Timer> _timers = new();
        
        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public TimerManager()
        {
            Register();
        }
        
        
        /// <summary>
        /// IManager Interface method to register itself to the ReferenceManager
        /// </summary>
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        
        
        /// <summary>
        /// Updates all active timers (provided by the UpdatableBase inheritance)
        /// </summary>
        public override void Update(float deltaTime)
        {
            foreach (var timerKey in _timers)
            {
                timerKey.Value.Update(deltaTime);
            }
        }
        
        
        /// <summary>
        /// Starts a timer with the given name, duration, callback, and loop option
        /// </summary>
        public Timer StartTimer(string name, float duration, Action onComplete, bool loop = false)
        {
            // If the timer already exist, replaces it by the new one
            if (_timers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset(name, duration, onComplete, loop);
                existingTimer.Resume();
                return existingTimer;
            }
            
            // Creates a new timer
            var t = new Timer(name, duration, onComplete, loop);
            _timers[name] = t;
            return t;
        }
        
        /// <summary>
        /// Pauses a timer
        /// </summary>
        public void StopTimer(string name)
        {
            if (_timers.TryGetValue(name, out var t))
            {
                t.Pause();
            }
        }
        
        /// <summary>
        /// Resumes a paused timer
        /// </summary>
        public void ResumeTimer(string name)
        {
            if (_timers.TryGetValue(name, out var t))
            {
                t.Resume();
            }
        }
        
        /// <summary>
        /// Checks if a timer is currently running
        /// </summary>
        public bool IsTimerRunning(string name)
        {
            return _timers.TryGetValue(name, out var t) && !t.IsComplete && !t.IsPaused;
        }
        
        /// <summary>
        /// Removes a timer from the list (deletes it)
        /// </summary>
        public void RemoveTimer(string name)
        {
            _timers.Remove(name);
        }

        #endregion
    }

    /// <summary>
    /// Individual timer
    /// </summary>
    public class Timer
    {
        #region Properties
        
        public string Name { get; private set; }
        public float Duration { get; private set; }
        public float Elapsed { get; private set; }
        public bool IsLooping { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsPaused => _paused;
        
        private Action _onComplete;
        private bool _paused;
        
        #endregion

        #region Methods
        public Timer(string name, float duration, Action onComplete, bool loop)
        {
            Reset(name, duration, onComplete, loop);
        }
        
        public void Reset(string name, float duration, Action onComplete, bool loop)
        {
            Name = name;
            Duration = duration;
            _onComplete = onComplete;
            IsLooping = loop;
            Elapsed = 0f;
            IsComplete = false;
            _paused = false;
        }
        
        public void Update(float deltaTime)
        {
            if (_paused || IsComplete)
                return;

            Elapsed += deltaTime;
            if (Elapsed >= Duration)
            {
                if (IsLooping)
                {
                    Elapsed = 0f;
                }
                else
                {
                    IsComplete = true;
                    _onComplete.Invoke();
                }
            }
        }

        public void Pause() => _paused = true;
        public void Resume() => _paused = false;
        
        #endregion
    }

}