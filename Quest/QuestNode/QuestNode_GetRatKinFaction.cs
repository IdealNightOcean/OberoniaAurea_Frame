using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

public class QuestNode_GetRatkinFaction : QuestNode_GetFaction
{
    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        return faction.IsRatkinFaction() && base.IsGoodFaction(faction, slate);
    }
}