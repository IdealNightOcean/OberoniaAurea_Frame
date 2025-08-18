using RimWorld;
using RimWorld.QuestGen;

namespace OberoniaAurea_Frame;

//游戏经历时间是否达到某个值
public class QuestNode_EarliestDayPassed : QuestNode
{
    public SlateRef<int> earliestDayPassed;
    protected override bool TestRunInt(Slate slate)
    {
        return GenDate.DaysPassed >= earliestDayPassed.GetValue(slate);
    }
    protected override void RunInt() { }

}