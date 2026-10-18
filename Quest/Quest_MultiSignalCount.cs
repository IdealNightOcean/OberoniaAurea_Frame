using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 任务节点：多重信号计数。 
/// </summary>
public class QuestNode_MultiSignalCount : QuestNode
{
    /// <summary>
    /// 输入信号。 
    /// </summary>
    [NoTranslate]
    public SlateRef<string> inSignal;
    /// <summary>
    /// 达成目标计数后输出的信号。 
    /// </summary>
    [NoTranslate]
    public SlateRef<string> outSignalAchieved;

    /// <summary>
    /// 目标计数。 
    /// </summary>
    public SlateRef<int> targetCount = 1;
    /// <summary>
    /// 是否仅触发一次信号。 
    /// </summary>
    public SlateRef<bool> oneTimeSignal = true;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return targetCount.GetValue(slate) > 0;
    }

    /// <inheritdoc/>
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
    /// <summary>
    /// 输入信号。 
    /// </summary>
    public string inSignal;
    /// <summary>
    /// 达成目标计数后输出的信号。 
    /// </summary>
    public string outSignalAchieved;
    /// <summary>
    /// 目标计数。 
    /// </summary>
    public int targetCount = 1;
    /// <summary>
    /// 是否仅触发一次信号。 
    /// </summary>
    public bool oneTimeSignal;

    private int receivedCount;
    private bool sentOnce;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        outSignalAchieved = null;
    }

    /// <inheritdoc/>
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