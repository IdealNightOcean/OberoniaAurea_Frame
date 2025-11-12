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
    protected const string IsMainFactionSlate = "isMainFaction";
    protected const string UniqueQuestDescSlate = "uniqueQuestDesc";
    protected const string UniqueLeavingLetterSlate = "uniqueLeavingLetter";

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

        public List<Pawn> pawns;

        public QuestParameter()
        {
            map = QuestGen.slate.Get<Map>("map") ?? QuestGen_Get.GetMap();
        }

        public QuestParameter(Map map)
        {
            this.map = map;
        }
    }

    protected QuestParameter questParameter;

    protected virtual PawnKindDef FixedPawnKind => PawnKindDefOf.Refugee;
    protected virtual ThoughtDef ThoughtToAdd => null;

    protected override void RunInt()
    {
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;

        if (!InitQuestParameter())
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: false, playSound: false);
            return;
        }

        slate.Set("allowFutureReward", questParameter.allowFutureReward);
        slate.Set("allowJoinOffer", questParameter.allowJoinOffer);

        Faction faction = GetOrGenerateFaction();
        if (faction is null || faction.HostileTo(Faction.OfPlayer))
        {
            quest.End(QuestEndOutcome.Unknown, sendLetter: false, playSound: false);
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
            quest.End(QuestEndOutcome.Unknown, sendLetter: false, playSound: false);
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

    protected virtual bool InitQuestParameter()
    {
        questParameter = new QuestParameter();
        return true;
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

    protected virtual void SetQuestEndComp(QuestPart_OARefugeeInteractions questPart_Interactions, string failSignal, string delayFailSignal, string successSignal) { }

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
                        quest.Letter(letterDef: LetterDefOf.PositiveEvent, text: "[lodgersLeavingLetterText]", label: "[lodgersLeavingLetterLabel]");
                    });
                quest.Leave(questParameter.pawns, sendStandardLetter: false, leaveOnCleanup: false, inSignalRemovePawn: inSignalRemovePawn, wakeUp: true);
            },
            inSignalEnable: lodgerArrivalSignal,
            inSignalDisable: inSignalDisable,
            expiryInfoPart: "GuestsDepartsIn".Translate(),
            expiryInfoPartTip: "GuestsDepartsOn".Translate(),
            debugLabel: "QuestDelay");
    }

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
        quest.End(QuestEndOutcome.Success, questParameter.goodwillSuccess / 2, faction, questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, sendStandardLetter: true);

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

    protected override bool TestRunInt(Slate slate) => QuestGen_Get.GetMap() is not null;
}