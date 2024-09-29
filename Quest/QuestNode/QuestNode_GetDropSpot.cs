using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GetDropSpot : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    public SlateRef<bool> centerClose;
    public SlateRef<float> minDistanceFromEdge;

    protected override bool TestRunInt(Slate slate)
    {
        if (slate.Exists(storeAs.GetValue(slate)))
        {
            return true;
        }
        if (TryFindDropSpot(slate, out IntVec3 spawnSpot))
        {
            slate.Set(storeAs.GetValue(slate), spawnSpot);
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.Exists(storeAs.GetValue(slate)) && TryFindDropSpot(slate, out IntVec3 dropSpot))
        {
            slate.Set(storeAs.GetValue(slate), dropSpot);
        }
    }

    private bool TryFindDropSpot(Slate slate, out IntVec3 dropSpot)
    {
        Map map = slate.Get<Map>("map");
        if (map == null)
        {
            dropSpot = IntVec3.Invalid;
            return false;
        }
        if (centerClose.GetValue(slate))
        {
            if (DropCellFinder.TryFindRaidDropCenterClose(out dropSpot, map))
            {
                return true;
            }
            else
            {
                dropSpot = DropCellFinder.FindRaidDropCenterDistant(map, true);
                return dropSpot != IntVec3.Invalid;
            }
        }
        if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) && !x.Fogged(map) && (float)x.DistanceToEdge(map) >= minDistanceFromEdge.GetValue(slate) && map.reachability.CanReachColony(x), map, 1000, out dropSpot))
        {
            return true;
        }
        return false;
    }
}