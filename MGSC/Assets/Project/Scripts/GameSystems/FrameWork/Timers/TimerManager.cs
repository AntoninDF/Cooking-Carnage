using Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
        
        public TimerManager()
        {
            GameLoop.OnReset += RemoveFinishedTimers;
        }

#if UNITY_EDITOR
        static float elapsedSinceLastLog = 0f;
#endif
        /// <summary>
        /// Updates all active timers
        /// </summary>
        private static void Update()
        {
            if (!Globals.Authorizations.AllowTimerUpdate) return;
            var keys = new List<string>(timers.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (!timers.ContainsKey(key) || timers[key] == null) continue;
                if (!timers[key].FrameBased)
                {
                    timers[key].Update(Time.deltaTime);
                }
            }

#if UNITY_EDITOR && EXTENSIVE_DEBUG
            // This huge debug block should be removed later
            float logCooldown = 2f;
            elapsedSinceLastLog += Time.deltaTime;
            if (elapsedSinceLastLog >= logCooldown)
            {
                elapsedSinceLastLog -= logCooldown;
                Log();
            }

            void Log()
            {
                Debug.Log("__________ " + "TimerManager" + " __________");

                string result = "All timers :";
                foreach (var kvp in timers)
                {
                    result += "\n\t" + kvp.Key + " : \t\t" + Mathf.Round(kvp.Value.Elapsed*100f)/100f + "s/" + kvp.Value.Duration + "s (ProgressCallback = '" + kvp.Value.onUpdate + "', EndCallback = '" + kvp.Value.onComplete + "'";
                }
                Debug.Log(result);

                Debug.Log("__________ " + "##########" + " __________");

            }
#endif

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
            
            
            return t;
        }
        public static Timer StartGameplayTimer(string name, float duration, Action onComplete, Action<float> onUpdate = null)
        {
            var t = StartTimer(name, duration, onComplete, onUpdate);
            GameLoop.BindTimerToGamePause(t);
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
        /// Pauses a timer
        /// </summary>
        public static void StopTimer(Timer timer)
        {
            if (timer != null)
                timer.Pause();
        }
        
        /// <summary>
        /// Resumes a paused timer
        /// </summary>
        public static void ResumeTimer(string name)
        {
            if (timers.TryGetValue(name, out var t))
                t.Resume();
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

        /// <summary>
        /// Returns a float value of the time left for this timer in seconds. If the timer wasn't found, returns 0
        /// </summary>
        /// <param name="name">The name of the requested timer</param>
        public static float TimeLeft(string name)
        {
            if (timers.TryGetValue(name, out var t) && t != null)
                return TimeLeft(t);
            return 0f;
        }
        /// <summary>
        /// Returns a float value of the time left for this timer in seconds. If the timer wasn't found, returns 0
        /// </summary>
        /// <param name="name">The name of the requested timer</param>
        public static float TimeLeft(Timer timer)
        {
            if (timer != null)
                return timer.Duration - timer.Elapsed;
            return 0f;
        }
        
        /// <summary>
        /// Removes a timer from the list (deletes it)
        /// </summary>
        public static void RemoveTimer(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!timers.ContainsKey(name)) return;
            RemoveTimer(timers[name]);
        }
        /// <summary>
        /// Removes a timer from the list (deletes it)
        /// </summary>
        public static void RemoveTimer(Timer timer)
        {
            if (timer == null) return;
            GameLoop.UnbindTimerFromGamePause(timer);
            timers.Remove(timer.Name);
            
            //this.Log(LogType.Log, "Timer " + name + " has been erased");
        }

        public static void RemoveFinishedTimers()
        {
            string[] names = timers.Keys.ToArray();
            foreach (var name in names)
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
        /// <summary>
        /// OnUpdate is invoked with the elapsed time as a parameter. Divide it by <see cref="Duration"/> to get the normalized progression
        /// </summary>
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
            if (paused || IsComplete) return;

            if (!FrameBased)
            {
                Elapsed += deltaTime;
                if (Elapsed >= Duration)
                {
                    Elapsed = Duration;
                    onUpdate?.Invoke(Elapsed); 
                    IsComplete = true;
                    onComplete?.Invoke();
                    return;
                }
            }

            onUpdate?.Invoke(Elapsed);

            if (FrameBased)
            {
                Elapsed += 1;
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