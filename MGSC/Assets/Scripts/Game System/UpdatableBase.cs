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
        public abstract void FixedUpdate();

        public virtual void RegisterToGameLoop()
        {
            if (Reference.TryGetManager<GameLoop>(out var loop))
                loop.RegisterUpdatable(this);
            else
                Debug.LogWarning($"{GetType().Name}: GameLoop not ready yet!");
        }

        public virtual void UnregisterFromGameLoop()
        {
            if (Reference.TryGetManager<GameLoop>(out var loop))
                loop.UnregisterUpdatable(this);
        }
    }
}