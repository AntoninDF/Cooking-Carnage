namespace GameSystem
{
    public interface IManager
    {
        public void Register()
        {
            ReferenceManager.Instance.SetManager(this);
        }
    }
}