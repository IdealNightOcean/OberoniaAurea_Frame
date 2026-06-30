using System;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 交换地图后销毁的组件属性。
/// </summary>
public class CompProperties_DestroyAfterSwapMap : CompProperties
{
    public CompProperties_DestroyAfterSwapMap() => compClass = typeof(CompDestroyAfterSwapMap);
}

/// <summary>
/// 切换地图后自动销毁的组件。
/// </summary>
public class CompDestroyAfterSwapMap : ThingComp
{
    public override void PostSwapMap()
    {
        if (parent.Spawned && !parent.Destroyed)
            parent.Destroy();
    }
}

/// <summary>
/// （已废弃）交换地图后销毁的组件属性（旧命名）。
/// </summary>
[Obsolete("曾经错误的命名，应使用 CompProperties_DestroyAfterSwapMap")]
public class CompProperties_DestoryAfterSwapMap : CompProperties
{
    public CompProperties_DestoryAfterSwapMap() => compClass = typeof(CompDestoryAfterSwapMap);
}

[Obsolete("曾经错误的命名，应使用 CompProperties_DestroyAfterSwapMap")]
/// <summary>
/// （已废弃）交换地图后自动销毁的组件。
/// </summary>
public class CompDestoryAfterSwapMap : ThingComp
{
    public override void PostSwapMap()
    {
        if (parent.Spawned && !parent.Destroyed)
            parent.Destroy();
    }
}