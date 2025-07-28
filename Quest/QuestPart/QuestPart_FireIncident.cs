using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestPart_FireIncident : QuestPart
{
    public string inSignal;

    public IncidentDef incident;
    protected IncidentParms incidentParms;
    public float currentPointsFactor = 1f;
    public float minPoints = -1f;
    public float maxPoints = 99999f;

    protected bool worldIncident;
    protected MapParent mapParent;

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
            {
                yield return questLookTarget;
            }
            if (mapParent is not null)
            {
                yield return mapParent;
            }
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (signal.tag != inSignal || incidentParms is null)
        {
            return;
        }
        if (worldIncident)
        {
            ResolveParms_World();
            OAFrame_MiscUtility.TryFireIncidentNow(incident, incidentParms);
            incidentParms.target = Find.World;
        }
        else if (mapParent is not null && mapParent.HasMap)
        {
            Map targetMap = mapParent.Map;
            ResolveParms_Map(targetMap);
            OAFrame_MiscUtility.TryFireIncidentNow(incident, incidentParms);
            incidentParms.target = null;
        }
    }

    protected void ResolveParms_Map(Map map)
    {
        incidentParms.forced = true;
        incidentParms.quest = quest;
        incidentParms.target = map;
        float points = incidentParms.points;
        if (points < 0f)
        {
            points = StorytellerUtility.DefaultThreatPointsNow(map) * currentPointsFactor;
        }
        if (maxPoints >= minPoints)
        {
            float trueMinPoints = Mathf.Max(minPoints, 0f);
            points = Mathf.Clamp(points, trueMinPoints, maxPoints);
        }
        incidentParms.points = points;
    }
    protected void ResolveParms_World()
    {
        incidentParms.forced = true;
        incidentParms.quest = quest;
        incidentParms.target = Find.World;
        float points = incidentParms.points;
        if (points < 0f)
        {
            points = StorytellerUtility.DefaultThreatPointsNow(Find.World) * currentPointsFactor;
        }
        if (maxPoints >= minPoints)
        {
            float trueMinPoints = Mathf.Max(minPoints, 0f);
            points = Mathf.Clamp(points, trueMinPoints, maxPoints);
        }
        incidentParms.points = points;
    }

    public void SetIncidentParms_MapParent(IncidentParms parms, MapParent mapParent = null)
    {
        incidentParms = parms;
        if (incidentParms.target is Map map && map.Parent is not null)
        {
            this.mapParent = map.Parent;
            incidentParms.target = null;
        }
        else
        {
            this.mapParent = mapParent;
        }
        worldIncident = false;
    }
    public void SetIncidentParms_World(IncidentParms parms)
    {
        incidentParms = parms;
        incidentParms.target = Find.World;
        worldIncident = true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Defs.Look(ref incident, "incident");
        Scribe_Deep.Look(ref incidentParms, "incidentParms");
        Scribe_Values.Look(ref currentPointsFactor, "currentPointsFactor", 1f);
        Scribe_Values.Look(ref minPoints, "minPoints", -1f);
        Scribe_Values.Look(ref maxPoints, "maxPoints", 99999f);
        Scribe_Values.Look(ref worldIncident, "worldIncident", defaultValue: false);
        Scribe_References.Look(ref mapParent, "mapParent");
    }

    public override void AssignDebugData()
    {
        base.AssignDebugData();
        inSignal = "DebugSignal" + Rand.Int;
        if (Find.AnyPlayerHomeMap is not null)
        {
            incident = IncidentDefOf.RaidEnemy;
            IncidentParms incidentParms = new()
            {
                target = Find.RandomPlayerHomeMap,
                points = 500f
            };
            SetIncidentParms_MapParent(incidentParms);
        }
    }
}

