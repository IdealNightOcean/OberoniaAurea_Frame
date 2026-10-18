using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：根据物品类别（<see cref="ThingCategoryDef"/>）计算特定数量的预期市场价值。 
/// </summary>
public class QueseNode_GetExpectedCategoryMarketValue : QuestNode
{
    /// <summary>
    /// 存储标签：物品类别。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeThingCategoryAs;
    /// <summary>
    /// 存储标签：市场价值。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeMarketValueAs;

    /// <summary>
    /// 期望的物品类别。
    /// </summary>
    public SlateRef<ThingCategoryDef> expectedThingCategory;
    /// <summary>
    /// 该类别的平均单价。
    /// </summary>
    public SlateRef<float> unitPrice;
    /// <summary>
    /// 期望的物品数量。
    /// </summary>
    public SlateRef<int> expectedThingCount;
    /// <summary>
    /// 是否将物品类别一并存入 Slate。
    /// </summary>
    public SlateRef<bool> storeThingCategory;

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
        float expectedMarketValue = unitPrice.GetValue(slate) * expectedThingCount.GetValue(slate);
        slate.Set(storeMarketValueAs.GetValue(slate), expectedMarketValue);
        if (storeThingCategory.GetValue(slate))
        {
            slate.Set(storeThingCategoryAs.GetValue(slate), expectedThingCategory.GetValue(slate));
        }
    }
}