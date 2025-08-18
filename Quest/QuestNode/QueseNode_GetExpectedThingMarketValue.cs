using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//根据物品Def计算特定数量的物品价值
public class QueseNode_GetExpectedThingMarketValue : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeThingDefAs;
    [NoTranslate]
    public SlateRef<string> storeMarketValueAs;

    public SlateRef<ThingDef> expectedThingDef;
    public SlateRef<int> expectedThingCount;

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
        ThingDef thingDef = expectedThingDef.GetValue(slate);
        float expectedMarketValue = thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * expectedThingCount.GetValue(slate);

        if (storeThingDefAs.GetValue(slate) is not null)
        {
            slate.Set(storeThingDefAs.GetValue(slate), thingDef);
        }
        if (storeMarketValueAs.GetValue(slate) is not null)
        {
            slate.Set(storeMarketValueAs.GetValue(slate), expectedMarketValue);
        }
    }
}