using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 大地图事件点：关联多个派系的事件点。
/// </summary>
public abstract class WorldObject_WithMutiFactions : WorldObject_InteractiveBase
{
    protected List<Faction> participantFactions = [];
    public List<Faction> ParticipantFactions => participantFactions;

    /// <summary>
    /// 设置参与派系。
    /// </summary>
    public void SetParticipantFactions(IEnumerable<Faction> newPaFactions)
    {
        participantFactions.Clear();
        AddParticipantFactions(newPaFactions);
    }

    /// <summary>
    /// 添加参与派系。
    /// </summary>
    public void AddParticipantFaction(Faction newPaFaction)
    {
        if (newPaFaction is not null)
        {
            participantFactions.Add(newPaFaction);
        }
    }

    /// <summary>
    /// 批量添加参与派系。
    /// </summary>
    public void AddParticipantFactions(IEnumerable<Faction> newPaFactions)
    {
        participantFactions ??= [];
        foreach (Faction faction in newPaFactions)
        {
            participantFactions.AddUnique(faction);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref participantFactions, "participantFactions", LookMode.Reference);
    }
}