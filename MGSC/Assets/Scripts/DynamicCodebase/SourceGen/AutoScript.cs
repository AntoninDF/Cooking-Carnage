
using System;
using System.Reflection;
using UnityEditor;

namespace SourceGen
{
    [InitializeOnLoad]
    public static class AutoScript
    {
        private static DateTime lastUpdateTime = DateTime.MinValue;
        static AutoScript()
        {
            if (DateTime.Now > lastUpdateTime.AddSeconds(10))
                Regenerate();
        }

        private static void Regenerate()
        {
            UnityEngine.Debug.Log("Regenerating");
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(var type in asm.GetTypes())
                {
                    foreach(var method  in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<RunOnDomainReloadAttribute>() != null)
                        {
                            if (!method.IsStatic)
                                throw new Exception("Methods marked with 'RunOnDomainReload' attribute must be static : " + type.Name + " => " + method.Name);
                            method.Invoke(null, null);

                        }
                    }
                }
            }
        }
    }
}