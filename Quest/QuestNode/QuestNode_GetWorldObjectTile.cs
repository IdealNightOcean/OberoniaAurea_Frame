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
        return SetVars(slate);
    }
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }
    protected bool SetVars(Slate slate)
    {
        WorldObject worldObject = this.worldObject.GetValue(slate);
        if (worldObject is not null && worldObject.Spawned)
        {
            slate.Set(storeAs.GetValue(slate), worldObject.Tile);
            return true;
        }
        return false;
    }
}