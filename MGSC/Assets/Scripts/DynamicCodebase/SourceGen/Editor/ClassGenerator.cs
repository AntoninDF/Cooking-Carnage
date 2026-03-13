
using FMODUnity;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace SourceGen
{
    public class ClassGenerator : ScopedSourceGenerator
    {
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
        public OverrideModifier OverrideModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPartial { get; set; }
        protected System.Type InheritedClass { get; set; } = null;
        protected List<System.Type> Interfaces { get; set; } = new ();

        #region Methods
        public ClassGenerator(string name)
        {
            this.name = name;
        }
        public void InheritFrom<T>() where T : class => InheritFrom(typeof(T));
        public void InheritFrom(System.Type type)
        {
            if (!type.IsClass)
                throw new System.Exception("Cannot inherit from non-class types : " + type.FullName);
            if (type.IsInterface)
                throw new System.Exception("Cannot inherit from interfaces. Use Implement() instead : " + type.FullName);
            if (type.IsSealed)
                throw new System.Exception("Cannot inherit from sealed types : " + type.FullName);

            InheritedClass = type;
        }
        public void Implement<T>() => Implement(typeof(T));
        public void Implement(System.Type Interface)
        {
            var i = Interface;
            if (!i.IsInterface)
                throw new System.Exception("Cannot implement something other than an interface : " + i.FullName);

            Interfaces.Add(i);
        }

        public NamespaceGenerator WithNamespace(string Namespace)
        {
            var ns = new NamespaceGenerator(Namespace);
            ns.Add(this);
            return ns;
        }

        public void AddContructor(AccessModifier accessModifier, string content, params (string parameterType, string parameterName)[] parameters)
        {
            string formattedParameters = "";
            foreach (var parameter in parameters)
            {
                formattedParameters += parameter.parameterType + " " + parameter.parameterName + ", ";
            }
            if (formattedParameters != "") 
                formattedParameters = formattedParameters.Remove(formattedParameters.Length - 2, 2);
            Add(new RawTextGenerator("\n" + accessModifier.ToString() + name + "(" + ")" + "\n{" + "\n}"));
        }

        public override string ToString()
        {
            string Access = "", Override = "", InheritanceChain = "", Static = "", Partial = "";

            if (GetKeyword(AccessModifier) != "")
                Access = GetKeyword(AccessModifier) + " ";

            if (GetKeyword(OverrideModifier) != "")
                Override = GetKeyword(OverrideModifier) + " ";

            if (InheritedClass != null || Interfaces.Count > 0)
            {
                InheritanceChain = " : ";
                List<System.Type> types = new List<System.Type>();
                types.Add(InheritedClass);
                types = types.Concat(Interfaces).ToList();
                foreach(System.Type type in types)
                {
                    InheritanceChain += type.Name;
                    InheritanceChain += ", ";
                }
                InheritanceChain = InheritanceChain.Remove(InheritanceChain.Length - 2, 2);
            }

            if (IsStatic) Static = "static ";
            if (IsPartial) Partial = "partial ";

            return Access + Static + Override + Partial + "class " + name + InheritanceChain + base.ToString();
        }
        #endregion
    }
}