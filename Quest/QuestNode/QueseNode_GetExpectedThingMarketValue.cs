using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：根据物品Def计算特定数量的预期市场价值。 
/// </summary>
public class QueseNode_GetExpectedThingMarketValue : QuestNode
{
    /// <summary>
    /// 存储标签：物品Def。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeThingDefAs;
    /// <summary>
    /// 存储标签：物品数量。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeThingCountAs;
    /// <summary>
    /// 存储标签：市场价值。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeMarketValueAs;

    /// <summary>
    /// 期望的物品Def。
    /// </summary>
    public SlateRef<ThingDef> expectedThingDef;
    /// <summary>
    /// 期望的物品数量。
    /// </summary>
    public SlateRef<int> expectedThingCount;

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
        ThingDef thingDef = expectedThingDef.GetValue(slate);
        float expectedMarketValue = thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * expectedThingCount.GetValue(slate);

        if (storeThingDefAs.GetValue(slate) is not null)
        {
            slate.Set(storeThingDefAs.GetValue(slate), thingDef);
        }
        if (storeThingCountAs.GetValue(slate) is not null)
        {
            slate.Set(storeThingCountAs.GetValue(slate), expectedThingCount);
        }
        if (storeMarketValueAs.GetValue(slate) is not null)
        {
            slate.Set(storeMarketValueAs.GetValue(slate), expectedMarketValue);
        }
    }
}