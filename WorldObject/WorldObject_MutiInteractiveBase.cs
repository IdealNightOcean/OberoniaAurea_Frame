using RimWorld.Planet;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_MutiInteractiveBase : WorldObject_InteractiveBase
{
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Notify_CaravanArrived(caravan, 0);
    }

    public abstract void Notify_CaravanArrived(Caravan caravan, int visitType);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
    }
}