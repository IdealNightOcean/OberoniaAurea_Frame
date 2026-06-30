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
    protected virtual WorldObjectDef FixedCaravanDef => OAFrameDefOf.OAFrame_FixedCaravan;
    public virtual string FixedCaravanName => null;

    protected bool isWorking;
    /// <summary>
    /// 是否正在进行交互工作。
    /// </summary>
    public bool IsWorking => isWorking;

    protected int ticksRemaining;
    [Unsaved] protected bool interrupt;

    protected FixedCaravan associatedFixedCaravan;
    /// <summary>
    /// 获取关联的固定远行队（<see cref="FixedCaravan"/>）。
    /// </summary>
    public FixedCaravan AssociatedFixedCaravan => associatedFixedCaravan;
    /// <summary>
    /// 获取所需时间。
    /// </summary>
    public virtual int TicksNeeded => 30000;

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

    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (isWorking)
        {
            WorkTickInterval(delta);
        }
    }

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
        FixedCaravan fixedCaravan = OAFrame_FixedCaravanUtility.CreateFixedCaravan(caravan, FixedCaravanDef, this);
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
            OAFrame_FixedCaravanUtility.ConvertToCaravan(associatedFixedCaravan);
        }

        Reset();
    }

    protected abstract void FinishWork();
    protected abstract void InterruptWork();
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

    public override void Destroy()
    {
        if (isWorking)
        {
            EndWork(interrupt: true, convertToCaravan: true);
        }
        base.Destroy();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isWorking, nameof(isWorking), defaultValue: false);
        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), 0);

        Scribe_References.Look(ref associatedFixedCaravan, nameof(associatedFixedCaravan));
    }
}