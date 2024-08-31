using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//获取特定派系的基地
public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> ignoreConditionsIfNecessary; //必要时忽视一切条件

    public SlateRef<int> originTile = -1; //搜索起点Tile，-1时默认为玩家派系基地
    public SlateRef<float> maxTileDistance; //距离originTile最大距离
    public SlateRef<bool> nearFirst = true;

    public SlateRef<Faction> faction;

    protected virtual Settlement RandomNearbySettlement(int originTile, Slate slate)
    {
        Faction faction = this.faction.GetValue(slate);
        if (faction == null)
        {
            return null;
        }

        Settlement outSettlement = null;

        List<Settlement> settlementList = Find.WorldObjects.SettlementBases;
        Dictionary<Settlement, float> potentialSettle = [];
        float distance = 999999f;
        for (int i = 0; i < settlementList.Count; i++)
        {
            Settlement settle = settlementList[i];
            if (IsGoodSettlement(settle, originTile, slate, out distance))
            {
                potentialSettle.Add(settle, distance);
            }
        }
        if (potentialSettle.Any())
        {
            if (nearFirst.GetValue(slate))
            {
                potentialSettle.OrderBy(sd => sd.Value);
                outSettlement = potentialSettle.First().Key;
            }
            else
            {
                outSettlement = potentialSettle.RandomElement().Key;
            }
        }

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
    protected bool IsGoodSettlement(Settlement settlement, int originTile, Slate slate, out float distance)
    {
        distance = 999999f;
        if (!settlement.Visitable || settlement.Faction != faction)
        {
            return false;
        }
        distance = Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile);
        if (distance > maxTileDistance.GetValue(slate))
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
