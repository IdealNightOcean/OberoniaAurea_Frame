using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

//根据物品Def计算特定数量的物品价值
public class QueseNode_GetExpectedThingMarketValue : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeThingAs;
    [NoTranslate]
    public SlateRef<string> storeMarketValueAs;

    public SlateRef<ThingDef> expectedThingDef;
    public SlateRef<int> expectedThingCount;
    public SlateRef<bool> storeThing;

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }
    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    protected void SetVars(Slate slate)
    {
        ThingDef thingDef = expectedThingDef.GetValue(slate);
        float expectedMarketValue = thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * expectedThingCount.GetValue(slate);
        slate.Set(storeMarketValueAs.GetValue(slate), expectedMarketValue);
        if (storeThing.GetValue(slate))
        {
            slate.Set(storeThingAs.GetValue(slate), thingDef);
        }
    }
}