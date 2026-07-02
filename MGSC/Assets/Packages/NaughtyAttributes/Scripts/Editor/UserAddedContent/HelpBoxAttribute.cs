using NaughtyAttributes;
using System;
using UnityEditor;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class HelpBoxAttribute : Attribute, INaughtyAttribute
{
    public string Message;
    public MessageType MessageType;
    public HelpBoxAttribute(string message, MessageType messageType=MessageType.Info)
    {
        Message = message;
        MessageType = messageType;
    }
}