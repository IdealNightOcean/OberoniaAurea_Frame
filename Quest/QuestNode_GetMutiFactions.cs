using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//QuestNode：获取指定数量多个派系
public class QuestNode_GetMutiFactions : OberoniaAurea_Frame.QuestNode_GetFaction
{
    public SlateRef<IntRange> factionCount;
    public SlateRef<bool> ignoreMinCountIfNessary = true; //必要时忽略数量下限
    protected override bool TestRunInt(Slate slate)
    {
        if (TryFindFactions(out List<Faction> factions, factionCount.GetValue(slate), slate))
        {
            slate.Set(storeAs.GetValue(slate), factions);
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (TryFindFactions(out List<Faction> factions, factionCount.GetValue(slate), QuestGen.slate))
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), factions);
            foreach (Faction f in factions)
            {
                if (!f.Hidden)
                {
                    QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                    questPart_InvolvedFactions.factions.Add(f);
                    QuestGen.quest.AddPart(questPart_InvolvedFactions);
                }
            }
        }
    }

    private bool TryFindFactions(out List<Faction> factions, IntRange factionCount, Slate slate)
    {
        factions = Find.FactionManager.GetFactions(allowHidden: true).Where(f => IsGoodFaction(f, slate)).InRandomOrder().Take(factionCount.RandomInRange).ToList();
        return factions.Count > 0 && (ignoreMinCountIfNessary.GetValue(slate) || factions.Count >= factionCount.min);
    }
}
