using SourceGen;
using System;
using System.Collections.Generic;
using System.Reflection;

public class EventLinkerGenerator
{
    private static string OutputPath => "Assets/Scripts/DynamicCodebase/EventBindings/";

    [RunOnDomainReload]
    public static void RegenerateEventLinkerEvents()
    {
        List<BindEventAttribute> fields = new();
        List<string> fieldNames = new();
        List<string> fieldPaths = new();

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
                            UnityEngine.Debug.Log(member.Name + " has a valid type");
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

        MakeFile(fieldNames, fieldPaths)
            .ToFile(OutputPath + "EventLinkerExtension");

    }

    private static SourceGenerator MakeFile(List<string> eventNames, List<string> eventPaths)
    {
        ScriptGenerator result = new("EventLinkerExtension");

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