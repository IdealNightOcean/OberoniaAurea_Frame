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

    public SlateRef<int> originTile = Tile.Invalid; //搜索起点Tile，-1时默认为玩家派系基地
    public SlateRef<float> maxTileDistance; //距离originTile最大距离
    public SlateRef<bool> preferCloser = true; //就近优先

    public SlateRef<Faction> faction;

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
        if (!ResolveOriginTile(slate, out int centerTile))
        {
            return false;
        }
        Settlement settlement = RandomNearbySettlement(centerTile, slate);
        if (settlement is not null)
        {
            slate.Set(storeAs.GetValue(slate), settlement);
            return true;
        }
        return false;
    }

    private bool ResolveOriginTile(Slate slate, out int centerTile)
    {
        centerTile = originTile.GetValue(slate);
        if (centerTile < 0)
        {
            Map map = slate.Get<Map>("map");
            if (map is null)
            {
                return false;
            }
            centerTile = map.Tile;
        }
        return centerTile >= 0;
    }

    protected virtual Settlement RandomNearbySettlement(int centerTile, Slate slate)
    {
        Faction faction = this.faction.GetValue(slate);
        if (faction is null)
        {
            return null;
        }

        List<Settlement> settlementList = Find.WorldObjects.SettlementBases;
        List<(Settlement, float)> potentialSettle = [];
        float maxDistance = maxTileDistance.GetValue(slate);
        for (int i = 0; i < settlementList.Count; i++)
        {
            Settlement settle = settlementList[i];
            if (IsGoodSettlement(settle, faction, centerTile, maxDistance, out float distance))
            {
                potentialSettle.Add((settle, distance));
            }
        }
        if (potentialSettle.Count > 0)
        {
            if (preferCloser.GetValue(slate))
            {
                potentialSettle.OrderBy(sd => sd.Item2);
                return potentialSettle.First().Item1;
            }
            else
            {
                return potentialSettle.RandomElement().Item1;
            }
        }

        if (ignoreConditionsIfNecessary.GetValue(slate))
        {
            return Find.WorldObjects.SettlementBases.Where(s => s.Faction == faction && s.Visitable)
                                                    .OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(centerTile, s.Tile))
                                                    .Take(3)
                                                    .FirstOrFallback(null);
        }
        return null;
    }

    protected bool IsGoodSettlement(Settlement settlement, Faction faction, int originTile, float maxDistance, out float distance)
    {
        distance = 999999f;
        if (!settlement.Visitable || settlement.Faction != faction)
        {
            return false;
        }
        distance = Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile);
        if (distance > maxDistance)
        {
            return false;
        }
        if (!Find.WorldReachability.CanReach(originTile, settlement.Tile))
        {
            return false;
        }
        return true;
    }

}