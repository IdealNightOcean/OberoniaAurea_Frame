using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class MapComponent_SpecialBuildingManager : MapComponent
{
    [Unsaved] private Dictionary<ThingDef, int> buildingTable = [];

    public MapComponent_SpecialBuildingManager(Map map) : base(map) { }

    public void AddBuilding(ThingDef def)
    {
        buildingTable[def] = buildingTable.TryGetValue(def, fallback: 0) + 1;
    }

    public void RemoveBuilding(ThingDef def)
    {
        int newCount = buildingTable.TryGetValue(def, fallback: -999) - 1;
        if (newCount == -1000)
        {
            return;
        }
        else if (newCount <= 0)
        {
            buildingTable.Remove(def);
        }
        else
        {
            buildingTable[def] = newCount;
        }
    }

    public bool HasBuilding(ThingDef def)
    {
        return buildingTable.ContainsKey(def);
    }

    public int GetBuildingCount(ThingDef def)
    {
        if (buildingTable.TryGetValue(def, out int count))
        {
            return count;
        }
        return 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref buildingTable, "buildingTable", LookMode.Def, LookMode.Value);
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            buildingTable.RemoveAll(kv => kv.Key is null || kv.Value <= 0);
        }
    }
}
