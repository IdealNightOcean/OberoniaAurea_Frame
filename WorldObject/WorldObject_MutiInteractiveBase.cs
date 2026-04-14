using RimWorld.Planet;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_MutiInteractiveBase : WorldObject_InteractiveBase
{
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Notify_CaravanArrived(caravan, 0);
    }

    /// <summary>
    /// 通知远行队（<see cref="Caravan"/>）到达（带访问类型）。
    /// </summary>
    public abstract void Notify_CaravanArrived(Caravan caravan, int visitType);
}