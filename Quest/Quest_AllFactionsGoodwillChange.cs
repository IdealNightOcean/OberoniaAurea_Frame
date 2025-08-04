using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
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
            canSendMessage = canSendMessage.GetValue(slate),
            canSendHostilityLetter = canSendHostilityLetter.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_AllFactionsGoodwillChange);
    }
}

public class QuestPart_AllFactionsGoodwillChange : QuestPart
{
    public string inSignal;

    public int goodwillChange;
    public HistoryEventDef historyEvent;
    public bool canSendMessage = true;
    public bool canSendHostilityLetter = true;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            Faction player = Faction.OfPlayer;
            foreach (Faction faction in Find.FactionManager.AllFactionsListForReading.Where(IsGoodFaction))
            {
                faction.TryAffectGoodwillWith(player, goodwillChange, canSendMessage, canSendHostilityLetter, historyEvent);
            }
        }
    }
    protected bool IsGoodFaction(Faction faction)
    {
        if (faction.defeated || !faction.HasGoodwill)
        {
            return false;
        }
        if (faction == Faction.OfPlayer)
        {
            return false;
        }
        return true;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        historyEvent = null;
        goodwillChange = 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref historyEvent, "historyEvent");
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref goodwillChange, "goodwillChange", 0);
        Scribe_Values.Look(ref canSendMessage, "canSendMessage", defaultValue: true);
        Scribe_Values.Look(ref canSendHostilityLetter, "canSendHostilityLetter", defaultValue: true);
    }
}