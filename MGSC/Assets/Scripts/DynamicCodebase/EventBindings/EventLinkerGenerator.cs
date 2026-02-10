using SourceGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;

public static class EventLinkerGenerator
{
    private static string OutputPath => "Assets/Scripts/DynamicCodebase/EventBindings/EventLinkerExtension";

    [RunOnDomainReload]
    public static void RegenerateEventLinkerEvents()
    {
        List<BindEventAttribute> fields = new();
        List<string> fieldNames = new();
        List<string> fieldPaths = new();
        int hash;

        // Get all the fields that should be replicated in the event linker
        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach(var type in asm.GetTypes())
            {
                foreach (var member in type.GetFields())
                {
                    var attr = member.GetCustomAttribute<BindEventAttribute>();
                    if (attr != null)
                    {
                        if (IsValidType(member))
                        {
                            if (string.IsNullOrEmpty(attr.Name))
                                throw new Exception("BindEvent name parameter cannot be empty : " + member.Name);
                            fields.Add(attr);
                        }
                    }
                }
            }
        }

        hash = fields.GetHashCode();
        if (CheckFileForHashCode(OutputPath, hash))
        {
            UnityEngine.Debug.Log("Regeneration aborted");
            return;
        }

        foreach (var field in fields)
        {
            string parsedName = field.Name.Split("/")[^1];

            // The EventLinker cannot have two events with the same name
            if (fieldNames.Contains(field.Name))
            {
                throw new Exception("The EventLinker cannot contain two events with the same name : " + field.Name);
            }
            fieldNames.Add(parsedName);
            fieldPaths.Add(field.Name);
        }

        MakeFile(fieldNames, fieldPaths, hash)
            .ToFile(OutputPath);
    }

    private static bool CheckFileForHashCode(string filePath, int hashcode)
    {
        if (!AssetDatabase.AssetPathExists(filePath))
            return false;

        string file = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(file))
            return false;
        if (!file.Contains("<HASHCODE>"))
            return false;

        return true;
    }

    private static SourceGenerator MakeFile(List<string> eventNames, List<string> eventPaths, int hashcode=0)
    {
        ScriptGenerator result = new("EventLinkerExtension");

        result.Comment("// <HASHCODE> " + hashcode.ToString());

        result.Using("UnityEngine");
        result.Using("UnityEngine.Events");
        result.Using("System.Collections.Generic");

        ClassGenerator Class = new("EventLinker");
        Class.IsPartial = true;
        Class.InheritFrom<UnityEngine.MonoBehaviour>();

        for (int i = 0; i< eventPaths.Count; i++)
        {
            eventPaths[i] = "\"" + eventPaths[i] + "\"";
        }
        string options = "\nnew() {\n\t" + string.Join(",\n\t", eventPaths) + "\n}";
        Class.Add(new PropertyGenerator<List<string>>("options", defaultValue:options, setAccessor:SetAccessor.None));

        foreach(var name in eventNames)
        {
            Class.Add(new FieldGenerator<UnityEngine.Events.UnityEvent>(name));
        }

        result.Add(Class);

        return result;
    }

    private static bool IsValidType(FieldInfo member)
    {
        if (member.FieldType == typeof(Action))
            return true;
        return false;
    }
}