using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_InitiateTradeRequest : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<WorldObject> worldObject;

    public SlateRef<ThingDef> requestedThingDef;

    public SlateRef<int> requestedThingCount;

    public SlateRef<int> duration;

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

    protected override bool TestRunInt(Slate slate)
    {
        return requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
    }
}

public class QuestPart_InitiateTradeRequest : QuestPart
{
    public string inSignal;

    public WorldObject worldObject;

    public ThingDef requestedThingDef;

    public int requestedCount;

    public int requestDuration;

    public bool keepAfterQuestEnds;

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

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref worldObject, "worldObject");
        Scribe_Defs.Look(ref requestedThingDef, "requestedThingDef");
        Scribe_Values.Look(ref requestedCount, "requestedCount", 0);
        Scribe_Values.Look(ref requestDuration, "requestDuration", 0);
        Scribe_Values.Look(ref keepAfterQuestEnds, "keepAfterQuestEnds", defaultValue: false);
    }

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
            if (worldObject?.Faction is not null)
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
