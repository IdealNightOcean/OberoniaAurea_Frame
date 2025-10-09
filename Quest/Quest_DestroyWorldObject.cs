using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_DestroyWorldObject : RimWorld.QuestGen.QuestNode_DestroyWorldObject
{
    public SlateRef<bool> destroyOnCleanup = true;
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_DestroyWorldObject questPart_DestroyWorldObject = new()
        {
            worldObject = worldObject.GetValue(slate),
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            DestroyOnCleanup = destroyOnCleanup.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_DestroyWorldObject);
    }
}

public class QuestPart_DestroyWorldObject : RimWorld.QuestPart_DestroyWorldObject
{
    public bool DestroyOnCleanup = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref DestroyOnCleanup, "DestroyOnCleanup", defaultValue: true);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        if (DestroyOnCleanup && worldObject is not null && !worldObject.Destroyed)
        {
            TryRemove(worldObject);
        }
    }
}
