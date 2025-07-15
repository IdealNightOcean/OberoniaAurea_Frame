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
    public static bool TryFindNewAvaliableTile(out int tile, int rootTile = -1, int minDist = 7, int maxDist = 27, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        tile = Tile.Invalid;
        if (rootTile == Tile.Invalid)
        {
            return false;
        }

        return TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out tile, IsValidAvaliableTileForNewObject, ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound);
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
