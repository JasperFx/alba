using System.Collections.Concurrent;
using System.Reflection;

namespace Alba.Internal;

internal static class TypeMemberCache
{
    private static readonly ConcurrentDictionary<Type, (PropertyInfo[] Properties, FieldInfo[] Fields)> _members = new();

    public static (PropertyInfo[] Properties, FieldInfo[] Fields) MembersOf(Type type)
    {
        return _members.GetOrAdd(type, t => (
            t.GetProperties().Where(x => x.CanRead).ToArray(),
            t.GetFields()));
    }
}
