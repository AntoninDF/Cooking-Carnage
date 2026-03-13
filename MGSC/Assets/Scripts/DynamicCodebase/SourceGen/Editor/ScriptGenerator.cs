

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace SourceGen
{
    public class ScriptGenerator : ScopedSourceGenerator
    {
        #region Methods

        public ScriptGenerator(string name)
        {
            this.name = name;
        }
        public override void ToFile(string folderPath, string name)
        {
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(name))
                throw new ArgumentNullException("folderPath or name was null");
            if (AssetDatabase.AssetPathExists(folderPath))
            {
                string fullPath = Path.Combine(folderPath, name + ".cs");
                using (var text = File.CreateText(fullPath))
                {
                    text.WriteLine(ToString());
                }
                AssetDatabase.Refresh();
                return;
            }
            else
            {
                throw new ArgumentException("Invalid path given : " + folderPath);
            }
        }
        public override void ToFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath was null");

            var path = fullPath.Split("/");
            string name = path[path.Length - 1];
            string folderPath = "";
            if (path.Length > 1)
            {
                string[] fPath = new string[path.Length - 1];
                for (int i = 0; i < fPath.Length; i++) fPath[i] = path[i];
                folderPath = string.Join("/", fPath);
            }

            ToFile(folderPath, name);
        }

        public override string ToString()
        {
            string result = "";

            foreach (var source in sources)
            {
                result += source.ToString();
                result += "";
            }
            return result;
        }

        public virtual void Using(string Namespace)
        {
            AddFirst(new UsingNamespaceGenerator(Namespace));
        }
        //public virtual void Namespace(string Namespace) => Add(new NamespaceGenerator(Namespace));
        #endregion
    }
}