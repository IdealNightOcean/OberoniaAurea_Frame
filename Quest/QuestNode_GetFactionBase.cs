using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

//用于获取派系
public class QuestNode_GetFactionBase : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef;

    public SlateRef<bool> allowEnemy;

    public SlateRef<bool> allowNeutral = true;

    public SlateRef<bool> allowAlly;

    public SlateRef<bool> allowAskerFaction;

    public SlateRef<bool> allowPermanentEnemy;

    public SlateRef<bool> mustBePermanentEnemy;

    public SlateRef<bool> playerCantBeAttackingCurrently;

    public SlateRef<bool> leaderMustBeSafe;

    public SlateRef<bool> mustHaveGoodwillRewardsEnabled;

    public SlateRef<Pawn> ofPawn;

    public SlateRef<Thing> mustBeHostileToFactionOf;

    public SlateRef<IEnumerable<Faction>> exclude;

    public SlateRef<IEnumerable<Faction>> allowedHiddenFactions;

    protected override bool TestRunInt(Slate slate)
    {
        if (slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) && IsGoodFaction(var, slate))
        {
            return true;
        }
        if (TryFindFaction(out var, slate))
        {
            slate.Set(storeAs.GetValue(slate), var);
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
        bool allowHidden = allowedHiddenFactions.GetValue(slate) != null;
        return (from x in Find.FactionManager.GetFactions(allowHidden)
                where x != null && IsGoodFaction(x, slate)
                select x).TryRandomElement(out faction);
    }

    protected virtual bool IsGoodFaction(Faction faction, Slate slate)
    {
        if (faction.Hidden && !allowedHiddenFactions.GetValue(slate).Contains(faction))
        {
            return false;
        }
        if (ofPawn.GetValue(slate) != null && faction != ofPawn.GetValue(slate).Faction)
        {
            return false;
        }
        if (exclude.GetValue(slate) != null && exclude.GetValue(slate).Contains(faction))
        {
            return false;
        }
        if(faction.def.permanentEnemy)
        {
            if(!allowPermanentEnemy.GetValue(slate))
            {
                return false;
            }
        }
        else if(mustBePermanentEnemy.GetValue(slate))
        {
            return false;
        }
        if (!allowEnemy.GetValue(slate) && faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        if (!allowNeutral.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Neutral)
        {
            return false;
        }
        if (!allowAlly.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Ally)
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
        if (leaderMustBeSafe.GetValue(slate) && (faction.leader == null || faction.leader.Spawned || faction.leader.IsPrisoner))
        {
            return false;
        }
        Thing value = mustBeHostileToFactionOf.GetValue(slate);
        if (value != null && value.Faction != null && (value.Faction == faction || !faction.HostileTo(value.Faction)))
        {
            return false;
        }
        return true;
    }
}
