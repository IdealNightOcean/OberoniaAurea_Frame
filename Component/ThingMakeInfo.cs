using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 物品制作信息，记录要制作的物品及其数量、品质和材料。
/// </summary>
public class ThingMakeInfo : IExposable
{
    private ThingDef thingDef;
    /// <summary>
    /// 物品定义
    /// </summary>
    public ThingDef ThingDef
    {
        get => thingDef;
        set => thingDef = value;
    }

    private int count;
    /// <summary>
    /// 物品数量
    /// </summary>
    public int Count
    {
        get => count;
        set => count = Mathf.Max(0, value);
    }

    private QualityCategory? quality;
    /// <summary>
    /// 品质（可为 <see langword="null"/>）
    /// </summary>
    public QualityCategory? Quality
    {
        get => quality;
        set => quality = value;
    }

    private ThingDef stuffDef;
    /// <summary>
    /// 材料定义（可为 <see langword="null"/>）
    /// </summary>
    public ThingDef StuffDef
    {
        get => stuffDef;
        set => stuffDef = value;
    }

    /// <summary>
    /// 默认构造。
    /// </summary>
    public ThingMakeInfo() { }

    /// <summary>
    /// 构造，指定物品定义和数量。
    /// </summary>
    public ThingMakeInfo(ThingDef thingDef, int count)
    {
        this.ThingDef = thingDef;
        this.Count = count;
    }

    /// <summary>
    /// 构造，指定物品定义、数量、品质和材料。
    /// </summary>
    public ThingMakeInfo(ThingDef thingDef, int count, QualityCategory quality, ThingDef stuffDef)
    {
        this.ThingDef = thingDef;
        this.Count = count;
        this.Quality = quality;
        this.StuffDef = stuffDef;
    }

    /// <summary>
    /// 数据的保存与读取。
    /// </summary>
    public void ExposeData()
    {
        Scribe_Defs.Look(ref thingDef, nameof(thingDef));
        Scribe_Values.Look(ref count, nameof(count), defaultValue: 0);
        Scribe_Values.Look(ref quality, nameof(quality));
        Scribe_Defs.Look(ref stuffDef, nameof(stuffDef));
    }
}
