
    namespace Game_System.Interface
    {
        public interface IUpdatable
        {
            void Update(float deltaTime);

            void RegisterToGameLoop();
            void UnregisterFromGameLoop();
        }
    }