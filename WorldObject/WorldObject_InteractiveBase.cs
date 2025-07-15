using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class WorldObject_InteractiveBase : WorldObject, ICaravanAssociate, IQuestAssociate
{
    protected virtual string VisitLabel => null;
    protected Quest quest;
    public Quest AssociatedQuest => quest;

    private Material cachedMat;
    public override Material Material
    {
        get
        {
            cachedMat ??= MaterialPool.MatFrom(color: (Faction is null) ? Color.white : Faction.Color, texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
            return cachedMat;
        }
    }

    public void SetAssociatedQuest(Quest quest)
    {
        this.quest = quest;
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
        return CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this, VisitLabel);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref quest, "quest");
    }
}