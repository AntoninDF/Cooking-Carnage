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
        
        #endregion

        #region Fields
        
        private readonly Dictionary<string, Timer> timers = new();
       
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
        public void Unregister()
        {
            ReferenceManager.Instance.RemoveManager(this);
        }
        public void Init() { }


        /// <summary>
        /// Updates all active timers (provided by the UpdatableBase inheritance)
        /// </summary>
        public override void Update(float deltaTime)
        {
            var keys = new List<string>(timers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (!timers[key].FrameBased)
                {
                    timers[key].Update(deltaTime);
                }
            }
        }

        public override void FixedUpdate()
        {
            var keys = new List<string>(timers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (timers[key].FrameBased)
                {
                    timers[key].Update();
                }
            }
        }
        
        
        /// <summary>
        /// Starts a timer with the given name, duration, callback, and loop option
        /// </summary>
        public Timer StartTimer( string name, float duration, Action onComplete ,bool frameBased = false)
        {
            // If the timer already exist, replaces it by the new one
            if (timers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset( name, duration, onComplete, frameBased );
                existingTimer.Resume();
                
                this.Log(LogType.Log, "Timer " + name + " has been reset");

                return existingTimer;
            }
            
            // Creates a new timer
            var t = new Timer( name, duration, onComplete, frameBased );
            timers[name] = t;
            
            this.Log(LogType.Log, "Timer " + name + " has been launched");
            
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
                
                this.Log(LogType.Log, "Timer " + name + " is paused ");
            }
            else
            {
                this.Log(LogType.Warning, "Timer " + name + "cannot be paused, it doesn't exist");
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
                
                this.Log(LogType.Log, "Timer " + name + " is now resumed ");
            }
            else
            {
                this.Log(LogType.Warning, "Timer " + name + "cannot be paused, it doesn't exist");
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
            
            this.Log(LogType.Log, "Timer " + name + " has been erased");
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
        
        public bool FrameBased { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsPaused => paused;
        
        private Action onComplete;
        private bool paused;
        
        #endregion

        #region Methods
        public Timer(string name, float duration, Action onComplete, bool frameBased)
        {
            Reset(name, duration, onComplete, frameBased);
        }
        
        public void Reset( string name, float duration, Action actionOnComplete, bool frameBased)
        {
            Name = name;
            this.onComplete = actionOnComplete;
            FrameBased = frameBased;
            Duration = frameBased ? Mathf.RoundToInt(duration) : duration;
            Elapsed = 0f;
            IsComplete = false;
            paused = false;
        }
        
        public void Update(float deltaTime=0)
        {
            if (paused || IsComplete)
                return;

            if (!FrameBased)
            {

                Elapsed += deltaTime;
                if (Elapsed >= Duration)
                {
                    IsComplete = true;
                    onComplete?.Invoke();
                }
            }
            else
            {
                Elapsed+=1;

                if (Elapsed >= Duration)
                {
                    IsComplete = true;
                    onComplete?.Invoke();
                }
            }
        }
        
        public void End()
        {
            Elapsed = Duration;
        }

        public void Pause() => paused = true;
        public void Resume() => paused = false;
        
        #endregion
    }

}