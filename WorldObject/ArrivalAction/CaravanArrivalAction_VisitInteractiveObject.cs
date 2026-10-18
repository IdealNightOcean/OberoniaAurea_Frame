using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 远行队访问交互世界对象的动作。
/// </summary>
public class CaravanArrivalAction_VisitInteractiveObject : CaravanArrivalAction
{
    /// <summary>
    /// 目标交互世界对象。
    /// </summary>
    protected WorldObject_InteractiveBase worldObject;

    /// <inheritdoc/>
    public override string Label => "OAFrame_VisitObject".Translate(worldObject.Label);
    /// <inheritdoc/>
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);
    /// <summary>
    /// 创建远行队访问交互世界对象的动作。
    /// </summary>
    public CaravanArrivalAction_VisitInteractiveObject() { }

    /// <summary>
    /// 使用交互世界对象初始化远行队到达动作。
    /// </summary>
    /// <param name="worldObject">目标交互世界对象。</param>
    public CaravanArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject) => this.worldObject = worldObject;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Arrived(Caravan caravan) => worldObject.Notify_CaravanArrived(caravan);


    /// <summary>
    /// 检查是否可以访问交互世界对象。
    /// </summary>
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject) => worldObject?.Spawned ?? false;

    /// <summary>
    /// 获取浮动菜单选项。
    /// </summary>
    /// <param name="caravan">远行队。</param>
    /// <param name="worldObject">目标交互世界对象。</param>
    /// <param name="label">自定义浮动菜单标签。</param>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OAFrame_VisitObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <summary>
    /// 获取带接受报告的浮动菜单选项。
    /// </summary>
    /// <param name="caravan">远行队。</param>
    /// <param name="worldObject">目标交互世界对象。</param>
    /// <param name="acceptanceReport">访问接受报告。</param>
    /// <param name="label">自定义浮动菜单标签。</param>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OAFrame_VisitObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, nameof(worldObject));
    }
}

/// <summary>
/// 远行队以指定类型访问多交互世界对象的动作。
/// </summary>
public class CaravanArrivalAction_VisitInteractiveObject_Muti : CaravanArrivalAction
{
    /// <summary>
    /// 目标交互世界对象。
    /// </summary>
    protected WorldObject_MutiInteractiveBase worldObject;
    /// <summary>
    /// 访问类型标识。
    /// </summary>
    protected int visitType;
    /// <inheritdoc/>
    public override string Label => "OAFrame_VisitObject".Translate(worldObject.Label);
    /// <inheritdoc/>
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);

    /// <summary>
    /// 创建按类型访问多交互世界对象的动作。
    /// </summary>
    public CaravanArrivalAction_VisitInteractiveObject_Muti() { }

    /// <summary>
    /// 使用多交互世界对象和访问类型初始化远行队到达动作。
    /// </summary>
    /// <param name="worldObject">目标多交互世界对象。</param>
    /// <param name="visitTypeInt">访问类型标识。</param>
    public CaravanArrivalAction_VisitInteractiveObject_Muti(WorldObject_MutiInteractiveBase worldObject, int visitTypeInt)
    {
        this.worldObject = worldObject;
        visitType = visitTypeInt;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Arrived(Caravan caravan) => worldObject.Notify_CaravanArrived(caravan, visitType);

    /// <summary>
    /// 检查是否可以访问交互世界对象。
    /// </summary>
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject) => worldObject?.Spawned ?? false;

    /// <summary>
    /// 获取浮动菜单选项。
    /// </summary>
    /// <param name="caravan">远行队。</param>
    /// <param name="worldObject">目标多交互世界对象。</param>
    /// <param name="visitTypeInt">访问类型标识。</param>
    /// <param name="label">自定义浮动菜单标签。</param>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OAFrame_VisitObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <summary>
    /// 获取带接受报告的浮动菜单选项。
    /// </summary>
    /// <param name="caravan">远行队。</param>
    /// <param name="worldObject">目标多交互世界对象。</param>
    /// <param name="visitTypeInt">访问类型标识。</param>
    /// <param name="acceptanceReport">访问接受报告。</param>
    /// <param name="label">自定义浮动菜单标签。</param>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OAFrame_VisitObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, nameof(worldObject));
    }
}