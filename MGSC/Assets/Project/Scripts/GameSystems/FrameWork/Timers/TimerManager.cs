using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GameSystem
{
    /// <summary>
    /// A Gamesystem used to manage all the timers throughout the game
    /// </summary>
    public class TimerManager
    {
        #region Properties
        
        public static Dictionary<string, Timer> Timers => timers;
        
        #endregion

        #region Fields
        
        private static readonly Dictionary<string, Timer> timers = new();
       
        #endregion

        #region Methods

        /// <summary>
        /// Insert the static update method in the player loop right between the PreUpdate and Update
        /// </summary>
        [RuntimeInitializeOnLoadMethod] static void AddToPlayerLoop() => 
            PlayerLoopUtility.InsertMethodCallInPlayerLoopAfter<PreUpdate, TimerManager>(Update);
        
        /// <summary>
        /// Updates all active timers
        /// </summary>
        private static void Update()
        {
            if (!Globals.Gameplay.AllowTimerUpdate) return;
            var keys = new List<string>(timers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (!timers[key].FrameBased)
                {
                    timers[key].Update(Time.deltaTime);
                }
            }
        }

        
        /// <summary>
        /// Starts a timer with the given name, duration, callback, and loop option.
        /// OnUpdate is invoked with the time elapsed in seconds as parameter.
        /// </summary>
        public static Timer StartTimer( string name, float duration, Action onComplete, Action<float> onUpdate=null)
        {
            // If the timer already exist, replaces it by the new one
            if (timers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset( name, duration, onComplete, onUpdate );
                existingTimer.Resume();
                
                //this.Log(LogType.Log, "Timer " + name + " has been reset");

                return existingTimer;
            }
            if (onUpdate == null)
                onUpdate = (elapsed) => { };
            
            // Creates a new timer
            var t = new Timer( name, duration, onComplete, onUpdate);
            timers[name] = t;
            
//            Debug.Log("[TimerManager] TIMER STARTED");
            //this.Log(LogType.Log, "Timer " + name + " has been launched");
            
            return t;
        }
        
        /// <summary>
        /// Pauses a timer
        /// </summary>
        public static void StopTimer(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (timers.TryGetValue(name, out var t))
            {
                t.Pause();
            }
        }
        
        /// <summary>
        /// Resumes a paused timer
        /// </summary>
        public static void ResumeTimer(string name)
        {
            if (timers.TryGetValue(name, out var t))
            {
                t.Resume();
                
                //this.Log(LogType.Log, "Timer " + name + " is now resumed ");
            }
            else
            {
                //this.Log(LogType.Warning, "Timer " + name + "cannot be paused, it doesn't exist");
            }
        }
        
        /// <summary>
        /// Checks if a timer is currently running
        /// </summary>
        public static bool IsTimerRunning(string name, bool fullName =true)
        {
            if (fullName)
            {
                return timers.TryGetValue(name, out var t) && !t.IsComplete && !t.IsPaused;
            }
            else
            {
                var timerEntry = timers.FirstOrDefault(kvp => kvp.Key.Contains(name));
                return timerEntry.Value != null && !timerEntry.Value.IsComplete && !timerEntry.Value.IsPaused;
            }
        }

        public static float TimeLeft(string name)
        {
            if (timers.TryGetValue(name, out var t) && t != null)
                return t.Duration - t.Elapsed;
            return 0f;
        }
        
        /// <summary>
        /// Removes a timer from the list (deletes it)
        /// </summary>
        public static void RemoveTimer(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!timers.ContainsKey(name)) return;
            timers.Remove(name);
            
            //this.Log(LogType.Log, "Timer " + name + " has been erased");
        }

        public static void RemoveFinishedTimers()
        {
            foreach (var name in timers.Keys)
            {
                RemoveTimer(name);
            }
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
        
        public Action onComplete;
        public Action<float> onUpdate;
        private bool paused;
        
        #endregion

        #region Methods
        public Timer(string name, float duration, Action onComplete, Action<float> onUpdate)
        {
            Reset(name, duration, onComplete, onUpdate);
        }
        
        public void Reset( string name = null, float duration=0f, Action actionOnComplete = null, Action<float> actionOnUpdate=null)
        {
            Name = name == null ? (Name == null ? "" : Name) : name;
            this.onComplete = actionOnComplete;
            this.onUpdate = actionOnUpdate;
            Duration = duration;
            Elapsed = 0f;
            IsComplete = false;
            paused = false;
        }
        
        public void Update(float deltaTime=0)
        {
            if (paused || IsComplete)
                return;

            onUpdate?.Invoke(Elapsed);
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