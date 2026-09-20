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

    /// <summary>
    /// The public property and field values of the target as name/text pairs
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> ValuesOf(Type type, object? target,
        bool writablePropertiesOnly)
    {
        var (properties, fields) = MembersOf(type);

        foreach (var property in properties)
        {
            if (writablePropertiesOnly && !property.CanWrite) continue;

            yield return new(property.Name, property.GetValue(target)?.ToString() ?? string.Empty);
        }

        foreach (var field in fields)
        {
            yield return new(field.Name, field.GetValue(target)?.ToString() ?? string.Empty);
        }
    }
}
