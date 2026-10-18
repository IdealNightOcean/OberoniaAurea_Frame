using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：获取派系。 
/// </summary>
public class QuestNode_GetFaction : QuestNode
{
    /// <summary>
    /// 存储标签：派系。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <summary>
    /// 派系定义。
    /// </summary>
    public SlateRef<FactionDef> factionDef;

    /// <summary>
    /// 是否允许敌对方系。
    /// </summary>
    public SlateRef<bool> allowEnemy;

    /// <summary>
    /// 是否允许中立派系。
    /// </summary>
    public SlateRef<bool> allowNeutral = true;

    /// <summary>
    /// 是否允许盟友派系。
    /// </summary>
    public SlateRef<bool> allowAlly = true;

    /// <summary>
    /// 是否允许永久敌人派系。
    /// </summary>
    public SlateRef<bool> allowPermanentEnemy;

    /// <summary>
    /// 是否允许隐藏派系。
    /// </summary>
    public SlateRef<bool> allowHiddenFactions;

    /// <summary>
    /// 是否允许已战败派系。
    /// </summary>
    public SlateRef<bool> allowDefeatedFactions;

    /// <summary>
    /// 是否允许非人类派系。
    /// </summary>
    public SlateRef<bool> allowNonHumanlikeFactions;

    /// <summary>
    /// 是否必须是永久敌人派系。
    /// </summary>
    public SlateRef<bool> mustBePermanentEnemy;

    /// <summary>
    /// 玩家当前是否不能正在攻击该派系。
    /// </summary>
    public SlateRef<bool> playerCantBeAttackingCurrently;

    /// <summary>
    /// 派系领袖是否必须处于安全状态。
    /// </summary>
    public SlateRef<bool> leaderMustBeSafe;

    /// <summary>
    /// 派系是否必须启用善行奖励。
    /// </summary>
    public SlateRef<bool> mustHaveGoodwillRewardsEnabled;

    /// <summary>
    /// 指定获取其所属派系的角色。
    /// </summary>
    public SlateRef<Pawn> ofPawn;

    /// <summary>
    /// 派系必须与指定派系为敌对关系。
    /// </summary>
    public SlateRef<Faction> mustBeHostileToFaction;

    /// <summary>
    /// 需要排除的派系。
    /// </summary>
    public SlateRef<IEnumerable<Faction>> exclude;

    /// <summary>
    /// 派系校验器。
    /// </summary>
    protected FactionValidator FactionValidatorInt { get; set; }

    /// <summary>
    /// 初始化派系校验器。
    /// </summary>
    /// <param name="slate">任务数据。 </param>
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
            foreach (Faction faction in excludeEnumer)
            {
                FactionValidatorInt.Exclude.Add(faction);
            }
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (GetValidFaction(slate, out Faction faction))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            if (!faction.Hidden)
            {
                Utility.OAFrame_QuestUtility.AddInvolvedFaction(QuestGen.quest, faction);
            }
        }
    }

    /// <summary>
    /// 获取有效派系。
    /// </summary>
    /// <param name="slate">任务数据。 </param>
    /// <param name="faction">获取到的有效派系。 </param>
    /// <returns>是否成功获取到有效派系。 </returns>
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

    /// <summary>
    /// 判断派系是否符合条件。
    /// </summary>
    /// <param name="faction">要判断的派系。 </param>
    /// <param name="slate">任务数据。 </param>
    /// <returns>是否符合条件。 </returns>
    protected virtual bool IsGoodFaction(Faction faction, Slate slate) => FactionValidatorInt.IsValid(faction);

    /// <summary>
    /// 派系校验器。
    /// </summary>
    protected class FactionValidator
    {
        /// <summary>
        /// 派系定义。
        /// </summary>
        public FactionDef FactionDef { get; set; }

        /// <summary>
        /// 是否允许敌对方系。
        /// </summary>
        public bool AllowEnemy { get; set; }

        /// <summary>
        /// 是否允许中立派系。
        /// </summary>
        public bool AllowNeutral { get; set; } = true;

        /// <summary>
        /// 是否允许盟友派系。
        /// </summary>
        public bool AllowAlly { get; set; } = true;

        /// <summary>
        /// 是否允许永久敌人派系。
        /// </summary>
        public bool AllowPermanentEnemy { get; set; }

        /// <summary>
        /// 是否允许隐藏派系。
        /// </summary>
        public bool AllowHiddenFactions { get; set; }

        /// <summary>
        /// 是否允许已战败派系。
        /// </summary>
        public bool AllowDefeatedFactions { get; set; }

        /// <summary>
        /// 是否允许非人类派系。
        /// </summary>
        public bool AllowNonHumanlikeFactions { get; set; }

        /// <summary>
        /// 是否必须是永久敌人派系。
        /// </summary>
        public bool MustBePermanentEnemy { get; set; }

        /// <summary>
        /// 玩家当前是否不能正在攻击该派系。
        /// </summary>
        public bool PlayerCantBeAttackingCurrently { get; set; }

        /// <summary>
        /// 派系领袖是否必须处于安全状态。
        /// </summary>
        public bool LeaderMustBeSafe { get; set; }

        /// <summary>
        /// 派系是否必须启用善行奖励。
        /// </summary>
        public bool MustHaveGoodwillRewardsEnabled { get; set; }

        /// <summary>
        /// 指定获取其所属派系的角色。
        /// </summary>
        public Pawn OfPawn { get; set; }

        /// <summary>
        /// 派系必须与指定派系为敌对关系。
        /// </summary>
        public Faction MustBeHostileToFaction { get; set; }

        private HashSet<Faction> exclude;

        /// <summary>
        /// 需要排除的派系集合。
        /// </summary>
        public HashSet<Faction> Exclude => exclude ??= [];

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public FactionValidator() { }

        /// <summary>
        /// 判断派系是否有效。
        /// </summary>
        /// <param name="faction">要判断的派系。 </param>
        /// <returns>是否为有效派系。 </returns>
        public bool IsValid(Faction faction)
        {
            if (faction is null || faction.IsPlayerSafe())
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