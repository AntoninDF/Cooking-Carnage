
public class EventContext
{
    public EventContextType Type => _type;
    public int intValue;
    public string stringValue;
    public float floatValue;
    public bool boolValue;
    public object objectValue;

    private EventContextType _type;

    public EventContext(int value)
    {
        intValue = value;
        _type = EventContextType.Int;
    }
    public EventContext(string value)
    {
        stringValue = value;
        _type = EventContextType.String;
    }
    public EventContext(float value)
    {
        floatValue = value;
        _type = EventContextType.Float;
    }
    public EventContext(bool value)
    {
        boolValue = value;
        _type = EventContextType.Bool;
    }
    public EventContext(object value)
    {
        objectValue = value;
        _type = EventContextType.Object;
    }

    public enum EventContextType
    {
        Int,
        Float,
        String,
        Bool,
        Object
    }
}

