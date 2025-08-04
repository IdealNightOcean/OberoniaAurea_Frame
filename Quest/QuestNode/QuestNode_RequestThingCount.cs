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
    public SlateRef<int> maxCountLimit;

    protected override bool TestRunInt(Slate slate)
    {
        return SetVars(slate);
    }
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    protected bool SetVars(Slate slate)
    {
        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            return false;
        }
        int requestThingCount = baseRange.GetValue(slate).RandomInRange;
        float preOffsetWealth = this.preOffsetWealth.GetValue(slate);
        int preOffsetCount = this.preOffsetCount.GetValue(slate);
        int maxCountLimit = this.maxCountLimit.GetValue(slate);
        if (preOffsetWealth != 0f && preOffsetCount != 0)
        {
            requestThingCount += (int)(map.PlayerWealthForStoryteller / preOffsetWealth) * preOffsetCount;
        }
        requestThingCount = (int)(requestThingCount * (1f + baseOffset.GetValue(slate).RandomInRange));
        if (maxCountLimit > 0)
        {
            requestThingCount = Mathf.Min(requestThingCount, maxCountLimit);
        }
        slate.Set(storeAs.GetValue(slate), requestThingCount);
        return requestThingCount > 0;
    }
}