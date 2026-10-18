using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务部件：难民交互处理。 
/// </summary>
public class QuestPart_OARefugeeInteractions : QuestPartActivable
{
    private enum InteractionResponseType
    {
        AssaultColony,
        Leave,
        BadThought
    }

    /// <summary>
    /// 是否允许袭击殖民地。
    /// </summary>
    public bool allowAssaultColony = true;
    /// <summary>
    /// 是否允许离开。
    /// </summary>
    public bool allowLeave = true;
    /// <summary>
    /// 是否允许产生不良想法。
    /// </summary>
    public bool allowBadThought = true;

    /// <summary>
    /// 被招募的输入信号。
    /// </summary>
    public string inSignalRecruited;
    /// <summary>
    /// 离开地图的输入信号。
    /// </summary>
    public string inSignalLeftMap;

    /// <summary>
    /// 被摧毁的输入信号。
    /// </summary>
    public string inSignalDestroyed;
    /// <summary>
    /// 被逮捕的输入信号。
    /// </summary>
    public string inSignalArrested;
    /// <summary>
    /// 手术违规的输入信号。
    /// </summary>
    public string inSignalSurgeryViolation;
    /// <summary>
    /// 作为灵能仪式目标的输入信号。
    /// </summary>
    public string inSignalPsychicRitualTarget;
    /// <summary>
    /// 被绑架的输入信号。
    /// </summary>
    public string inSignalKidnapped;
    /// <summary>
    /// 袭击殖民地的输入信号。
    /// </summary>
    public string inSignalAssaultColony;
    /// <summary>
    /// 被遗弃的输入信号。
    /// </summary>
    public string inSignalLeftBehind;
    /// <summary>
    /// 被放逐的输入信号。
    /// </summary>
    public string inSignalBanished;

    /// <summary>
    /// 被摧毁且袭击殖民地的输出信号。
    /// </summary>
    public string outSignalDestroyed_AssaultColony;
    /// <summary>
    /// 被摧毁且离开殖民地的输出信号。
    /// </summary>
    public string outSignalDestroyed_LeaveColony;
    /// <summary>
    /// 被摧毁且产生不良想法的输出信号。
    /// </summary>
    public string outSignalDestroyed_BadThought;

    /// <summary>
    /// 被逮捕且袭击殖民地的输出信号。
    /// </summary>
    public string outSignalArrested_AssaultColony;
    /// <summary>
    /// 被逮捕且离开殖民地的输出信号。
    /// </summary>
    public string outSignalArrested_LeaveColony;
    /// <summary>
    /// 被逮捕且产生不良想法的输出信号。
    /// </summary>
    public string outSignalArrested_BadThought;

    /// <summary>
    /// 被遗弃且离开殖民地的输出信号。
    /// </summary>
    public string outSignalLeftBehind_LeaveColony;
    /// <summary>
    /// 被遗弃且产生不良想法的输出信号。
    /// </summary>
    public string outSignalLeftBehind_BadThought;

    /// <summary>
    /// 手术违规且袭击殖民地的输出信号。
    /// </summary>
    public string outSignalSurgeryViolation_AssaultColony;
    /// <summary>
    /// 手术违规且离开殖民地的输出信号。
    /// </summary>
    public string outSignalSurgeryViolation_LeaveColony;
    /// <summary>
    /// 手术违规且产生不良想法的输出信号。
    /// </summary>
    public string outSignalSurgeryViolation_BadThought;

    /// <summary>
    /// 作为灵能仪式目标且袭击殖民地的输出信号。
    /// </summary>
    public string outSignalPsychicRitualTarget_AssaultColony;
    /// <summary>
    /// 作为灵能仪式目标且离开殖民地的输出信号。
    /// </summary>
    public string outSignalPsychicRitualTarget_LeaveColony;
    /// <summary>
    /// 作为灵能仪式目标且产生不良想法的输出信号。
    /// </summary>
    public string outSignalPsychicRitualTarget_BadThought;

    /// <summary>
    /// 最后一名被逮捕的输出信号。
    /// </summary>
    public string outSignalLast_Arrested;
    /// <summary>
    /// 最后一名被摧毁的输出信号。
    /// </summary>
    public string outSignalLast_Destroyed;
    /// <summary>
    /// 最后一名被绑架的输出信号。
    /// </summary>
    public string outSignalLast_Kidnapped;
    /// <summary>
    /// 最后一名被招募的输出信号。
    /// </summary>
    public string outSignalLast_Recruited;
    /// <summary>
    /// 最后一名被放逐的输出信号。
    /// </summary>
    public string outSignalLast_Banished;
    /// <summary>
    /// 最后一名被遗弃的输出信号。
    /// </summary>
    public string outSignalLast_LeftBehind;

