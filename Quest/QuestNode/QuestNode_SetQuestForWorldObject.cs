using RimWorld.Planet;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

public class QuestNode_SetQuestForWorldObject : QuestNode
{
    public SlateRef<WorldObject> worldObject;
    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        (worldObject.GetValue(QuestGen.slate) as IQuestAssociate)?.SetAssociatedQuest(QuestGen.quest);
    }
}
