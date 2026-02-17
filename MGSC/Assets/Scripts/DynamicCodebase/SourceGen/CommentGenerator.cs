

namespace SourceGen
{
    public class CommentGenerator : SourceGenerator
    {
        public CommentGenerator(string name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return "// " + name + "\n";
        }
    }
}