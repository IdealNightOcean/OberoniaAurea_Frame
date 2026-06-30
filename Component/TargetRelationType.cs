using System;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace OberoniaAurea_Frame;

/// <summary>
/// 目标关系类型
/// </summary>
[Flags]
public enum TargetRelationType
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,
    /// <summary>
    /// 自身
    /// </summary>
    Self = 1 << 0,
    /// <summary>
    /// 相同派系
    /// </summary>
    SameFaction = 1 << 1,

    /// <summary>
    /// 盟友派系
    /// </summary>
    Ally = 1 << 2,
    /// <summary>
    /// 中立派系
    /// </summary>
    Neutral = 1 << 3,
    /// <summary>
    /// 敌对派系
    /// </summary>
    Hostile = 1 << 4,

    /// <summary>
    /// 非自身派系，包含盟友派系、中立派系、敌对派系。
    /// </summary>
    NonSameFaction = Ally | Neutral | Hostile,
    /// <summary>
    /// 非敌对，包含自身、相同派系、盟友派系、中立派系。
    /// </summary>
    NonHostile = Self | SameFaction | Ally | Neutral,

    /// <summary>
    /// 全部，包含自身、相同派系、盟友派系、中立派系、敌对派系。
    /// </summary>
    All = Self | SameFaction | Ally | Neutral | Hostile
}
