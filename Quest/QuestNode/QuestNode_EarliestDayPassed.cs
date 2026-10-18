using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：游戏经历时间是否达到某个值。 
/// </summary>
public class QuestNode_EarliestDayPassed : QuestNode
{
    /// <summary>
    /// 最早历经天数。
    /// </summary>
    public SlateRef<int> earliestDayPassed;
    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return GenDate.DaysPassed >= earliestDayPassed.GetValue(slate);
    }
    /// <inheritdoc/>
    protected override void RunInt() { }

}