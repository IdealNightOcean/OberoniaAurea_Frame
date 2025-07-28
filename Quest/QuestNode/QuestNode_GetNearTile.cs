using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GetNearTile : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<WorldObject> worldObject;
    public SlateRef<PlanetTile> centerTile = PlanetTile.Invalid;
    public SlateRef<bool> canBeSpace;
    public SlateRef<bool> preferNeighborTiles;
    public SlateRef<bool> preferCloserTiles = true;
    public SlateRef<int> minDist = 0;
    public SlateRef<int> maxDist = 999;

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
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.TryGet(storeAs.GetValue(slate), out int _) && TryFindTile(slate, out PlanetTile tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
        }
    }
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

    protected bool TryFindTile(Slate slate, out PlanetTile tile)
    {
        tile = PlanetTile.Invalid;
        if (!ResloveCenterTile(slate, out PlanetTile rootTile))
        {
            return false;
        }
        if (preferNeighborTiles.GetValue(slate))
        {
            if (OAFrame_TileFinderUtility.GetAvailableNeighborTile(rootTile, out tile))
            {
                return true;
            }
        }

        int minDist = this.minDist.GetValue(slate);
        int maxDist = this.maxDist.GetValue(slate);

        TileFinderMode tileFinderMode = preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random;
        return OAFrame_TileFinderUtility.TryFindNewAvaliableTile(out tile, rootTile, minDist, maxDist, tileFinderMode);
    }
}