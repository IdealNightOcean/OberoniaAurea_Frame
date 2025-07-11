using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;

namespace OberoniaAurea_Frame;

public class QuestNode_GenerateWorldObjectWithMutiFactions : QuestNode_GenerateInteractiveWorldObject
{
    public SlateRef<IEnumerable<Faction>> participantFactions;

    protected override WorldObject_InteractiveBase GenerateWorldObject(Slate slate)
    {
        WorldObject_WithMutiFactions worldObject = (WorldObject_WithMutiFactions)WorldObjectMaker.MakeWorldObject(def.GetValue(slate));

        IEnumerable<Faction> participantFactions = this.participantFactions.GetValue(slate);
        if (participantFactions is not null)
        {
            worldObject.AddParticipantFactions(participantFactions);
        }

        return worldObject;
    }
}
