using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取世界对象的地块。 
/// </summary>
public class QuestNode_GetWorldObjectTile : QuestNode
{
    /// <summary>
    /// 存储标签：世界对象地块。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }

    /// <inheritdoc/>
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        WorldObject worldObject = this.worldObject.GetValue(slate);
        if (worldObject is not null && worldObject.Spawned)
        {
            slate.Set(storeAs.GetValue(slate), worldObject.Tile);
        }
    }
}