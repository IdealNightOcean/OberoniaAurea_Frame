using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GenerateThingDefCount : QuestNode
{
    public SlateRef<ThingDef> thingDef;
    public SlateRef<int> count;

    public SlateRef<bool> storeAsClass;
    [NoTranslate]
    public SlateRef<string> storeAs;
    [NoTranslate]
    public SlateRef<string> addToList;

    protected override bool TestRunInt(Slate slate)
    {
        return thingDef.GetValue(slate) is not null && count.GetValue(slate) > 0;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (storeAsClass.GetValue(slate))
        {
            ThingDefCountClass thingDefCountClass = new(thingDef.GetValue(slate), count.GetValue(slate));
            if (storeAs.GetValue(slate) is not null)
            {
                QuestGen.slate.Set(storeAs.GetValue(slate), thingDefCountClass);
            }
            if (addToList.GetValue(slate) is not null)
            {
                QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thingDefCountClass);
            }
        }
        else
        {
            ThingDefCount thingDefCount = new(thingDef.GetValue(slate), count.GetValue(slate));
            if (storeAs.GetValue(slate) is not null)
            {
                QuestGen.slate.Set(storeAs.GetValue(slate), thingDefCount);
            }
            if (addToList.GetValue(slate) is not null)
            {
                QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thingDefCount);
            }
        }
    }
}