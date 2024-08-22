using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;
public class QuestNode_GetRatKinFaction : OberoniaAurea_Frame.QuestNode_GetFaction
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