using OberoniaAurea_Frame.Utility;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：触发事件。 
/// </summary>
public class QuestNode_FireIncident : QuestNode
{
    /// <summary>
    /// 触发事件的信号。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> inSignal;

    /// <summary>
    /// 事件定义。
    /// </summary>
    public SlateRef<IncidentDef> incidentDef;

    /// <summary>
    /// 事件参数。
    /// </summary>
    public SlateRef<IncidentParms> parms;

    /// <summary>
    /// 是否为世界事件。
    /// </summary>
    public SlateRef<bool> worldIncident;

    /// <summary>
    /// 目标地图父对象。
    /// </summary>
    public SlateRef<MapParent> mapParent;

    /// <summary>
    /// 最小事件点数。
    /// </summary>
    public SlateRef<float> minPoints = -1f;

    /// <summary>
    /// 最大事件点数。
    /// </summary>
    public SlateRef<float> maxPoints = 99999f;

    /// <summary>
    /// 当前点数倍率。
    /// </summary>
    public SlateRef<float> currentPointsFactor = 1f;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        if (incidentDef.GetValue(slate) is null)
        {
            return false;
        }
        if (minPoints.GetValue(slate) > maxPoints.GetValue(slate))
        {
            return false;
        }
        if (currentPointsFactor.GetValue(slate) <= 0f)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 解析事件参数。
    /// </summary>
    /// <param name="slate">当前石板上下文。</param>
    /// <returns>解析后的事件参数。</returns>
    protected virtual IncidentParms ResolveParms(Slate slate)
    {
        IncidentParms parms = this.parms.GetValue(slate);
        parms ??= new IncidentParms();
        return parms;
    }

    /// <summary>
    /// 解析事件触发目标是否有效。
    /// </summary>
    /// <param name="slate">当前石板上下文。</param>
    /// <returns>目标是否有效。</returns>
    protected bool ResolveIncidentTarget(Slate slate)
    {
        if (!worldIncident.GetValue(slate))
        {
            if (mapParent.GetValue(slate) is not null)
            {
                return true;
            }
            else if (parms.GetValue(slate) is not null)
            {
                return parms.GetValue(slate).target is Map;
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!ResolveIncidentTarget(slate))
        {
            return;
        }
        IncidentParms incidentParms = ResolveParms(slate);
        QuestPart_FireIncident questPart_FireIncident = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            incident = incidentDef.GetValue(slate),
            minPoints = minPoints.GetValue(slate),
            maxPoints = maxPoints.GetValue(slate),
            currentPointsFactor = currentPointsFactor.GetValue(slate),
        };
        if (worldIncident.GetValue(slate))
        {
            questPart_FireIncident.SetIncidentParms_World(incidentParms);
        }
        else
        {
            questPart_FireIncident.SetIncidentParms_MapParent(incidentParms, mapParent.GetValue(slate));
        }
        QuestGen.quest.AddPart(questPart_FireIncident);
    }
}

/// <summary> 
/// 任务部件：触发事件。 
/// </summary>
public class QuestPart_FireIncident : QuestPart
{
    /// <summary>
    /// 触发事件的信号。
    /// </summary>
    public string inSignal;

    /// <summary>
    /// 事件定义。
    /// </summary>
    public IncidentDef incident;

    /// <summary>
    /// 事件参数。
    /// </summary>
    protected IncidentParms incidentParms;

    /// <summary>
    /// 当前点数倍率。
    /// </summary>
    public float currentPointsFactor = 1f;

    /// <summary>
    /// 最小事件点数。
    /// </summary>
    public float minPoints = -1f;

    /// <summary>
    /// 最大事件点数。
    /// </summary>
    public float maxPoints = 99999f;

    /// <summary>
    /// 是否为世界事件。
    /// </summary>
    protected bool worldIncident;

    /// <summary>
    /// 目标地图父对象。
    /// </summary>
    protected MapParent mapParent;



    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (signal.tag != inSignal || incidentParms is null)
        {
            return;
        }
        if (worldIncident)
        {
            ResolveParms_World();
            OAFrame_IncidentUtility.TryFireIncidentNow(incident, incidentParms);
            incidentParms.target = Find.World;
        }
        else if (mapParent is not null && mapParent.HasMap)
        {
            Map targetMap = mapParent.Map;
            ResolveParms_Map(targetMap);
            OAFrame_IncidentUtility.TryFireIncidentNow(incident, incidentParms);
            incidentParms.target = null;
        }
    }

    /// <summary>
    /// 解析地图事件参数。
    /// </summary>
    /// <param name="map">目标地图。</param>
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

    /// <summary>
    /// 解析世界事件参数
    /// </summary>
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

    /// <summary>
    /// 设置地图类型事件参数
    /// </summary>
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

    /// <summary>
    /// 设置世界类型事件参数
    /// </summary>
    public void SetIncidentParms_World(IncidentParms parms)
    {
        incidentParms = parms;
        incidentParms.target = Find.World;
        worldIncident = true;
    }

    /// <inheritdoc/>
    public override void Cleanup()
    {
        base.Cleanup();
        inSignal = null;
        incident = null;
        incidentParms = null;
        currentPointsFactor = 1f;
        minPoints = -1f;
        maxPoints = 99999f;
        mapParent = null;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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