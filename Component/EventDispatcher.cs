using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 一个简单事件分发器
/// 用于注册、注销和触发事件
/// </summary>
/// <remarks>
/// <para>- 该分发器单个事件调用出现异常时不会中断调用链</para>
/// <para>- 线程不安全</para>
/// </remarks>
public class EventDispatcher<TDelegate> where TDelegate : Delegate
{
    private readonly List<TDelegate> handlers = [];

    /// <summary>
    /// 注册事件处理器
    /// </summary>
    /// <param name="handler">要注册的处理器委托</param>
    /// <returns>注册成功返回 <see langword="true"/>；<paramref name="handler"/> 为 <see langword="null"/> 或已存在时返回 <see langword="false"/></returns>
    public bool Register(TDelegate handler)
    {
        if (handler is null)
            return false;
        if (handlers.Contains(handler))
            return false;

        handlers.Add(handler);
        return true;
    }

    /// <summary>
    /// 注销事件处理器
    /// </summary>
    /// <param name="handler">要注销的处理器委托</param>
    /// <returns>成功移除返回 <see langword="true"/>；处理器未注册时返回 <see langword="false"/></returns>
    public bool Deregister(TDelegate handler) => handlers.Remove(handler);

    /// <summary>
    /// 触发事件，遍历调用所有已注册的处理器
    /// </summary>
    /// <remarks>
    /// <para>- 外部调用者可以在调用过程中被修改内部委托列表，此时会导致异常</para>
    /// <para>- 线程不安全</para>
    /// </remarks>
    public void Raise(Action<TDelegate> invoker)
    {
        if (invoker is null || handlers.Count == 0)
            return;

        foreach (TDelegate handler in handlers)
        {
            try
            {
                invoker(handler);
            }
            catch (Exception ex)
            {
                Log.Error($"[OAFrame] 事件处理器 {handler.Method.DeclaringType?.Name}.{handler.Method.Name} 执行时出现异常，已跳过该处理器并继续调用链，异常：{ex}");
            }
        }
    }

    /// <summary>
    /// 触发事件，通过快照遍历调用所有已注册的处理器，允许在调用过程中修改处理器列表
    /// </summary>
    /// <remarks>
    /// <para>- 线程不安全</para>
    /// </remarks>
    public void RaiseSafe(Action<TDelegate> invoker)
    {
        if (invoker is null || handlers.Count == 0)
            return;

        TDelegate[] snapshot = [.. handlers];
        foreach (TDelegate handler in snapshot)
        {
            try
            {
                invoker(handler);
            }
            catch (Exception ex)
            {
                Log.Error($"[OAFrame] 事件处理器 {handler.Method.DeclaringType?.Name}.{handler.Method.Name} 执行时出现异常，已跳过该处理器并继续调用后续处理器，异常：{ex}");
            }
        }
    }
}