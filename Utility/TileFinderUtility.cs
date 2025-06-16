using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_TileFinderUtility
{
    public static bool GetAvailableNeighborTile(PlanetTile rootTile, out PlanetTile tile, bool exclusion = true)
    {
        List<PlanetTile> allNeighborTiles = [];
        tile = -1;
        Find.WorldGrid.GetTileNeighbors(rootTile, allNeighborTiles);
        var neighborTiles = allNeighborTiles.Where(t => !Find.World.Impassable(t));
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
    public static bool TryFindNewAvaliableTile(out PlanetTile tile, PlanetTile nearThisTile, int minDist = 7, int maxDist = 27, bool canBeSpace = false, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        PlanetTile rootTile;
        if (nearThisTile.Valid && (canBeSpace || !nearThisTile.LayerDef.isSpace))
        {
            rootTile = nearThisTile;
        }
        else if (!TileFinder.TryFindRandomPlayerTile(out rootTile, allowCaravans: false, (PlanetTile x) => FindAvaliableTile(x, minDist, maxDist, tileFinderMode, exitOnFirstTileFound).Valid))
        {
            tile = PlanetTile.Invalid;
            return false;
        }
        tile = FindAvaliableTile(rootTile, minDist, maxDist, tileFinderMode, exitOnFirstTileFound);
        return tile.Valid;
    }

    public static PlanetTile FindAvaliableTile(int rootTile, int minDist = 7, int maxDist = 27, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false)
    {
        if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out PlanetTile result, IsValidAvaliableTileForNewObject, ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound))
        {
            return result;
        }
        return TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out result, (PlanetTile x) => IsValidAvaliableTileForNewObject(x) && (!Find.World.Impassable(x) || Find.WorldGrid[x].WaterCovered), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: true, exitOnFirstTileFound) ? result : PlanetTile.Invalid;
    }
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
