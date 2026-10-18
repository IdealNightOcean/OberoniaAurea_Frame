using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取附近地块。 
/// </summary>
public class QuestNode_GetNearTile : QuestNode
{
    /// <summary>
    /// 存储标签：找到的附近地块。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 用于推导中心地块的世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;
    /// <summary>
    /// 搜索的中心地块；无效时回退到世界对象或当前地图地块。
    /// </summary>
    public SlateRef<PlanetTile> centerTile = PlanetTile.Invalid;
    /// <summary>
    /// 结果是否允许为太空地块。
    /// </summary>
    public SlateRef<bool> canBeSpace;
    /// <summary>
    /// 是否优先返回邻居地块。
    /// </summary>
    public SlateRef<bool> preferNeighborTiles;
    /// <summary>
    /// 是否优先返回更近的合法地块。
    /// </summary>
    public SlateRef<bool> preferCloserTiles = true;
    /// <summary>
    /// 结果与中心地块的最小距离。
    /// </summary>
    public SlateRef<int> minDist = 0;
    /// <summary>
    /// 结果与中心地块的最大距离。
    /// </summary>
    public SlateRef<int> maxDist = 999;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet(storeAs.GetValue(slate), out PlanetTile _))
        {
            return true;
        }
        else if (TryFindTile(slate, out PlanetTile tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
            return true;
        }
        return false;
    }
    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.TryGet(storeAs.GetValue(slate), out int _) && TryFindTile(slate, out PlanetTile tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
        }
    }

    /// <summary>
    /// 解析搜索用的中心地块：优先用 <paramref name="slate"/> 中的中心地块，其次世界对象地块，最后当前地图地块。
    /// </summary>
    /// <param name="slate">任务 Slate 数据。</param>
    /// <param name="tile">解析出的中心地块。</param>
    /// <returns>是否成功解析，且符合太空地块限制。</returns>
    protected bool ResloveCenterTile(Slate slate, out PlanetTile tile)
    {
        tile = PlanetTile.Invalid;
        if (centerTile.GetValue(slate).Valid)
        {
            tile = centerTile.GetValue(slate);
        }
        else if (worldObject.GetValue(slate) is not null)
        {
            tile = worldObject.GetValue(slate).Tile;
        }
        else
        {
            Map map = slate.Get<Map>("map");
            if (map is not null)
            {
                tile = map.Tile;
            }
        }

        return tile.Valid && (canBeSpace.GetValue(slate) || !tile.LayerDef.isSpace);
    }

    /// <summary>
    /// 尝试在满足距离与邻居偏好的前提下查找并返回一个可用地块。
    /// </summary>
    /// <param name="slate">任务 Slate 数据。</param>
    /// <param name="tile">查找出的可用地块。</param>
    /// <returns>是否成功查找到可用地块。</returns>
    protected bool TryFindTile(Slate slate, out PlanetTile tile)
    {
        tile = PlanetTile.Invalid;
        if (!ResloveCenterTile(slate, out PlanetTile rootTile))
        {
            return false;
        }
        if (preferNeighborTiles.GetValue(slate))
        {
            if (Utility.OAFrame_TileFinderUtility.GetAvailableNeighborTile(rootTile, out tile))
            {
                return true;
            }
        }

        int minDist = this.minDist.GetValue(slate);
        int maxDist = this.maxDist.GetValue(slate);
        if (minDist > maxDist)
        {
            (minDist, maxDist) = (maxDist, minDist);
        }

        TileFinderMode tileFinderMode = preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random;
        return Utility.OAFrame_TileFinderUtility.TryFindNewAvaliableTile(out tile, rootTile, minDist, maxDist, tileFinderMode);
    }
}