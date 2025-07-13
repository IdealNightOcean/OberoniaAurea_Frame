using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_TileFinderUtility
{
    public static bool GetAvailableNeighborTile(int rootTile, out int tile, bool exclusion = true)
    {
        List<int> allNeighborTiles = [];
        tile = -1;
        Find.WorldGrid.GetTileNeighbors(rootTile, allNeighborTiles);
        IEnumerable<int> neighborTiles = allNeighborTiles.Where(t => !Find.World.Impassable(t));
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
    public static bool TryFindNewAvaliableTile(out int tile, int nearThisTile = -1, int minDist = 7, int maxDist = 27, bool allowCaravans = false, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        int rootTile;
        if (nearThisTile != -1)
        {
            rootTile = nearThisTile;
        }
        else if (!TileFinder.TryFindRandomPlayerTile(out rootTile, allowCaravans, x => FindAvaliableTile(x, minDist, maxDist, tileFinderMode, exitOnFirstTileFound) != -1))
        {
            tile = -1;
            return false;
        }
        tile = FindAvaliableTile(rootTile, minDist, maxDist, tileFinderMode, exitOnFirstTileFound);
        return tile != -1;
    }

    public static int FindAvaliableTile(int rootTile, int minDist = 7, int maxDist = 27, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out int result, IsValidAvaliableTileForNewObject, ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound))
        {
            return result;
        }
        return TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out result, x => IsValidAvaliableTileForNewObject(x) && (!Find.World.Impassable(x) || Find.WorldGrid[x].WaterCovered), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: true, exitOnFirstTileFound) ? result : (-1);
    }

    public static bool IsValidAvaliableTileForNewObject(int tile)
    {
        Tile worldTile = Find.WorldGrid[tile];
        if (!worldTile.biome.canBuildBase || !worldTile.biome.implemented)
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
