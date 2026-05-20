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
    /// <summary>
    /// 检查两个<see cref="Def"/>是否相同且非空。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSameDefNonNullable<T>(this T def, T other) where T : Def => def is not null && def == other;

    /// <summary>
    /// 尝试立刻触发事件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFireIncidentNow(IncidentDef incidentDef, IncidentParms parms, bool force = false)
    {
        if (force || incidentDef.Worker.CanFireNow(parms))
        {
            return incidentDef.Worker.TryExecute(parms);
        }
        return false;
    }

    /// <summary>
    /// 添加队列事件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddNewQueuedIncident(IncidentDef incidentDef, int delayTicks, IncidentParms parms, int retryDurationTicks = 0)
    {
        if (parms is null)
        {
            Log.Error($"Try add a new queued incident,but {nameof(IncidentParms)} is NULL.");
            return;
        }
        if (parms.target is null)
        {
            Log.Error($"Try add a new queued incident,but {nameof(IncidentParms)}.{nameof(IncidentParms.target)} is NULL.");
            return;
        }
        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + delayTicks, parms, retryDurationTicks);
    }

    /// <summary>
    /// 创建物品。
    /// </summary>
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

    /// <summary>
    /// 创建带颜色和符号的整数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredIntNamedArgument(int value, string name, bool reverse = false)
    {
        return value.ToStringWithSign().Colorize((reverse ^ value < 0) ? ColorLibrary.RedReadable : Color.green).Named(name);
    }

    /// <summary>
    /// 创建带颜色和符号的浮点数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredFloatNamedArgument(float value, string name, string format = "0.##", bool reverse = false)
    {
        return value.ToStringWithSign(format).Colorize((reverse ^ value < 0f) ? ColorLibrary.RedReadable : Color.green).Named(name);
    }

    /// <summary>
    /// 创建带颜色和符号的百分比数命名参数。默认正数显示绿色，负数显示红色。
    /// </summary>
    /// <param name="reverse">是否反转颜色逻辑。为 <see langword="true"/> 时正数显示红色，负数显示绿色。</param>
    public static NamedArgument ColoredPercentNamedArgument(float value, string name, string format = "0.##", bool reverse = false)
    {
        return value.ToStringPercentSigned(format).Colorize((reverse ^ value < 0f) ? ColorLibrary.RedReadable : Color.green).Named(name);
    }

    /// <summary>
    /// 验证单例是否为空。
    /// </summary>
    public static void ValidateSingleton<T>(T instance, string instanceName) where T : class
    {
        if (instance is not null)
        {
            throw new InvalidOperationException($"{instanceName} is not null when constructing. {typeof(T).Name} is a simple singleton. Use {instanceName} instead of creating new instance.");
        }
    }
}