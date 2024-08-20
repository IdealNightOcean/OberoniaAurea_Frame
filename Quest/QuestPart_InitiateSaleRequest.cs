using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//初始化给予玩家型交易comp
public class QuestPart_InitiateSaleRequest : QuestPart
{
    public string inSignal;
    public Settlement settlement;
    public ThingDef requestedThingDef;
    public int requestedCount;
    public int requestDuration;

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
            {
                yield return questLookTarget;
            }
            if (settlement != null)
            {
                yield return settlement;
            }
        }
    }

    public override IEnumerable<Faction> InvolvedFactions
    {
        get
        {
            foreach (Faction involvedFaction in base.InvolvedFactions)
            {
                yield return involvedFaction;
            }
            if (settlement.Faction != null)
            {
                yield return settlement.Faction;
            }
        }
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
    {
        get
        {
            foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
            {
                yield return hyperlink;
            }
            yield return new Dialog_InfoCard.Hyperlink(requestedThingDef);
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (!(signal.tag == inSignal))
        {
            return;
        }
        SaleRequestComp component = settlement.GetComponent<SaleRequestComp>();
        if (component != null)
        {
            if (component.ActiveRequest)
            {
                Log.Error("Settlement " + settlement.Label + " already has an active sale request.");
                return;
            }
            component.InitSaleRequest(requestedThingDef, requestedCount, requestDuration);
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        SaleRequestComp component = settlement.GetComponent<SaleRequestComp>();
        component?.Disable();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref settlement, "settlement");
        Scribe_Defs.Look(ref requestedThingDef, "requestedThingDef");
        Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
        Scribe_Values.Look(ref requestDuration, "requestDuration", 0);
    }

    public override void AssignDebugData()
    {
        base.AssignDebugData();
        inSignal = "DebugSignal" + Rand.Int;
        settlement = Find.WorldObjects.Settlements.Where(delegate (Settlement x)
        {
            SaleRequestComp component = x.GetComponent<SaleRequestComp>();
            return component != null && !component.ActiveRequest && x.Faction != Faction.OfPlayer;
        }).RandomElementWithFallback();
        settlement ??= Find.WorldObjects.Settlements.RandomElementWithFallback();
        requestedThingDef = ThingDefOf.Silver;
        requestedCount = 100;
        requestDuration = 60000;
    }
}
