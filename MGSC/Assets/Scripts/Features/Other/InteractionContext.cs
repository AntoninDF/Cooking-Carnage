public class InteractionContext
{
    public object Interactor { get; set; }


    public InteractionContext(object interactor)
    {
        Interactor = interactor;
    }
}
