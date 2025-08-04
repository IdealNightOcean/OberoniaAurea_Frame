using RimWorld;
using RimWorld.QuestGen;
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
        return ValidSeason(slate);
    }
    protected override void RunInt()
    { }

    protected bool ValidSeason(Slate slate)
    {
        Map map = slate.Get<Map>("map");
        if (map is null)
        {
            return false;
        }
        Season season = GenLocalDate.Season(map);
        return season switch
        {
            Season.Spring => allowSpring.GetValue(slate),
            Season.Summer or Season.PermanentSummer => allowSummer.GetValue(slate),
            Season.Fall => allowFall.GetValue(slate),
            Season.Winter or Season.PermanentWinter => allowWinter.GetValue(slate),
            _ => false,
        };
    }

}