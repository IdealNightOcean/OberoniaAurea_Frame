﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

//大地图事件点：关联多个派系的事件点
public abstract class WorldObject_WithMutiFactions : WorldObject_InteractiveBase
{
    protected List<Faction> participantFactions = [];
    public List<Faction> ParticipantFactions => participantFactions;

    public void SetParticipantFactions(List<Faction> newPaFactions)
    {
        participantFactions.Clear();
        AddParticipantFactions(newPaFactions);
    }

    public void AddParticipantFaction(Faction newPaFaction)
    {
        if (newPaFaction is not null)
        {
            participantFactions.Add(newPaFaction);
        }
    }
    public void AddParticipantFactions(List<Faction> newPaFactions)
    {
        participantFactions.AddRange(newPaFactions);
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref participantFactions, "participantFactions", LookMode.Reference);
    }
}