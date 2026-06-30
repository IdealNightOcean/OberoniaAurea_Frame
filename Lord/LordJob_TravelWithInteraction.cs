using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 带有交互功能的旅行职责。
/// </summary>
public class LordJob_TravelWithInteraction : LordJob_Travel
{
    protected IntVec3 interactTravelDest;

    public LordJob_TravelWithInteraction() { }

    public LordJob_TravelWithInteraction(IntVec3 travelDest) : base(travelDest)
    {
        interactTravelDest = travelDest;
    }

    public override StateGraph CreateGraph()
    {
        StateGraph stateGraph = new();
        LordToil_TravelWithInteraction lordToil_Travel = new(interactTravelDest);
        stateGraph.StartingToil = lordToil_Travel;
        LordToil_DefendPoint lordToil_DefendPoint = new(canSatisfyLongNeeds: false);
        stateGraph.AddToil(lordToil_DefendPoint);

        Transition transition1 = new(lordToil_Travel, lordToil_DefendPoint);
        transition1.AddTrigger(new Trigger_PawnHarmed());
        transition1.AddPreAction(new TransitionAction_SetDefendLocalGroup());
        transition1.AddPostAction(new TransitionAction_EndAllJobs());
        stateGraph.AddTransition(transition1);

        Transition transition2 = new(lordToil_DefendPoint, lordToil_Travel);
        transition2.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
        transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
        stateGraph.AddTransition(transition2);
        return stateGraph;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref interactTravelDest, nameof(interactTravelDest));
    }
}