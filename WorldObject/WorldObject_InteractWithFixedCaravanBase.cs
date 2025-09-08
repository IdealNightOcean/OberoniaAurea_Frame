using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_InteractWithFixedCaravanBase : WorldObject_InteractiveBase, IFixedCaravanAssociate
{
    protected virtual WorldObjectDef FixedCaravanDef => OAFrameDefOf.OAFrame_FixedCaravan;
    public virtual string FixedCaravanName => null;

    protected bool isWorking;
    protected int ticksRemaining;
    [Unsaved] protected bool interrupt;

    protected FixedCaravan associatedFixedCaravan;
    public FixedCaravan AssociatedFixedCaravan => associatedFixedCaravan;
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

    public virtual void PreConvertToCaravanByPlayer()
    {
        EndWork(interrupt: true, convertToCaravan: false);
    }
    public virtual void PostConvertToCaravan(Caravan caravan) { }

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
        Scribe_Values.Look(ref isWorking, "isWorking", defaultValue: false);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);

        Scribe_References.Look(ref associatedFixedCaravan, "associatedFixedCaravan");
    }
}