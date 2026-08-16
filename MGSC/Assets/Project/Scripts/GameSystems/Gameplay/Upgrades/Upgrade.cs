using System;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace Upgrades
{

    public abstract class Upgrade
    {
        #region Properties
        
        public int               Level     => _level;
        public UpgradeConfig     Config    => _config;
        
        [GlobalEvent("Upgrades/OnUpgradeSuccessfullyBought")]
        public static Action OnUpgradeSuccess;
        #endregion

        #region Fields

        protected UpgradeConfig _config;
        protected int           _level;          
        
        #endregion

        #region Methods

        protected Upgrade()
        { }

        internal void SetConfig(UpgradeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config),$"[{GetType().Name}] UpgradeConfig cannot be null");
        }
        
        public virtual void LevelUp()
        {
            if (!CanLevelUp())
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot level up further (level {_level}/{_config?.MaxLevel})");
                return;
            }

            Reference.GetManager<UpgradeManager>().LevelUpHistory.Add(
                new() { 
                    TargetUpgrade = new(GetType()), 
                    PreviousUpgradeLevel = _level, 
                    NewUpgradeLevel = _level+1, 
                    Price = GetPrice(), 
                    ShiftNumber = Globals.Gameplay.ShiftNumber 
                });
            _level++;
            // Debug.Log($"[{GetType().Name}] Level {_level}/{Config?.MaxLevel}");
            OnUpgradeSuccess?.Invoke();
        }
        
        public virtual float GetPrice()   => _config.PriceCurve.Evaluate(_level) * Globals.BalancingWeights.UpgradePriceMultiplier;
        public virtual bool  CanLevelUp() => _level < _config.MaxLevel;

        public void SetLevel(int level)
        {
            _level = level;
            NotifyAllSubscribers();
        }
        
        internal abstract void UnsubscribeObject(object subscriber);
        internal abstract void ClearSubscribers();

        /// <summary>
        /// Permet de notifier tous les abonnés sans augmenter de niveau
        /// </summary>
        internal abstract void NotifyAllSubscribers();
        
        #endregion
    }
    
    /// <summary>
    /// Classe permettant aux classes héritées de souscrire automatiquement à UgradeManager
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public abstract class Upgrade<TSelf> : Upgrade where TSelf : Upgrade<TSelf>
    {
        private readonly List<IUpgradable<TSelf>> _subscribers = new();

        public void Subscribe(IUpgradable<TSelf> subscriber)
        {
            if (subscriber == null || _subscribers.Contains(subscriber)) return;
            _subscribers.Add(subscriber);
            subscriber.OnUpgradeInit((TSelf)this);
        }
        
        
        public void Unsubscribe(IUpgradable<TSelf> subscriber) => _subscribers.Remove(subscriber);

        internal override void UnsubscribeObject(object subscriber)
        {
            if (subscriber is IUpgradable<TSelf> typed)
                _subscribers.Remove(typed);
        }

        internal override void ClearSubscribers() => _subscribers.Clear();

        public override void LevelUp()
        {
            base.LevelUp();
            NotifyAllSubscribers();
        }

        internal override void NotifyAllSubscribers()
        {
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                var sub = _subscribers[i];
                if (sub is UnityEngine.Object uo && uo == null)
                {
                    //Debug.LogError("An Upgrade subscriber has been destroyed, level up impossible");
                    _subscribers.RemoveAt(i);
                    continue;
                }
                sub?.OnUpgraded((TSelf)this);
            }
        }
    }
    
}