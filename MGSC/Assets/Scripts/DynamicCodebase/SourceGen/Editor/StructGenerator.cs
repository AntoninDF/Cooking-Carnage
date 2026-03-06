
using System.Collections.Generic;
using System.Linq;

namespace SourceGen
{
    public class StructGenerator : ScopedSourceGenerator
    {
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
        #region Methods
        public StructGenerator(string name)
        {
            this.name = name;
        }

        public void Add<T>(FieldGenerator<T> field) => sources.Add(field);
        public void Add<T>(PropertyGenerator<T> property) => sources.Add(property);


        public override string ToString()
        {
            string Access = "";

            if (GetKeyword(AccessModifier) != "")
                Access = GetKeyword(AccessModifier) + " ";

            return Access + "struct " + name + base.ToString();
        }
        #endregion
    }
}