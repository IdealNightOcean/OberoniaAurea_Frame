using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 任务节点：获取地图父对象。
/// </summary>
public class QuestNode_GetMapParent : QuestNode
{
    /// <summary>
    /// 存储标签：地图父对象。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 目标地图。
    /// </summary>
    public SlateRef<Map> targetMap;

    /// <inheritdoc/>
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }

    private void SetVars(Slate slate)
    {
        MapParent mapParent = targetMap.GetValue(slate)?.Parent;
        if (mapParent is not null)
        {
            slate.Set(storeAs.GetValue(slate), mapParent);
        }
    }
}