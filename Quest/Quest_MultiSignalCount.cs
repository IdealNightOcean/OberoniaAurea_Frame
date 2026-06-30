using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_MultiSignalCount : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;
    [NoTranslate]
    public SlateRef<string> outSignalAchieved;

    public SlateRef<int> targetCount = 1;
    public SlateRef<bool> oneTimeSignal = true;

    protected override bool TestRunInt(Slate slate)
    {
        return targetCount.GetValue(slate) > 0;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_MultiSignalCount questPart_MultiSignalCount = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            outSignalAchieved = QuestGenUtility.HardcodedSignalWithQuestID(outSignalAchieved.GetValue(slate)),
            targetCount = targetCount.GetValue(slate),
            oneTimeSignal = oneTimeSignal.GetValue(slate)
        };
        QuestGen.quest.AddPart(questPart_MultiSignalCount);
    }
}
/// <summary> 
/// 任务部件：多重信号计数。 
/// </summary>
public class QuestPart_MultiSignalCount : QuestPart
{
    public string inSignal;
    public string outSignalAchieved;
    public int targetCount = 1;
    public bool oneTimeSignal;

    private int receivedCount;
    private bool sentOnce;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, nameof(inSignal));
        Scribe_Values.Look(ref outSignalAchieved, nameof(outSignalAchieved));

        Scribe_Values.Look(ref targetCount, nameof(targetCount), 1);
        Scribe_Values.Look(ref receivedCount, nameof(receivedCount), 0);
        Scribe_Values.Look(ref oneTimeSignal, nameof(oneTimeSignal), defaultValue: false);
        Scribe_Values.Look(ref sentOnce, nameof(sentOnce), defaultValue: false);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        outSignalAchieved = null;
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (oneTimeSignal && sentOnce)
        {
            return;
        }

        if (signal.tag == inSignal)
        {
            if (++receivedCount >= targetCount)
            {
                sentOnce = true;
                Find.SignalManager.SendSignal(new Signal(outSignalAchieved));
            }
        }
    }
}