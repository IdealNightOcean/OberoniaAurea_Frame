using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea_Frame;

public class CooldownRecordManager : IExposable
{
    private Dictionary<string, CooldownRecord> records = [];

    public void ExposeData()
    {

        Scribe_Collections.Look(ref records, nameof(records), LookMode.Value, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            records ??= [];
            records.RemoveAll(kv => kv.Key is null || kv.Value.ShouldRemove);
        }
    }

    /// <summary>
    /// 检查冷却记录是否存在。
    /// </summary>
    public bool HasRecordOfKey(string key)
    {
        return records.ContainsKey(key);
    }

    /// <summary>
    /// 注册冷却记录。
    /// </summary>
    public void RegisterRecord(string key, int cdTicks, bool removeWhenExpired = false)
    {
        if (key.NullOrEmpty())
        {
            Log.Error("Trt register a CooldownRecord with a null or empty string key.");
            return;
        }

        records[key] = new CooldownRecord(cdTicks, removeWhenExpired);
    }

    /// <summary>
    /// 注销冷却记录。
    /// </summary>
    public void DeregisterRecord(string key)
    {
        records.Remove(key);
    }

    /// <summary>
    /// 检查指定冷却记录是否处于冷却中。
    /// </summary>
    public bool IsInCooldown(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.IsInCooldown;
        }
        return false;
    }

    /// <summary>
    /// 获取剩余冷却时间。
    /// </summary>
    public int GetCooldownTicksLeft(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.CooldownTicksLeft;
        }
        return -1;
    }

    /// <summary>
    /// 获取距上次激活的时间。
    /// </summary>
    public int GetTicksSinceLastActive(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.TicksSinceLastActive;
        }
        return -1;
    }

    /// <summary>
    /// 获取冷却记录详情信息。
    /// </summary>
    public string GetCDRecordsDetailInfo()
    {
        if (records.NullOrEmpty())
        {
            return "None";
        }

        StringBuilder sb = new();
        int i = 0;
        foreach (KeyValuePair<string, CooldownRecord> kv in records)
        {
            sb.AppendInNewLine($"{++i}. ({kv.Key}: {kv.Value})");
        }
        return sb.ToString();
    }
}