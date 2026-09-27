using System;

namespace NaughtyAttributes
{
    public enum EInfoBoxType
    {
        Normal,
        Warning,
        Error
    }

    /// <summary>
    /// Draws a box with a message in the inspector. Can be used to display information, warnings or errors. Works for fields and classes. Can be used multiple times on the same field or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class InfoBoxAttribute : DrawerAttribute
    {
        public string Text { get; private set; }
        public EInfoBoxType Type { get; private set; }

        public InfoBoxAttribute(string text, EInfoBoxType type = EInfoBoxType.Normal)
        {
            Text = text;
            Type = type;
        }
    }
}
