

namespace SourceGen
{
    public class RawTextGenerator : SourceGenerator
    {
        public RawTextGenerator(string name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return name;
        }
    }
}