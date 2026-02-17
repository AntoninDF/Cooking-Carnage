

using System.Collections.Generic;

namespace SourceGen
{
    public abstract class ScopedSourceGenerator : SourceGenerator
    {

        protected List<SourceGenerator> sources = new();

        #region Methods


        public override string ToString()
        {
            string result = "";
            result += "\n{\n";
            foreach (var source in sources)
            {
                result += Indent(source.ToString());
                result += "\n";
            }
            result += "}";

            return result;
        }

        public virtual void Add(SourceGenerator element)
        {
            if (sources == null)
                sources = new();

            sources.Add(element);
        }

        public virtual void Comment(string comment) => Add(new CommentGenerator(comment));

        #endregion
    }
}