using System;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame.Utility;

/// <summary>
/// 游戏玩法相关的通用工具方法
/// </summary>
public static class GamePlayUtility
{
    /// <summary>
    /// 是否处于上帝模式（同时要求开启开发者模式）
    /// </summary>
    public static bool GodMode => DebugSettings.godMode && Prefs.DevMode;

    /// <summary>
    /// 构造单例前验证实例尚不存在；若 <paramref name="instance"/> 不为 <see langword="null"/> 则抛出 <see cref="InvalidOperationException"/>
    /// </summary>
    /// <typeparam name="T">单例类型</typeparam>
    /// <param name="instance">构造前持有的单例实例，必须为 <see langword="null"/></param>
    /// <param name="instanceName">单例实例的字段或属性名称，用于拼接异常信息</param>
    /// <exception cref="InvalidOperationException">当 <paramref name="instance"/> 已存在时抛出</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateSingleton<T>(T instance, string instanceName) where T : class
    {
        if (instance is not null)
        {
            throw new InvalidOperationException($"[OAFrame]构造时 {instanceName} 已不为空。{typeof(T).Name} 是单例类型，请直接使用 {instanceName}，不要重复创建实例。");
        }
    }
}
