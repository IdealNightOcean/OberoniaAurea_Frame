using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 初始化类型物品交易请求
/// </summary>
public class QuestNode_InitiateCategoryTradeRequest : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<ThingCategoryDef> requestedCategoryDef;
    public SlateRef<int> requestedCount;
    public SlateRef<WorldObject> worldObject;
    public SlateRef<int> duration;
    public SlateRef<bool> allowInsectMeat = false;
    public SlateRef<bool> allowHumanlikeMeat = false;

    protected override bool TestRunInt(Slate slate)
    {
        if (requestedCount.GetValue(slate) <= 0 || requestedCategoryDef.GetValue(slate) is null)
            return false;
        if (duration.GetValue(slate) <= 0)
            return false;
        CategoryTradeRequestComp categoryTradeRequestComp = worldObject.GetValue(slate)?.GetComponent<CategoryTradeRequestComp>();
        if (categoryTradeRequestComp is null || categoryTradeRequestComp.ActiveRequest)
            return false;

        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_InitiateCategoryTradeRequest questPart_InitiateCategoryTradeRequest = new()
        {
            worldObject = worldObject.GetValue(slate),
            requestedCategoryDef = requestedCategoryDef.GetValue(slate),
            requestedCount = requestedCount.GetValue(slate),
            requestDuration = duration.GetValue(slate),
            requestAllowInsectMeat = allowInsectMeat.GetValue(slate),
            requestAllowHumanlikeMeat = allowHumanlikeMeat.GetValue(slate),
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_InitiateCategoryTradeRequest);
    }

}

/// <summary> 
/// 任务部件：初始化类型物品交易请求。 
/// </summary>
public class QuestPart_InitiateCategoryTradeRequest : QuestPart
{
    public string inSignal;
    public WorldObject worldObject;
    public ThingCategoryDef requestedCategoryDef;
    public int requestedCount;
    public int requestDuration;
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
            if (worldObject is not null)
            {
                yield return worldObject;
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
            if (worldObject.Faction is not null)
            {
                yield return worldObject.Faction;
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
            CategoryTradeRequestComp requestComp = worldObject.GetComponent<CategoryTradeRequestComp>();
            if (requestComp is not null)
            {
                if (requestComp.ActiveRequest)
                {
                    Log.Error("WorldObject " + worldObject.Label + " already has an active category trade request.");
                    return;
                }
                requestComp.InitTradeRequest(requestedCategoryDef, requestedCount, requestDuration, requestAllowInsectMeat, requestAllowHumanlikeMeat);
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        worldObject?.GetComponent<CategoryTradeRequestComp>()?.Disable();
        requestedCategoryDef = null;
        requestedCount = 0;
        requestDuration = 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, nameof(inSignal));
        Scribe_References.Look(ref worldObject, nameof(worldObject));
        Scribe_Defs.Look(ref requestedCategoryDef, nameof(requestedCategoryDef));
        Scribe_Values.Look(ref requestedCount, nameof(requestedCount), 0);
        Scribe_Values.Look(ref requestDuration, nameof(requestDuration), 0);
        Scribe_Values.Look(ref requestAllowInsectMeat, nameof(requestAllowInsectMeat), defaultValue: false);
        Scribe_Values.Look(ref requestAllowHumanlikeMeat, nameof(requestAllowHumanlikeMeat), defaultValue: false);
    }
}