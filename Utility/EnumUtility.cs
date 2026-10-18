using System.Runtime.CompilerServices;

namespace OberoniaAurea_Frame.Utility;

/// <summary> 
/// 枚举工具类。 
/// </summary>
public static class OAFrame_EnumUtility
{
    /// <summary>
    /// 检查 <see cref="TargetRelationType"/> 是否包含指定标志位。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsFlag(this TargetRelationType value, TargetRelationType flag) => (value & flag) == flag;

    /// <summary>
    /// 检查 <see cref="TargetRelationType"/> 是否包含任一指定标志位。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAnyFlag(this TargetRelationType value, TargetRelationType flag) => (value & flag) != 0;


    /// <summary>
    /// 检查 <see cref="RaceType"/> 是否包含指定标志位。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsFlag(this RaceType value, RaceType flag) => (value & flag) == flag;

}
