using System;

namespace OberoniaAurea_Frame;

/// <summary>
/// 目标种族类型
/// </summary>
[Flags]
public enum RaceType
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,
    /// <summary>
    /// 人类
    /// </summary>
    Humanlike = 1 << 0,
    /// <summary>
    /// 动物
    /// </summary>
    Animal = 1 << 1,
    /// <summary>
    /// 机械体
    /// </summary>
    Mechanoid = 1 << 2,
    /// <summary>
    /// 异常实体
    /// </summary>
    AnomalyEntity = 1 << 5,
    /// <summary>
    /// 无人机
    /// </summary>
    Drone = 1 << 6,

    /// <summary>
    /// 全部普通种族，包含人类、动物、机械体、异常实体。
    /// </summary>
    AllNormal = Humanlike | Animal | Mechanoid | AnomalyEntity,
    /// <summary>
    /// 全部种族，包含所有普通种族和无人机。
    /// </summary>
    All = AllNormal | Drone
}