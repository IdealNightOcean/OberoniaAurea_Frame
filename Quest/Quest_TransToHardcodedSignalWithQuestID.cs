using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_TransToHardcodedSignalWithQuestID : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    [NoTranslate]
    public SlateRef<string> originalSignal;

    public SlateRef<bool> addTransPart;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        string original = originalSignal.GetValue(slate);
        if (original is null)
        {
            return;
        }

        string resultSignal = QuestGenUtility.HardcodedSignalWithQuestID(original);
        if (storeAs.GetValue(slate) is string storeName)
        {
            slate.Set(storeName, resultSignal);
        }

        if (addTransPart.GetValue(slate))
        {
            QuestPart_TransToHardcodedSignalWithQuestID questPart_TransToHardcodedSignalWithQuestID = new(original, resultSignal);
            QuestGen.quest.AddPart(questPart_TransToHardcodedSignalWithQuestID);
        }
    }
}

public class QuestPart_TransToHardcodedSignalWithQuestID : QuestPart
{
    private string inSignal;
    private string outSignal;

    public QuestPart_TransToHardcodedSignalWithQuestID() { }

    public QuestPart_TransToHardcodedSignalWithQuestID(string originalSignal, string resultSignal)
    {
        SetSignalTrans(originalSignal, resultSignal);
    }

    public void SetSignalTrans(string originalSignal, string resultSignal)
    {
        inSignal = originalSignal;
        outSignal = resultSignal;
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        outSignal = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref outSignal, "outSignal");
    }
}