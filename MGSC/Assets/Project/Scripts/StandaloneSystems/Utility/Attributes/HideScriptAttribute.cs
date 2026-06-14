// Source - https://stackoverflow.com/a/49301462
// Posted by High, modified by community. See post 'Timeline' for change history
// Retrieved 2026-03-05, License - CC BY-SA 3.0

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class HideScriptFieldAttribute : Attribute { }
