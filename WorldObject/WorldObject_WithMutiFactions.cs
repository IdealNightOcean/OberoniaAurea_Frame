using RimWorld;
using System;
using System.Collections.Generic;

namespace OberoniaAurea_Frame;

/// <summary>
/// 大地图事件点：关联多个派系的事件点。
/// </summary>
/// <remarks>此类已废弃，功能已合并至 <see cref="WorldObject_InteractiveBase"></see></remarks>
[Obsolete("此类已废弃，功能已合并至 WorldObject_InteractiveBase")]
public abstract class WorldObject_WithMutiFactions : WorldObject_InteractiveBase
{
    /// <summary>
    /// 设置参与派系
    /// </summary>
    public void SetParticipantFactions(IEnumerable<Faction> newPaFactions)
    {
        if (newPaFactions is not null)
            participantFactions.AddRange(newPaFactions);
    }

    /// <summary>
    /// 添加参与派系
    /// </summary>
    public void AddParticipantFaction(Faction newPaFaction)
    {
        if (newPaFaction is not null)
        {
            participantFactions.Add(newPaFaction);
        }
    }

    /// <summary>
    /// 批量添加参与派系
    /// </summary>
    public void AddParticipantFactions(IEnumerable<Faction> newPaFactions)
    {
        if (newPaFactions is not null)
            participantFactions.AddRange(newPaFactions);
    }

}