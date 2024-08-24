using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//统一影响全部派系关系（有QuestPart）
public class QuestNode_AllFactionsGoodwillChange : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<int> goodwillChange;
    public SlateRef<HistoryEventDef> historyEvent;
    public SlateRef<bool> canSendMessage = true;
    public SlateRef<bool> canSendHostilityLetter = true;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_AllFactionsGoodwillChange questPart_AllFactionsGoodwillChange = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            goodwillChange = goodwillChange.GetValue(slate),
            historyEvent = historyEvent.GetValue(slate),
            canSendMessage = canSendMessage.Equals(slate),
            canSendHostilityLetter = canSendHostilityLetter.Equals(slate)
        };
        QuestGen.quest.AddPart(questPart_AllFactionsGoodwillChange);
    }
}
