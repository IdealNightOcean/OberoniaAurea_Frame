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
        if (GetValidFaction(slate, out Faction faction))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (GetValidFaction(slate, out Faction faction))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            if (!faction.Hidden)
            {
                QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                questPart_InvolvedFactions.factions.Add(faction);
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    protected virtual bool GetValidFaction(Slate slate, out Faction faction)
    {
        if (factionDef.GetValue(slate) is not null)
        {
            return SetFaction(out faction, slate);
        }
        if (TryFindFaction(out faction, slate))
        {
            return true;
        }
        faction = null;
        return false;
    }

    protected bool SetFaction(out Faction faction, Slate slate)
    {
        FactionDef fDef = factionDef.GetValue(slate);
        if (fDef is not null)
        {
            faction = Find.FactionManager.AllFactionsListForReading.Where(f => f.def == fDef && IsGoodFaction(f, slate)).RandomElementWithFallback(null);
            return faction is not null;
        }
        else
        {
            faction = null;
            return false;
        }
    }

    protected bool TryFindFaction(out Faction faction, Slate slate)
    {
        return (from x in Find.FactionManager.GetFactions(allowHiddenFactions.GetValue(slate), allowDefeatedFactions.GetValue(slate), allowNonHumanlikeFactions.GetValue(slate))
                where IsGoodFaction(x, slate)
                select x).TryRandomElement(out faction);
    }

    protected virtual bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (ofPawn.GetValue(slate) is not null && faction != ofPawn.GetValue(slate).Faction)
        {
            return false;
        }
        if (exclude.GetValue(slate) is not null && exclude.GetValue(slate).Contains(faction))
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
        Faction hostileFaction = mustBeHostileToFaction.GetValue(slate);
        if (hostileFaction is not null && !faction.HostileTo(hostileFaction))
        {
            return false;
        }
        return true;
    }

    private static bool IsLeaderSafe(Pawn leader)
    {
        if (leader is null || leader.Spawned || leader.IsPrisoner)
        {
            return false;
        }
        return true;
    }
}