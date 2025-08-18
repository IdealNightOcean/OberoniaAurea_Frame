using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_GetMapParent : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<Map> targetMap;

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }

    private void SetVars(Slate slate)
    {
        MapParent mapParent = targetMap.GetValue(slate)?.Parent;
        if (mapParent is not null)
        {
            slate.Set(storeAs.GetValue(slate), mapParent);
        }
    }
}