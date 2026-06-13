using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace DynamicCodebase
{

    public static class ScriptGenerator
    {
        #region Properties


        #endregion


        #region Methods
        private static string GetBoilerplate(string name)
        {
            string content = null;
            string path = Path.Combine(PathResolver.BoilerplatePath, name);
            if (AssetDatabase.AssetPathExists(path))
                content = File.ReadAllText(path);
            return content;
        }
        /// <summary>
        /// Takes in a boilerplate code, and replaces it's markups by the content
        /// </summary>
        /// <param name="boilerplate">The input code boilerplate that will get formatted</param>
        /// <param name="content">Specifies wich markups to replace by which content</param>
        /// <returns></returns>
        public static string Format(string boilerplate, Dictionary<string, string> content)
        {
            if (string.IsNullOrEmpty(boilerplate) || content == null)
                return string.Empty;

            List<KeyValuePair<string, string>> pairs = new();
            List<KeyValuePair<string, string>> sorted = new();
            foreach (var kvp  in content)
                if (kvp.Key != "")
                    pairs.Add(kvp);

            int pairCount = pairs.Count;

            // Make several sorting passes in case some keys contain each other, to evaluate the unnested ones first (this avoids breaking keys)
            while (sorted.Count < pairCount)
            {
                // For each key, check if it is contained by another one
                foreach (var kvp in pairs)
                {
                    bool isContained = false;
                    foreach (var kvp2 in pairs)
                    {
                        // Of course, don't check it against itself
                        if (kvp.Key == kvp2.Key) continue;
                        if (kvp2.Key.Contains(kvp.Key))
                        {
                            isContained = true;
                            continue;
                        }
                    }
                    // If the key isn't contained by another key, we can safely add it to the list to be evaluated before the nested ones
                    if (!isContained)
                        sorted.Add(kvp);
                }
                // Update the list of keys to be evaluated to only check the remaining ones
                foreach (var kvp in sorted)
                    if (pairs.Contains(kvp))
                        pairs.Remove(kvp);
            }

            string result = string.Copy(boilerplate);
            // Finally, apply the replacements in order
            foreach (var kvp in sorted)
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }

            return result;
        }
        public static string Format(string boilerplate, params (string, string)[] content)
        {
            Dictionary<string, string> dict = new ();
            foreach (var tuple in content)
                dict[tuple.Item1] = tuple.Item2;
            return Format(boilerplate, dict);
        }
        public static string Format(string boilerplate, string markup, string content ) => boilerplate.Replace(markup, content);
        public static string FormatWithNewMarkupLine(string boilerplate, string markup, string content) => boilerplate.Replace(markup, content + "\n\t" + markup);
        public static string AddUsingDirective(string boilerplate, string libraryName)
        {
            string result = string.Copy(boilerplate);
            result = "using " + libraryName + ";\n" + boilerplate;
            return result;
        }
        public static string AddNamespace(string boilerplate, string namespaceName)
        {
            string result = string.Copy(boilerplate);
            int firstDeclarationIndex = result.IndexOf("public");
            string libraries = result.Substring(0, firstDeclarationIndex-1);
            string content = result.Substring(firstDeclarationIndex );
            content = Indent(content);
            result = libraries + "\nnamespace " + namespaceName + "\n{\n\t" + content + "\n}";
            return result;
        }
        public static string Indent(string text) => text.Replace("\n", "\n\t");
        public static bool TrySaveToFile(string path, string name, string content)
        {
            if (AssetDatabase.AssetPathExists(path))
            {
                using (var text = File.CreateText(Path.Combine(path, name)))
                {
                    text.WriteLine(content);
                }
                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning("[DynamicCodebase] : The specified path does not exist : " + path);
                return false;
            }
        }
        #endregion
    }
}