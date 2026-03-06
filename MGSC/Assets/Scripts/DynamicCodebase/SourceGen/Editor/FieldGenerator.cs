

using System.ComponentModel;

namespace SourceGen
{
    public class FieldGenerator<T> : SourceGenerator
    {
        #region Fields
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
        public bool IsStatic { get; set; }
        public string DefaultValue { get; set; }

        #endregion

        #region Methods
        public FieldGenerator
            (
            string name,
            AccessModifier modifier = AccessModifier.Public,
            bool isStatic = false,
            string defaultValue = ""
            )
        {
            this.name = name;
            AccessModifier = modifier;
            IsStatic = isStatic;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Only supports auto-properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string Access = "", Static = "", Type = "", value = "";

            if (GetKeyword(AccessModifier) != "")
                Access = GetKeyword(AccessModifier) + " ";
            if (IsStatic)
                Static = "static ";
            Type = GetKeyword<T>() + " ";
            if (!string.IsNullOrEmpty(DefaultValue))
                value = " = " + DefaultValue;


            return Access + Static + Type + name + value + ";";
        }
        #endregion
    }
}