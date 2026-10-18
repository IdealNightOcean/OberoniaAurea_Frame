using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

/// <summary>
/// 与物体交互的工作驱动抽象类。
/// </summary>
public abstract class JobDriver_InteractWithThingAtOnce : JobDriver
{
    /// <summary>
    /// 交互目标。
    /// </summary>
    protected Thing InteractTarget => TargetThingA;

    /// <inheritdoc/>
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, InteractTarget.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch);
        Toil interact = ToilMaker.MakeToil("InteractToils");
        interact.initAction = delegate
        {
            InteractAction(pawn);
        };
        yield return interact;
    }

    /// <summary>
    /// 执行交互操作。
    /// </summary>
    /// <param name="pawn">执行交互的<see cref="Pawn"/>。</param>
    protected abstract void InteractAction(Pawn pawn);
}