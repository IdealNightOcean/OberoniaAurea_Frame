using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class LordJob_VisitColonyBase : LordJob
{
    protected Faction faction;
    protected IntVec3 chillSpot;
    protected int? durationTicks;
    public List<Thing> gifts;
    public StateGraph exitSubgraph;

    public LordJob_VisitColonyBase() { }

    public LordJob_VisitColonyBase(Faction faction, IntVec3 chillSpot, int? durationTicks = null)
    {
        this.faction = faction;
        this.chillSpot = chillSpot;
        this.durationTicks = durationTicks;
    }

    protected virtual LordToil_DefendPoint GetDefendPointLordToil()
    {
        return new LordToil_DefendPoint(chillSpot);
    }

    public override StateGraph CreateGraph()
    {
        StateGraph stateGraph = new();
        LordToil lordToil = stateGraph.StartingToil = stateGraph.AttachSubgraph(new LordJob_Travel(chillSpot).CreateGraph()).StartingToil;
        LordToil_DefendPoint lordToil_DefendPoint = GetDefendPointLordToil();
        stateGraph.AddToil(lordToil_DefendPoint);
        LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new();
        stateGraph.AddToil(lordToil_TakeWoundedGuest);
        exitSubgraph = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();

        LordToil exitMapToil = stateGraph.AttachSubgraph(exitSubgraph).StartingToil;
        LordToil target = exitSubgraph.lordToils[1];
        LordToil_ExitMap lordToil_ExitMap = new(LocomotionUrgency.Walk, canDig: true);
        stateGraph.AddToil(lordToil_ExitMap);

        Transition transition = new(lordToil, exitMapToil);
        transition.AddSources(lordToil_DefendPoint);
        transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
        if (faction is not null)
        {
            transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition);

        Transition transition2 = new(lordToil, exitMapToil);
        transition2.AddSources(lordToil_DefendPoint);
        transition2.AddTrigger(new Trigger_PawnExperiencingAnomalousWeather());
        if (faction is not null)
        {
            transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsAnomalousWeather".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition2.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition2);

        Transition transition3 = new(lordToil, lordToil_ExitMap);
        transition3.AddSources(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
        transition3.AddSources(exitSubgraph.lordToils);
        transition3.AddTrigger(new Trigger_PawnCannotReachMapEdge());
        if (faction is not null)
        {
            transition3.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
        }
        stateGraph.AddTransition(transition3);

        Transition transition4 = new(lordToil_ExitMap, exitMapToil);
        transition4.AddTrigger(new Trigger_PawnCanReachMapEdge());
        transition4.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        transition4.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition4);

        Transition transition5 = new(lordToil, lordToil_DefendPoint);
        transition5.AddTrigger(new Trigger_Memo("TravelArrived"));
        stateGraph.AddTransition(transition5);
        if (faction is not null)
        {
            Transition transition6 = new(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
            transition6.AddTrigger(new Trigger_WoundedGuestPresent());
            transition6.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            stateGraph.AddTransition(transition6);
        }

        Transition transition7 = new(lordToil_DefendPoint, target);
        transition7.AddSources(lordToil_TakeWoundedGuest, lordToil);
        transition7.AddTrigger(new Trigger_BecamePlayerEnemy());
        transition7.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition7.AddPostAction(new TransitionAction_WakeAll());
        transition7.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition7);

        Transition transition8 = new(lordToil_DefendPoint, exitMapToil);
        int tickLimit = (!DebugSettings.instantVisitorsGift || faction == null) ? (durationTicks ?? Rand.Range(8000, 22000)) : 0;
        transition8.AddTrigger(new Trigger_TicksPassed(tickLimit));
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