    /// <summary>
    /// 最后一名离开地图且全部健康的输出信号。
    /// </summary>
    public string outSignalLast_LeftMapAllHealthy;
    /// <summary>
    /// 最后一名离开地图且存在不健康的输出信号。
    /// </summary>
    public string outSignalLast_LeftMapAllNotHealthy;

    /// <summary>
    /// 参与交互的难民列表。
    /// </summary>
    public List<Pawn> pawns = [];
    [Unsaved] private List<InteractionResponseType> availableInteractions;

    /// <summary>
    /// 难民所属的派系。
    /// </summary>
    public Faction faction;

    /// <summary>
    /// 难民所在的地图父对象。
    /// </summary>
    public MapParent mapParent;

    /// <summary>
    /// 离开地图时不健康的难民数量。
    /// </summary>
    public int pawnsLeftUnhealthy;

    /// <summary>
    /// 使用默认信号初始化交互设置
    /// </summary>
    /// <param name="allowAssaultColony">是否允许袭击殖民地。</param>
    /// <param name="allowLeave">是否允许离开。</param>
    /// <param name="allowBadThought">是否允许产生不良想法。</param>
    public void InitWithDefaultSingals(bool allowAssaultColony, bool allowLeave, bool allowBadThought)
    {
        this.allowAssaultColony = allowAssaultColony;
        this.allowLeave = allowLeave;
        this.allowBadThought = allowBadThought;
        if (!allowAssaultColony && !allowLeave && !allowBadThought)
        {
            allowLeave = true;
            Log.Error("No interaction available - enabling allowLeave as fallback");
        }
        InitAvailableInteractions();

        inSignalAssaultColony = QuestGen.GenerateNewSignal("AssaultColony");

        inSignalDestroyed = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed");
        inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.SurgeryViolation");
        inSignalPsychicRitualTarget = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.PsychicRitualTarget");
        inSignalKidnapped = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped");
        inSignalLeftMap = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap");
        inSignalLeftBehind = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftBehind");
        inSignalBanished = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished");

        if (allowAssaultColony)
        {
            outSignalDestroyed_AssaultColony = QuestGen.GenerateNewSignal("LodgerDestroyed_AssaultColony");
            outSignalArrested_AssaultColony = QuestGen.GenerateNewSignal("LodgerArrested_AssaultColony");
            outSignalSurgeryViolation_AssaultColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_AssaultColony");
            outSignalPsychicRitualTarget_AssaultColony = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_AssaultColony");
        }

        if (allowLeave)
        {
            outSignalDestroyed_LeaveColony = QuestGen.GenerateNewSignal("LodgerDestroyed_LeaveColony");
            outSignalArrested_LeaveColony = QuestGen.GenerateNewSignal("LodgerArrested_LeaveColony");
            outSignalSurgeryViolation_LeaveColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_LeaveColony");
            outSignalLeftBehind_LeaveColony = QuestGen.GenerateNewSignal("LodgerLeftBehind_LeaveColony");
            outSignalPsychicRitualTarget_LeaveColony = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_LeaveColony");
        }

        if (allowBadThought)
        {
            outSignalDestroyed_BadThought = QuestGen.GenerateNewSignal("LodgerDestroyed_BadThought");
            outSignalArrested_BadThought = QuestGen.GenerateNewSignal("LodgerArrested_BadThought");
            outSignalSurgeryViolation_BadThought = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_BadThought");
            outSignalLeftBehind_BadThought = QuestGen.GenerateNewSignal("LodgerLeftBehind_BadThought");
            outSignalPsychicRitualTarget_BadThought = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_BadThought");
        }

        outSignalLast_Destroyed = QuestGen.GenerateNewSignal("LastLodger_Destroyed");
        outSignalLast_Arrested = QuestGen.GenerateNewSignal("LastLodger_Arrested");
        outSignalLast_Kidnapped = QuestGen.GenerateNewSignal("LastLodger_Kidnapped");
        outSignalLast_Recruited = QuestGen.GenerateNewSignal("LastLodger_Recruited");
        outSignalLast_LeftMapAllHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllHealthy");
        outSignalLast_LeftMapAllNotHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllNotHealthy");
        outSignalLast_Banished = QuestGen.GenerateNewSignal("LastLodger_Banished");
        outSignalLast_LeftBehind = QuestGen.GenerateNewSignal("LastLodger_LeftBehind");
    }

