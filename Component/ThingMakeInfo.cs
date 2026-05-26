using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 物品制作信息，记录要制作的物品及其数量、品质和材料。
/// </summary>
public class ThingMakeInfo : IExposable
{
    /// <summary>
    /// 物品定义
    /// </summary>
    public ThingDef ThingDef;

    /// <summary>
    /// 物品数量
    /// </summary>
    public int Count;

    /// <summary>
    /// 品质（可为 <see langword="null"/>）
    /// </summary>
    public QualityCategory? Quality;

    /// <summary>
    /// 材料定义（可为 <see langword="null"/>）
    /// </summary>
    public ThingDef StuffDef;

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
        Scribe_Defs.Look(ref ThingDef, nameof(ThingDef));
        Scribe_Values.Look(ref Count, nameof(Count), defaultValue: 0);
        Scribe_Values.Look(ref Quality, nameof(Quality));
        Scribe_Defs.Look(ref StuffDef, nameof(StuffDef));
    }
}
