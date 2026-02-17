

namespace SourceGen
{
    public enum AccessModifier
    {
        None,
        Public,
        Protected,
        Private,
        Internal
    }

    public enum OverrideModifier
    {
        None,
        Abstract,
        Virtual,
        Override,
        Sealed
    }

    public enum SetAccessor
    {
        None,
        Set,
        PrivateSet,
        ProtectedSet,
    }

    public interface IEmptyTest
    {

    }
}