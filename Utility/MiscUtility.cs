using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame.Utility;

/// <summary> 
/// 杂项工具类。
///  </summary>
[StaticConstructorOnStartup]
public static class OAFrame_MiscUtility
{
    /// <summary>
    /// 检查两个<see cref="Def"/>是否相同且非空。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSameDefNonNullable<T>(this T def, T other) where T : Def => def is not null && def == other;
}