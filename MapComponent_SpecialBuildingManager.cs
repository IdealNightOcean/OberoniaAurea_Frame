using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class MapComponent_SpecialBuildingManager : MapComponent
{
    private Dictionary<ThingDef, int> buildingTable;

    public MapComponent_SpecialBuildingManager(Map map) : base(map) { }

    public void AddBuilding(ThingDef def)
    {
        if (buildingTable is null)
        {
            buildingTable = new Dictionary<ThingDef, int>() { { def, 1 } };
        }
        else
        {
            buildingTable[def] = buildingTable.TryGetValue(def, fallback: 0) + 1;
        }
    }

    public void RemoveBuilding(ThingDef def)
    {
        if(buildingTable is null)
        {
            return;
        }
        
        int newCount = buildingTable.TryGetValue(def, fallback: -999) - 1;
        if (newCount == -1000)
        {
            return;
        }
        else if (newCount <= 0)
        {
            buildingTable.Remove(def);
            if(buildingTable.Count == 0)
            {
                buildingTable = null;
            }
        }
        else
        {
            buildingTable[def] = newCount;
        }
    }

    public bool HasBuilding(ThingDef def)
    {
        return buildingTable?.ContainsKey(def) ?? false;
    }

    public int GetBuildingCount(ThingDef def)
    {
        return buildingTable?.TryGetValue(def, fallback: 0) ?? 0;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref buildingTable, "buildingTable", LookMode.Def, LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            buildingTable?.RemoveAll(kv => kv.Key is null || kv.Value <= 0);
            if (buildingTable.NullOrEmpty())
            {
                buildingTable = null;
            }
        }
    }
}
