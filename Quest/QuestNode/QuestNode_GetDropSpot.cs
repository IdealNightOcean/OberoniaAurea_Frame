using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取空投点位置。 
/// </summary>
public class QuestNode_GetDropSpot : QuestNode
{
    /// <summary>
    /// 存储标签：找到的空投落点。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;
    /// <summary>
    /// 是否优先使用交易点作为空投落点。
    /// </summary>
    public SlateRef<bool> useTradeSpot;
    /// <summary>
    /// 是否优先靠近地图中心。
    /// </summary>
    public SlateRef<bool> preferCenterClose;
    /// <summary>
    /// 是否优先靠近殖民地。
    /// </summary>
    public SlateRef<bool> preferCloseColony;
    /// <summary>
    /// 落点距地图边缘的最小距离。
    /// </summary>
    public SlateRef<float> minDistanceFromEdge;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
        dropSpot = IntVec3.Invalid;
        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            return false;
        }
        if (useTradeSpot.GetValue(slate))
        {
            dropSpot = DropCellFinder.TradeDropSpot(map);
            return true;
        }
        if (preferCenterClose.GetValue(slate))
        {
            if (DropCellFinder.TryFindRaidDropCenterClose(out dropSpot, map))
            {
                return true;
            }
        }
        dropSpot = IntVec3.Invalid;
        if (preferCloseColony.GetValue(slate))
        {
            dropSpot = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.Two);
        }

        if (dropSpot != IntVec3.Invalid)
        {
            return true;
        }

        if (CellFinderLoose.TryGetRandomCellWith(c => c.Standable(map) && !c.Roofed(map) && !c.Fogged(map) && c.DistanceToEdge(map) >= minDistanceFromEdge.GetValue(slate) && map.reachability.CanReachColony(c), map, 500, out dropSpot))
        {
            return true;
        }
        return false;
    }
}