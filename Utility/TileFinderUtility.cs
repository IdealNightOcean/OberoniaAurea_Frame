using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_TileFinderUtility
{
    /// <summary>
    /// 获取可用的相邻地块。
    /// </summary>
    public static bool GetAvailableNeighborTile(PlanetTile rootTile, out PlanetTile tile, bool exclusion = true)
    {
        List<PlanetTile> allNeighborTiles = [];
        tile = -1;
        Find.WorldGrid.GetTileNeighbors(rootTile, allNeighborTiles);
        IEnumerable<PlanetTile> neighborTiles = allNeighborTiles.Where(t => !Find.World.Impassable(t));
        if (neighborTiles.Any())
        {
            if (exclusion)
            {
                WorldObjectsHolder worldObjects = Find.WorldObjects;
                foreach (int item in neighborTiles)
                {
                    if (!worldObjects.AnyWorldObjectAt(item))
                    {
                        tile = item;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                tile = neighborTiles.RandomElement();
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 尝试查找新的可用地块。
    /// </summary>
    public static bool TryFindNewAvaliableTile(out PlanetTile tile, PlanetTile rootTile, int minDist = 7, int maxDist = 27, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        tile = PlanetTile.Invalid;
        if (!rootTile.Valid)
        {
            return false;
        }

        return TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out tile, IsValidAvaliableTileForNewObject, ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound);
    }

    /// <summary>
    /// 检查地块是否可用于新建世界对象。
    /// </summary>
    public static bool IsValidAvaliableTileForNewObject(PlanetTile tile)
    {
        Tile worldTile = Find.WorldGrid[tile];
        if (!worldTile.PrimaryBiome.canBuildBase || !worldTile.PrimaryBiome.implemented)
        {
            return false;
        }
        if (worldTile.hilliness == Hilliness.Impassable)
        {
            return false;
        }
        if (Find.WorldObjects.AnyWorldObjectAt(tile))
        {
            return false;
        }
        return true;
    }
}
