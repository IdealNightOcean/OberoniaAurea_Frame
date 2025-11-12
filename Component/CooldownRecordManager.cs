using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea_Frame;

public class CooldownRecordManager : IExposable
{
    private Dictionary<string, CooldownRecord> records = [];

    public void ExposeData()
    {

        Scribe_Collections.Look(ref records, "records", LookMode.Value, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            records ??= [];
            records.RemoveAll(kv => kv.Key is null || kv.Value.ShouldRemove);
        }
    }

    public bool HasRecordOfKey(string key)
    {
        return records.ContainsKey(key);
    }

    public void RegisterRecord(string key, int cdTicks, bool removeWhenExpired = false)
    {
        if (key.NullOrEmpty())
        {
            Log.Error("Trt register a CooldownRecord with a null or empty string key.");
            return;
        }

        records[key] = new CooldownRecord(cdTicks, removeWhenExpired);
    }

    public void DeregisterRecord(string key)
    {
        records.Remove(key);
    }

    public bool IsInCooldown(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.IsInCooldown;
        }
        return false;
    }

    public int GetCooldownTicksLeft(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.CooldownTicksLeft;
        }
        return -1;
    }

    public int GetTicksSinceLastActive(string key)
    {
        if (records.TryGetValue(key, out CooldownRecord record))
        {
            return record.TicksSinceLastActive;
        }
        return -1;
    }

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