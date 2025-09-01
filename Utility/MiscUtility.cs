using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_MiscUtility
{
    //尝试立刻触发事件
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFireIncidentNow(IncidentDef incidentDef, IncidentParms parms, bool force = false)
    {
        if (force || incidentDef.Worker.CanFireNow(parms))
        {
            return incidentDef.Worker.TryExecute(parms);
        }
        return false;
    }

    //添加队列事件
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddNewQueuedIncident(IncidentDef incidentDef, int delayTicks, IncidentParms parms, int retryDurationTicks = 0)
    {
        if (parms is null)
        {
            Log.Error("Try add a new queued incident,but IncidentParms is NULL.");
            return;
        }
        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + delayTicks, parms, retryDurationTicks);
    }

    //创建物品
    public static List<Thing> TryGenerateThing(ThingDef def, int count)
    {
        List<Thing> list = [];
        int stackLimit = def.stackLimit;
        int remaining = count;
        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = Mathf.Min(remaining, stackLimit);
            list.Add(thing);
            remaining -= stackLimit;
        }
        return list;
    }

    public static void ValidateSingleton<T>(T instance, string instanceName) where T : class
    {
        if (instance is not null)
        {
            throw new InvalidOperationException($"{instanceName} is not null when constructing. {typeof(T).Name} is a simple singleton. Use {instanceName} instead of creating new instance.");
        }
    }
}