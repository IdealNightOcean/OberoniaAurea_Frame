using RimWorld;
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
        public bool allowJoinOffer = true;
        public bool allowFutureReward = true;

        private int lodgerCount = 1;
        private int childCount = 0;
        public int LodgerCount { get { return lodgerCount; } set { lodgerCount = Mathf.Max(1, value); } }
        public int ChildCount { get { return childCount; } set { childCount = Mathf.Clamp(value, 0, lodgerCount); } }

        public int questDurationTicks = 60000;
        public int arrivalDelayTicks = -1;

        public int goodwillSuccess;
        public int goodwillFailure;
        public FloatRange rewardValueRange;

        public Map map;
        public Faction faction;
        public PawnKindDef fixedPawnKind;
        public ThoughtDef addMemory;
        public List<Pawn> pawns;

        public QuestParameter()
        {
            map = QuestGen_Get.GetMap();
            fixedPawnKind = PawnKindDefOf.Refugee;
        }

        public QuestParameter(Map map)
        {
            this.map = map;
            fixedPawnKind = PawnKindDefOf.Refugee;
        }
    }

    protected QuestParameter questParameter;

    protected override void RunInt()
    {
        InitQuestParameter();
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;

        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: true, playSound: false);
            return;
        }
        questParameter.faction = faction;
        slate.Set("map", questParameter.map);
        slate.Set("faction", faction);
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
            quest.End(QuestEndOutcome.Unknown, sendLetter: true, playSound: false);
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

        SetQuestEndLetters(questPart_RefugeeInteractions);

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony);

        SetQuestEndCompCommon(questPart_RefugeeInteractions);
        SetPawnsLeaveComp(lodgerArrivalSignal, lodgerArrestedOrRecruited);
    }

    protected virtual void InitQuestParameter()
    {
        questParameter = new QuestParameter();
    }

    protected virtual void ClearQuestParameter()
    {
        questParameter = null;
    }

    protected virtual List<Pawn> GeneratePawns(string lodgerRecruitedSignal = null)
    {
        Quest quest = QuestGen.quest;
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
                    inSignal = QuestGen.slate.Get<string>("inSignal"),
                    pawn = pawn,
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
        }
        return pawns;
    }

    protected virtual void PostPawnGenerated(Pawn pawn) { }

    protected virtual void PawnArrival(string lodgerArrivalSignal)
    {
        Quest quest = QuestGen.quest;

        if (questParameter.arrivalDelayTicks > 0)
        {
            quest.Delay(questParameter.arrivalDelayTicks, delegate
            {
                quest.PawnsArrive(questParameter.pawns,
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
            quest.PawnsArrive(questParameter.pawns,
                              inSignal: null,
                              questParameter.map.Parent,
                              arrivalMode: null,
                              joinPlayer: true,
                              walkInSpot: null,
                              customLetterLabel: "[lodgersArriveLetterLabel]",
                              customLetterText: "[lodgersArriveLetterText]");
            quest.SendSignals(outSignals: [lodgerArrivalSignal]);
        }
    }

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

    protected virtual void AddQuestAward(QuestPart_Choice.Choice choice) { }

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

    protected virtual void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string bigFailSignal, string successSignal) { }

    protected virtual void SetPawnsLeaveComp(string lodgerArrivalSignal, string inSignalRemovePawn)
    {
        if (questParameter.questDurationTicks > 0)
        {
            DefaultDelayLeaveComp(lodgerArrivalSignal, inSignalDisable: null, inSignalRemovePawn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void DefaultDelayLeaveComp(string lodgerArrivalSignal, string inSignalDisable, string inSignalRemovePawn)
    {
        Quest quest = QuestGen.quest;

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
                quest.Leave(questParameter.pawns, inSignal: null, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn, wakeUp: true);
            },
            inSignalEnable: lodgerArrivalSignal,
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

    protected void SetQuestEndLetters(QuestPart_OARefugeeInteractions questPart_RefugeeInteractions)
    {
        Quest quest = QuestGen.quest;

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

    protected void SetQuestEndCompCommon(QuestPart_OARefugeeInteractions questPart_RefugeeInteractions)
    {
        Quest quest = QuestGen.quest;
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

        SetQuestEndComp(questPart_RefugeeInteractions, failSignal, bigFailSignal, successSignal);

        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, failSignal);
        quest.End(QuestEndOutcome.Fail, questParameter.goodwillFailure, faction, bigFailSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Success, 0, null, questPart_RefugeeInteractions.outSignalLast_Recruited, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess / 2, faction, questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.SignalPass(delegate
        {
            if (questParameter.allowFutureReward && ModsConfig.RoyaltyActive)
            {
                FloatRange marketValueRange = questParameter.rewardValueRange * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, questParameter.pawns, marketValueRange);
            }
            quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess, faction, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        }, inSignal: questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy);
    }

    protected override bool TestRunInt(Slate slate) => QuestGen_Get.GetMap() is not null;
}