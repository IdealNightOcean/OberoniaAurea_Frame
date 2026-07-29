using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 冷却记录管理器。
/// </summary>
public class CooldownRecordManager : IExposable
{
    private Dictionary<string, CooldownRecord> records = [];

    /// <summary>
    /// 序列化/反序列化此对象需持久保存的字段。
    /// </summary>
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
    /// <param name="key">冷却记录的键</param>
    /// <returns>是否存在对应的冷却记录</returns>
    public bool HasRecordOfKey(string key)
    {
        return records.ContainsKey(key);
    }

    /// <summary>
    /// 注册冷却记录。
    /// </summary>
    /// <param name="key">冷却记录的键</param>
    /// <param name="cdTicks">冷却时长（Tick数）</param>
    /// <param name="removeWhenExpired">过期后是否自动移除</param>
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
    /// <param name="key">冷却记录的键</param>
    public void DeregisterRecord(string key)
    {
        records.Remove(key);
    }

    /// <summary>
    /// 检查指定冷却记录是否处于冷却中。
    /// </summary>
    /// <param name="key">冷却记录的键</param>
    /// <returns>是否处于冷却中</returns>
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
    /// <param name="key">冷却记录的键</param>
    /// <returns>剩余冷却时刻数；不存在时返回 -1</returns>
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
    /// <param name="key">冷却记录的键</param>
    /// <returns>距上次激活的时刻数；不存在时返回 -1</returns>
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
    /// <returns>冷却记录详情字符串</returns>
    public string GetCDRecordsDetailInfo()
    {
        if (records.NullOrEmpty())
        {
            return "None";
        }

        StringBuilder detailBuilder = new();
        int i = 0;
        foreach (KeyValuePair<string, CooldownRecord> kv in records)
        {
            detailBuilder.AppendInNewLine($"{++i}. ({kv.Key}: {kv.Value})");
        }
        return detailBuilder.ToString();
    }
}