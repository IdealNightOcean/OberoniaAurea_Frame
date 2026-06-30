using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：生成包含多个派系的世界对象。 
/// </summary>
public class QuestNode_GenerateWorldObjectWithMutiFactions : QuestNode_GenerateInteractiveWorldObject
{
    public SlateRef<IEnumerable<Faction>> participantFactions;

    protected override WorldObject_InteractiveBase GenerateWorldObject(Slate slate)
    {
        WorldObject_InteractiveBase worldObject = (WorldObject_InteractiveBase)WorldObjectMaker.MakeWorldObject(def.GetValue(slate));

        IEnumerable<Faction> participantFactions = this.participantFactions.GetValue(slate);
        if (participantFactions is not null)
        {
            worldObject.ParticipantFactions.AddRange(participantFactions);
        }

        return worldObject;
    }
}