using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GenerateInteractiveWorldObject : QuestNode
{
    public SlateRef<WorldObjectDef> def;
    public SlateRef<PlanetTile> tile;
    public SlateRef<Faction> faction;

    [NoTranslate]
    public SlateRef<string> storeAs;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        WorldObject_InteractiveBase worldObject = GenerateWorldObject(slate);
        worldObject.Tile = tile.GetValue(slate);
        worldObject.SetAssociatedQuest(QuestGen.quest);
        if (faction.GetValue(slate) is not null)
        {
            worldObject.SetFaction(faction.GetValue(slate));
        }
        if (storeAs.GetValue(slate) is not null)
        {
            slate.Set(storeAs.GetValue(slate), worldObject);
        }
    }

    protected virtual WorldObject_InteractiveBase GenerateWorldObject(Slate slate)
    {
        return (WorldObject_InteractiveBase)WorldObjectMaker.MakeWorldObject(def.GetValue(slate));
    }

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }
}
