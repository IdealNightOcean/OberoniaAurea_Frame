using System;
using System.Runtime.CompilerServices;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 枚举工具类。 
/// </summary>
[Obsolete("请使用 OberoniaAurea_Frame.Utility 命名空间下同名工具类。")]
public static class OARO_EnumUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsFlag(this TargetRelationType value, TargetRelationType flag) => (value & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyFlag(this TargetRelationType value, TargetRelationType flag) => (value & flag) != 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsFlag(this RaceType value, RaceType flag) => (value & flag) == flag;

}
