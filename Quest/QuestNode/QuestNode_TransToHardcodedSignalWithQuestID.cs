using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

internal class QuestNode_TransToHardcodedSignalWithQuestID : QuestNode
{
    [NoTranslate]
    public SlateRef<string> originalSignal;

    [NoTranslate]
    public SlateRef<string> storeAs;

    protected override bool TestRunInt(Slate slate)
    {
        return originalSignal.GetValue(slate) is not null;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        string original = originalSignal.GetValue(slate);
        if (original is not null)
        {
            string resultSignal = QuestGenUtility.HardcodedSignalWithQuestID(original);
            slate.Set(storeAs.GetValue(slate), resultSignal);
        }
    }
}
