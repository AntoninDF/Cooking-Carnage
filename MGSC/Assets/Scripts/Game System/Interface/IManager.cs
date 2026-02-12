public interface IManager
{
    // Register should always be the only method called in a manager's constructor
    public void Register();
    public void Unregister();
    // ReferenceManager.Instance.SetManager(this);

    // Init should contain initialization logic for this manager,
    // and references to other managers can be retrieved but not yet used,
    // because these managers might not be initialized yet.
    // For that, use post init
    public void Init();

    // In post init, you can use methods that rely on other managers being initialized.
    public void PostInit() { }
}