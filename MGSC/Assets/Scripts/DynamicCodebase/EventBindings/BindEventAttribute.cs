public class BindEventAttribute : System.Attribute
{
    public BindEventAttribute(string name)
        { this.Name = name; }
    public string Name { get; set; }
}