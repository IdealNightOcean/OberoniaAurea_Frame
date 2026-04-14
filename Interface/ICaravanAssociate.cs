using RimWorld.Planet;

namespace OberoniaAurea_Frame;

/// <summary>
/// 远行队（<see cref="Caravan"/>）关联接口。
/// </summary>
public interface ICaravanAssociate
{
    /// <summary>
    /// 通知远行队（<see cref="Caravan"/>）到达。
    /// </summary>
    void Notify_CaravanArrived(Caravan caravan);
}