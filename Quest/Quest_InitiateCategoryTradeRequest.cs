using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//初始化类型物品交易请求（有QuestPart）
public class QuestNode_InitiateCategoryTradeRequest : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<ThingCategoryDef> requestedCategoryDef;
    public SlateRef<int> requestedThingCount;
    public SlateRef<int> requestQuality = -1;
    public SlateRef<Settlement> settlement;
    public SlateRef<int> duration;
    public SlateRef<bool> requestIsMeat = false;
    public SlateRef<bool> requestAllowInsectMeat = false;
    public SlateRef<bool> requestAllowHumanlikeMeat = false;

    protected override bool TestRunInt(Slate slate)
    {
        return settlement.GetValue(slate) is not null && requestedThingCount.GetValue(slate) > 0 && requestedCategoryDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_InitiateCategoryTradeRequest questPart_InitiateCategoryTradeRequest = new()
        {
            settlement = settlement.GetValue(slate),
            requestedCategoryDef = requestedCategoryDef.GetValue(slate),
            requestedCount = requestedThingCount.GetValue(slate),
            requestDuration = duration.GetValue(slate),
            requestIsMeat = requestIsMeat.GetValue(slate),
            requestAllowInsectMeat = requestAllowInsectMeat.GetValue(slate),
            requestAllowHumanlikeMeat = requestAllowHumanlikeMeat.GetValue(slate),
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_InitiateCategoryTradeRequest);
    }

}

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
            if (settlement is not null)
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
            if (settlement.Faction is not null)
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
            if (component is not null)
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
        inSignal = null;
        settlement?.GetComponent<CategoryTradeRequestComp>()?.Disable();
        requestedCategoryDef = null;
        requestedCount = 0;
        requestDuration = 0;
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
            return component is not null && !component.ActiveRequest && x.Faction != Faction.OfPlayer;
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