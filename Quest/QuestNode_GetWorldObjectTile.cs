using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GetWorldObjectTile : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<WorldObject> worldObject;

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        SetVars(slate);
    }
    protected void SetVars(Slate slate)
    {
        WorldObject worldObject = this.worldObject.GetValue(slate);
        if (worldObject != null && worldObject.Spawned)
        {
            slate.Set(storeAs.GetValue(slate), worldObject.Tile);
        }
    }
}