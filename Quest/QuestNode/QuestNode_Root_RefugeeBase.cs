﻿using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    protected sealed class QuestParameter
    {
        public bool allowAssaultColony = true;
        public bool allowLeave = true;
        public bool allowBadThought = true;

        private int lodgerCount = 1;
        private int childCount = 0;
        public int LodgerCount { get { return lodgerCount; } set { lodgerCount = Mathf.Max(1, value); } }
        public int ChildCount { get { return childCount; } set { childCount = Mathf.Clamp(value, 0, lodgerCount); } }

        public int questDurationTicks = 60000;
        public int arrivalDelayTicks = -1;

        public int goodwillSuccess;
        public int goodwillFailure;
        public FloatRange rewardValueRange;

        public readonly Quest quest;
        public readonly Slate slate;
        public Map map;
        public Faction faction;
        public PawnKindDef fixedPawnKind;
        public ThoughtDef addMemory;

        public QuestParameter(Faction faction, Map map)
        {
            this.map = map;
            this.faction = faction;

            fixedPawnKind = PawnKindDefOf.Refugee;
            quest = QuestGen.quest;
            slate = QuestGen.slate;
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
        if (pawns.NullOrEmpty())
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: true, playSound: false);
            return;
        }

        quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);

        string lodgerArrivalSignal = null;
        if (questParameter.arrivalDelayTicks > 0)
        {
            lodgerArrivalSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrival");
            quest.Delay(questParameter.arrivalDelayTicks, delegate
            {
                quest.PawnsArrive(pawns,
                                  inSignal: null,
                                  questParameter.map.Parent,
                                  arrivalMode: null,
                                  joinPlayer: true,
                                  walkInSpot: null,
                                  customLetterLabel: "[lodgersArriveLetterLabel]",
                                  customLetterText: "[lodgersArriveLetterText]");
            },
            inSignalEnable: null,
            inSignalDisable: null,
            outSignalComplete: lodgerArrivalSignal);
        }
        else
        {
            quest.PawnsArrive(pawns,
                              inSignal: null,
                              questParameter.map.Parent,
                              arrivalMode: null,
                              joinPlayer: true,
                              walkInSpot: null,
                              customLetterLabel: "[lodgersArriveLetterLabel]",
                              customLetterText: "[lodgersArriveLetterText]");

        }

        SetQuestAward(questParameter);

        QuestPart_OARefugeeInteractions questPart_RefugeeInteractions = new()
        {
            inSignalEnable = questParameter.slate.Get<string>("inSignal"),
            faction = faction,
            mapParent = questParameter.map.Parent,
            inSignalArrested = lodgerArrestedSignal,
            inSignalRecruited = lodgerRecruitedSignal,
            signalListenMode = QuestPart.SignalListenMode.Always
        };
        questPart_RefugeeInteractions.InitWithDefaultSingals(questParameter.allowAssaultColony, questParameter.allowLeave, questParameter.allowBadThought);
        questPart_RefugeeInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_RefugeeInteractions);

        SetQuestEndLetters(questParameter, questPart_RefugeeInteractions);

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony);

        SetQuestEndCompCommon(questParameter, questPart_RefugeeInteractions, pawns);
        SetPawnsLeaveComp(questParameter, pawns, lodgerArrivalSignal, lodgerArrestedOrRecruited);
        SetSlateValue(questParameter, pawns);
    }

    protected virtual QuestParameter InitQuestParameter(Faction faction)
    {
        return new QuestParameter(faction, QuestGen_Get.GetMap());
    }

    protected List<Pawn> GeneratePawns(QuestParameter questParameter, string lodgerRecruitedSignal = null)
    {
        Quest quest = questParameter.quest;
        List<Pawn> pawns = [];
        int adultCount = questParameter.LodgerCount - questParameter.ChildCount;
        ThoughtDef addMemory = questParameter.addMemory;
        PawnKindDef fixedPawnKind = questParameter.fixedPawnKind;
        for (int i = 0; i < questParameter.LodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = i < adultCount ? DevelopmentalStage.Adult : DevelopmentalStage.Child;
            Pawn pawn = quest.GeneratePawn(fixedPawnKind,
                                           questParameter.faction,
                                           allowAddictions: true,
                                           forcedTraits: null,
                                           biocodeWeaponChance: 0f,
                                           mustBeCapableOfViolence: true,
                                           extraPawnForExtraRelationChance: null,
                                           relationWithExtraPawnChanceFactor: 0f,
                                           biocodeApparelChance: 0f,
                                           ensureNonNumericName: false,
                                           forceGenerateNewPawn: true,
                                           developmentalStages: developmentalStages,
                                           allowPregnant: false);


            pawns.Add(pawn);

            PostPawnGenerated(pawn);
            if (addMemory is not null)
            {
                QuestPart_AddMemoryThought questPart_AddMemoryThought = new()
                {
                    inSignal = questParameter.slate.Get<string>("inSignal"),
                    pawn = pawn,
                };
                quest.AddPart(questPart_AddMemoryThought);
            }

            quest.PawnJoinOffer(pawn,
                "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")),
                "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")),
                "LetterJoinOfferText".Translate(pawn.Named("PAWN"),
                questParameter.map.Parent.Named("MAP")),
                delegate
                {
                    quest.JoinPlayer(questParameter.map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                    quest.Letter(LetterDefOf.PositiveEvent,
                                 inSignal: null,
                                 chosenPawnSignal: null,
                                 relatedFaction: null,
                                 useColonistsOnMap: null,
                                 useColonistsFromCaravanArg: false,
                                 QuestPart.SignalListenMode.OngoingOnly,
                                 lookTargets: null,
                                 filterDeadPawnsFromLookTargets: false,
                                 label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap,
                                 text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                    quest.SignalPass(null, null, lodgerRecruitedSignal);
                },
                delegate
                {
                    quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
                },
                inSignal: null, outSignalPawnAccepted: null, outSignalPawnRejected: null,
                charity: true);
        }
        return pawns;
    }

    protected virtual void PostPawnGenerated(Pawn pawn) { }

    protected virtual void SetQuestAward(QuestParameter questParameter)
    {
        QuestPart_Choice questPart_Choice = questParameter.quest.RewardChoice();
        QuestPart_Choice.Choice choice = new()
        {
            rewards =
            {
                new Reward_VisitorsHelp(),
                new Reward_PossibleFutureReward()
            }
        };

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
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where(p => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
        Find.FactionManager.Add(faction);
        return faction;
    }

    protected virtual void SetQuestEndComp(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string bigFailSignal, string successSignal) { }

    protected virtual void SetPawnsLeaveComp(QuestParameter questParameter, List<Pawn> pawns, string inSignalEnable, string inSignalRemovePawn)
    {
        if (questParameter.questDurationTicks > 0)
        {
            DefaultDelayLeaveComp(questParameter, pawns, inSignalEnable, inSignalDisable: null, inSignalRemovePawn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void DefaultDelayLeaveComp(QuestParameter questParameter, List<Pawn> pawns, string inSignalEnable, string inSignalDisable, string inSignalRemovePawn)
    {
        Quest quest = questParameter.quest;

        quest.Delay(questParameter.questDurationTicks,
            delegate
            {
                quest.SignalPassWithFaction(questParameter.faction, action: null,
                    delegate
                    {
                        quest.Letter(LetterDefOf.PositiveEvent,
                                     inSignal: null,
                                     chosenPawnSignal: null,
                                     relatedFaction: null,
                                     useColonistsOnMap: null,
                                     useColonistsFromCaravanArg: false,
                                     QuestPart.SignalListenMode.OngoingOnly,
                                     lookTargets: null,
                                     filterDeadPawnsFromLookTargets: false,
                                     text: "[lodgersLeavingLetterText]",
                                     textRules: null,
                                     label: "[lodgersLeavingLetterLabel]");
                    });
                quest.Leave(pawns, inSignal: null, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);
            },
            inSignalEnable: inSignalEnable,
            inSignalDisable: inSignalDisable,
            outSignalComplete: null,
            reactivatable: false,
            inspectStringTargets: null,
            inspectString: null,
            isQuestTimeout: false,
            expiryInfoPart: "GuestsDepartsIn".Translate(),
            expiryInfoPartTip: "GuestsDepartsOn".Translate(),
            debugLabel: "QuestDelay");
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

    private void SetQuestEndCompCommon(QuestParameter questParameter, QuestPart_OARefugeeInteractions questPart_RefugeeInteractions, List<Pawn> pawns)
    {
        Quest quest = questParameter.quest;
        Faction faction = questParameter.faction;

        string failSignal = QuestGenUtility.HardcodedSignalWithQuestID("refugee.Fail");
        string bigFailSignal = QuestGenUtility.HardcodedSignalWithQuestID("refugee.BigFail");
        string successSignal = QuestGenUtility.HardcodedSignalWithQuestID("refugee.Success");

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

    protected virtual void SetSlateValue(QuestParameter questParameter, List<Pawn> pawns)
    {
        Slate slate = questParameter.slate;

        slate.Set("map", questParameter.map);
        slate.Set("faction", questParameter.faction);
        slate.Set("questDurationTicks", questParameter.questDurationTicks);
        slate.Set("lodgerCount", questParameter.LodgerCount);
        slate.Set("lodgersCountMinusOne", questParameter.LodgerCount - 1);
        slate.Set("lodgers", pawns);
        slate.Set("asker", pawns.First());
        if (questParameter.arrivalDelayTicks > 0)
        {
            slate.Set("arrivalDelayTicks", questParameter.arrivalDelayTicks);
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        return QuestGen_Get.GetMap() is not null;
    }


}
