using System;

namespace SourceGen
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RunOnDomainReloadAttribute : Attribute { }
}