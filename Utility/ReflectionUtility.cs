using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_ReflectionUtility
{
    public static BindingFlags InstanceAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    /// <summary>
    /// 获取对象字段值。
    /// </summary>
    public static T GetFieldValue<T>(object obj, string name, T fallback)
    {
        object obj2 = (obj?.GetType().GetField(name, InstanceAttr))?.GetValue(obj);
        if (obj2 is not null)
        {
            return (T)obj2;
        }
        return fallback;
    }

    /// <summary>
    /// 设置对象字段值。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFieldValue(object obj, string name, object value)
    {
        (obj?.GetType().GetField(name, InstanceAttr))?.SetValue(obj, value);
    }
}