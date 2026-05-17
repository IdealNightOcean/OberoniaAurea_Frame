using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;

namespace OberoniaAurea_Frame;

public class QuestNode_SetMutiFactionsForWorldObject : QuestNode
{
    public SlateRef<WorldObject> worldObject;
    public SlateRef<IEnumerable<Faction>> participantFactions;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (worldObject.GetValue(slate) is WorldObject_InteractiveBase mutiFactionsObj)
        {
            IEnumerable<Faction> participantFactions = this.participantFactions.GetValue(slate);
            if (participantFactions is not null)
            {
                mutiFactionsObj.ParticipantFactions.AddRange(participantFactions);
            }
        }
    }
}