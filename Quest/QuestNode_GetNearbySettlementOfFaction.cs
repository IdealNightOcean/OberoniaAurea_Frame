using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//获取特定派系的据点
public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> ignoreConditionsIfNecessary = true; //必要时忽视一切条件

    public SlateRef<int> originTile = -1; //搜索起点Tile，-1时默认为玩家派系基地
    public SlateRef<float> maxTileDistance; //距离originTile最大距离

    public SlateRef<Faction> faction;

    protected virtual Settlement RandomNearbySettlement(int originTile, Slate slate)
    {
        Faction faction = this.faction.GetValue(slate);
        if (faction == null)
        {
            return null;
        }
        Settlement outSettlement = Find.WorldObjects.SettlementBases.Where(delegate (Settlement settlement)
        {
            return IsGoodSettlement(settlement, originTile, slate);
        }).RandomElementWithFallback();

        if (ignoreConditionsIfNecessary.GetValue(slate) && outSettlement == null)
        {
            outSettlement = Find.WorldObjects.SettlementBases.Where(delegate (Settlement settlement)
            {
                if (!settlement.Visitable || settlement.Faction != faction)
                {
                    return false;
                }
                return true;
            }).RandomElementWithFallback();
        }
        return outSettlement;
    }
    protected bool IsGoodSettlement(Settlement settlement, int originTile, Slate slate)
    {
        if (!settlement.Visitable || settlement.Faction != faction)
        {
            return false;
        }
        if (Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) > maxTileDistance.GetValue(slate))
        {
            return false;
        }
        if (!Find.WorldReachability.CanReach(originTile, settlement.Tile))
        {
            return false;
        }
        return true;
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return SetVars(slate);
    }
    protected bool SetVars(Slate slate)
    {
        int originTile = this.originTile.GetValue(slate);
        if (originTile < 0)
        {
            Map map = slate.Get<Map>("map");
            originTile = map.Tile;
        }
        Settlement settlement = RandomNearbySettlement(originTile, slate);
        if (settlement != null)
        {
            slate.Set(storeAs.GetValue(slate), settlement);
            return true;
        }
        return false;
    }
}
