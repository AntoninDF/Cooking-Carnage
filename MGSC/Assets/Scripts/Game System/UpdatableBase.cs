using UnityEngine;

namespace GameSystem
{
    public abstract class UpdatableBase 
    {
        protected UpdatableBase()
        {
            RegisterToGameLoop();
        }

        public abstract void Update(float deltaTime);

        public virtual void RegisterToGameLoop()
        {
            if (ReferenceManager.Instance.TryGetManager<GameLoop>(out var loop))
                loop.RegisterUpdatable(this);
            else
                Debug.LogWarning($"{GetType().Name}: GameLoop not ready yet!");
        }

        public virtual void UnregisterFromGameLoop()
        {
            if (ReferenceManager.Instance.TryGetManager<GameLoop>(out var loop))
                loop.UnregisterUpdatable(this);
        }
    }
}