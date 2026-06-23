/// <summary>
/// Mark a static Action member as a global event, accessible from any EventDisplay in the scene
/// The action MUST be static
/// </summary>
public class GlobalEventAttribute : System.Attribute
{
    public GlobalEventAttribute(string name)
        { this.Name = name; }
    public string Name { get; set; }
}