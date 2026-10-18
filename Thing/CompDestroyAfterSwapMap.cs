using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 交换地图后销毁的组件属性。
/// </summary>
public class CompProperties_DestroyAfterSwapMap : CompProperties
{
    /// <summary>
    /// 初始化切换地图后销毁的组件属性。
    /// </summary>
    public CompProperties_DestroyAfterSwapMap() => compClass = typeof(CompDestroyAfterSwapMap);
}

/// <summary>
/// 切换地图后自动销毁的组件。
/// </summary>
public class CompDestroyAfterSwapMap : ThingComp
{
    /// <inheritdoc/>
    public override void PostSwapMap()
    {
        if (parent.Spawned && !parent.Destroyed)
            parent.Destroy();
    }
}