

using System.Collections.Generic;
using System.Linq;

namespace SourceGen
{
    public class UsingNamespaceGenerator : SourceGenerator
    {
        protected List<string> namespaces = new();
        public UsingNamespaceGenerator(string Namespace)
        {
            this.name = Namespace;
            Add(Namespace);
        }
        public UsingNamespaceGenerator(params string[] namespaces) => Add(namespaces);
        public UsingNamespaceGenerator(params System.Type[] requiredTypes) => Add(requiredTypes);

        public void Add(string Namespace)
        {
            this.namespaces.Add(Namespace); 
        }
        public void Add(params string[] namespaces)
        {
            this.namespaces = this.namespaces.Concat(namespaces).ToList();
        }
        public void Add(params System.Type[] requiredTypes)
        {
            this.namespaces = this.namespaces.Concat(requiredTypes.Select((type) => type.Namespace)).ToList();
        }

        public override string ToString()
        {
            string result = "";
            foreach (var type in namespaces.ToHashSet())
            {
                result += "using " + type + ";\n";
            }
            return result;
        }
    }
}