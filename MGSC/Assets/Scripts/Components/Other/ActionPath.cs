using System;
using System.Reflection;

[System.Serializable]
public class ActionPath
{
    public string ActionVariableName;
    /// <summary>
    /// Name of the action with the full path containing the base component and all the intermediate features, separated by '/'
    /// </summary>
    public string FullName => ToString();
    public Type Type => SerializedType.SystemType;
    public SerializableSystemType SerializedType;
    public FeaturePath FeaturePath;

    public ActionPath(string actionVariableName, Type actionType, FeaturePath featurePath)
    {
        this.ActionVariableName = actionVariableName;
        this.SerializedType = new SerializableSystemType(actionType);
        this.FeaturePath = featurePath;
    }

    public override string ToString()
    {
        string result = FeaturePath + "/" + ActionVariableName;
        return result;
    }

    public MemberInfo GetMemberInfo()
    {
        var members = FeaturePath.Type.GetMember(ActionVariableName);
        if (members == null || members.Length == 0) return null;
        return members[0];
    }

    public bool IsValid()
    {
        if (GetMemberInfo() == null) return false;
        if (FeaturePath.OwningObject == null) return false;
        return true;
    }
}
