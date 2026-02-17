
using System;
using System.Linq;

namespace SourceGen
{
    public abstract class SourceGenerator
    {
        #region Fields
        public string name = "";

        #endregion

        #region Methods

        public virtual void ToFile(string folderPath, string name) => ToFile(folderPath + "/" + name);
        public virtual void ToFile(string fullPath)
        {
            var script = new ScriptGenerator(name);
            script.Add(this);
            script.ToFile(fullPath);
        }
        public override abstract string ToString();
        public virtual string Indent(string text)
        {
            string result = text;
            result = "\t" + result;
            result = result.Replace("\n", "\n\t");

            return result;
        }
        public static string GetKeyword(AccessModifier modifier)
        {
            switch (modifier)
            {
                case AccessModifier.Public: return "public";
                case AccessModifier.Private: return "private";
                case AccessModifier.Protected: return "protected";
                case AccessModifier.Internal: return "internal";
            }
            return "";
        }
        public static string GetKeyword(OverrideModifier modifier)
        {
            switch (modifier)
            {
                case OverrideModifier.Abstract: return "abstract";
                case OverrideModifier.Virtual: return "virtual";
                case OverrideModifier.Override: return "override";
                case OverrideModifier.Sealed: return "sealed";
            }
            return "";
        }
        public static string GetKeyword(SetAccessor modifier)
        {
            switch (modifier)
            {
                case SetAccessor.Set: return "set";
                case SetAccessor.PrivateSet: return "private set";
                case SetAccessor.ProtectedSet: return "protected set";
            }
            return "";
        }
        public static string GetKeyword<T>() => GetKeyword(typeof(T));
        public static string GetKeyword(System.Type type, int recursionDepth = 3)
        {
            if (recursionDepth <= 0)
                throw new Exception("Recursion depth exceeded");

            switch (System.Type.GetTypeCode(type))
            {
                case System.TypeCode.Byte: return "byte";
                case System.TypeCode.SByte: return "sbyte";
                case System.TypeCode.Int16: return "short";
                case System.TypeCode.Int32: return "int";
                case System.TypeCode.Int64: return "long";
                case System.TypeCode.UInt16: return "ushort";
                case System.TypeCode.UInt32: return "uint";
                case System.TypeCode.UInt64: return "ulong";
                case System.TypeCode.Single: return "float";
                case System.TypeCode.Double: return "double";
                case System.TypeCode.Boolean: return "bool";
                //case System.TypeCode.Object: return "object";
                case System.TypeCode.Char: return "char";
                case System.TypeCode.String: return "string";
                case System.TypeCode.Decimal: return "decimal";
                case System.TypeCode.DateTime: return "DateTime";
            }

            var args = type.GetGenericArguments();
            string[] argNames = new string[args.Length];

            string result = type.Name;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    argNames[i] = GetKeyword(args[i], recursionDepth - 1);
                }
                result = result.Remove(result.Length - 2);
                result = result + "<" + string.Join(", ", argNames) + ">";
            }

            return result;
        }
        
        #endregion
    }
}