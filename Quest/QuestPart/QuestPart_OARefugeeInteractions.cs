using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class QuestPart_OARefugeeInteractions : QuestPartActivable
{
    private enum InteractionResponseType
    {
        AssaultColony,
        Leave,
        BadThought
    }

    public bool allowAssaultColony = true;

    public bool allowLeave = true;

    public bool allowBadThought = true;

    public string inSignalDestroyed;

    public string inSignalArrested;

    public string inSignalSurgeryViolation;

    public string inSignalPsychicRitualTarget;

    public string inSignalRecruited;

    public string inSignalKidnapped;

    public string inSignalAssaultColony;

    public string inSignalLeftMap;

    public string inSignalBanished;

    public string outSignalDestroyed_AssaultColony;

    public string outSignalDestroyed_LeaveColony;

    public string outSignalDestroyed_BadThought;

    public string outSignalArrested_AssaultColony;

    public string outSignalArrested_LeaveColony;

    public string outSignalArrested_BadThought;

    public string outSignalSurgeryViolation_AssaultColony;

    public string outSignalSurgeryViolation_LeaveColony;

    public string outSignalSurgeryViolation_BadThought;

    public string outSignalPsychicRitualTarget_AssaultColony;

    public string outSignalPsychicRitualTarget_LeaveColony;

    public string outSignalPsychicRitualTarget_BadThought;

    public string outSignalLast_Arrested;

    public string outSignalLast_Destroyed;

    public string outSignalLast_Kidnapped;

    public string outSignalLast_Recruited;

    public string outSignalLast_LeftMapAllHealthy;

    public string outSignalLast_LeftMapAllNotHealthy;

    public string outSignalLast_Banished;

    public List<Pawn> pawns = [];

    public Faction faction;

    public MapParent mapParent;

    public int pawnsLeftUnhealthy;

    public void InitWithDefaultSingals(bool allowAssaultColony, bool allowLeave, bool allowBadThought)
    {
        this.allowAssaultColony = allowAssaultColony;
        this.allowLeave = allowLeave;
        this.allowBadThought = allowBadThought;

        inSignalDestroyed = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed");
        inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.SurgeryViolation");
        inSignalKidnapped = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped");
        inSignalAssaultColony = QuestGen.GenerateNewSignal("AssaultColony");
        inSignalLeftMap = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap");
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
            outSignalPsychicRitualTarget_LeaveColony = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_LeaveColony");
        }

        if (allowBadThought)
        {
            outSignalDestroyed_BadThought = QuestGen.GenerateNewSignal("LodgerDestroyed_BadThought");
            outSignalArrested_BadThought = QuestGen.GenerateNewSignal("LodgerArrested_BadThought");
            outSignalSurgeryViolation_BadThought = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_BadThought");
            outSignalPsychicRitualTarget_BadThought = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_BadThought");
        }

        outSignalLast_Destroyed = QuestGen.GenerateNewSignal("LastLodger_Destroyed");
        outSignalLast_Arrested = QuestGen.GenerateNewSignal("LastLodger_Arrested");
        outSignalLast_Kidnapped = QuestGen.GenerateNewSignal("LastLodger_Kidnapped");
        outSignalLast_Recruited = QuestGen.GenerateNewSignal("LastLodger_Recruited");
        outSignalLast_LeftMapAllHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllHealthy");
        outSignalLast_LeftMapAllNotHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllNotHealthy");
        outSignalLast_Banished = QuestGen.GenerateNewSignal("LastLodger_Banished");
    }

    protected override void ProcessQuestSignal(Signal signal)
    {
        if (signal.tag == inSignalRecruited && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
        {
            pawns.Remove(arg);
            arg.apparel?.UnlockAll(); //解锁被招募人员的装备
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Recruited, signal.args));
            }
        }
        if (signal.tag == inSignalKidnapped && signal.args.TryGetArg("SUBJECT", out Pawn arg2) && pawns.Contains(arg2))
        {
            pawns.Remove(arg2);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Kidnapped, signal.args));
            }
        }
        if (signal.tag == inSignalBanished && signal.args.TryGetArg("SUBJECT", out Pawn arg3) && pawns.Contains(arg3))
        {
            pawns.Remove(arg3);
            if (pawns.Count == 0)
            {
                Find.SignalManager.SendSignal(new Signal(outSignalLast_Banished, signal.args));
            }
        }
        if (signal.tag == inSignalLeftMap && signal.args.TryGetArg("SUBJECT", out Pawn arg4) && pawns.Contains(arg4))
        {
            pawns.Remove(arg4);
            if (arg4.Destroyed || arg4.InMentalState || arg4.health.hediffSet.BleedRateTotal > 0.001f)
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
        if (signal.tag == inSignalDestroyed && signal.args.TryGetArg("SUBJECT", out Pawn arg5) && pawns.Contains(arg5))
        {
            pawns.Remove(arg5);
            arg5.SetFaction(faction);
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
        if (signal.tag == inSignalArrested && signal.args.TryGetArg("SUBJECT", out Pawn arg6) && pawns.Contains(arg6))
        {
            pawns.Remove(arg6);
            bool inAggroMentalState = arg6.InAggroMentalState;
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
        if (signal.tag == inSignalSurgeryViolation && signal.args.TryGetArg("SUBJECT", out Pawn arg7) && pawns.Contains(arg7))
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
        if (signal.tag == inSignalPsychicRitualTarget && signal.args.TryGetArg("SUBJECT", out Pawn arg8) && pawns.Contains(arg8))
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
        if (inSignalAssaultColony is not null && signal.tag == inSignalAssaultColony)
        {
            AssaultColony(null);
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
        Lord lord = LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, canKidnap: true, canTimeoutOrFlee: true, sappers: false, useAvoidGridSmart: false, canSteal: true, breachers: false, canPickUpOpportunisticWeapons: true), mapParent.Map);
        for (int k = 0; k < pawns.Count; k++)
        {
            if (!pawns[k].Dead)
            {
                lord.AddPawn(pawns[k]);
            }
        }
        Complete();
    }

    private InteractionResponseType ChooseRandomInteraction()
    {
        List<InteractionResponseType> interactionResponseTypes = [];
        if (allowAssaultColony)
        {
            interactionResponseTypes.Add(InteractionResponseType.AssaultColony);
        }
        if (allowLeave)
        {
            interactionResponseTypes.Add(InteractionResponseType.Leave);
        }
        if (allowBadThought)
        {
            interactionResponseTypes.Add(InteractionResponseType.BadThought);
        }

        if (interactionResponseTypes.Any())
        {
            return interactionResponseTypes.RandomElement();
        }
        else
        {
            return InteractionResponseType.Leave;
        }
    }

    public override void Notify_FactionRemoved(Faction f)
    {
        if (faction == f)
        {
            faction = null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allowAssaultColony, "allowAssaultColony", defaultValue: true);
        Scribe_Values.Look(ref allowLeave, "allowLeave", defaultValue: true);
        Scribe_Values.Look(ref allowBadThought, "allowBadThought", defaultValue: true);
        Scribe_Values.Look(ref inSignalDestroyed, "inSignalDestroyed");
        Scribe_Values.Look(ref inSignalArrested, "inSignalArrested");
        Scribe_Values.Look(ref inSignalSurgeryViolation, "inSignalSurgeryViolation");
        Scribe_Values.Look(ref inSignalPsychicRitualTarget, "inSignalPsychicRitualTarget");
        Scribe_Values.Look(ref inSignalRecruited, "inSignalRecruited");
        Scribe_Values.Look(ref inSignalKidnapped, "inSignalKidnapped");
        Scribe_Values.Look(ref inSignalAssaultColony, "inSignalAssaultColony");
        Scribe_Values.Look(ref inSignalLeftMap, "inSignalLeftMap");
        Scribe_Values.Look(ref inSignalBanished, "inSignalBanished");
        Scribe_Values.Look(ref outSignalDestroyed_AssaultColony, "outSignalDestroyed_AssaultColony");
        Scribe_Values.Look(ref outSignalDestroyed_LeaveColony, "outSignalDestroyed_LeaveColony");
        Scribe_Values.Look(ref outSignalDestroyed_BadThought, "outSignalDestroyed_BadThought");
        Scribe_Values.Look(ref outSignalArrested_AssaultColony, "outSignalArrested_AssaultColony");
        Scribe_Values.Look(ref outSignalArrested_LeaveColony, "outSignalArrested_LeaveColony");
        Scribe_Values.Look(ref outSignalArrested_BadThought, "outSignalArrested_BadThought");
        Scribe_Values.Look(ref outSignalSurgeryViolation_AssaultColony, "outSignalSurgeryViolation_AssaultColony");
        Scribe_Values.Look(ref outSignalSurgeryViolation_LeaveColony, "outSignalSurgeryViolation_LeaveColony");
        Scribe_Values.Look(ref outSignalSurgeryViolation_BadThought, "outSignalSurgeryViolation_BadThought");
        Scribe_Values.Look(ref outSignalPsychicRitualTarget_AssaultColony, "outSignalPsychicRitualTarget_AssaultColony");
        Scribe_Values.Look(ref outSignalPsychicRitualTarget_LeaveColony, "outSignalPsychicRitualTarget_LeaveColony");
        Scribe_Values.Look(ref outSignalPsychicRitualTarget_BadThought, "outSignalPsychicRitualTarget_BadThought");
        Scribe_Values.Look(ref outSignalLast_Arrested, "outSignalLastArrested");
        Scribe_Values.Look(ref outSignalLast_Destroyed, "outSignalLastDestroyed");
        Scribe_Values.Look(ref outSignalLast_Kidnapped, "outSignalLastKidnapped");
        Scribe_Values.Look(ref outSignalLast_Recruited, "outSignalLastRecruited");
        Scribe_Values.Look(ref outSignalLast_LeftMapAllHealthy, "outSignalLastLeftMapAllHealthy");
        Scribe_Values.Look(ref outSignalLast_LeftMapAllNotHealthy, "outSignalLastLeftMapAllNotHealthy");
        Scribe_Values.Look(ref outSignalLast_Banished, "outSignalLast_Banished");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_References.Look(ref faction, "faction");
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Values.Look(ref pawnsLeftUnhealthy, "pawnsLeftUnhealthy", 0);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pawns.RemoveAll(p => p is null);
        }
    }
}