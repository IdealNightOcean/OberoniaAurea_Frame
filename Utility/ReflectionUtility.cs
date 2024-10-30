using System.Reflection;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class ReflectionUtility
{
    public static BindingFlags InstanceAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public static T GetFieldValue<T>(object obj, string name, T fallback)
    {
        object obj2 = (obj?.GetType().GetField(name, InstanceAttr))?.GetValue(obj);
        if (obj2 != null)
        {
            return (T)obj2;
        }
        return fallback;
    }
    public static void SetFieldValue(object obj, string name, object value)
    {
        (obj?.GetType().GetField(name, InstanceAttr))?.SetValue(obj, value);
    }
}