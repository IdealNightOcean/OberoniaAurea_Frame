using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
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

    protected FactionValidator FactionValidatorInt { get; set; }

    protected void InitFactionValidator(Slate slate)
    {
        FactionValidatorInt = new()
        {
            FactionDef = factionDef.GetValue(slate),
            AllowEnemy = allowEnemy.GetValue(slate),
            AllowNeutral = allowNeutral.GetValue(slate),
            AllowAlly = allowAlly.GetValue(slate),
            AllowPermanentEnemy = allowPermanentEnemy.GetValue(slate),
            AllowHiddenFactions = allowHiddenFactions.GetValue(slate),
            AllowDefeatedFactions = allowDefeatedFactions.GetValue(slate),
            AllowNonHumanlikeFactions = allowNonHumanlikeFactions.GetValue(slate),
            MustBePermanentEnemy = mustBePermanentEnemy.GetValue(slate),
            PlayerCantBeAttackingCurrently = playerCantBeAttackingCurrently.GetValue(slate),
            LeaderMustBeSafe = leaderMustBeSafe.GetValue(slate),
            MustHaveGoodwillRewardsEnabled = mustHaveGoodwillRewardsEnabled.GetValue(slate),
            OfPawn = ofPawn.GetValue(slate),
            MustBeHostileToFaction = mustBeHostileToFaction.GetValue(slate),
        };

        IEnumerable<Faction> excludeEnumer = exclude.GetValue(slate);
        if (excludeEnumer is not null)
        {
            foreach (var faction in excludeEnumer)
            {
                FactionValidatorInt.Exclude.Add(faction);
            }
        }
    }

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
                OAFrame_QuestUtility.AddInvolvedFaction(QuestGen.quest, faction);
            }
        }
    }

    protected virtual bool GetValidFaction(Slate slate, out Faction faction)
    {
        InitFactionValidator(slate);

        if (slate.TryGet(storeAs.GetValue(slate), out faction) && IsGoodFaction(faction, slate))
            return true;

        List<Faction> potentialFactions = [];
        foreach (Faction f in Find.FactionManager.AllFactions)
        {
            if (IsGoodFaction(f, slate))
                potentialFactions.Add(f);
        }

        if (potentialFactions.Count == 0)
        {
            faction = null;
            return false;
        }
        else
        {
            faction = potentialFactions.RandomElement();
            return true;
        }
    }

    protected virtual bool IsGoodFaction(Faction faction, Slate slate) => FactionValidatorInt.IsValid(faction);

    protected struct FactionValidator
    {
        public FactionDef FactionDef { get; set; }
        public bool AllowEnemy { get; set; }
        public bool AllowNeutral { get; set; } = true;
        public bool AllowAlly { get; set; } = true;
        public bool AllowPermanentEnemy { get; set; }
        public bool AllowHiddenFactions { get; set; }
        public bool AllowDefeatedFactions { get; set; }
        public bool AllowNonHumanlikeFactions { get; set; }

        public bool MustBePermanentEnemy { get; set; }
        public bool PlayerCantBeAttackingCurrently { get; set; }
        public bool LeaderMustBeSafe { get; set; }
        public bool MustHaveGoodwillRewardsEnabled { get; set; }
        public Pawn OfPawn { get; set; }
        public Faction MustBeHostileToFaction { get; set; }

        private HashSet<Faction> exclude;
        public HashSet<Faction> Exclude => exclude ??= [];

        public FactionValidator() { }

        public bool IsValid(Faction faction)
        {
            if (faction is null)
                return false;

            if (FactionDef is not null && faction.def != FactionDef)
                return false;

            if (OfPawn is not null && faction != OfPawn.Faction)
                return false;

            if (Exclude.Contains(faction))
                return false;

            if (faction.def.permanentEnemy)
            {
                if (!AllowPermanentEnemy)
                    return false;
            }
            else if (MustBePermanentEnemy)
                return false;

            switch (faction.PlayerRelationKind)
            {
                case FactionRelationKind.Hostile:
                    if (!AllowEnemy) return false;
                    break;
                case FactionRelationKind.Neutral:
                    if (!AllowNeutral) return false;
                    break;
                case FactionRelationKind.Ally:
                    if (!AllowAlly) return false;
                    break;
                default: break;
            }

            if (MustHaveGoodwillRewardsEnabled && !faction.allowGoodwillRewards)
                return false;

            if (PlayerCantBeAttackingCurrently && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
                return false;

            if (LeaderMustBeSafe && !IsLeaderSafe(faction.leader))
                return false;

            if (MustBeHostileToFaction is not null && !faction.HostileTo(MustBeHostileToFaction))
                return false;

            return true;
        }

        private static bool IsLeaderSafe(Pawn leader) => leader is not null && !leader.Spawned && !leader.IsPrisoner;
    }
}