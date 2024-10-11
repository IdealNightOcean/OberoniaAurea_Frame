﻿using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_InteractiveBase : WorldObject
{
    protected WorldObject associateWorldObject;
    public WorldObject AssociateWorldObject
    {
        get { return associateWorldObject; }
        set
        {
            if (value != null)
            {
                associateWorldObject = value;
            }
        }
    }
    private Material cachedMat;
    public override Material Material
    {
        get
        {
            if (cachedMat == null)
            {
                cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : base.Faction.Color, texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
            }
            return cachedMat;
        }
    }
    public virtual void Notify_CaravanArrived(Caravan caravan) { }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption2 in GetSpecificFloatMenuOptions(caravan))
        {
            yield return floatMenuOption2;
        }
    }
    public virtual IEnumerable<FloatMenuOption> GetSpecificFloatMenuOptions(Caravan caravan)
    {
        return CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
    }
}