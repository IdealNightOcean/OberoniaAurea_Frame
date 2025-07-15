using Verse;

namespace OberoniaAurea_Frame;

public class CompProperties_DestoryAfterSwapMap : CompProperties
{
    public CompProperties_DestoryAfterSwapMap()
    {
        compClass = typeof(CompDestoryAfterSwapMap);
    }
}

public class CompDestoryAfterSwapMap : ThingComp
{
    public override void PostSwapMap()
    {
        if (parent.Spawned)
        {
            parent.Destroy();
        }
    }
}
