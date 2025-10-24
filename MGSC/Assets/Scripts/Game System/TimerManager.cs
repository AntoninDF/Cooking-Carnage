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

        public TimerManager()
        {
            Register();
        }
        
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
        
        public override void Update(float deltaTime)
        {
            foreach (var timerKey in _timers)
            {
                timerKey.Value.Update(deltaTime);
            }
        }
        
        public Timer StartTimer(string name, float duration, Action onComplete, bool loop = false)
        {
            if (_timers.TryGetValue(name, out var existingTimer))
            {
                existingTimer.Reset(name, duration, onComplete, loop);
                existingTimer.Resume();
                return existingTimer;
            }

            var t = new Timer(name, duration, onComplete, loop);
            _timers[name] = t;
            return t;
        }
        
        public void StopTimer(string name)
        {
            if (_timers.TryGetValue(name, out var t))
            {
                t.Pause();
            }
        }
        
        public void ResumeTimer(string name)
        {
            if (_timers.TryGetValue(name, out var t))
            {
                t.Resume();
            }
        }
        
        public bool IsTimerRunning(string name)
        {
            return _timers.TryGetValue(name, out var t) && !t.IsComplete && !t.IsPaused;
        }
        
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