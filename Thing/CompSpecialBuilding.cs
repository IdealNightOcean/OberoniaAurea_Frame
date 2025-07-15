using Verse;

namespace OberoniaAurea_Frame;

public class CompProperties_SpecialBuilding : CompProperties
{
    public CompProperties_SpecialBuilding()
    {
        compClass = typeof(CompSpecialBuilding);
    }
}

public class CompSpecialBuilding : ThingComp
{
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            parent.Map?.GetComponent<MapComponent_SpecialBuildingManager>()?.AddBuilding(parent.def);
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        map?.GetComponent<MapComponent_SpecialBuildingManager>()?.RemoveBuilding(parent.def);
    }
}
