using RimWorld.Planet;
using RimWorld.QuestGen;
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
