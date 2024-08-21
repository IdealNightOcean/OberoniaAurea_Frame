using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;
public class QuestNode_GetRatKinFaction : QuestNode_GetFaction
{
    protected override bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (!faction.IsRatkinFaction())
        {
            return false;
        }
        return base.IsGoodFaction(faction, slate);
    }
}