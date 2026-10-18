using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：计算物品需求数，主要用于交易类任务。 
/// </summary>
public class QuestNode_RequestThingCount : QuestNode
{
    /// <summary>
    /// 存储标签：计算得到的所需数量。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 数量基础随机范围。
    /// </summary>
    public SlateRef<IntRange> baseRange;
    /// <summary>
    /// 数量基础偏移比例范围。
    /// </summary>
    public SlateRef<FloatRange> baseOffset = FloatRange.Zero;
    /// <summary>
    /// 每达到该玩家财富值时额外增加的物品数对应的财富除数。
    /// </summary>
    public SlateRef<float> preOffsetWealth;
    /// <summary>
    /// 达到 <see cref="preOffsetWealth"/> 时额外增加的物品数量。
    /// </summary>
    public SlateRef<int> preOffsetCount;
    /// <summary>
    /// 结果数量下限。
    /// </summary>
    public SlateRef<int> minCountLimit = 1;
    /// <summary>
    /// 结果数量上限，小于等于0时表示无限制。
    /// </summary>
    public SlateRef<int> maxCountLimit = -1;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }
    /// <inheritdoc/>
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        int requestThingCount = baseRange.GetValue(slate).RandomInRange;
        float preOffsetWealth = this.preOffsetWealth.GetValue(slate);
        int preOffsetCount = this.preOffsetCount.GetValue(slate);

        if (preOffsetWealth != 0f && preOffsetCount != 0)
        {
            requestThingCount += (int)(WealthUtility.PlayerWealth / preOffsetWealth) * preOffsetCount;
        }
        requestThingCount = (int)(requestThingCount * (1f + baseOffset.GetValue(slate).RandomInRange));

        int trueMin = Mathf.Max(minCountLimit.GetValue(slate), 1);
        int trueMax = maxCountLimit.GetValue(slate);
        if (trueMax > 0)
        {
            if (trueMin > trueMax)
            {
                (trueMin, trueMax) = (trueMax, trueMin);
            }
        }
        else
        {
            trueMax = int.MaxValue - 1;
        }
        requestThingCount = Mathf.Clamp(requestThingCount, trueMin, trueMax);
        slate.Set(storeAs.GetValue(slate), requestThingCount);
    }
}