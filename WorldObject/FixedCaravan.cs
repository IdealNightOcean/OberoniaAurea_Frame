using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class FixedCaravan : WorldObject, IRenameable, IThingHolder
{
    private Material cachedMat;
    public override Material Material
    {
        get
        {
            if (cachedMat is null)
            {
                cachedMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
            }
            return cachedMat;
        }
    }
    public override Color ExpandingIconColor => base.Faction.Color;

    public string curName;
    public string RenamableLabel
    {
        get
        {
            return curName ?? BaseLabel;
        }
        set
        {
            curName = value;
        }
    }
    public string BaseLabel => def.label;
    public string InspectLabel => RenamableLabel;
    public ThingOwner GetDirectlyHeldThings()
    {
        return pawns;
    }
    public virtual void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }
    public ThingOwner<Pawn> pawns;
    public List<Pawn> PawnsListForReading => pawns.InnerListForReading;
    public int PawnsCount => pawns.Count;
    protected IEnumerable<Thing> AllItems => OAFrame_FixedCaravanUtility.AllInventoryItems(this);

    protected bool skillsDirty = true;
    protected readonly Dictionary<SkillDef, int> totalSkills = [];

    public int ticksRemaining;
    public FixedCaravan()
    {
        pawns = new ThingOwner<Pawn>(this, oneStackOnly: false, LookMode.Reference);
    }
    public void AddPawn(Pawn pawn, bool addCarriedPawnToWorldPawnsIfAny = true)
    {
        if (pawn is null)
        {
            Log.Warning("Tried to add a null pawn to " + this);
            return;
        }
        if (pawn.Dead)
        {
            Log.Warning(string.Concat("Tried to add ", pawn, " to ", this, ", but this pawn is dead."));
            return;
        }
        Pawn carriedPawn = pawn.carryTracker.CarriedThing as Pawn;
        if (carriedPawn is not null)
        {
            pawn.carryTracker.innerContainer.Remove(carriedPawn);
        }
        pawn.DeSpawnOrDeselect();
        if (pawns.TryAdd(pawn))
        {
            if (CaravanUtility.ShouldAutoCapture(pawn, base.Faction))
            {
                pawn.guest.CapturedBy(base.Faction);
            }
            if (carriedPawn is not null)
            {
                if (CaravanUtility.ShouldAutoCapture(carriedPawn, base.Faction))
                {
                    carriedPawn.guest.CapturedBy(base.Faction, pawn);
                }
                AddPawn(carriedPawn, addCarriedPawnToWorldPawnsIfAny);
                if (addCarriedPawnToWorldPawnsIfAny)
                {
                    Find.WorldPawns.PassToWorld(carriedPawn);
                }
            }
        }
        else
        {
            Log.Error(string.Concat("Couldn't add pawn ", pawn, " to caravan."));
        }
    }
    public void RemovePawn(Pawn pawn)
    {
        pawns.Remove(pawn);
    }
    public void RemoveAllPawns()
    {
        pawns.Clear();
    }
    public bool ContainsPawn(Pawn pawn)
    {
        return pawns.Contains(pawn);
    }
    public void AddPawnOrItem(Thing thing, bool addCarriedPawnToWorldPawnsIfAny = true)
    {
        if (thing is null)
        {
            Log.Warning("Tried to add a null thing to " + this);
        }
        else if (thing is Pawn p)
        {
            AddPawn(p, addCarriedPawnToWorldPawnsIfAny);
        }
        else
        {
            OAFrame_FixedCaravanUtility.GiveThing(this, thing);
        }
    }

    protected virtual void PreConvertToCaravanByPlayer()
    { }
    public abstract void Notify_ConvertToCaravan();

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        Command_Action command_Convert = new()
        {
            defaultLabel = "CommandReformCaravan".Translate(),
            defaultDesc = "CommandReformCaravanDesc".Translate(),
            icon = FormCaravanComp.FormCaravanCommand,
            Disabled = (PawnsCount == 0),
            action = delegate
            {
                PreConvertToCaravanByPlayer();
                OAFrame_FixedCaravanUtility.ConvertToCaravan(this);
            }
        };
        yield return command_Convert;

    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref pawns, "pawns", this);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref curName, "curName");
    }
}