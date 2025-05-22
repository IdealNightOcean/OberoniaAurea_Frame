using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

//右键菜单
public class CaravanArrivalAction_VisitInteractiveObject : CaravanArrivalAction
{
    protected WorldObject_InteractiveBase worldObject;

    public override string Label => "OAFrame_VisitInteractiveObject".Translate(worldObject.Label);
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);
    public CaravanArrivalAction_VisitInteractiveObject()
    { }

    public CaravanArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject)
    {
        this.worldObject = worldObject;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
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

    public override void Arrived(Caravan caravan)
    {
        worldObject.Notify_CaravanArrived(caravan);
    }
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject)
    {
        return worldObject?.Spawned ?? false;
    }
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OA_VisitInteractiveObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_InteractiveBase worldObject, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject(worldObject), (label ?? "OA_VisitInteractiveObject").Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, "worldObject");
    }
}

public class CaravanArrivalAction_VisitInteractiveObject_Muti : CaravanArrivalAction
{
    protected WorldObject_MutiInteractiveBase worldObject;
    protected int visitType;
    public override string Label => "OAFrame_VisitInteractiveObject".Translate(worldObject.Label);
    public override string ReportString => "CaravanVisiting".Translate(worldObject.Label);

    public CaravanArrivalAction_VisitInteractiveObject_Muti()
    { }

    public CaravanArrivalAction_VisitInteractiveObject_Muti(WorldObject_MutiInteractiveBase worldObject, int visitTypeInt)
    {
        this.worldObject = worldObject;
        this.visitType = visitTypeInt;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
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

    public override void Arrived(Caravan caravan)
    {
        worldObject.Notify_CaravanArrived(caravan, visitType);
    }
    public static FloatMenuAcceptanceReport CanVisit(WorldObject_InteractiveBase worldObject)
    {
        return worldObject?.Spawned ?? false;
    }
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(worldObject), () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OA_VisitInteractiveObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MutiInteractiveBase worldObject, int visitTypeInt, FloatMenuAcceptanceReport acceptanceReport, string label = null)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => acceptanceReport, () => new CaravanArrivalAction_VisitInteractiveObject_Muti(worldObject, visitTypeInt), label ?? "OA_VisitInteractiveObject".Translate(worldObject.Label), caravan, worldObject.Tile, worldObject);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, "worldObject");
    }
}