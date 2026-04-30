using System.Collections;
using System.Reflection;

namespace DrakeToolbox.Reflection
{
    public static class ReflectionUtilities
    {
        public static bool IsCollection(this FieldInfo field)
        {
            return field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType));
        }
    }
}