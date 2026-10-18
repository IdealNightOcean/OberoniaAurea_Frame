using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：难民任务基础节点。 
/// </summary>
public class QuestNode_Root_RefugeeBase : QuestNode
{
    /// <summary>
    /// Slate 键：是否为主要派系。 
    /// </summary>
    protected const string IsMainFactionSlate = "isMainFaction";
    /// <summary>
    /// Slate 键：独特任务描述。 
    /// </summary>
    protected const string UniqueQuestDescSlate = "uniqueQuestDesc";
    /// <summary>
    /// Slate 键：独特离开信件。 
    /// </summary>
    protected const string UniqueLeavingLetterSlate = "uniqueLeavingLetter";

    /// <summary>
    /// 敌对派系敌对者角色的生成参数。 
    /// </summary>
    protected static readonly QuestGen_Pawns.GetPawnParms FactionOpponentPawnParams = new()
    {
        mustBeWorldPawn = true,
        mustBeFactionLeader = false,
        mustBeNonHostileToPlayer = true
    };

    /// <summary>
    /// 难民任务参数类，承载任务运行所需的各种可配置数据。 
    /// </summary>
    protected sealed class QuestParameter
    {
        /// <summary>
        /// 是否允许袭击殖民地。 
        /// </summary>
        public bool allowAssaultColony = true;
        /// <summary>
        /// 是否允许离开。 
        /// </summary>
        public bool allowLeave = true;
        /// <summary>
        /// 是否允许不良想法。 
        /// </summary>
        public bool allowBadThought = true;
        /// <summary>
        /// 是否允许加入邀请。 
        /// </summary>
        public bool allowJoinOffer = true;
        /// <summary>
        /// 是否允许未来奖励。 
        /// </summary>
        public bool allowFutureReward = true;

        private int lodgerCount = 1;
        private int childCount = 0;
        /// <summary>
        /// 寄宿者总数，最小为 1。 
        /// </summary>
        public int LodgerCount { get { return lodgerCount; } set { lodgerCount = Mathf.Max(1, value); } }
        /// <summary>
        /// 儿童数量，限制在 0 到寄宿者总数之间。 
        /// </summary>
        public int ChildCount { get { return childCount; } set { childCount = Mathf.Clamp(value, 0, lodgerCount); } }

        /// <summary>
        /// 任务持续的时间刻数。 
        /// </summary>
        public int questDurationTicks = 60000;
        /// <summary>
        /// 到达延迟的时间刻数，小于等于 0 表示无延迟。 
        /// </summary>
        public int arrivalDelayTicks = -1;

        /// <summary>
        /// 成功时获得的好感度。 
        /// </summary>
        public int goodwillSuccess;
        /// <summary>
        /// 失败时损失的好感度。 
        /// </summary>
        public int goodwillFailure;
        /// <summary>
        /// 奖励价值的浮动范围。 
        /// </summary>
        public FloatRange rewardValueRange;

        /// <summary>
        /// 任务地图。 
        /// </summary>
        public Map map;
        /// <summary>
        /// 任务派系。 
        /// </summary>
        public Faction faction;

        /// <summary>
        /// 任务角色列表。 
        /// </summary>
        public List<Pawn> pawns;

        /// <summary>
        /// 构造函数，从 Slate 或默认获取地图。 
        /// </summary>
        public QuestParameter()
        {
            map = QuestGen.slate.Get<Map>("map") ?? QuestGen_Get.GetMap();
        }

        /// <summary>
        /// 构造函数，指定任务地图。 
        /// </summary>
        /// <param name="map">任务地图。</param>
        public QuestParameter(Map map)
        {
            this.map = map;
        }
    }

    /// <summary>
    /// 当前难民任务参数。 
    /// </summary>
    protected QuestParameter questParameter;

    /// <summary>
    /// 固定的角色种类。 
    /// </summary>
    protected virtual PawnKindDef FixedPawnKind => PawnKindDefOf.Refugee;
    /// <summary>
    /// 要添加的想法。 
    /// </summary>
    protected virtual ThoughtDef ThoughtToAdd => null;

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;

        if (!InitQuestParameter())
        {
            QuestGen_End.End(quest, QuestEndOutcome.Unknown, sendStandardLetter: false, playSound: false);
            return;
        }

