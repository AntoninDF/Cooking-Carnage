

using System.ComponentModel;

namespace SourceGen
{
    public class PropertyGenerator<T> : SourceGenerator
    {
        #region Fields
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
        public bool IsStatic { get; set; }
        public SetAccessor SetAccessor { get; set; } = SetAccessor.Set;
        public string DefaultValue { get; set; }

        #endregion

        #region Methods
        public PropertyGenerator
            (
            string name, 
            AccessModifier modifier = AccessModifier.Public,
            bool isStatic=false, 
            SetAccessor setAccessor=SetAccessor.Set,
            string defaultValue = ""
            )
        {
            this.name = name;
            AccessModifier = modifier;
            SetAccessor = setAccessor;
            IsStatic = isStatic;
            DefaultValue = defaultValue;
        }
            
        /// <summary>
        /// Only supports auto-properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string Access = "", Static = "", Type = "", Set = "", value = "";

            if (GetKeyword(AccessModifier) != "")
                Access = GetKeyword(AccessModifier) + " ";
            if (IsStatic)
                Static = "static ";
            if (GetKeyword(SetAccessor) != "")
                Set = GetKeyword(SetAccessor) + ";";
            Type = GetKeyword<T>() + " ";
            if (!string.IsNullOrEmpty(DefaultValue))
                value = " = " + DefaultValue + ";";

            string ending = "";
            if (SetAccessor == SetAccessor.None)
                ending = " => " + DefaultValue + ";";
            else
                ending = " { get; " + Set + "}" + value;

                return Access + Static + Type + name + ending;
        }
        #endregion
    }
}