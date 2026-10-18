using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：季节性限制。 
/// </summary>
public class QuestNode_SeasonalRestriction : QuestNode
{
    /// <summary>
    /// 是否允许春季。
    /// </summary>
    public SlateRef<bool> allowSpring = true;
    /// <summary>
    /// 是否允许夏季。
    /// </summary>
    public SlateRef<bool> allowSummer = true;
    /// <summary>
    /// 是否允许秋季。
    /// </summary>
    public SlateRef<bool> allowFall = true;
    /// <summary>
    /// 是否允许冬季。
    /// </summary>
    public SlateRef<bool> allowWinter = true;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        Season season;
        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            season = GenDate.Season(GenTicks.TicksAbs, Vector2.zero);
        }
        else
        {
            season = GenLocalDate.Season(map);
        }

        return season switch
        {
            Season.Spring => allowSpring.GetValue(slate),
            Season.Summer or Season.PermanentSummer => allowSummer.GetValue(slate),
            Season.Fall => allowFall.GetValue(slate),
            Season.Winter or Season.PermanentWinter => allowWinter.GetValue(slate),
            _ => false,
        };
    }
    /// <inheritdoc/>
    protected override void RunInt() { }
}