using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：转换为含任务ID的硬编码信号。 
/// </summary>
public class QuestNode_TransToHardcodedSignalWithQuestID : QuestNode
{
    /// <summary>
    /// 存储标签：转换结果。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 原始信号。 
    /// </summary>
    [NoTranslate]
    public SlateRef<string> originalSignal;

    /// <summary>
    /// 是否添加转换任务部件。 
    /// </summary>
    public SlateRef<bool> addTransPart;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    /// <inheritdoc/>
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
/// <summary> 
/// 任务部件：转换为含任务ID的硬编码信号。 
/// </summary>
public class QuestPart_TransToHardcodedSignalWithQuestID : QuestPart
{
    private string inSignal;
    private string outSignal;

    /// <summary>
    /// 默认构造函数。
    /// </summary>
    public QuestPart_TransToHardcodedSignalWithQuestID() { }

    /// <summary>
    /// 构造函数：根据原始信号与转换结果信号初始化。
    /// </summary>
    /// <param name="originalSignal">原始信号。</param>
    /// <param name="resultSignal">转换后的结果信号。</param>
    public QuestPart_TransToHardcodedSignalWithQuestID(string originalSignal, string resultSignal)
    {
        SetSignalTrans(originalSignal, resultSignal);
    }

    /// <summary>
    /// 设置信号转换。
    /// </summary>
    /// <param name="originalSignal">原始信号。</param>
    /// <param name="resultSignal">转换后的结果信号。</param>
    public void SetSignalTrans(string originalSignal, string resultSignal)
    {
        inSignal = originalSignal;
        outSignal = resultSignal;
    }

    /// <inheritdoc/>
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == inSignal)
        {
            Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
        }
    }

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        outSignal = null;
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, nameof(inSignal));
        Scribe_Values.Look(ref outSignal, nameof(outSignal));
    }
}