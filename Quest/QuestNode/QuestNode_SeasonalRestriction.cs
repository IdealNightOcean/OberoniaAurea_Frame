using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_SeasonalRestriction : QuestNode
{
    public SlateRef<bool> allowSpring = true;
    public SlateRef<bool> allowSummer = true;
    public SlateRef<bool> allowFall = true;
    public SlateRef<bool> allowWinter = true;

    protected override bool TestRunInt(Slate slate)
    {
        Season season;
        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            season = GenDate.Season(GenTicks.TicksAbs, Vector2.zero);
        }
        else
        {
            season = GenLocalDate.Season(map);
        }

        return season switch
        {
            Season.Spring => allowSpring.GetValue(slate),
            Season.Summer or Season.PermanentSummer => allowSummer.GetValue(slate),
            Season.Fall => allowFall.GetValue(slate),
            Season.Winter or Season.PermanentWinter => allowWinter.GetValue(slate),
            _ => false,
        };
    }
    protected override void RunInt() { }
}