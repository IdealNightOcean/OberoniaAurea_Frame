using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 与固定远行队交互的世界对象基类。 
/// </summary>
public abstract class WorldObject_InteractWithFixedCaravanBase : WorldObject_InteractiveBase, IFixedCaravanAssociate
{
    /// <summary>
    /// 创建固定远行队时使用的世界对象定义。
    /// </summary>
    protected virtual WorldObjectDef FixedCaravanDef => OAFrameDefOf.OAFrame_FixedCaravan;
    /// <inheritdoc/>
    public virtual string FixedCaravanName => null;

    /// <summary>
    /// 是否正在进行交互工作。
    /// </summary>
    protected bool isWorking;
    /// <summary>
    /// 是否正在进行交互工作。
    /// </summary>
    public bool IsWorking => isWorking;

    /// <summary>
    /// 剩余的工作 tick 数。
    /// </summary>
    protected int ticksRemaining;
    /// <summary>
    /// 是否请求中断当前工作。
    /// </summary>
    [Unsaved] protected bool interrupt;

    /// <summary>
    /// 关联的固定远行队（<see cref="FixedCaravan"/>）。
    /// </summary>
    protected FixedCaravan associatedFixedCaravan;
    /// <summary>
    /// 获取关联的固定远行队（<see cref="FixedCaravan"/>）。
    /// </summary>
    public FixedCaravan AssociatedFixedCaravan => associatedFixedCaravan;
    /// <summary>
    /// 获取完成交互工作所需的时间（tick）。
    /// </summary>
    public virtual int TicksNeeded => 30000;

    /// <inheritdoc/>
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        if (isWorking)
        {
            Messages.Message("OAFrame_Message_AlreadyHasFixedCaravan".Translate(), MessageTypeDefOf.RejectInput, historical: false);

        }
        else if (caravan.IsExactTypeCaravan())
        {
            StartWork(caravan);
        }
    }

    /// <inheritdoc />
    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (isWorking)
        {
            WorkTickInterval(delta);
        }
    }

    /// <summary>
    /// 工作间隔的 tick 处理。
    /// </summary>
    /// <param name="delta">间隔 tick 数</param>
    protected virtual void WorkTickInterval(int delta)
    {
        if ((ticksRemaining -= delta) <= 0)
        {
            EndWork(interrupt: false, convertToCaravan: true);
        }
    }

    /// <summary>
    /// 开始工作。
    /// </summary>
    public virtual bool StartWork(Caravan caravan)
    {
        FixedCaravan fixedCaravan = Utility.OAFrame_FixedCaravanUtility.CreateFixedCaravan(caravan, FixedCaravanDef, this);
        if (fixedCaravan is null)
        {
            Reset();
            return false;
        }
        associatedFixedCaravan = fixedCaravan;
        Find.WorldObjects.Add(fixedCaravan);
        Find.WorldSelector.Select(fixedCaravan);

        isWorking = true;
        interrupt = false;
        ticksRemaining = TicksNeeded;

        return true;
    }

    /// <summary>
    /// 结束工作。
    /// </summary>
    public void EndWork(bool interrupt = false, bool convertToCaravan = true)
    {
        if (isWorking)
        {
            isWorking = false;
            this.interrupt = interrupt;
            if (interrupt)
            {
                InterruptWork();
            }
            else
            {
                FinishWork();
            }
        }

        if (convertToCaravan && associatedFixedCaravan is not null)
        {
            Utility.OAFrame_FixedCaravanUtility.ConvertToCaravan(associatedFixedCaravan);
        }

        Reset();
    }

    /// <summary>
    /// 完成工作时的处理（由子类实现）。
    /// </summary>
    protected abstract void FinishWork();
    /// <summary>
    /// 工作中断时的处理（由子类实现）。
    /// </summary>
    protected abstract void InterruptWork();
    /// <summary>
    /// 重置工作状态。
    /// </summary>
    protected virtual void Reset()
    {
        isWorking = false;
        ticksRemaining = TicksNeeded;
        associatedFixedCaravan = null;
    }
    /// <summary>
    /// 玩家主动将固定远行队转换为远行队的预处理。
    /// </summary>
    public virtual void PreConvertToCaravanByPlayer()
    {
        EndWork(interrupt: true, convertToCaravan: false);
    }

    /// <summary>
    /// 固定远行队转换为远行队后的处理。
    /// </summary>
    public virtual void PostConvertToCaravan(Caravan caravan) { }

    /// <summary>
    /// 获取固定远行队工作描述。
    /// </summary>
    public virtual string FixedCaravanWorkDesc()
    {
        return "OAFrame_FixedCaravanWork_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod());
    }

    /// <inheritdoc />
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (DebugSettings.ShowDevGizmos && isWorking)
        {
            yield return new Command_Action
            {
                defaultLabel = "Dev: Finish Work",
                action = () => EndWork(interrupt: false, convertToCaravan: true),
            };
        }
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        if (isWorking)
        {
            EndWork(interrupt: true, convertToCaravan: true);
        }
        base.Destroy();
    }

    /// <inheritdoc />
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isWorking, nameof(isWorking), defaultValue: false);
        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), 0);

        Scribe_References.Look(ref associatedFixedCaravan, nameof(associatedFixedCaravan));
    }
}