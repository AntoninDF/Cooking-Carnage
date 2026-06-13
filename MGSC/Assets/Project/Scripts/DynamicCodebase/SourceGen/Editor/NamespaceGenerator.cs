

namespace SourceGen
{
    public class NamespaceGenerator : ScopedSourceGenerator
    {
        public NamespaceGenerator(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return "namespace " + name + base.ToString();
        }
    }
}