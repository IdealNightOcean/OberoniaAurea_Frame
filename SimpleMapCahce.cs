using System;
using Verse;

namespace OberoniaAurea_Frame;

public struct SimpleMapCahce<T> where T : struct
{
    private Map cachedMap;
    private readonly bool onlyPlayerHome;

    private T cachedResult;
    private readonly T defaultValue;

    private int nextCheckTick;
    private readonly int cacheInterval;

    private readonly Func<Map, T> checker;

    public SimpleMapCahce(int cacheInterval, T defaultValue, bool onlyPlayerHome, Func<Map, T> checker)
    {
        cachedMap = null;
        cachedResult = defaultValue;
        this.defaultValue = defaultValue;
        nextCheckTick = -1;

        this.onlyPlayerHome = onlyPlayerHome;
        this.cacheInterval = cacheInterval;
        this.checker = checker;
    }

    public void Reset()
    {
        cachedMap = null;
        cachedResult = defaultValue;
        nextCheckTick = -1;
    }

    public T GetCachedResult(Map map)
    {
        if (map is null || (onlyPlayerHome && !map.IsPlayerHome))
        {
            return defaultValue;
        }

        try
        {
            if (map == cachedMap)
            {
                if (Find.TickManager.TicksGame > nextCheckTick)
                {
                    cachedResult = checker(map);
                    nextCheckTick = Find.TickManager.TicksGame + cacheInterval;
                }
            }
            else
            {
                cachedMap = map;
                cachedResult = checker(map);
                nextCheckTick = Find.TickManager.TicksGame + cacheInterval;
            }
        }
        catch
        {
            return defaultValue;
        }

        return cachedResult;
    }

}