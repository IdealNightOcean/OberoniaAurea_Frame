using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

//计算物品需求数，主要用于交易类任务
public class QuestNode_RequestThingCount : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<IntRange> baseRange;
    public SlateRef<FloatRange> baseOffset = FloatRange.Zero;
    public SlateRef<float> preOffsetWealth;
    public SlateRef<int> preOffsetCount;
    public SlateRef<int> minCountLimit = 1;
    public SlateRef<int> maxCountLimit = -1;

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }
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