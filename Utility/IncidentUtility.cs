using RimWorld;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame.Utility;

/// <summary>
/// 事件（<see cref="IncidentDef"/>）相关的扩展工具方法
/// </summary>
public static class OAFrame_IncidentUtility
{
    /// <summary>
    /// 尝试立刻触发事件
    /// </summary>
    /// <param name="incidentDef">要触发的事件定义</param>
    /// <param name="parms">事件参数</param>
    /// <param name="force">是否跳过 <see cref="IncidentWorker.CanFireNow"/> 检查强制执行</param>
    /// <returns>事件成功执行返回 <see langword="true"/>；未强制执行且当前不可触发时返回 <see langword="false"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFireIncidentNow(this IncidentDef incidentDef, IncidentParms parms, bool force = false)
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
    /// <param name="incidentDef">要加入故事讲述者队列的事件定义</param>
    /// <param name="delayTicks">相对当前游戏刻的延迟刻数，事件将在该刻数后触发</param>
    /// <param name="parms">事件参数，不能为 <see langword="null"/>，且其 <see cref="IncidentParms.target"/> 不能为 <see langword="null"/></param>
    /// <param name="retryDurationTicks">事件到时无法触发时的重试持续刻数，0 表示不重试</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddNewQueuedIncident(this IncidentDef incidentDef, int delayTicks, IncidentParms parms, int retryDurationTicks = 0)
    {
        if (parms is null)
        {
            Log.Error($"[OAFrame] 尝试添加队列事件，但 {nameof(IncidentParms)} 为空。");
            return;
        }
        if (parms.target is null)
        {
            Log.Error($"[OAFrame] 尝试添加队列事件，但 {nameof(IncidentParms)}.{nameof(IncidentParms.target)} 为空。");
            return;
        }
        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + delayTicks, parms, retryDurationTicks);
    }
}
