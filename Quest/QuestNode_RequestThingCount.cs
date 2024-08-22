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
    public SlateRef<FloatRange> baseOffset = new FloatRange(1f);
    public SlateRef<float> preOffsetWealth = -1;
    public SlateRef<int> preOffsetCount = 0;
    public SlateRef<float> maxWealthLimit = -1;

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
        if (map == null)
        {
            return false;
        }
        int requestThingCount = baseRange.GetValue(slate).RandomInRange;
        float preOffsetWealth = this.preOffsetWealth.GetValue(slate);
        int preOffsetCount = this.preOffsetCount.GetValue(slate);
        float maxWealthLimit = this.maxWealthLimit.GetValue(slate);
        if (preOffsetWealth > 0 && preOffsetCount > 0)
        {
            float playerWealthForStoryteller = maxWealthLimit > 0 ? Mathf.Min(maxWealthLimit, map.PlayerWealthForStoryteller) : map.PlayerWealthForStoryteller;
            requestThingCount += (int)(playerWealthForStoryteller / preOffsetWealth) * preOffsetCount;
        }
        requestThingCount = (int)(requestThingCount * (1f + baseOffset.GetValue(slate).RandomInRange));
        slate.Set(storeAs.GetValue(slate), requestThingCount);
        return true;
    }
}