    /// <inheritdoc/>
    protected override void ProcessQuestSignal(Signal signal)
    {
        if (inSignalAssaultColony is not null && signal.tag == inSignalAssaultColony)
        {
            AssaultColony(null);
        }

        if (!signal.args.TryGetArg("SUBJECT", out Pawn argPawn) || !pawns.Contains(argPawn))
        {
            return;
        }

        string signalTag = signal.tag;

        if (signalTag == inSignalRecruited)
        {
            pawns.Remove(argPawn);
            argPawn.apparel?.UnlockAll(); //解锁被招募人员的装备
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Recruited, signal.args));
            }
        }

        else if (signalTag == inSignalKidnapped)
        {
            pawns.Remove(argPawn);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Kidnapped, signal.args));
            }
        }

        else if (signalTag == inSignalBanished)
        {
            pawns.Remove(argPawn);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Banished, signal.args));
            }
        }

        else if (signalTag == inSignalLeftMap)
        {
            pawns.Remove(argPawn);
            if (argPawn.Destroyed || argPawn.InMentalState || argPawn.health.hediffSet.BleedRateTotal > 0.001f)
            {
                pawnsLeftUnhealthy++;
            }
            int num = pawns.Count(p => p.Downed);
            if (pawns.Count - num <= 0)
            {
                if (pawnsLeftUnhealthy > 0 || num > 0)
                {
                    pawns.Clear();
                    pawnsLeftUnhealthy += num;
                    Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllNotHealthy, signal.args));
                }
                else
                {
                    Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftMapAllHealthy, signal.args));
                }
            }
        }

        else if (signalTag == inSignalLeftBehind)
        {
            pawns.Remove(argPawn);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_LeftBehind, signal.args));
            }
            else
            {
                signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
                switch (ChooseRandomInteraction())
                {
                    case InteractionResponseType.AssaultColony:
                    case InteractionResponseType.Leave:
                        LeavePlayer();
                        Find.SignalManager.SendSignal(new Signal(outSignalLeftBehind_LeaveColony, signal.args));
                        break;
                    case InteractionResponseType.BadThought:
                        Find.SignalManager.SendSignal(new Signal(outSignalLeftBehind_BadThought, signal.args));
                        break;
                }
            }
        }

        else if (signalTag == inSignalDestroyed)
        {
            pawns.Remove(argPawn);
            argPawn.SetFaction(faction);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Destroyed, signal.args));
            }
            else
            {
                signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
                switch (ChooseRandomInteraction())
                {
                    case InteractionResponseType.AssaultColony:
                        AssaultColony(HistoryEventDefOf.QuestPawnLost);
                        Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_AssaultColony, signal.args));
                        break;
                    case InteractionResponseType.Leave:
                        LeavePlayer();
                        Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_LeaveColony, signal.args));
                        break;
                    case InteractionResponseType.BadThought:
                        Find.SignalManager.SendSignal(new Signal(outSignalDestroyed_BadThought, signal.args));
                        break;
                }
            }
        }

        else if (signalTag == inSignalArrested)
        {
            pawns.Remove(argPawn);
            bool inAggroMentalState = argPawn.InAggroMentalState;
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Arrested, signal.args));
            }
            else if (!inAggroMentalState)
            {
                signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
                switch (ChooseRandomInteraction())
                {
                    case InteractionResponseType.AssaultColony:
                        AssaultColony(HistoryEventDefOf.QuestPawnArrested);
                        Find.SignalManager.SendSignal(new Signal(outSignalArrested_AssaultColony, signal.args));
                        break;
                    case InteractionResponseType.Leave:
                        LeavePlayer();
                        Find.SignalManager.SendSignal(new Signal(outSignalArrested_LeaveColony, signal.args));
                        break;
                    case InteractionResponseType.BadThought:
                        Find.SignalManager.SendSignal(new Signal(outSignalArrested_BadThought, signal.args));
                        break;
                }
            }
        }

        else if (signalTag == inSignalSurgeryViolation)
        {
            signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
            switch (ChooseRandomInteraction())
            {
                case InteractionResponseType.AssaultColony:
                    AssaultColony(HistoryEventDefOf.PerformedHarmfulSurgery);
                    Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_AssaultColony, signal.args));
                    break;
                case InteractionResponseType.Leave:
                    LeavePlayer();
                    Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_LeaveColony, signal.args));
                    break;
                case InteractionResponseType.BadThought:
                    Find.SignalManager.SendSignal(new Signal(outSignalSurgeryViolation_BadThought, signal.args));
                    break;
            }
        }

        else if (signalTag == inSignalPsychicRitualTarget)
        {
            signal.args.Add(pawns.Count.Named("PAWNSALIVECOUNT"));
            switch (ChooseRandomInteraction())
            {
                case InteractionResponseType.AssaultColony:
                    AssaultColony(HistoryEventDefOf.WasPsychicRitualTarget);
                    Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_AssaultColony, signal.args));
                    break;
                case InteractionResponseType.Leave:
                    LeavePlayer();
                    Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_LeaveColony, signal.args));
                    break;
                case InteractionResponseType.BadThought:
                    Find.SignalManager.SendSignal(new Signal(outSignalPsychicRitualTarget_BadThought, signal.args));
                    break;
            }
        }
    }

    private void LeavePlayer()
    {
        for (int i = 0; i < pawns.Count; i++)
        {
            if (faction != pawns[i].Faction)
            {
                pawns[i].SetFaction(faction);
            }
        }
        LeaveQuestPartUtility.MakePawnsLeave(pawns, sendLetter: false, quest);
        Complete();
    }

    private void AssaultColony(HistoryEventDef reason)
    {
        if (faction.HasGoodwill)
        {
            Faction.OfPlayer.TryAffectGoodwillWith(faction, Faction.OfPlayer.GoodwillToMakeHostile(faction), canSendMessage: true, canSendHostilityLetter: false, reason);
        }
        else
        {
            faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, canSendHostilityLetter: false);
        }
        for (int i = 0; i < pawns.Count; i++)
        {
            pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedByQuest);
        }
        for (int j = 0; j < pawns.Count; j++)
        {
            pawns[j].SetFaction(faction);
            if (!pawns[j].Awake())
            {
                RestUtility.WakeUp(pawns[j]);
            }
        }
        Dictionary<Map, Lord> dictionary = [];
        for (int k = 0; k < pawns.Count; k++)
        {
            Pawn pawn = pawns[k];
            Map mapHeld = pawn.MapHeld;
            if (!pawn.Dead)
            {
                if (!dictionary.TryGetValue(mapHeld, out Lord mapLord))
                {
                    mapLord = dictionary[mapHeld] = LordMaker.MakeNewLord(faction,
                                                                          new LordJob_AssaultColony(faction,
                                                                                                    canKidnap: true,
                                                                                                    canTimeoutOrFlee: true,
                                                                                                    sappers: false,
                                                                                                    useAvoidGridSmart: false,
                                                                                                    canSteal: true,
                                                                                                    breachers: false,
                                                                                                    canPickUpOpportunisticWeapons: true),
                                                                          mapHeld);
                }
                mapLord.AddPawn(pawn);
            }
        }
        Complete();
    }

    private void InitAvailableInteractions()
    {
        availableInteractions = [];
        if (allowAssaultColony)
        {
            availableInteractions.Add(InteractionResponseType.AssaultColony);
        }
        if (allowLeave)
        {
            availableInteractions.Add(InteractionResponseType.Leave);
        }
        if (allowBadThought)
        {
            availableInteractions.Add(InteractionResponseType.BadThought);
        }
        if (availableInteractions.NullOrEmpty())
        {
            availableInteractions.Add(InteractionResponseType.Leave);
        }
    }

    private InteractionResponseType ChooseRandomInteraction()
    {
        if (availableInteractions.NullOrEmpty())
        {
            InitAvailableInteractions();
        }
        return availableInteractions.RandomElementWithFallback(InteractionResponseType.Leave);
    }

    /// <inheritdoc/>
    public override void Notify_FactionRemoved(Faction f)
    {
        if (faction == f)
        {
            faction = null;
        }
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allowAssaultColony, nameof(allowAssaultColony), defaultValue: true);
        Scribe_Values.Look(ref allowLeave, nameof(allowLeave), defaultValue: true);
        Scribe_Values.Look(ref allowBadThought, nameof(allowBadThought), defaultValue: true);

        Scribe_Values.Look(ref inSignalRecruited, nameof(inSignalRecruited));
        Scribe_Values.Look(ref inSignalLeftMap, nameof(inSignalLeftMap));

        Scribe_Values.Look(ref inSignalAssaultColony, nameof(inSignalAssaultColony));

        Scribe_Values.Look(ref inSignalDestroyed, nameof(inSignalDestroyed));
        Scribe_Values.Look(ref inSignalArrested, nameof(inSignalArrested));
        Scribe_Values.Look(ref inSignalSurgeryViolation, nameof(inSignalSurgeryViolation));
        Scribe_Values.Look(ref inSignalPsychicRitualTarget, nameof(inSignalPsychicRitualTarget));
        Scribe_Values.Look(ref inSignalKidnapped, nameof(inSignalKidnapped));
        Scribe_Values.Look(ref inSignalLeftBehind, nameof(inSignalLeftBehind));
        Scribe_Values.Look(ref inSignalBanished, nameof(inSignalBanished));

        Scribe_Values.Look(ref outSignalDestroyed_AssaultColony, nameof(outSignalDestroyed_AssaultColony));
        Scribe_Values.Look(ref outSignalDestroyed_LeaveColony, nameof(outSignalDestroyed_LeaveColony));
        Scribe_Values.Look(ref outSignalDestroyed_BadThought, nameof(outSignalDestroyed_BadThought));

        Scribe_Values.Look(ref outSignalArrested_AssaultColony, nameof(outSignalArrested_AssaultColony));
        Scribe_Values.Look(ref outSignalArrested_LeaveColony, nameof(outSignalArrested_LeaveColony));
        Scribe_Values.Look(ref outSignalArrested_BadThought, nameof(outSignalArrested_BadThought));

        Scribe_Values.Look(ref outSignalLeftBehind_LeaveColony, nameof(outSignalLeftBehind_LeaveColony));
        Scribe_Values.Look(ref outSignalLeftBehind_BadThought, nameof(outSignalLeftBehind_BadThought));

        Scribe_Values.Look(ref outSignalSurgeryViolation_AssaultColony, nameof(outSignalSurgeryViolation_AssaultColony));
        Scribe_Values.Look(ref outSignalSurgeryViolation_LeaveColony, nameof(outSignalSurgeryViolation_LeaveColony));
        Scribe_Values.Look(ref outSignalSurgeryViolation_BadThought, nameof(outSignalSurgeryViolation_BadThought));

        Scribe_Values.Look(ref outSignalPsychicRitualTarget_AssaultColony, nameof(outSignalPsychicRitualTarget_AssaultColony));
        Scribe_Values.Look(ref outSignalPsychicRitualTarget_LeaveColony, nameof(outSignalPsychicRitualTarget_LeaveColony));
        Scribe_Values.Look(ref outSignalPsychicRitualTarget_BadThought, nameof(outSignalPsychicRitualTarget_BadThought));

        Scribe_Values.Look(ref outSignalLast_Arrested, "outSignalLastArrested");
        Scribe_Values.Look(ref outSignalLast_Destroyed, "outSignalLastDestroyed");
        Scribe_Values.Look(ref outSignalLast_Kidnapped, "outSignalLastKidnapped");
        Scribe_Values.Look(ref outSignalLast_Recruited, "outSignalLastRecruited");
        Scribe_Values.Look(ref outSignalLast_Banished, "outSignalLast_Banished");
        Scribe_Values.Look(ref outSignalLast_LeftBehind, "outSignalLast_LeftBehind");

        Scribe_Values.Look(ref outSignalLast_LeftMapAllHealthy, "outSignalLastLeftMapAllHealthy");
        Scribe_Values.Look(ref outSignalLast_LeftMapAllNotHealthy, "outSignalLastLeftMapAllNotHealthy");

        Scribe_Collections.Look(ref pawns, nameof(pawns), LookMode.Reference);
        Scribe_References.Look(ref faction, nameof(faction));
        Scribe_References.Look(ref mapParent, nameof(mapParent));
        Scribe_Values.Look(ref pawnsLeftUnhealthy, nameof(pawnsLeftUnhealthy), 0);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pawns.RemoveAll(p => p is null);
        }
    }
}