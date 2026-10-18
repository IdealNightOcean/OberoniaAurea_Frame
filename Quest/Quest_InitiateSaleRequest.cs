using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 初始化给予玩家型交易
/// </summary>
public class QuestNode_InitiateSaleRequest : QuestNode
{
    /// <summary>
    /// 触发信号。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> inSignal;

    /// <summary>
    /// 请求的物品定义。
    /// </summary>
    public SlateRef<ThingDef> requestedThingDef;

    /// <summary>
    /// 请求的物品数量。
    /// </summary>
    public SlateRef<int> requestedCount;

    /// <summary>
    /// 目标世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;

    /// <summary>
    /// 请求持续时间。
    /// </summary>
    public SlateRef<int> duration;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return requestedCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
    }

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_InitiateSaleRequest questPart_InitSaleRequest = new()
        {
            worldObject = worldObject.GetValue(slate),
            requestedThingDef = requestedThingDef.GetValue(slate),
            requestedCount = requestedCount.GetValue(slate),
            requestedDuration = duration.GetValue(slate),
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_InitSaleRequest);
    }

}

/// <summary> 
/// 任务部件：初始化给予玩家型交易。 
/// </summary>
public class QuestPart_InitiateSaleRequest : QuestPart
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
    public int requestedDuration;

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
            if (worldObject.Faction is not null)
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
            SaleRequestComp requestComp = worldObject.GetComponent<SaleRequestComp>();
            if (requestComp is not null)
            {
                if (requestComp.ActiveRequest)
                {
                    Log.Error("WorldObject " + worldObject.Label + " already has an active sale request.");
                    return;
                }
                requestComp.InitSaleRequest(requestedThingDef, requestedCount, requestedDuration);
            }
        }
    }

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        worldObject?.GetComponent<SaleRequestComp>()?.Disable();
        worldObject = null;
        requestedThingDef = null;
        requestedCount = 0;
        requestedDuration = 0;
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, nameof(inSignal));
        Scribe_References.Look(ref worldObject, nameof(worldObject));
        Scribe_Defs.Look(ref requestedThingDef, nameof(requestedThingDef));
        Scribe_Values.Look(ref requestedCount, nameof(requestedCount), 0);
        Scribe_Values.Look(ref requestedDuration, nameof(requestedDuration), 0);
    }
}