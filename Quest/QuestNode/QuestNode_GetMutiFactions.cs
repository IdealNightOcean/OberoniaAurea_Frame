using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    private bool TryFindFactions(out List<Faction> factions, IntRange factionCountRange, Slate slate)
    {
        InitFactionValidator(slate);

        factions = slate.Get<List<Faction>>(storeAs.GetValue(slate));
        factions ??= [];

        if (!factions.NullOrEmpty())
            factions.RemoveAll(f => !IsGoodFaction(f, slate));

        if (factions.Count >= factionCountRange.min)
            return true;

        List<Faction> potentialFactions = [];
        foreach (Faction f in Find.FactionManager.AllFactions)
        {
            if (IsGoodFaction(f, slate))
            {
                potentialFactions.Add(f);
            }
        }

        if (potentialFactions.Count == 0)
            return false;

        int takeCount = Mathf.Min(factionCountRange.RandomInRange, potentialFactions.Count);
        factions = potentialFactions.TakeRandomElements(factionCountRange.RandomInRange).ToList();
        return ignoreMinCountIfNessary.GetValue(slate) || factions.Count >= factionCountRange.min;
    }
}