        slate.Set("allowFutureReward", questParameter.allowFutureReward);
        slate.Set("allowJoinOffer", questParameter.allowJoinOffer);

        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            QuestGen_End.End(quest, QuestEndOutcome.Unknown, sendStandardLetter: false, playSound: false);
            return;
        }
        questParameter.faction = faction;
        slate.Set("map", questParameter.map);
        slate.Set("faction", faction);
        if (!slate.TryGet(IsMainFactionSlate, out bool _))
        {
            slate.Set(IsMainFactionSlate, !faction.temporary);
        }
        slate.Set("questDurationTicks", questParameter.questDurationTicks);
        if (questParameter.arrivalDelayTicks > 0)
        {
            slate.Set("arrivalDelayTicks", questParameter.arrivalDelayTicks);
        }

        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
        quest.AnySignal(inSignals: [lodgerRecruitedSignal, lodgerArrestedSignal], outSignals: [lodgerArrestedOrRecruited]);

        List<Pawn> pawns = GeneratePawns(lodgerRecruitedSignal);
        if (pawns.NullOrEmpty())
        {
            QuestGen_End.End(quest, QuestEndOutcome.Unknown, sendStandardLetter: false, playSound: false);
            return;
        }
        questParameter.pawns = pawns;

        slate.Set("lodgers", pawns);
        slate.Set("asker", pawns[0]);
        slate.Set("lodgerCount", pawns.Count);
        slate.Set("lodgersCountMinusOne", pawns.Count - 1);

        quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);
        quest.SetAllApparelLocked(pawns);

        string lodgerArrivalSignal = QuestGenUtility.HardcodedSignalWithQuestID("Lodger_Arrival");
        PawnArrival(lodgerArrivalSignal);

        SetQuestAward();

        QuestPart_OARefugeeInteractions questPart_RefugeeInteractions = new()
        {
            inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
            faction = faction,
            mapParent = questParameter.map.Parent,
            inSignalArrested = lodgerArrestedSignal,
            inSignalRecruited = lodgerRecruitedSignal,
            signalListenMode = QuestPart.SignalListenMode.Always
        };
        questPart_RefugeeInteractions.InitWithDefaultSingals(questParameter.allowAssaultColony, questParameter.allowLeave, questParameter.allowBadThought);
        questPart_RefugeeInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_RefugeeInteractions);

        if (questParameter.allowBadThought)
        {
            quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_BadThought);
            quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_BadThought);
            quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought);
            quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerLeftBehind, questPart_RefugeeInteractions.outSignalLeftBehind_BadThought);
        }

        SetPawnsLeaveComp(lodgerArrivalSignal, lodgerArrestedOrRecruited);
        SetQuestEndCompCommon(questPart_RefugeeInteractions);
    }

    /// <summary>
    /// 初始化任务参数。 
    /// </summary>
    /// <returns>初始化是否成功。</returns>
    protected virtual bool InitQuestParameter()
    {
        questParameter = new QuestParameter();
        return true;
    }

    /// <summary>
    /// 清空任务参数。 
    /// </summary>
    protected virtual void ClearQuestParameter()
    {
        questParameter = null;
    }

    /// <summary>
    /// 生成寄宿者角色列表。 
    /// </summary>
    /// <param name="lodgerRecruitedSignal">寄宿者被招募的信号。</param>
    /// <returns>生成的寄宿者角色列表。</returns>
    protected virtual List<Pawn> GeneratePawns(string lodgerRecruitedSignal = null)
    {
        Quest quest = QuestGen.quest;
        List<Pawn> pawns = [];
        int adultCount = questParameter.LodgerCount - questParameter.ChildCount;

        PawnKindDef fixedPawnKind = FixedPawnKind ?? PawnKindDefOf.Refugee;

        for (int i = 0; i < questParameter.LodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = i < adultCount ? DevelopmentalStage.Adult : DevelopmentalStage.Child;
            Pawn pawn = quest.GeneratePawn(kindDef: fixedPawnKind,
                                           faction: questParameter.faction,
                                           forceGenerateNewPawn: true,
                                           developmentalStages: developmentalStages,
                                           allowPregnant: false);

            pawns.Add(pawn);

            PostPawnGenerated(pawn, lodgerRecruitedSignal);
        }

        return pawns;
    }

    /// <summary>
    /// 角色生成后的后续处理，例如添加想法或加入邀请。 
    /// </summary>
    /// <param name="pawn">已生成的角色。</param>
    /// <param name="lodgerRecruitedSignal">寄宿者被招募的信号。</param>
    protected virtual void PostPawnGenerated(Pawn pawn, string lodgerRecruitedSignal)
    {
        Quest quest = QuestGen.quest;
        if (ThoughtToAdd is not null)
        {
            QuestPart_AddMemoryThought questPart_AddMemoryThought = new()
            {
                inSignal = QuestGen.slate.Get<string>("inSignal"),
                pawn = pawn,
                def = ThoughtToAdd
            };
            quest.AddPart(questPart_AddMemoryThought);
        }

        if (questParameter.allowJoinOffer)
        {
            quest.PawnJoinOffer(pawn,
            "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")),
            "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")),
            "LetterJoinOfferText".Translate(pawn.Named("PAWN"),
            questParameter.map.Parent.Named("MAP")),
            delegate
            {
                quest.JoinPlayer(questParameter.map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                quest.Letter(letterDef: LetterDefOf.PositiveEvent,
                             signalListenMode: QuestPart.SignalListenMode.OngoingOnly,
                             label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap,
                             text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                quest.SignalPass(outSignal: lodgerRecruitedSignal);
            },
            delegate
            {
                quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
            },
            charity: true);
        }
    }

    /// <summary>
    /// 处理寄宿者到达，可带延迟。 
    /// </summary>
    /// <param name="lodgerArrivalSignal">寄宿者到达的信号。</param>
    protected virtual void PawnArrival(string lodgerArrivalSignal)
    {
        Quest quest = QuestGen.quest;

        if (questParameter.arrivalDelayTicks > 0)
        {
            quest.Delay(questParameter.arrivalDelayTicks, delegate
            {
                quest.PawnsArrive(pawns: questParameter.pawns,
                                  mapParent: questParameter.map.Parent,
                                  joinPlayer: true,
                                  customLetterLabel: "[lodgersArriveLetterLabel]",
                                  customLetterText: "[lodgersArriveLetterText]");
            },
            outSignalComplete: lodgerArrivalSignal);
        }
        else
        {
            quest.PawnsArrive(pawns: questParameter.pawns,
                              mapParent: questParameter.map.Parent,
                              joinPlayer: true,
                              customLetterLabel: "[lodgersArriveLetterLabel]",
                              customLetterText: "[lodgersArriveLetterText]");
            quest.SendSignals(outSignals: [lodgerArrivalSignal]);
        }
    }

    /// <summary>
    /// 设置任务奖励选择。 
    /// </summary>
    protected void SetQuestAward()
    {
        QuestPart_Choice questPart_Choice = QuestGen.quest.RewardChoice();
        QuestPart_Choice.Choice choice = new()
        {
            rewards =
            {
                new Reward_VisitorsHelp()
            }
        };
        if (questParameter.allowFutureReward)
        {
            choice.rewards.Add(new Reward_PossibleFutureReward());
        }

        if (questParameter.goodwillSuccess != 0)
        {
            choice.rewards.Add(new Reward_Goodwill()
            {
                amount = questParameter.goodwillSuccess,
                faction = questParameter.faction
            });
        }

        if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.FluidIdeo is not null)
        {
            choice.rewards.Add(new Reward_DevelopmentPoints(QuestGen.quest));
        }

        AddQuestAward(choice);

        questPart_Choice.choices.Add(choice);
    }

    /// <summary>
    /// 派生类可重写以向奖励选择添加自定义奖励。 
    /// </summary>
    /// <param name="choice">奖励选择。</param>
    protected virtual void AddQuestAward(QuestPart_Choice.Choice choice) { }

    /// <summary>
    /// 获取或生成难民派系。 
    /// </summary>
    /// <returns>生成或获取到的派系。</returns>
    protected virtual Faction GetOrGenerateFaction()
    {
        List<FactionRelation> list = [];
        foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
        {
            if (!item.def.permanentEnemy)
            {
                list.Add(new FactionRelation
                {
                    other = item,
                    kind = FactionRelationKind.Neutral
                });
            }
        }
        FactionGeneratorParms parms = new(FactionDefOf.OutlanderRefugee, default, true);
        if (ModsConfig.IdeologyActive)
        {
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where(p => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
        Find.FactionManager.Add(faction);
        return faction;
    }

    /// <summary>
    /// 设置任务结束组件相关信号。 
    /// </summary>
    /// <param name="questPart_Interactions">难民交互任务组件。</param>
    /// <param name="failSignal">任务失败信号。</param>
    /// <param name="delayFailSignal">迟延失败信号。</param>
    /// <param name="successSignal">任务成功信号。</param>
    protected virtual void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string delayFailSignal, string successSignal) { }

    /// <summary>
    /// 设置寄宿者离开组件。 
    /// </summary>
    /// <param name="lodgerArrivalSignal">寄宿者到达的信号。</param>
    /// <param name="inSignalRemovePawn">移除角色的信号。</param>
    protected virtual void SetPawnsLeaveComp(string lodgerArrivalSignal, string inSignalRemovePawn)
    {
        if (questParameter.questDurationTicks > 0)
        {
            DefaultDelayLeaveComp(lodgerArrivalSignal, inSignalDisable: null, inSignalRemovePawn);
        }
    }

    /// <summary>
    /// 设置默认的寄宿者延迟离开组件。 
    /// </summary>
    /// <param name="lodgerArrivalSignal">寄宿者到达的信号。</param>
    /// <param name="inSignalDisable">禁用离开的信号，可为空。</param>
    /// <param name="inSignalRemovePawn">移除角色的信号。</param>
    protected void DefaultDelayLeaveComp(string lodgerArrivalSignal, string inSignalDisable, string inSignalRemovePawn)
    {
        Quest quest = QuestGen.quest;

        quest.Delay(questParameter.questDurationTicks,
            delegate
            {
                quest.SignalPassWithFaction(questParameter.faction, action: null,
                    delegate
                    {
                        quest.Letter(letterDef: LetterDefOf.PositiveEvent, text: "[lodgersLeavingLetterText]", label: "[lodgersLeavingLetterLabel]");
                    });
                quest.Leave(questParameter.pawns, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn: inSignalRemovePawn, wakeUp: true);
            },
            inSignalEnable: lodgerArrivalSignal,
            inSignalDisable: inSignalDisable,
            expiryInfoPart: "GuestsDepartsIn".Translate(),
            expiryInfoPartTip: "GuestsDepartsOn".Translate(),
            debugLabel: "GuestsDepartsDelay");
    }

    /// <summary>
    /// 设置任务结束时的各类信件。 
    /// </summary>
    /// <param name="questPart_RefugeeInteractions">难民交互任务组件。</param>
    protected void SetQuestEndLetters(QuestPart_OARefugeeInteractions questPart_RefugeeInteractions)
    {
        Quest quest = QuestGen.quest;

        if (questParameter.allowAssaultColony)
        {
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony, text: "[lodgerDiedAttackPlayerLetterText]", label: "[lodgerDiedAttackPlayerLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalArrested_AssaultColony, text: "[lodgerArrestedAttackPlayerLetterText]", label: "[lodgerArrestedAttackPlayerLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony, text: "[lodgerViolatedAttackPlayerLetterText]", label: "[lodgerViolatedAttackPlayerLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalPsychicRitualTarget_AssaultColony, text: "[lodgerPsychicRitualTargetAttackPlayerLetterText]", label: "[lodgerPsychicRitualTargetAttackPlayerLetterLabel]");
        }

        if (questParameter.allowLeave)
        {
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony, text: "[lodgerDiedLeaveMapLetterText]", label: "[lodgerDiedLeaveMapLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalArrested_LeaveColony, text: "[lodgerArrestedLeaveMapLetterText]", label: "[lodgerArrestedLeaveMapLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony, text: "[lodgerViolatedLeaveMapLetterText]", label: "[lodgerViolatedLeaveMapLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalPsychicRitualTarget_LeaveColony, text: "[lodgerPsychicRitualTargetLeaveMapLetterText]", label: "[lodgerPsychicRitualTargetLeaveMapLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalLeftBehind_LeaveColony, text: "[lodgerLeftBehindLeaveMapLetterText]", label: "[lodgerLeftBehindLeaveMapLetterLabel]");
        }

        if (questParameter.allowBadThought)
        {
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalDestroyed_BadThought, text: "[lodgerDiedMemoryThoughtLetterText]", label: "[lodgerDiedMemoryThoughtLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalArrested_BadThought, text: "[lodgerArrestedMemoryThoughtLetterText]", label: "[lodgerArrestedMemoryThoughtLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought, text: "[lodgerViolatedMemoryThoughtLetterText]", label: "[lodgerViolatedMemoryThoughtLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalPsychicRitualTarget_BadThought, text: "[lodgerPsychicRitualTargetMemoryThoughtLetterText]", label: "[lodgerPsychicRitualTargetMemoryThoughtLetterLabel]");
            quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalLeftBehind_BadThought, text: "[lodgerLeftBehindMemoryThoughtLetterText]", label: "[lodgerLeftBehindMemoryThoughtLetterLabel]");
        }

        quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalLast_Destroyed, text: "[lodgersAllDiedLetterText]", label: "[lodgersAllDiedLetterLabel]");
        quest.Letter(letterDef: LetterDefOf.NegativeEvent, inSignal: questPart_RefugeeInteractions.outSignalLast_Arrested, text: "[lodgersAllArrestedLetterText]", label: "[lodgersAllArrestedLetterLabel]");
    }

    /// <summary>
    /// 设置任务结束组件的通用逻辑，包括失败、迟延失败与成功信号。 
    /// </summary>
    /// <param name="questPart_RefugeeInteractions">难民交互任务组件。</param>
    protected void SetQuestEndCompCommon(QuestPart_OARefugeeInteractions questPart_RefugeeInteractions)
    {
        Quest quest = QuestGen.quest;
        Faction faction = questParameter.faction;

        SetQuestEndLetters(questPart_RefugeeInteractions);

        string failSignal = QuestGenUtility.HardcodedSignalWithQuestID("RefugeeQuest_Fail");
        string delayFailSignal = QuestGenUtility.HardcodedSignalWithQuestID("RefugeeQuest_DelayFail");
        string successSignal = QuestGenUtility.HardcodedSignalWithQuestID("RefugeeQuest_Success");

        quest.AnySignal(inSignals:
        [
            questPart_RefugeeInteractions.outSignalLast_Destroyed,
            questPart_RefugeeInteractions.outSignalLast_Arrested,
            questPart_RefugeeInteractions.outSignalLast_LeftBehind,
            QuestGenUtility.HardcodedSignalWithQuestID("faction.BecameHostileToPlayer")
        ], outSignals: [failSignal]);

        List<string> delayFailureSignals =
        [
            questPart_RefugeeInteractions.outSignalLast_Kidnapped,
            questPart_RefugeeInteractions.outSignalLast_Banished
        ];
        if (questParameter.allowAssaultColony)
        {
            delayFailureSignals.AddRange(
            [
                questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony,
                questPart_RefugeeInteractions.outSignalArrested_AssaultColony,
                questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony,
                questPart_RefugeeInteractions.outSignalPsychicRitualTarget_AssaultColony
            ]);
        }
        if (questParameter.allowLeave)
        {
            delayFailureSignals.AddRange(
            [
                questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony,
                questPart_RefugeeInteractions.outSignalArrested_LeaveColony,
                questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony,
                questPart_RefugeeInteractions.outSignalPsychicRitualTarget_LeaveColony,
                questPart_RefugeeInteractions.outSignalLeftBehind_LeaveColony
            ]);
        }
        quest.AnySignal(inSignals: delayFailureSignals, outSignals: [delayFailSignal]);
        quest.AnySignal(inSignals:
        [
            questPart_RefugeeInteractions.outSignalLast_Recruited,
            questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy,
            questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy
        ], outSignals: [successSignal]);

        SetQuestEndComp(questPart_RefugeeInteractions, failSignal, delayFailSignal, successSignal);

        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, failSignal, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, delayFailSignal, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Success, inSignal: questPart_RefugeeInteractions.outSignalLast_Recruited, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess / 2, faction, inSignal: questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, sendStandardLetter: true);

        quest.SignalPass(delegate
        {
            if (questParameter.allowFutureReward && ModsConfig.RoyaltyActive)
            {
                FloatRange marketValueRange = questParameter.rewardValueRange * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, questParameter.pawns, marketValueRange);
            }
            quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess, faction, sendStandardLetter: true);
        }, inSignal: questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy);
    }

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate) => QuestGen_Get.GetMap() is not null;
}