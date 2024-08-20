using RimWorld;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//统一影响全部派系关系
public class QuestPart_AllFactionsGoodwillChange : QuestPart
{
    public string inSignal;

    public int goodwillChange;
    public HistoryEventDef historyEvent = null;
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
