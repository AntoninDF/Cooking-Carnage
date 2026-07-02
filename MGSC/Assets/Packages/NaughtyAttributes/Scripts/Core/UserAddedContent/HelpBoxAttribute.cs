using NaughtyAttributes;
using System;
using UnityEditor;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class HelpBoxAttribute : Attribute, INaughtyAttribute
{
    public string Message;
    public EInfoBoxType MessageType;
    public HelpBoxAttribute(string message, EInfoBoxType messageType = EInfoBoxType.Normal)
    {
        Message = message;
        MessageType = messageType;
    }
}