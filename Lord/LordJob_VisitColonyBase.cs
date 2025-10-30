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
    public StateGraph exitSubgraph;

    public override bool ShouldExistWithoutPawns => true;

    public LordJob_VisitColonyBase() { }

    public LordJob_VisitColonyBase(Faction faction, IntVec3 chillSpot, int? durationTicks = null)
    {
        this.faction = faction;
        this.chillSpot = chillSpot;
        this.durationTicks = durationTicks;
    }

    public virtual IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
    {
        return Enumerable.Empty<FloatMenuOption>();
    }

    public override StateGraph CreateGraph()
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

        exitSubgraph = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
        LordToil exitMapTravelToil = stateGraph.AttachSubgraph(exitSubgraph).StartingToil;
        LordToil exitMapToil = exitSubgraph.lordToils[1];
        LordToil_ExitMap lordToil_ExitMap = new(LocomotionUrgency.Walk, canDig: true);
        stateGraph.AddToil(lordToil_ExitMap);

        Transition transition3 = new(lordToil_Travel, exitMapTravelToil);
        transition3.AddSources(lordToil_DefendTargetPoint);
        transition3.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
        if (faction is not null)
        {
            transition3.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition3.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition3.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition3);

        Transition transition4 = new(lordToil_Travel, exitMapTravelToil);
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
        transition5.AddSources(exitSubgraph.lordToils);
        transition5.AddTrigger(new Trigger_PawnCannotReachMapEdge());
        if (faction is not null)
        {
            transition5.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        stateGraph.AddTransition(transition5);

        Transition transition6 = new(lordToil_ExitMap, exitMapTravelToil);
        transition6.AddTrigger(new Trigger_PawnCanReachMapEdge());
        transition6.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition6.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition6);

        Transition transition7 = new(lordToil_Travel, lordToil_DefendTargetPoint);
        transition7.AddTrigger(new Trigger_Memo("TravelArrived"));
        stateGraph.AddTransition(transition7);
        if (faction is not null)
        {
            Transition transition8 = new(lordToil_DefendTargetPoint, lordToil_TakeWoundedGuest);
            transition8.AddTrigger(new Trigger_WoundedGuestPresent());
            transition8.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            stateGraph.AddTransition(transition8);
        }

        Transition transition9 = new(lordToil_DefendTargetPoint, exitMapToil);
        transition9.AddSources(lordToil_TakeWoundedGuest, lordToil_Travel);
        transition9.AddTrigger(new Trigger_BecamePlayerEnemy());
        transition9.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition9.AddPostAction(new TransitionAction_WakeAll());
        transition9.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition9);

        Transition transition10 = new(lordToil_DefendTargetPoint, exitMapTravelToil);
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

    public override void ExposeData()
    {
        Scribe_References.Look(ref faction, "faction");
        Scribe_Values.Look(ref chillSpot, "chillSpot");
        Scribe_Values.Look(ref durationTicks, "durationTicks");
        Scribe_Collections.Look(ref gifts, "gifts", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            gifts?.RemoveAll(t => t is null);
        }
    }
}