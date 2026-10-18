using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：为世界对象设置多个派系。 
/// </summary>
public class QuestNode_SetMutiFactionsForWorldObject : QuestNode
{
    /// <summary>
    /// 世界对象。
    /// </summary>
    public SlateRef<WorldObject> worldObject;
    /// <summary>
    /// 参与派系。
    /// </summary>
    public SlateRef<IEnumerable<Faction>> participantFactions;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }
    /// <inheritdoc/>
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