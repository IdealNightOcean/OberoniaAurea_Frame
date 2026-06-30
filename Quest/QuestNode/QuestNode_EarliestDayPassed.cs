using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：游戏经历时间是否达到某个值。 
/// </summary>
public class QuestNode_EarliestDayPassed : QuestNode
{
    public SlateRef<int> earliestDayPassed;
    protected override bool TestRunInt(Slate slate)
    {
        return GenDate.DaysPassed >= earliestDayPassed.GetValue(slate);
    }
    protected override void RunInt() { }

}