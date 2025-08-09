using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public class MapParent_Enterable : MapParent, IQuestAssociate
{
    protected Quest quest;
    public Quest AssociatedQuest => quest;

    public void SetAssociatedQuest(Quest quest)
    {
        this.quest = quest;
    }

    public virtual FloatMenuAcceptanceReport CanEnterMap(IEnumerable<IThingHolder> pods)
    {
        if (this.EnterCooldownBlocksEntering())
        {
            return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(this.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
        }
        return true;
    }

    public virtual FloatMenuAcceptanceReport CanEnterMap(Caravan caravan)
    {
        if (this.EnterCooldownBlocksEntering())
        {
            return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(this.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
        }
        return true;
    }

    public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
    {
        alsoRemoveWorldObject = false;
        if (Map.mapPawns.AnyPawnBlockingMapRemoval)
        {
            return false;
        }
        foreach (PocketMapParent pocketMap in Find.World.pocketMaps.ToList())
        {
            if (pocketMap.sourceMap == Map && pocketMap.Map.mapPawns.AnyPawnBlockingMapRemoval)
            {
                return false;
            }
        }
        if (ModsConfig.OdysseyActive && Map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
        {
            return false;
        }
        if (TransporterUtility.IncomingTransporterPreventingMapRemoval(Map))
        {
            return false;
        }
        return true;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        if (HasMap && Find.WorldSelector.SingleSelectedObject == this)
        {
            yield return SettleInExistingMapUtility.SettleCommand(Map, requiresNoEnemies: true);
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }

        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_GenerateAndEnter.GetFloatMenuOptions(caravan, this))
        {
            yield return floatMenuOption2;
        }
    }

    public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
        {
            yield return transportersFloatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitEnterableMap.GetFloatMenuOptions(launchAction, pods, this))
        {
            yield return floatMenuOption;
        }
    }

    public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption shuttleFloatMenuOption in base.GetShuttleFloatMenuOptions(pods, launchAction))
        {
            yield return shuttleFloatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitEnterableMap.GetFloatMenuOptions(launchAction, pods, this))
        {
            yield return floatMenuOption;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref quest, "quest");
    }
}


