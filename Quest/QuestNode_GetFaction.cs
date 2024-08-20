using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//用于获取派系
public class QuestNode_GetFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef;

    public SlateRef<bool> allowEnemy;

    public SlateRef<bool> allowNeutral = true;

    public SlateRef<bool> allowAlly = true;

    public SlateRef<bool> allowPermanentEnemy;

    public SlateRef<bool> allowHiddenFactions;

    public SlateRef<bool> allowDefeatedFactions;

    public SlateRef<bool> allowNonHumanlikeFactions;

    public SlateRef<bool> mustBePermanentEnemy;

    public SlateRef<bool> playerCantBeAttackingCurrently;

    public SlateRef<bool> leaderMustBeSafe;

    public SlateRef<bool> mustHaveGoodwillRewardsEnabled;

    public SlateRef<Pawn> ofPawn;

    public SlateRef<Faction> mustBeHostileToFaction;

    public SlateRef<IEnumerable<Faction>> exclude;

    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet<Faction>(storeAs.GetValue(slate), out var faction) && IsGoodFaction(faction, slate))
        {
            return true;
        }
        if (factionDef != null && SetFaction(out faction, slate))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            return true;
        }
        if (TryFindFaction(out faction, slate))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if ((factionDef != null && SetFaction(out var faction, slate)) || ((!QuestGen.slate.TryGet<Faction>(storeAs.GetValue(slate), out faction) || !IsGoodFaction(faction, QuestGen.slate)) && TryFindFaction(out faction, QuestGen.slate)))
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), faction);
            if (!faction.Hidden)
            {
                QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                questPart_InvolvedFactions.factions.Add(faction);
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    protected bool SetFaction(out Faction faction, Slate slate)
    {
        FactionDef fDef = factionDef.GetValue(slate);
        if (factionDef != null)
        {
            faction = OberoniaAureaFrameUtility.RandomFactionOfDef(fDef);
            return faction != null;
        }
        faction = null;
        return false;
    }

    protected bool TryFindFaction(out Faction faction, Slate slate)
    {
        return (from x in Find.FactionManager.GetFactions(allowHiddenFactions.GetValue(slate), allowDefeatedFactions.GetValue(slate), allowNonHumanlikeFactions.GetValue(slate))
                where x != null && IsGoodFaction(x, slate)
                select x).TryRandomElement(out faction);
    }

    protected virtual bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (ofPawn.GetValue(slate) != null && faction != ofPawn.GetValue(slate).Faction)
        {
            return false;
        }
        if (exclude.GetValue(slate) != null && exclude.GetValue(slate).Contains(faction))
        {
            return false;
        }
        if (faction.def.permanentEnemy)
        {
            if (!allowPermanentEnemy.GetValue(slate))
            {
                return false;
            }
        }
        else if (mustBePermanentEnemy.GetValue(slate))
        {
            return false;
        }
        FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
        if (!allowEnemy.GetValue(slate) && playerRelationKind == FactionRelationKind.Hostile)
        {
            return false;
        }
        if (!allowNeutral.GetValue(slate) && playerRelationKind == FactionRelationKind.Neutral)
        {
            return false;
        }
        if (!allowAlly.GetValue(slate) && playerRelationKind == FactionRelationKind.Ally)
        {
            return false;
        }

        if (playerCantBeAttackingCurrently.GetValue(slate) && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
        {
            return false;
        }
        if (mustHaveGoodwillRewardsEnabled.GetValue(slate) && !faction.allowGoodwillRewards)
        {
            return false;
        }
        if (leaderMustBeSafe.GetValue(slate) && !IsLeaderSafe(faction.leader))
        {
            return false;
        }
        Faction faction2 = mustBeHostileToFaction.GetValue(slate);
        if (faction2 == null || !faction.HostileTo(faction2))
        {
            return false;
        }
        return true;
    }

    private static bool IsLeaderSafe(Pawn leader)
    {
        if (leader == null || leader.Spawned || leader.IsPrisoner)
        {
            return false;
        }
        return true;
    }
}
