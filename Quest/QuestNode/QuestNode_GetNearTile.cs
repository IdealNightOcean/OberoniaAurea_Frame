using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GetNearTile : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<WorldObject> worldObject;
    public SlateRef<int> centerTile = Tile.Invalid;
    public SlateRef<bool> allowCaravans;
    public SlateRef<bool> preferNeighborTiles;
    public SlateRef<bool> preferCloserTiles = true;
    public SlateRef<int> minDist = 0;
    public SlateRef<int> maxDist = 999;

    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet(storeAs.GetValue(slate), out int _))
        {
            return true;
        }
        else if (TryFindTile(slate, out int tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
            return true;
        }
        return false;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.TryGet(storeAs.GetValue(slate), out int _) && TryFindTile(slate, out int tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
        }
    }
    protected bool ResloveCenterTile(Slate slate, out int tile)
    {
        tile = Tile.Invalid;
        if (centerTile.GetValue(slate) != Tile.Invalid)
        {
            tile = centerTile.GetValue(slate);
            return true;
        }
        if (worldObject.GetValue(slate) != null)
        {
            tile = worldObject.GetValue(slate).Tile;
            return tile != Tile.Invalid;
        }
        Map map = slate.Get<Map>("map");
        if(map != null)
        {
            tile = map.Tile;
            return true;
        }
        return false;
    }

    protected bool TryFindTile(Slate slate, out int tile)
    {
        tile = Tile.Invalid;
        if (!ResloveCenterTile(slate, out int rootTile))
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
        bool allowCaravans = this.allowCaravans.GetValue(slate);
        TileFinderMode tileFinderMode = preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random;
        return OAFrame_TileFinderUtility.TryFindNewAvaliableTile(out tile, rootTile, minDist, maxDist, allowCaravans, tileFinderMode);
    }
}