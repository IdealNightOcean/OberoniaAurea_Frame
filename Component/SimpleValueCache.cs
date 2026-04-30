using System;
using Verse;

namespace OberoniaAurea_Frame;

public struct SimpleValueCache<T> where T : unmanaged
{
    private T cachedResult;
    private readonly T defaultValue;

    private int nextCheckTick;
    private readonly int cacheInterval;

    private readonly Func<T> checker;

    /// <summary>
    /// 使用缓存间隔和检查器初始化值缓存。
    /// </summary>
    public SimpleValueCache(int cacheInterval, Func<T> checker)
    {
        defaultValue = default;
        cachedResult = default;
        nextCheckTick = -1;

        this.cacheInterval = cacheInterval;
        this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
    }

    /// <summary>
    /// 使用缓存间隔、默认值和检查器初始化值缓存。
    /// </summary>
    public SimpleValueCache(int cacheInterval, T defaultValue, Func<T> checker)
    {
        this.defaultValue = defaultValue;
        cachedResult = defaultValue;
        nextCheckTick = -1;

        this.cacheInterval = cacheInterval;
        this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
    }

    /// <summary>
    /// 重置缓存。
    /// </summary>
    public void Reset()
    {
        cachedResult = defaultValue;
        nextCheckTick = -1;
    }

    /// <summary>
    /// 获取缓存结果。
    /// </summary>
    public T GetCachedResult()
    {
        if (Find.TickManager.TicksGame > nextCheckTick)
        {
            nextCheckTick = Find.TickManager.TicksGame + cacheInterval;
            try
            {
                cachedResult = checker();
            }
            catch
            {
                return defaultValue;
            }
        }

        return cachedResult;
    }
}