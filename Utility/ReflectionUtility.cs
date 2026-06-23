using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_ReflectionUtility
{
    /// <summary>
    /// 实例绑定标志，用于反射获取实例成员。
    /// </summary>
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

    /// <summary>
    /// 通过反射创建对象的完整浅表副本，复制所有实例字段（包括私有字段）。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="source">源对象。</param>
    /// <returns>源对象的浅表副本；如果源对象为 <see langword="null"/>，则返回  <see langword="null"/>。</returns>
    public static T FullShallowCopy<T>(T source) where T : class
    {
        if (source is null)
            return null;

        Type sourceType = source.GetType();
        T dest = (T)Activator.CreateInstance(sourceType);
        FieldInfo[] fields = sourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            field.SetValue(dest, field.GetValue(source));
        }

        return dest;
    }
}