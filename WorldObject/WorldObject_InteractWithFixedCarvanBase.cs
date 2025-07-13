using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_InteractWithFixedCarvanBase : WorldObject_InteractiveBase, IFixedCaravanAssociate
{
    public virtual string FixedCaravanName => null;

    protected bool isWorking;
    protected int ticksRemaining;

    protected FixedCaravan associatedFixedCaravan;
    public virtual int TicksNeeded => 30000;

    public override void Notify_CaravanArrived(Caravan caravan)
    {
        if (isWorking || !OAFrame_CaravanUtility.IsExactTypeCaravan(caravan))
        {
            return;
        }
        StartWork(caravan);
    }

    public override void Tick()
    {
        base.Tick();
        if (isWorking)
        {
            WorkTick();
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void WorkTick()
    {
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            EndWork(interrupt: false, coverToCaravan: true);
        }
    }

    public virtual bool StartWork(Caravan caravan)
    {
        FixedCaravan fixedCaravan = OAFrame_FixedCaravanUtility.CreateFixedCaravan(caravan, this);
        if (fixedCaravan is null)
        {
            Reset();
            return false;
        }
        associatedFixedCaravan = fixedCaravan;
        Find.WorldObjects.Add(fixedCaravan);
        Find.WorldSelector.Select(fixedCaravan);

        isWorking = true;
        ticksRemaining = TicksNeeded;

        return true;
    }

    public void EndWork(bool interrupt = false, bool coverToCaravan = true)
    {
        if (isWorking)
        {
            isWorking = false;
            if (interrupt)
            {
                InterruptWork();
            }
            else
            {
                FinishWork();
            }
        }

        if (coverToCaravan && associatedFixedCaravan is not null)
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

    public virtual void PreConvertToCaravanByPlayer(FixedCaravan fixedCaravan)
    {
        EndWork(interrupt: true, coverToCaravan: false);
    }

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
                action = () => EndWork(interrupt: false, coverToCaravan: true),
            };
        }
    }

    public override void Destroy()
    {
        if (isWorking)
        {
            EndWork(interrupt: true, coverToCaravan: true);
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
