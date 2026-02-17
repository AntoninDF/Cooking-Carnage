
using System;
using System.Reflection;
using UnityEditor;

namespace SourceGen
{
    [InitializeOnLoad]
    public static class AutoScript
    {
        static AutoScript()
        {
            Regenerate();
        }
        /// <summary>
        /// Scans assemblies and finds all public static methods marked with "RunOnDomainReload" and runs them
        /// </summary>
        /// <param name="allAssemblies">If true, scans every assembly. If false, scans only the current assembly (which is much faster)</param>
        /// <exception cref="Exception"></exception>
        private static void Regenerate(bool allAssemblies=false)
        {
            float start, end;
            start = UnityEngine.Time.realtimeSinceStartup;
            Assembly[] assemblies;

            if (allAssemblies)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            else
            {
                assemblies = new Assembly[1];
                assemblies[0] = Assembly.GetExecutingAssembly();
            }

            foreach (Assembly asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<RunOnDomainReloadAttribute>() != null)
                        {
                            if (!method.IsStatic)
                            {
                                UnityEngine.Debug.LogWarning("Methods marked with 'RunOnDomainReload' attribute must be static : " + type.Name + " => " + method.Name);
                                continue;
                            }
                            method.Invoke(null, null);

                        }
                    }
                }
            }
            end = UnityEngine.Time.realtimeSinceStartup;
            UnityEngine.Debug.Log("Regenerated automatic scripts in " + (end - start) + "s");
        }
    }
}