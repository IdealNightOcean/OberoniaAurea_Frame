using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea_Frame;

/// <summary>
/// 与物体交互的组件属性。
/// </summary>
public class CompProperties_InteractWithThing : CompProperties
{
    /// <summary>
    /// 用于交互的<see cref="JobDef"/>。
    /// </summary>
    public JobDef interactJob;
}

/// <summary>
/// 与物体交互的组件抽象类。
/// </summary>
public abstract class CompInteractWithThing : ThingComp
{
    /// <summary>
    /// 获取与物体交互的组件属性。
    /// </summary>
    protected CompProperties_InteractWithThing Props => (CompProperties_InteractWithThing)props;

    /// <summary>
    /// 获取用于交互的<see cref="JobDef"/>。
    /// </summary>
    protected virtual JobDef InteractJob => Props.interactJob;

    /// <inheritdoc/>
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

    /// <summary>
    /// 开始对父物体执行交互，供子类重写。
    /// </summary>
    /// <param name="pawn">执行交互的<see cref="Pawn"/>。</param>
    protected virtual void StartInteraction(Pawn pawn)
    {
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InteractJob, parent), JobTag.Misc);
    }

    /// <summary>
    /// 处理交互结果。
    /// </summary>
    public abstract void InteractionResult(Pawn pawn);

    /// <summary>
    /// 检查指定<see cref="Pawn"/>目前是否可以进行交互。
    /// </summary>
    /// <param name="pawn">执行交互的<see cref="Pawn"/>。</param>
    protected virtual AcceptanceReport CanInteractNow(Pawn pawn)
    {
        if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            return "NoPath".Translate().CapitalizeFirst();
        }
        return true;
    }
}