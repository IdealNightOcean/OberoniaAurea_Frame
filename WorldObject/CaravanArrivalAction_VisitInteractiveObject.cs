using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

//右键菜单
public class CaravanArrivalAction_VisitInteractiveObject : CaravanArrivalAction
{
    protected WorldObject_InteractiveBase worldObject;

    public override string Label => "OAFrame_VisitObject".Translate(worldObject.Label);
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);
    public CaravanArrivalAction_VisitInteractiveObject() { }

    /// <summary>
    /// 使用交互世界对象初始化远行队到达动作。
    /// </summary>
    public CaravanArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject) => this.worldObject = worldObject;

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (worldObject is not null && worldObject.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(worldObject);
    }

    public override void Arrived(Caravan caravan) => worldObject.Notify_CaravanArrived(caravan);


    /// <summary>
    /// 检查是否可以访问交互世界对象。
    /// </summary>
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject) => worldObject?.Spawned ?? false;

    /// <summary>
    /// 获取浮动菜单选项。
    /// </summary>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OAFrame_VisitObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <summary>
    /// 获取带接受报告的浮动菜单选项。
    /// </summary>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OAFrame_VisitObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, nameof(worldObject));
    }
}

public class CaravanArrivalAction_VisitInteractiveObject_Muti : CaravanArrivalAction
{
    protected WorldObject_MutiInteractiveBase worldObject;
    protected int visitType;
    public override string Label => "OAFrame_VisitObject".Translate(worldObject.Label);
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);

    public CaravanArrivalAction_VisitInteractiveObject_Muti() { }

    /// <summary>
    /// 使用多交互世界对象和访问类型初始化远行队到达动作。
    /// </summary>
    public CaravanArrivalAction_VisitInteractiveObject_Muti(WorldObject_MutiInteractiveBase worldObject, int visitTypeInt)
    {
        this.worldObject = worldObject;
        visitType = visitTypeInt;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (worldObject is not null && worldObject.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(worldObject);
    }

    public override void Arrived(Caravan caravan) => worldObject.Notify_CaravanArrived(caravan, visitType);

    /// <summary>
    /// 检查是否可以访问交互世界对象。
    /// </summary>
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject) => worldObject?.Spawned ?? false;

    /// <summary>
    /// 获取浮动菜单选项。
    /// </summary>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OAFrame_VisitObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <summary>
    /// 获取带接受报告的浮动菜单选项。
    /// </summary>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OAFrame_VisitObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, nameof(worldObject));
    }
}