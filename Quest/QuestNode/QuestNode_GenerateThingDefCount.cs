using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GenerateThingDefCount : QuestNode
{
    SlateRef<ThingDef> thingDef;
    SlateRef<int> count;

    [NoTranslate]
    public SlateRef<string> storeAs;
    [NoTranslate]
    public SlateRef<string> addToList;

    protected override bool TestRunInt(Slate slate)
    {
        return thingDef.GetValue(slate) != null && count.GetValue(slate) > 0;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        ThingDefCount thingDefCount = new(thingDef.GetValue(slate), count.GetValue(slate));
        if (storeAs.GetValue(slate) != null)
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), thingDefCount);
        }
        if (addToList.GetValue(slate) != null)
        {
            QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thingDefCount);
        }
    }
}
