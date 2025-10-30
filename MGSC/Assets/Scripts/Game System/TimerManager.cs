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
        
        public Dictionary<string, Timer> Timers => timers;
        public Dictionary<string, FrameTimer> Frametimers => frameTimers;
        
        #endregion

        #region Fields
        
        private readonly Dictionary<string, Timer> timers = new();
        private readonly Dictionary<string, FrameTimer> frameTimers = new();
        
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
            var keys = new List<string>(timers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                timers[key].Update(deltaTime);
            }
        }

        public override void FixedUpdate()
        {
            var keys = new List<string>(frameTimers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                frameTimers[key].Update();
            }
        }
        
        
        /// <summary>
        /// Starts a timer with the given name, duration, callback, and loop option
        /// </summary>
        public Timer StartTimer(string name, float duration, Action onComplete, bool loop = false)
        {
            // If the timer already exist, replaces it by the new one
            if (timers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset(name, duration, onComplete, loop);
                existingTimer.Resume();
                return existingTimer;
            }
            
            // Creates a new timer
            var t = new Timer(name, duration, onComplete, loop);
            timers[name] = t;
            return t;
        }
        
        /// <summary>
        /// Starts a frame timer with the given name, amount of frames, callback, and loop option
        /// </summary>
        public FrameTimer StartFrameTimer(string name, int frames, Action onComplete, bool loop = false)
        {
            // If the timer already exist, replaces it by the new one
            if (frameTimers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset(name, frames, onComplete, loop);
                existingTimer.Resume();
                return existingTimer;
            }
            
            // Creates a new timer
            var t = new FrameTimer(name, frames, onComplete, loop);
            frameTimers[name] = t;
            return t;
        }
        
        
        
        /// <summary>
        /// Pauses a timer
        /// </summary>
        public void StopTimer(string name)
        {
            if (timers.TryGetValue(name, out var t))
            {
                t.Pause();
            }
        }
        
        /// <summary>
        /// Resumes a paused timer
        /// </summary>
        public void ResumeTimer(string name)
        {
            if (timers.TryGetValue(name, out var t))
            {
                t.Resume();
            }
        }
        
        /// <summary>
        /// Checks if a timer is currently running
        /// </summary>
        public bool IsTimerRunning(string name)
        {
            return timers.TryGetValue(name, out var t) && !t.IsComplete && !t.IsPaused;
        }
        
        /// <summary>
        /// Removes a timer from the list (deletes it)
        /// </summary>
        public void RemoveTimer(string name)
        {
            timers.Remove(name);
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
        public bool IsPaused => paused;
        
        private Action onComplete;
        private bool paused;
        
        #endregion

        #region Methods
        public Timer(string name, float duration, Action onComplete, bool loop)
        {
            Reset(name, duration, onComplete, loop);
        }
        
        public void Reset(string name, float duration, Action actionOnComplete, bool loop)
        {
            Name = name;
            Duration = duration;
            this.onComplete = actionOnComplete;
            IsLooping = loop;
            Elapsed = 0f;
            IsComplete = false;
            paused = false;
        }
        
        public void Update(float deltaTime)
        {
            if (paused || IsComplete)
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
                    onComplete.Invoke();
                }
            }
        }

        public void Pause() => paused = true;
        public void Resume() => paused = false;
        
        #endregion
    }

    public class FrameTimer
    {
        #region Properties

        public string Name { get; private set; }
        public int FrameDuration { get; private set; }
        public int ElapsedFrames { get; private set; }
        public bool IsLooping { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsPaused => _paused;

        private Action _onComplete;
        private bool _paused;

        #endregion

        #region Methods

        public FrameTimer(string name, int frames, Action onComplete, bool loop)
        {
            Reset(name, frames, onComplete, loop);
        }

        public void Reset(string name, int frames, Action onComplete, bool loop)
        {
            Name = name;
            FrameDuration = frames;
            _onComplete = onComplete;
            IsLooping = loop;
            ElapsedFrames = 0;
            IsComplete = false;
            _paused = false;
        }

        public void Update()
        {
            if (_paused || IsComplete)
                return;

            ElapsedFrames++;

            if (ElapsedFrames >= FrameDuration)
            {
                if (IsLooping)
                {
                    ElapsedFrames = 0;
                }
                else
                {
                    IsComplete = true;
                    _onComplete?.Invoke();
                }
            }
        }

        public void Pause() => _paused = true;
        public void Resume() => _paused = false;

        #endregion
    }

}