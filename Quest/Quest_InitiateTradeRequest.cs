using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;
/// <summary> 
/// 任务节点：初始化交易请求。 
/// </summary>
public class QuestNode_InitiateTradeRequest : QuestNode
{
    /// <summary>
    /// 触发信号。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> inSignal;

    /// <summary>
    /// 目标世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;

    /// <summary>
    /// 请求的物品定义。
    /// </summary>
    public SlateRef<ThingDef> requestedThingDef;

    /// <summary>
    /// 请求的物品数量。
    /// </summary>
    public SlateRef<int> requestedThingCount;

    /// <summary>
    /// 请求持续时间。
    /// </summary>
    public SlateRef<int> duration;

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_InitiateTradeRequest part = new QuestPart_InitiateTradeRequest
        {
            worldObject = worldObject.GetValue(slate),
            requestedThingDef = requestedThingDef.GetValue(slate),
            requestedCount = requestedThingCount.GetValue(slate),
            requestDuration = duration.GetValue(slate),
            keepAfterQuestEnds = false,
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(part);
    }

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
    }
}

/// <summary> 
/// 任务部件：初始化交易请求。 
/// </summary>
public class QuestPart_InitiateTradeRequest : QuestPart
{
    /// <summary>
    /// 触发信号。
    /// </summary>
    public string inSignal;

    /// <summary>
    /// 目标世界对象。
    /// </summary>
    public WorldObject worldObject;

    /// <summary>
    /// 请求的物品定义。
    /// </summary>
    public ThingDef requestedThingDef;

    /// <summary>
    /// 请求的物品数量。
    /// </summary>
    public int requestedCount;

    /// <summary>
    /// 请求持续时间。
    /// </summary>
    public int requestDuration;

    /// <summary>
    /// 是否在任务结束后保留交易请求。
    /// </summary>
    public bool keepAfterQuestEnds;

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        if (!keepAfterQuestEnds)
        {
            TradeRequestComp component = worldObject.GetComponent<TradeRequestComp>();
            if (component is not null && component.ActiveRequest)
            {
                component.Disable();
            }
        }
        inSignal = null;
        requestedCount = 0;
        requestDuration = 0;
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, nameof(inSignal));
        Scribe_References.Look(ref worldObject, nameof(worldObject));
        Scribe_Defs.Look(ref requestedThingDef, nameof(requestedThingDef));
        Scribe_Values.Look(ref requestedCount, nameof(requestedCount), 0);
        Scribe_Values.Look(ref requestDuration, nameof(requestDuration), 0);
        Scribe_Values.Look(ref keepAfterQuestEnds, nameof(keepAfterQuestEnds), defaultValue: false);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override IEnumerable<Faction> InvolvedFactions
    {
        get
        {
            foreach (Faction involvedFaction in base.InvolvedFactions)
            {
                yield return involvedFaction;
            }
            if (worldObject?.Faction is not null)
            {
                yield return worldObject.Faction;
            }
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            TradeRequestComp component = worldObject.GetComponent<TradeRequestComp>();
            if (component is not null)
            {
                if (component.ActiveRequest)
                {
                    Log.Error("WorldObject " + worldObject.Label + " already has an active trade request.");
                    return;
                }
                component.requestThingDef = requestedThingDef;
                component.requestCount = requestedCount;
                component.expiration = Find.TickManager.TicksGame + requestDuration;
            }
        }
    }
}
