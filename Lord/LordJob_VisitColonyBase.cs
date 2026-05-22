using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class LordJob_VisitColonyBase : LordJob, ILordFloatMenuProvider
{
    protected Faction faction;
    protected IntVec3 chillSpot;
    protected int? durationTicks;
    public List<Thing> gifts;

    protected bool dismissed;

    public override bool ShouldExistWithoutPawns => true;

    public LordJob_VisitColonyBase() { }

    /// <summary>
    /// 初始化访问职责。
    /// </summary>
    public LordJob_VisitColonyBase(Faction faction, IntVec3 chillSpot, int? durationTicks = null)
    {
        this.faction = faction;
        this.chillSpot = chillSpot;
        this.durationTicks = durationTicks;
    }

    /// <summary>
    /// 获取额外的浮动菜单选项。
    /// </summary>
    public virtual IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
    {
        return Enumerable.Empty<FloatMenuOption>();
    }

    public override StateGraph CreateGraph()
    {
        StateGraph stateGraph = new();
        StateGraph travelSubgraph = new LordJob_TravelWithInteraction(chillSpot).CreateGraph();
        LordToil lordToil_TravelWithInteraction = travelSubgraph.StartingToil;
        stateGraph.StartingToil = lordToil_TravelWithInteraction;

        LordToil_DefendPointWithInteraction lordToil_DefendPoint = new(chillSpot);
        stateGraph.AddToil(lordToil_DefendPoint);
        LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new();
        stateGraph.AddToil(lordToil_TakeWoundedGuest);

        StateGraph exitSubgraph = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
        stateGraph.AttachSubgraph(exitSubgraph);
        LordToil lordToil_ExitSubgraph_Start = exitSubgraph.StartingToil;
        LordToil lordToil_ExitSubgraph_Target = exitSubgraph.lordToils[1];

        LordToil_ExitMap lordToil_ExitMap = new(LocomotionUrgency.Sprint, canDig: true);
        stateGraph.AddToil(lordToil_ExitMap);

        Transition transition1 = new(lordToil_TravelWithInteraction, lordToil_ExitSubgraph_Start);
        transition1.AddSources(lordToil_DefendPoint);
        transition1.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
        if (faction is not null)
        {
            transition1.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition1.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition1.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition1);

        Transition transition2 = new(lordToil_TravelWithInteraction, lordToil_ExitSubgraph_Start);
        transition2.AddSources(lordToil_DefendPoint);
        transition2.AddTrigger(new Trigger_PawnExperiencingAnomalousWeather());
        if (faction is not null)
        {
            transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsAnomalousWeather".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition2.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition2);

        Transition transition3 = new(lordToil_TravelWithInteraction, lordToil_ExitMap);
        transition3.AddSources(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
        transition3.AddSources(exitSubgraph.lordToils);
        transition3.AddTrigger(new Trigger_PawnCannotReachMapEdge());
        if (faction is not null)
        {
            transition3.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        stateGraph.AddTransition(transition3);

        Transition transition4 = new(lordToil_ExitMap, lordToil_ExitSubgraph_Start);
        transition4.AddTrigger(new Trigger_PawnCanReachMapEdge());
        transition4.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition4.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition4);

        Transition transition5 = new(lordToil_TravelWithInteraction, lordToil_DefendPoint);
        transition5.AddTrigger(new Trigger_Memo("TravelArrived"));
        stateGraph.AddTransition(transition5);
        if (faction is not null)
        {
            Transition transition6 = new(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
            transition6.AddTrigger(new Trigger_WoundedGuestPresent());
            transition6.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            stateGraph.AddTransition(transition6);
        }

        Transition transition7 = new(lordToil_DefendPoint, lordToil_ExitSubgraph_Target);
        transition7.AddSources(lordToil_TakeWoundedGuest, lordToil_TravelWithInteraction);
        transition7.AddTrigger(new Trigger_BecamePlayerEnemy());
        transition7.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition7.AddPostAction(new TransitionAction_WakeAll());
        transition7.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition7);

        Transition transition8 = new(lordToil_DefendPoint, lordToil_ExitSubgraph_Start);
        int tickLimit = ((!DebugSettings.instantVisitorsGift || faction is null) ? (durationTicks ?? Rand.Range(8000, 22000)) : 0);
        transition8.AddTrigger(new Trigger_TicksPassed(tickLimit));
        transition8.AddTrigger(new Trigger_Custom(s => s.type == TriggerSignalType.Tick && dismissed));
        if (faction is not null)
        {
            transition8.AddPreAction(new TransitionAction_Message("VisitorsLeaving".Translate(faction.Name)));
        }
        if (gifts is not null)
        {
            transition8.AddPreAction(new TransitionAction_GiveGift
            {
                gifts = gifts
            });
        }
        else
        {
            transition8.AddPreAction(new TransitionAction_CheckGiveGift());
        }
        transition8.AddPostAction(new TransitionAction_WakeAll());
        transition8.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        stateGraph.AddTransition(transition8);
        return stateGraph;
    }

    public override void ExposeData()
    {
        Scribe_References.Look(ref faction, nameof(faction));
        Scribe_Values.Look(ref chillSpot, nameof(chillSpot));
        Scribe_Values.Look(ref durationTicks, nameof(durationTicks));
        Scribe_Values.Look(ref dismissed, nameof(dismissed));
        Scribe_Collections.Look(ref gifts, nameof(gifts), LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            gifts?.RemoveAll(t => t is null);
        }
    }

    /*
    private StateGraph CreateGraphOld()
    {
        StateGraph stateGraph = new();
        LordToil_TravelWithInteraction lordToil_Travel = new(chillSpot);
        stateGraph.StartingToil = lordToil_Travel;
        LordToil_DefendPoint lordToil_DefendOnTheSpot = new(canSatisfyLongNeeds: false);
        stateGraph.AddToil(lordToil_DefendOnTheSpot);

        Transition transition1 = new(lordToil_Travel, lordToil_DefendOnTheSpot);
        transition1.AddTrigger(new Trigger_PawnHarmed());
        transition1.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition1.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition1);

        Transition transition2 = new(lordToil_DefendOnTheSpot, lordToil_Travel);
        transition2.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
        transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        stateGraph.AddTransition(transition2);

        LordToil_DefendPointWithInteraction lordToil_DefendTargetPoint = new(chillSpot);
        stateGraph.AddToil(lordToil_DefendTargetPoint);
        LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new();
        stateGraph.AddToil(lordToil_TakeWoundedGuest);

        LordToil_ExitMap lordToil_ExitMap = new(LocomotionUrgency.Sprint, canDig: true);
        stateGraph.AddToil(lordToil_ExitMap);

        Transition transition3 = new(lordToil_Travel, lordToil_ExitMap);
        transition3.AddSources(lordToil_DefendTargetPoint);
        transition3.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
        if (faction is not null)
        {
            transition3.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition3.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition3.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition3);

        Transition transition4 = new(lordToil_Travel, lordToil_ExitMap);
        transition4.AddSources(lordToil_DefendTargetPoint);
        transition4.AddTrigger(new Trigger_PawnExperiencingAnomalousWeather());
        if (faction is not null)
        {
            transition4.AddPreAction(new TransitionAction_Message("MessageVisitorsAnomalousWeather".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition4.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition4.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition4);

        Transition transition5 = new(lordToil_Travel, lordToil_ExitMap);
        transition5.AddSources(lordToil_DefendTargetPoint, lordToil_TakeWoundedGuest);
        transition5.AddTrigger(new Trigger_PawnCannotReachMapEdge());
        if (faction is not null)
        {
            transition5.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        stateGraph.AddTransition(transition5);

        Transition transition6 = new(lordToil_Travel, lordToil_DefendTargetPoint);
        transition6.AddTrigger(new Trigger_Memo("TravelArrived"));
        stateGraph.AddTransition(transition6);
        if (faction is not null)
        {
            Transition transition7 = new(lordToil_DefendTargetPoint, lordToil_TakeWoundedGuest);
            transition7.AddTrigger(new Trigger_WoundedGuestPresent());
            transition7.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            stateGraph.AddTransition(transition7);
        }

        Transition transition8 = new(lordToil_DefendTargetPoint, lordToil_ExitMap);
        transition8.AddSources(lordToil_TakeWoundedGuest, lordToil_Travel);
        transition8.AddTrigger(new Trigger_BecamePlayerEnemy());
        transition8.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition8.AddPostAction(new TransitionAction_WakeAll());
        transition8.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition8);

        Transition transition9 = new(lordToil_DefendTargetPoint, lordToil_ExitMap);
        transition9.AddSources(lordToil_TakeWoundedGuest, lordToil_Travel);
        transition9.AddTrigger(new Trigger_Custom(s => s.type == TriggerSignalType.Tick && dismissed));
        transition9.AddPostAction(new TransitionAction_WakeAll());
        transition9.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition9);

        Transition transition10 = new(lordToil_DefendTargetPoint, lordToil_ExitMap);
        int tickLimit = (!DebugSettings.instantVisitorsGift || faction is null) ? (durationTicks ?? Rand.Range(8000, 22000)) : 0;
        transition10.AddTrigger(new Trigger_TicksPassed(tickLimit));
        if (faction is not null)
        {
            transition10.AddPreAction(new TransitionAction_Message("VisitorsLeaving".Translate(faction.Name)));
        }
        if (gifts is not null)
        {
            transition10.AddPreAction(new TransitionAction_GiveGift
            {
                gifts = gifts
            });
        }
        else
        {
            transition10.AddPreAction(new TransitionAction_CheckGiveGift());
        }
        transition10.AddPostAction(new TransitionAction_WakeAll());
        transition10.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        stateGraph.AddTransition(transition10);

        return stateGraph;
    }
    */
}