using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_InitiateSaleRequest : QuestNode
{
    public SlateRef<ThingDef> requestedThingDef;
    public SlateRef<int> requestedThingCount;
    public SlateRef<Settlement> settlement;
    public SlateRef<int> duration;

    protected override bool TestRunInt(Slate slate)
    {
        return settlement.GetValue(slate) != null && requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) != null && duration.GetValue(slate) > 0;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_InitiateSaleRequest questPart_InitSaleRequest = new()
        {
            settlement = settlement.GetValue(slate),
            requestedThingDef = requestedThingDef.GetValue(slate),
            requestedCount = requestedThingCount.GetValue(slate),
            requestDuration = duration.GetValue(slate),
            inSignal = slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_InitSaleRequest);
    }

}
