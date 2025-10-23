namespace GameSystem
{
    public class Manager
    {
        public Manager()
        {
            Register();
        }

        protected void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
    }
}