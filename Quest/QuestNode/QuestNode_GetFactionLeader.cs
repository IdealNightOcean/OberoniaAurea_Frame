using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取特定派系领导人。 
/// </summary>
public class QuestNode_GetFactionLeader : QuestNode
{
    /// <summary>
    /// 存储标签：派系领袖。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 目标派系。
    /// </summary>
    public SlateRef<Faction> faction;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Pawn leader = faction.GetValue(slate)?.leader;
        if (ValidLeader(leader))
        {
            slate.Set(storeAs.GetValue(slate), leader);
        }
    }

    private static bool ValidLeader(Pawn leader)
    {
        if (leader is null || leader.Dead)
        {
            return false;
        }
        if (!leader.Spawned)
        {
            return true;
        }
        if (leader.Downed || leader.IsPrisoner || !leader.Awake() || leader.InMentalState)
        {
            return false;
        }
        return true;
    }
}