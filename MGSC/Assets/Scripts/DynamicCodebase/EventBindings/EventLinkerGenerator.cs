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
        float end;
        float start = UnityEngine.Time.realtimeSinceStartup;
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

        fieldPaths.Sort();
        // UnityEngine.Debug.Log("Paths are : " + string.Join(", ", fieldPaths));
        hash = string.Join(", ", fieldPaths).GetHashCode();


        if (CheckFileForHashCode(OutputPath + ".cs", hash))
        {
            end = UnityEngine.Time.realtimeSinceStartup;
            // UnityEngine.Debug.Log("Regeneration aborted after " + (end-start) + "s");
            return;
        }

        MakeFile(fieldNames, fieldPaths, hash)
            .ToFile(OutputPath);
        end = UnityEngine.Time.realtimeSinceStartup;
        // UnityEngine.Debug.Log("Generation made in " + (end - start) + "s");
    }

    private static bool CheckFileForHashCode(string filePath, int hashcode)
    {
        string markup = "<HASHCODE>";
        if (!AssetDatabase.AssetPathExists(filePath))
            return false;

        string[] file = File.ReadAllLines(filePath);

        string hashString = "";
        foreach (var line in file)
        {
            if (line.Contains(markup))
            {
                hashString = line;
                continue;
            }
        }
        hashString = hashString.Replace("\n", "").Replace(" ", "").Replace(markup, "").Replace("//", "") ;
        // UnityEngine.Debug.Log("Found HashString is " + hashString + ", generated hashcode is " + hashcode);

        if (!int.TryParse(hashString, out int retrievedHash))
        {
            UnityEngine.Debug.LogWarning("Hashcode for '" + hashString + "' couldn't be read");
            return true;
        }
        
        if (retrievedHash != hashcode)
        {
            return false;
        }

        return true;
    }

    private static SourceGenerator MakeFile(List<string> eventNames, List<string> eventPaths, int hashcode=0)
    {
        ScriptGenerator result = new("EventLinkerExtension");

        result.Comment("<HASHCODE> " + hashcode.ToString());

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