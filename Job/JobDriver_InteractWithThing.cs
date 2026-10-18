using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

/// <summary>
/// 与物体交互的工作驱动抽象类。
/// </summary>
public abstract class JobDriver_InteractWithThing : JobDriver
{
    /// <summary>
    /// 工作扩展记录。
    /// </summary>
    protected JobExtensionRecord jobExtensionRecord;

    /// <summary>
    /// 每刻的工作量。
    /// </summary>
    protected float tickWorkAmount;
    /// <summary>
    /// 当前累计工作量。
    /// </summary>
    protected float curWorkAmount;
    /// <summary>
    /// 总工作量。
    /// </summary>
    protected float totalWorkAmount = 1;

    /// <summary>
    /// 工作进度百分比。
    /// </summary>
    protected float ProgressPercent => curWorkAmount / totalWorkAmount;

    /// <summary>
    /// 交互目标。
    /// </summary>
    protected Thing Target => TargetThingA;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <summary>
    /// 返回交互前的预备工作序列。
    /// </summary>
    protected virtual IEnumerable<Toil> PreInteractToils()
    {
        return Enumerable.Empty<Toil>();
    }

    /// <summary>
    /// 初始化工作相关数据。
    /// </summary>
    protected void JobInitAction()
    {
        curWorkAmount = 0f;
        tickWorkAmount = pawn.GetStatValue(jobExtensionRecord.jobStat) * jobExtensionRecord.statFactorForTickAmount;
        totalWorkAmount = Mathf.Max(1, GetTotalWorkAmount(jobExtensionRecord.defaultWorkAmount));
        PostJobInitAction();
    }

    /// <summary>
    /// 初始化完成后执行的操作。
    /// </summary>
    protected virtual void PostJobInitAction() { }

    /// <summary>
    /// 工作进行中的每刻操作。
    /// </summary>
    /// <param name="delta">经过的刻数。</param>
    protected virtual void JobTickIntervalAction(int delta)
    {
        pawn.rotationTracker.FaceTarget(Target);
        pawn.skills?.Learn(jobExtensionRecord.jobSkill, jobExtensionRecord.skillXpPerTick * delta);

        if ((curWorkAmount += (tickWorkAmount * delta)) >= totalWorkAmount)
        {
            ReadyForNextToil();
        }
    }

    /// <summary>
    /// 交互完成时执行的结果操作。
    /// </summary>
    /// <param name="pawn">执行交互的<see cref="Pawn"/>。</param>
    protected abstract void InteractionResult(Pawn pawn);

    /// <summary>
    /// 调整主工作序列。
    /// </summary>
    /// <param name="toil">主工作。</param>
    protected virtual void AdjuestMainToil(ref Toil toil) { }

    /// <summary>
    /// 获取总工作量。
    /// </summary>
    /// <param name="baseWorkAmount">基础工作量。</param>
    /// <returns>计算后的总工作量。</returns>
    protected virtual float GetTotalWorkAmount(float baseWorkAmount) => baseWorkAmount;

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref jobExtensionRecord, nameof(jobExtensionRecord));
        Scribe_Values.Look(ref tickWorkAmount, nameof(tickWorkAmount), 0);
        Scribe_Values.Look(ref curWorkAmount, nameof(curWorkAmount), 0);
        Scribe_Values.Look(ref totalWorkAmount, nameof(totalWorkAmount), 0);
    }

}
