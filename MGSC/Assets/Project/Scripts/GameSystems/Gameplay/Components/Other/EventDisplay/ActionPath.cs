using System;
using System.Reflection;

[System.Serializable]
public class ActionPath
{
    public string ActionVariableName;
    /// <summary>
    /// Name of the action with the full path containing the base component and all the intermediate features, separated by '/'
    /// </summary>
    public string FullName => source == ActionSource.Global ? "Global/" + ActionVariableName : "Local/" + FeaturePath.ToString() + "/" + ActionVariableName;
    public Type Type => SerializedType.SystemType;
    public SerializableSystemType SerializedType;
    public FeaturePath FeaturePath;
    public enum ActionSource {Local, Global}
    public ActionSource source;

    public ActionPath(string actionVariableName, Type actionType, FeaturePath featurePath)
    {
        source = ActionSource.Local;
        this.ActionVariableName = actionVariableName;
        this.SerializedType = new SerializableSystemType(actionType);
        this.FeaturePath = featurePath;
    }

    public ActionPath(string name, Type actionType)
    {
        source = ActionSource.Global;
        this.ActionVariableName = name;
        this.SerializedType = new SerializableSystemType(actionType);
        this.FeaturePath = null;
    }

    public override string ToString()
    {
        string result = FeaturePath + "/" + ActionVariableName;
        return result;
    }

    public MemberInfo GetMemberInfo()
    {
        if (source == ActionSource.Global || FeaturePath == null) return null;
        if (FeaturePath.Type == null) return null; 
        var members = FeaturePath.Type.GetMember(ActionVariableName);
        if (members == null || members.Length == 0) return null;
        return members[0];
    }

    public bool IsValid()
    {
        if (source == ActionSource.Global ) return true;
        if (GetMemberInfo() == null) return false;
        if (FeaturePath.OwningObject == null) return false;
        return true;
    }
}
