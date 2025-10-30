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
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<ThingDef> requestedThingDef;
    public SlateRef<int> requestedCount;
    public SlateRef<WorldObject> worldObject;
    public SlateRef<int> duration;

    protected override bool TestRunInt(Slate slate)
    {
        return requestedCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
    }
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

//初始化给予玩家型交易comp
public class QuestPart_InitiateSaleRequest : QuestPart
{
    public string inSignal;
    public WorldObject worldObject;
    public ThingDef requestedThingDef;
    public int requestedCount;
    public int requestedDuration;

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
            yield return new Dialog_InfoCard.Hyperlink(requestedThingDef);
        }
    }

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

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref worldObject, "worldObject");
        Scribe_Defs.Look(ref requestedThingDef, "requestedThingDef");
        Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
        Scribe_Values.Look(ref requestedDuration, "requestedDuration", 0);
    }
}