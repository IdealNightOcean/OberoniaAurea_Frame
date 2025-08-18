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

    public SimpleValueCache(int cacheInterval, Func<T> checker)
    {
        defaultValue = default;
        cachedResult = default;
        nextCheckTick = -1;

        this.cacheInterval = cacheInterval;
        this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
    }

    public SimpleValueCache(int cacheInterval, T defaultValue, Func<T> checker)
    {
        this.defaultValue = defaultValue;
        cachedResult = defaultValue;
        nextCheckTick = -1;

        this.cacheInterval = cacheInterval;
        this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
    }

    public void Reset()
    {
        cachedResult = defaultValue;
        nextCheckTick = -1;
    }

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