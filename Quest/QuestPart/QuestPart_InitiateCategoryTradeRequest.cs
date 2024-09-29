using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//初始化 类型物品交易请求comp
public class QuestPart_InitiateCategoryTradeRequest : QuestPart
{
    public string inSignal;
    public Settlement settlement;
    public ThingCategoryDef requestedCategoryDef;
    public int requestedCount;
    public int requestDuration;
    public bool requestIsMeat;
    public bool requestAllowInsectMeat;
    public bool requestAllowHumanlikeMeat;

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
            yield return new Dialog_InfoCard.Hyperlink(requestedCategoryDef);
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            CategoryTradeRequestComp component = settlement.GetComponent<CategoryTradeRequestComp>();
            if (component != null)
            {
                if (component.ActiveRequest)
                {
                    Log.Error("Settlement " + settlement.Label + " already has an active category trade request.");
                    return;
                }
                component.InitTradeRequest(requestedCategoryDef, requestedCount, requestDuration, requestIsMeat, requestAllowInsectMeat, requestAllowHumanlikeMeat);
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        Log.Message("clean");
        SaleRequestComp component = settlement.GetComponent<SaleRequestComp>();
        component?.Disable();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref settlement, "settlement");
        Scribe_Defs.Look(ref requestedCategoryDef, "requestedCategoryDef");
        Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
        Scribe_Values.Look(ref requestDuration, "requestDuration", 0);
        Scribe_Values.Look(ref requestIsMeat, "requestIsMeat", defaultValue: false);
        Scribe_Values.Look(ref requestAllowInsectMeat, "requestAllowInsectMeat", defaultValue: false);
        Scribe_Values.Look(ref requestAllowHumanlikeMeat, "requestAllowHumanlikeMeat", defaultValue: false);
    }

    public override void AssignDebugData()
    {
        base.AssignDebugData();
        inSignal = "DebugSignal" + Rand.Int;
        settlement = Find.WorldObjects.Settlements.Where(delegate (Settlement x)
        {
            CategoryTradeRequestComp component = x.GetComponent<CategoryTradeRequestComp>();
            return component != null && !component.ActiveRequest && x.Faction != Faction.OfPlayer;
        }).RandomElementWithFallback();
        settlement ??= Find.WorldObjects.Settlements.RandomElementWithFallback();
        requestedCategoryDef = ThingCategoryDefOf.StoneBlocks;
        requestedCount = 100;
        requestDuration = 60000;
        requestIsMeat = false;
        requestAllowInsectMeat = false;
        requestAllowHumanlikeMeat = false;
    }
}
