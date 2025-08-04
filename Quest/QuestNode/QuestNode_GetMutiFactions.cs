using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//QuestNode：获取指定数量多个派系
public class QuestNode_GetMutiFactions : QuestNode_GetFaction
{
    public SlateRef<IntRange> factionCount;
    public SlateRef<bool> ignoreMinCountIfNessary = true; //必要时忽略数量下限
    protected override bool TestRunInt(Slate slate)
    {
        if (factionCount.GetValue(slate).max <= 0)
        {
            return false;
        }
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
        if (TryFindFactions(out List<Faction> factions, factionCount.GetValue(slate), slate))
        {
            slate.Set(storeAs.GetValue(slate), factions);
            bool addFlag = false;
            QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
            foreach (Faction f in factions)
            {
                if (!f.Hidden)
                {
                    questPart_InvolvedFactions.factions.Add(f);
                    addFlag = true;
                }
            }
            if (addFlag)
            {
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    private bool TryFindFactions(out List<Faction> factions, IntRange factionCount, Slate slate)
    {
        factions = Find.FactionManager.GetFactions(allowHiddenFactions.GetValue(slate), allowDefeatedFactions.GetValue(slate), allowNonHumanlikeFactions.GetValue(slate)).Where(f => IsGoodFaction(f, slate)).InRandomOrder().Take(factionCount.RandomInRange).ToList();
        return factions.Count > 0 && (ignoreMinCountIfNessary.GetValue(slate) || factions.Count >= factionCount.min);
    }
}