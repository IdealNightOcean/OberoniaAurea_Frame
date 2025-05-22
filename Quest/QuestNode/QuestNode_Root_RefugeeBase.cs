using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_Root_RefugeeBase : QuestNode
{
    protected static readonly QuestGen_Pawns.GetPawnParms FactionOpponentPawnParams = new()
    {
        mustBeWorldPawn = true,
        mustBeFactionLeader = false,
        mustBeNonHostileToPlayer = true
    };

    protected struct QuestParameter
    {
        public bool allowAssaultColony = true;
        public bool allowLeave = true;
        public bool allowBadThought = true;

        private int lodgerCount = 1;
        private int childCount = 0;
        public int LodgerCount { readonly get { return lodgerCount; } set { lodgerCount = Mathf.Max(0, lodgerCount); } }
        public int ChildCount { readonly get { return childCount; } set { childCount = Mathf.Clamp(value, 0, lodgerCount); } }

        public int questDurationTicks = 60000;

        public int goodwillSuccess;
        public int goodwillFailure;
        public FloatRange rewardValueRange;

        public readonly Quest quest = QuestGen.quest;
        public readonly Slate slate = QuestGen.slate;
        public Map map;
        public Faction faction;

        public QuestParameter(Faction faction, Map map)
        {
            this.map = QuestGen_Get.GetMap();
            this.faction = faction;
        }
    }

    protected override void RunInt()
    {
        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            return;
        }

        QuestParameter questParameter = InitQuestParameter(faction);
        Quest quest = questParameter.quest;

        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");

        quest.AnySignal(inSignals: [lodgerRecruitedSignal, lodgerArrestedSignal], outSignals: [lodgerArrestedOrRecruited]);

        List<Pawn> pawns = GeneratePawns(questParameter, lodgerRecruitedSignal);
        Pawn asker = pawns.First();

        QuestPart_ExtraFaction questPart_ExtraFaction = quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);
        quest.PawnsArrive(pawns, null, questParameter.map.Parent, null, joinPlayer: true, null, "[lodgersArriveLetterLabel]", "[lodgersArriveLetterText]");

        SetQuestAward(questParameter);

        QuestPart_OARefugeeInteractions questPart_RefugeeInteractions = new()
        {
            inSignalEnable = questParameter.slate.Get<string>("inSignal"),
            faction = faction,
            mapParent = questParameter.map.Parent,
            signalListenMode = QuestPart.SignalListenMode.Always
        };
        questPart_RefugeeInteractions.InitWithDefaultSingals(questParameter.allowAssaultColony, questParameter.allowLeave, questParameter.allowBadThought); ;
        questPart_RefugeeInteractions.inSignalArrested = lodgerArrestedSignal;
        questPart_RefugeeInteractions.inSignalRecruited = lodgerRecruitedSignal;
        questPart_RefugeeInteractions.pawns.AddRange(pawns);

        SetQuestEndLetters(questParameter, questPart_RefugeeInteractions);

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony);

        SetQuestEndComp(questParameter, questPart_RefugeeInteractions, pawns);

        SetPawnsLeaveComp(questParameter, pawns, lodgerArrestedOrRecruited);
    }

    protected virtual QuestParameter InitQuestParameter(Faction faction)
    {
        return new QuestParameter(faction, QuestGen_Get.GetMap());
    }

    protected virtual List<Pawn> GeneratePawns(QuestParameter questParameter, string lodgerRecruitedSignal = null)
    {
        Quest quest = questParameter.quest;
        List<Pawn> pawns = [];
        for (int i = 0; i < questParameter.LodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = i < questParameter.ChildCount ? DevelopmentalStage.Child : DevelopmentalStage.Adult;
            Pawn pawn = quest.GeneratePawn(PawnKindDefOf.Refugee, questParameter.faction, allowAddictions: true, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, developmentalStages, allowPregnant: true);
            pawns.Add(pawn);
            quest.PawnJoinOffer(pawn,
                "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")),
                "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")),
                "LetterJoinOfferText".Translate(pawn.Named("PAWN"),
                questParameter.map.Parent.Named("MAP")),
                delegate
                {
                    quest.JoinPlayer(questParameter.map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                    quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                    quest.SignalPass(null, null, lodgerRecruitedSignal);
                },
                delegate
                {
                    quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
                },
                null, null, null, charity: true);
        }
        return pawns;
    }

    protected virtual void SetQuestAward(QuestParameter questParameter)
    {
        QuestPart_Choice questPart_Choice = questParameter.quest.RewardChoice();
        QuestPart_Choice.Choice choice = new()
        {
            rewards =
            {
                new Reward_VisitorsHelp(),
                new Reward_PossibleFutureReward(),
                new Reward_Goodwill()
                {
                    amount = questParameter.goodwillSuccess,
                    faction = questParameter.faction
                },
            }
        };
        if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.FluidIdeo != null)
        {
            choice.rewards.Add(new Reward_DevelopmentPoints(questParameter.quest));
        }
        questPart_Choice.choices.Add(choice);
    }

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
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef p) => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
        Find.FactionManager.Add(faction);
        return faction;
    }

    protected virtual void SetQuestEndComp(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string bigFailSignal, string successSignal) { }

    protected virtual void SetPawnsLeaveComp(QuestParameter questParameter, List<Pawn> pawns, string inSignalRemovePawn)
    {
        Quest quest = questParameter.quest;

        quest.Delay(questParameter.questDurationTicks, delegate
        {
            quest.SignalPassWithFaction(questParameter.faction, null, delegate
            {
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingLetterText]", null, "[lodgersLeavingLetterLabel]");
            });
            quest.Leave(pawns, null, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);
        }, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, "GuestsDepartsIn".Translate(), "GuestsDepartsOn".Translate(), "QuestDelay");

    }

    private void SetQuestEndLetters(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_RefugeeInteractions)
    {
        Quest quest = questParameter.quest;

        if (questParameter.allowAssaultColony)
        {
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedAttackPlayerLetterText]", null, "[lodgerDiedAttackPlayerLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedAttackPlayerLetterText]", null, "[lodgerArrestedAttackPlayerLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedAttackPlayerLetterText]", null, "[lodgerViolatedAttackPlayerLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalPsychicRitualTarget_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerPsychicRitualTargetAttackPlayerLetterText]", null, "[lodgerPsychicRitualTargetAttackPlayerLetterLabel]");
        }

        if (questParameter.allowLeave)
        {
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedLeaveMapLetterText]", null, "[lodgerDiedLeaveMapLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedLeaveMapLetterText]", null, "[lodgerArrestedLeaveMapLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedLeaveMapLetterText]", null, "[lodgerViolatedLeaveMapLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalPsychicRitualTarget_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerPsychicRitualTargetLeaveMapLetterText]", null, "[lodgerPsychicRitualTargetLeaveMapLetterLabel]");
        }

        if (questParameter.allowBadThought)
        {
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedMemoryThoughtLetterText]", null, "[lodgerDiedMemoryThoughtLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedMemoryThoughtLetterText]", null, "[lodgerArrestedMemoryThoughtLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedMemoryThoughtLetterText]", null, "[lodgerViolatedMemoryThoughtLetterLabel]");
            quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalPsychicRitualTarget_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerPsychicRitualTargetMemoryThoughtLetterText]", null, "[lodgerPsychicRitualTargetMemoryThoughtLetterLabel]");
        }

        quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalLast_Destroyed, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllDiedLetterText]", null, "[lodgersAllDiedLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalLast_Arrested, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllArrestedLetterText]", null, "[lodgersAllArrestedLetterLabel]");
    }

    private void SetQuestEndComp(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_RefugeeInteractions, List<Pawn> pawns)
    {
        Quest quest = questParameter.quest;
        Faction faction = questParameter.faction;



        string failSignal = QuestGenUtility.HardcodedSignalWithQuestID("childCare.Fail");
        string bigFailSignal = QuestGenUtility.HardcodedSignalWithQuestID("childCare.BigFail");
        string successSignal = QuestGenUtility.HardcodedSignalWithQuestID("childCare.Success");

        quest.AnySignal(inSignals: [questPart_RefugeeInteractions.outSignalLast_Destroyed, questPart_RefugeeInteractions.outSignalLast_Arrested], outSignals: [failSignal]);

        List<string> bigFailureSignals = [questPart_RefugeeInteractions.outSignalLast_Kidnapped, questPart_RefugeeInteractions.outSignalLast_Banished];
        if (questParameter.allowAssaultColony)
        {
            bigFailureSignals.AddRange([questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony,
                                        questPart_RefugeeInteractions.outSignalArrested_AssaultColony,
                                        questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony,
                                        questPart_RefugeeInteractions.outSignalPsychicRitualTarget_AssaultColony]);
        }
        if (questParameter.allowLeave)
        {
            bigFailureSignals.AddRange([questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony,
                                        questPart_RefugeeInteractions.outSignalArrested_LeaveColony,
                                        questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony,
                                        questPart_RefugeeInteractions.outSignalPsychicRitualTarget_LeaveColony]);
        }
        quest.AnySignal(inSignals: bigFailureSignals, outSignals: [bigFailSignal]);
        quest.AnySignal(inSignals: [questPart_RefugeeInteractions.outSignalLast_Recruited, questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy], outSignals: [successSignal]);

        SetQuestEndComp(questParameter, questPart_RefugeeInteractions, failSignal, bigFailSignal, successSignal);

        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, failSignal);
        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, bigFailSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Success, 0, null, questPart_RefugeeInteractions.outSignalLast_Recruited, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess / 2, faction, questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.SignalPass(delegate
        {
            if (ModsConfig.RoyaltyActive)
            {
                FloatRange marketValueRange = questParameter.rewardValueRange * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, pawns, marketValueRange);
            }
            quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess, faction, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        }, inSignal: questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return QuestGen_Get.GetMap() is not null;
    }


}
