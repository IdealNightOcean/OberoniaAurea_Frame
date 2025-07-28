using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class FixedCaravan : WorldObject, IThingHolder, IPawnRetentionHolder
{
    private Material cachedMat;
    public override Material Material
    {
        get
        {
            cachedMat ??= MaterialPool.MatFrom(Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, Faction.Color, WorldMaterials.WorldObjectRenderQueue);
            return cachedMat;
        }
    }
    public override Color ExpandingIconColor => Faction.Color;

    public string curName = null;
    public override string Label => curName ?? base.Label;
    public override bool HasName => !curName.NullOrEmpty();

    protected ThingOwner<Pawn> pawns;
    public List<Pawn> PawnsListForReading => pawns.InnerListForReading;
    public int PawnsCount => pawns.Count;
    protected IEnumerable<Thing> AllItems => OAFrame_FixedCaravanUtility.AllInventoryItems(this);

    protected bool skillsDirty = true;
    protected readonly Dictionary<SkillDef, int> totalSkills = [];

    protected WorldObject associatedWorldObject;
    [Unsaved] protected IFixedCaravanAssociate associatedInterface;

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
            if (CaravanUtility.ShouldAutoCapture(pawn, Faction))
            {
                pawn.guest.CapturedBy(Faction);
            }
            if (carriedPawn is not null)
            {
                if (CaravanUtility.ShouldAutoCapture(carriedPawn, Faction))
                {
                    carriedPawn.guest.CapturedBy(Faction, pawn);
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

    public void SetAssociatedWorldObject(WorldObject worldObject)
    {
        if (worldObject is null)
        {
            Log.Error($"Failed to set WorldObject to {this}: WorldObject is null");
            return;
        }

        associatedWorldObject = worldObject;
        TrySetAssociatedInterface();
        if (associatedInterface?.FixedCaravanName is not null)
        {
            curName = associatedInterface.FixedCaravanName;
        }
    }

    protected virtual void TrySetAssociatedInterface()
    {
        if (associatedWorldObject is IFixedCaravanAssociate caravanAssociate)
        {
            associatedInterface = caravanAssociate;
        }
    }

    protected virtual void PreConvertToCaravanByPlayer()
    {
        associatedInterface?.PreConvertToCaravanByPlayer();
    }

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
            Disabled = PawnsCount == 0,
            action = delegate
            {
                PreConvertToCaravanByPlayer();
                OAFrame_FixedCaravanUtility.ConvertToCaravan(this);
            }
        };
        yield return command_Convert;

    }

    public override string GetInspectString()
    {
        if (associatedInterface is null)
        {
            return base.GetInspectString();
        }
        else
        {
            StringBuilder stringBuilder = new(base.GetInspectString());
            stringBuilder.AppendInNewLine(associatedInterface.FixedCaravanWorkDesc());
            return stringBuilder.ToString();
        }
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return pawns;
    }
    public virtual void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref pawns, "pawns", this);
        Scribe_Values.Look(ref curName, "curName");
        Scribe_References.Look(ref associatedWorldObject, "associatedWorldObject");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            TrySetAssociatedInterface();
        }
    }
}