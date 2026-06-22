using Features;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class FeaturePath
{
    public string FeatureVariableName;
    public Type Type => SerializedType.SystemType;
    public SerializableSystemType SerializedType;
    [SerializeReference] public List<FeaturePath> Path; // The order is RootComponent -> This Feature
    public UnityEngine.Object OwningObject;

    public FeaturePath(string featureVariableName, SerializableSystemType type, List<FeaturePath> path, UnityEngine.Object owningComponent)
    {
        FeatureVariableName = featureVariableName;
        this.SerializedType = type;
        this.Path = path;
        OwningObject = owningComponent;
    }

    public List<ActionPath> GetActions()
    {
        List<ActionPath> result = new();

        foreach (var member in Type.GetMembers())
        {
            Type type = new PropertyOrFieldInfo(member).Type;
            if (type == null)
                continue;
            if (IsValidActionType(type))
            {
                ActionPath path = new(member.Name, type, this);
                result.Add(path);
            }
        }
        return result;
    }

    private bool IsValidActionType(Type type)
    {
        if (type == typeof(System.Action)) return true;
        Type generic = null;
        if (type.IsGenericTypeDefinition) generic = type;
        else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
        if (generic == null) return false;
        if (generic == typeof(System.Action<>)) return true;
        if (generic == typeof(System.Action<,>)) return true;
        if (generic == typeof(System.Action<,,>)) return true;
        return false;
    }

    public bool TryGetInstance(out Feature feature)
    {
        feature = null;
        if (OwningObject == null) return false;

        if (Path == null || Path.Count == 0)
        {
            return TryGetInstance(out feature, OwningObject as IFeatureCollection);
        }
        else
        {
            IFeatureCollection temp = OwningObject as IFeatureCollection;
            for (int i = 0; i < Path.Count; i++)
            {
                if (!Path[i].TryGetInstance(out feature, temp))
                    return false;
                temp = feature as IFeatureCollection;
            }
            TryGetInstance(out feature, temp);
        }
        if (feature  == null) return false;
        return true;
    }
    private bool TryGetInstance(out Feature feature, IFeatureCollection parent)
    {
        feature = null;
        if (parent == null) return false;

        MemberInfo[] featureInfos = parent.GetType().GetMember(FeatureVariableName);
        if (featureInfos == null || featureInfos.Length == 0) return false;

        var featureInfo = new PropertyOrFieldInfo(featureInfos[0]);
        feature = (Feature)featureInfo.GetValue(parent);
        if (feature == null) return false;

        return true;
    }

    public override string ToString()
    {
        string result = OwningObject.GetType().ToString() + "/";
        foreach (var feature in Path)
        {
            result += feature.FeatureVariableName;
            result += "/";
        }
        result += FeatureVariableName;
        return result;
    }

}
