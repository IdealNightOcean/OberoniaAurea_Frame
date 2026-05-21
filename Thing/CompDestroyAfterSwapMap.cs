using System;
using Verse;

namespace OberoniaAurea_Frame;

public class CompProperties_DestroyAfterSwapMap : CompProperties
{
    public CompProperties_DestroyAfterSwapMap() => compClass = typeof(CompDestroyAfterSwapMap);
}

public class CompDestroyAfterSwapMap : ThingComp
{
    public override void PostSwapMap()
    {
        if (parent.Spawned && !parent.Destroyed)
            parent.Destroy();
    }
}

[Obsolete("曾经错误的命名，应使用 CompProperties_DestroyAfterSwapMap")]
public class CompProperties_DestoryAfterSwapMap : CompProperties
{
    public CompProperties_DestoryAfterSwapMap() => compClass = typeof(CompDestoryAfterSwapMap);
}

[Obsolete("曾经错误的命名，应使用 CompProperties_DestroyAfterSwapMap")]
public class CompDestoryAfterSwapMap : ThingComp
{
    public override void PostSwapMap()
    {
        if (parent.Spawned && !parent.Destroyed)
            parent.Destroy();
    }
}