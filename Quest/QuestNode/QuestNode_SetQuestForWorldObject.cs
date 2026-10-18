using RimWorld.Planet;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：为世界对象设置任务关联。 
/// </summary>
public class QuestNode_SetQuestForWorldObject : QuestNode
{
    /// <summary>
    /// 世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;
    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    /// <inheritdoc/>
    protected override void RunInt()
    {
        (worldObject.GetValue(QuestGen.slate) as IQuestAssociate)?.SetAssociatedQuest(QuestGen.quest);
    }
}