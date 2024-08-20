using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//获取特定派系领导人
public class QuestNode_GetFactionLeader : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<Faction> faction;

    protected override bool TestRunInt(Slate slate)
    {
        Pawn leader = faction.GetValue(slate)?.leader;
        if (ValidLeader(leader))
        {
            slate.Set(storeAs.GetValue(slate), leader);
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction fVar = faction.GetValue(slate);
        Pawn leader = fVar?.leader;
        if (ValidLeader(leader))
        {
            QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
            questPart_InvolvedFactions.factions.Add(fVar);
            QuestGen.quest.AddPart(questPart_InvolvedFactions);
            QuestGen.slate.Set(storeAs.GetValue(slate), leader);
        }
    }

    private static bool ValidLeader(Pawn leader)
    {
        if (leader == null)
        {
            return false;
        }
        if (leader.Spawned && (leader.Downed || leader.IsPrisoner || !leader.Awake() || leader.InMentalState))
        {
            return false;
        }
        return true;
    }

}
