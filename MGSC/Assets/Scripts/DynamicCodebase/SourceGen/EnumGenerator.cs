

using System.Collections.Generic;

namespace SourceGen
{
    public class EnumGenerator : ScopedSourceGenerator
    {
        public AccessModifier AccessModifier { get; set; }

        public EnumGenerator(string name)
        {
            this.name = name;
        }
        public EnumGenerator(string name, AccessModifier accessModifier)
        {
            this.name = name;
            AccessModifier = accessModifier;
        }
        public override void ToFile(string fullPath)
        {
            throw new System.NotImplementedException();
        }
        public override string ToString()
        {
            // Try to remove the trailing comma and endl on the last member
            SourceGenerator lastMember = null;
            if (sources.Count > 0)
                lastMember = sources[sources.Count - 1];
            if (lastMember != null && lastMember.name.EndsWith(","))
                lastMember.name = lastMember.name.Remove(lastMember.name.Length - 1);

            // Make the final string
            string result = GetKeyword(AccessModifier) + " enum " + name + base.ToString();

            // Add the trailing comma and endl back
            if (lastMember != null)
                lastMember.name += ",";

            return result;
        }

        public void Add(string memberName)
        {
            sources.Add(new RawTextGenerator(memberName + ","));
        }
    }
}