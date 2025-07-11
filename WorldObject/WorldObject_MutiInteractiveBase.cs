using RimWorld.Planet;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_MutiInteractiveBase : WorldObject_InteractiveBase
{
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Notify_CaravanArrived(caravan, 0);
    }

    public abstract void Notify_CaravanArrived(Caravan caravan, int visitType);

}