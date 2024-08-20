using RimWorld.Planet;
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

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref associateWorldObject, "associateWorldObject");
    }
}