using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取鼠族派系。 
/// </summary>
public class QuestNode_GetRatkinFaction : QuestNode_GetFaction
{
    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        return faction.IsRatkinFaction() && base.IsGoodFaction(faction, slate);
    }
}