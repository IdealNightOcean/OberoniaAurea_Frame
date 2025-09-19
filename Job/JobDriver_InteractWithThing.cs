using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

public abstract class JobDriver_InteractWithThing : JobDriver
{
    protected JobExtensionRecord jobExtensionRecord;

    protected float tickWorkAmount;
    protected float curWorkAmount;
    protected float totalWorkAmount = 1;

    protected float ProgressPercent => curWorkAmount / totalWorkAmount;

    protected Thing Target => TargetThingA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        JobExtension jobExtension = job.def.GetModExtension<JobExtension>();
        if (jobExtension is null)
        {
            return false;
        }

        jobExtensionRecord = jobExtension.jobExtensionRecord;
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        foreach (Toil preToil in PreInteractToils())
        {
            yield return preToil;
        }
        PathEndMode pathEndMode = TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch;
        yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
        Toil toil = ToilMaker.MakeToil("MakeNewToils");
        toil.FailOnCannotTouch(TargetIndex.A, pathEndMode);

        toil.handlingFacing = true;
        toil.initAction = JobInitAction;
        toil.tickIntervalAction = JobTickIntervalAction;

        if (jobExtensionRecord.jobEffecter is not null)
        {
            toil.WithEffect(jobExtensionRecord.jobEffecter, TargetIndex.A);
        }

        toil.WithProgressBar(TargetIndex.A, () => ProgressPercent, interpolateBetweenActorAndTarget: false, -0.5f, alwaysShow: true);

        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.activeSkill = () => jobExtensionRecord.jobSkill;
        AdjuestMainToil(ref toil);
        yield return toil;
        yield return Toils_General.Do(delegate
        {
            InteractionResult(pawn);
        });
    }

    protected virtual IEnumerable<Toil> PreInteractToils()
    {
        return Enumerable.Empty<Toil>();
    }

    protected void JobInitAction()
    {
        curWorkAmount = 0f;
        tickWorkAmount = pawn.GetStatValue(jobExtensionRecord.jobStat) * jobExtensionRecord.statFactorForTickAmount;
        totalWorkAmount = Mathf.Max(1, GetTotalWorkAmount(jobExtensionRecord.defaultWorkAmount));
        PostJobInitAction();
    }

    protected virtual void PostJobInitAction() { }

    protected virtual void JobTickIntervalAction(int delta)
    {
        pawn.rotationTracker.FaceTarget(Target);
        pawn.skills?.Learn(jobExtensionRecord.jobSkill, jobExtensionRecord.skillXpPerTick * delta);

        if ((curWorkAmount += (tickWorkAmount * delta)) >= totalWorkAmount)
        {
            ReadyForNextToil();
        }
    }

    protected abstract void InteractionResult(Pawn pawn);

    protected virtual void AdjuestMainToil(ref Toil toil) { }

    protected virtual float GetTotalWorkAmount(float baseWorkAmount) => baseWorkAmount;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref jobExtensionRecord, "jobExtensionRecord");
        Scribe_Values.Look(ref tickWorkAmount, "tickWorkAmount", 0);
        Scribe_Values.Look(ref curWorkAmount, "curWorkAmount", 0);
        Scribe_Values.Look(ref totalWorkAmount, "totalWorkAmount", 0);
    }

}
