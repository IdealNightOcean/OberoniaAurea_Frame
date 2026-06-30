using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

/// <summary> 运输舱访问交互世界对象的动作。 </summary>
public class TransportersArrivalAction_VisitInteractiveObject : TransportersArrivalAction_FormCaravan
{
    protected WorldObject_InteractiveBase worldObject;

    public TransportersArrivalAction_VisitInteractiveObject() { }

    /// <summary>
    /// 使用交互世界对象初始化载具到达动作。
    /// </summary>
    public TransportersArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject)
    {
        this.worldObject = worldObject;
    }

    /// <summary>
    /// 使用交互世界对象和到达消息键初始化载具到达动作。
    /// </summary>
    public TransportersArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject, string arrivalMessageKey) : base(arrivalMessageKey)
    {
        this.worldObject = worldObject;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, nameof(worldObject));
    }

    public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        Pawn anyPawn = null;
        for (int i = 0; i < transporters.Count; i++)
        {
            foreach (Thing item in transporters[i].GetDirectlyHeldThings())
            {
                if (item is Pawn p)
                {
                    anyPawn = p;
                    break;
                }
            }
        }

        base.Arrived(transporters, tile);

        if (anyPawn is not null)
        {
            Caravan caravan = anyPawn.GetCaravan();
            if (caravan is not null)
            {
                worldObject?.Notify_CaravanArrived(caravan);
            }
        }
    }

    /// <summary>
    /// 获取浮动菜单选项。
    /// </summary>
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, WorldObject_InteractiveBase worldObject)
    {
        foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanFormCaravanAt(pods, worldObject.Tile),
                                                                                                         arrivalActionGetter: () => new TransportersArrivalAction_VisitInteractiveObject(worldObject),
                                                                                                         label: "VisitSettlement".Translate(worldObject.Label),
                                                                                                         launchAction: launchAction,
                                                                                                         destinationTile: worldObject.Tile))
        {
            yield return floatMenuOption;
        }
    }
}
