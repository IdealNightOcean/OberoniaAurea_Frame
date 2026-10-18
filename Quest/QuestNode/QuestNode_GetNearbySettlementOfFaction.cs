using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取特定派系的基地。 
/// </summary>
public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    /// <summary>
    /// 存储标签：找到的派系基地。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 必要时是否忽视各种限制条件直接选择派系基地。
    /// </summary>
    public SlateRef<bool> ignoreConditionsIfNecessary = true; //必要时忽视一切条件

    /// <summary>
    /// 搜索起点地块，无效时默认为玩家派系基地。
    /// </summary>
    public SlateRef<PlanetTile> originTile = PlanetTile.Invalid; //搜索起点Tile，-1时默认为玩家派系基地
    /// <summary>
    /// 与搜索起点的最大距离。
    /// </summary>
    public SlateRef<float> maxTileDistance; //距离originTile最大距离
    /// <summary>
    /// 是否优先选择更近的基地。
    /// </summary>
    public SlateRef<bool> preferCloser = true; //就近优先

    /// <summary>
    /// 目标派系。
    /// </summary>
    public SlateRef<Faction> faction;

    /// <inheritdoc/>
    protected override void RunInt() => SetVars(QuestGen.slate);

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate) => SetVars(slate);

    /// <summary>
    /// 设置结果：在符合条件的前提下选择并存储一个派系基地。
    /// </summary>
    /// <param name="slate">任务 Slate 数据。</param>
    /// <returns>是否成功选择并存储了基地。</returns>
    protected bool SetVars(Slate slate)
    {
        if (!ResolveOriginTile(slate, out PlanetTile centerTile))
            return false;

        Settlement settlement = slate.Get<Settlement>((storeAs.GetValue(slate)));

        if (settlement is not null)
        {
            Faction faction = this.faction.GetValue(slate);
            float maxDistance = maxTileDistance.GetValue(slate);
            if (IsGoodSettlement(settlement, faction, centerTile, maxDistance, out _))
            {
                return true;
            }
        }

        settlement = RandomNearbySettlement(centerTile, slate);
        if (settlement is not null)
        {
            slate.Set(storeAs.GetValue(slate), settlement);
            return true;
        }
        return false;
    }

    private bool ResolveOriginTile(Slate slate, out PlanetTile centerTile)
    {
        centerTile = originTile.GetValue(slate);
        if (centerTile.Valid)
        {
            return true;
        }

        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            centerTile = PlanetTile.Invalid;
            return false;
        }

        centerTile = map.Tile;
        return centerTile.Valid;
    }

    /// <summary>
    /// 随机选择一个符合条件的目标派系基地；必要时可忽视全部限制直接就近选取。
    /// </summary>
    /// <param name="centerTile">搜索中心地块。</param>
    /// <param name="slate">任务 Slate 数据。</param>
    /// <returns>选中的基地，未找到时返回 null。</returns>
    protected virtual Settlement RandomNearbySettlement(PlanetTile centerTile, Slate slate)
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
                return potentialSettle.MinBy(sd => sd.Item2).Item1;
            }
            else
            {
                return potentialSettle.RandomElement().Item1;
            }
        }

        if (ignoreConditionsIfNecessary.GetValue(slate))
        {
            return Find.WorldObjects.SettlementBases.Where(s => s.Faction == faction && s.Tile.Layer == centerTile.Layer && s.Visitable)
                                                    .OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(centerTile, s.Tile))
                                                    .Take(3)
                                                    .FirstOrFallback(null);
        }

        return null;
    }

    /// <summary>
    /// 判断某个基地是否属于目标派系、同一图层、可访问且距离在允许范围内。
    /// </summary>
    /// <param name="settlement">待判断的基地。</param>
    /// <param name="faction">目标派系。</param>
    /// <param name="centerTile">搜索中心地块。</param>
    /// <param name="maxDistance">允许的最大距离。</param>
    /// <param name="distance">输出该基地与中心地块的距离。</param>
    /// <returns>是否满足全部条件。</returns>
    protected bool IsGoodSettlement(Settlement settlement, Faction faction, PlanetTile centerTile, float maxDistance, out float distance)
    {
        distance = 999999f;
        if (settlement.Faction != faction || settlement.Tile.Layer != centerTile.Layer || !settlement.Visitable)
        {
            return false;
        }
        distance = Find.WorldGrid.ApproxDistanceInTiles(centerTile, settlement.Tile);
        if (distance > maxDistance)
        {
            return false;
        }
        if (!Find.WorldReachability.CanReach(centerTile, settlement.Tile))
        {
            return false;
        }
        return true;
    }
}