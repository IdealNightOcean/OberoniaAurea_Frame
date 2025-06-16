using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//初始化给予玩家型交易（有QuestPart）
public class QuestNode_InitiateSaleRequest : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<ThingDef> requestedThingDef;
    public SlateRef<int> requestedThingCount;
    public SlateRef<Settlement> settlement;
    public SlateRef<int> duration;

    protected override bool TestRunInt(Slate slate)
    {
        return settlement.GetValue(slate) is not null && requestedThingCount.GetValue(slate) > 0 && requestedThingDef.GetValue(slate) is not null && duration.GetValue(slate) > 0;
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
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_InitSaleRequest);
    }

}
