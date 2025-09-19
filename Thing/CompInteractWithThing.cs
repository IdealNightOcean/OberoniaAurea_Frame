using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

public class CompProperties_InteractWithThing : CompProperties
{
    public JobDef interactJob;
}

public abstract class CompInteractWithThing : ThingComp
{
    protected CompProperties_InteractWithThing Props => (CompProperties_InteractWithThing)props;

    protected virtual JobDef InteractJob => Props.interactJob;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        foreach (FloatMenuOption menuOption in base.CompFloatMenuOptions(selPawn))
        {
            yield return menuOption;
        }
        AcceptanceReport acceptanceReport = CanInteractNow(selPawn);
        if (acceptanceReport)
        {
            yield return new FloatMenuOption("Uses".Translate(parent.Label), delegate
            {
                StartInteraction(selPawn);
            });
        }
        else if (!acceptanceReport.Reason.NullOrEmpty())
        {
            yield return new FloatMenuOption("CannotUseReason".Translate(acceptanceReport.Reason), null);
        }
        else
        {
            yield return new FloatMenuOption("CannotUseReason".Translate(parent.Label), null);
        }
    }

    protected virtual void StartInteraction(Pawn pawn)
    {
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InteractJob, parent), JobTag.Misc);
    }

    public abstract void InteractionResult(Pawn pawn);

    protected virtual AcceptanceReport CanInteractNow(Pawn pawn)
    {
        if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            return "NoPath".Translate().CapitalizeFirst();
        }
        return true;
    }
}