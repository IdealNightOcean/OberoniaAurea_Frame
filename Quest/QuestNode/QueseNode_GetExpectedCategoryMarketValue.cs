using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//根据物品Def计算特定数量的物品价值
public class QueseNode_GetExpectedCategoryMarketValue : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeThingCategoryAs;
    [NoTranslate]
    public SlateRef<string> storeMarketValueAs;

    public SlateRef<ThingCategoryDef> expectedThingCategory;
    public SlateRef<float> unitPrice;
    public SlateRef<int> expectedThingCount;
    public SlateRef<bool> storeThingCategory;

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
        float expectedMarketValue = unitPrice.GetValue(slate) * expectedThingCount.GetValue(slate);
        slate.Set(storeMarketValueAs.GetValue(slate), expectedMarketValue);
        if (storeThingCategory.GetValue(slate))
        {
            slate.Set(storeThingCategoryAs.GetValue(slate), expectedThingCategory.GetValue(slate));
        }
    }
}