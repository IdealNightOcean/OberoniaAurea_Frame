using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 用于配置给予<see cref="Hediff"/>的参数集合。
/// </summary>
public class HediffGiveParams : IExposable
{

    /// <summary>
    /// 要给予的<see cref="HediffDef"/>。
    /// </summary>
    public HediffDef HediffToGive;

    /// <summary>
    /// 要给予<see cref="Hediff"/>的目标身体部位。为 <see langword="null"/> 时使用全身。
    /// </summary>
    public BodyPartRecord BodyPartRecordToGive;

    /// <summary>
    /// 初始严重度。小于等于0时使用<see cref="Hediff"/>自身的默认值。
    /// </summary>
    public float InitSeverity = -1f;

    /// <summary>
    /// 当目标已有同类型<see cref="Hediff"/>时，增加其严重度。为 <see langword="null"/> 时不做调整。
    /// </summary>
    public float AddSeverityIfExist;

    /// <summary>
    /// 覆盖<see cref="Hediff"/>的消失时间（Tick数）。小于等于0时使用<see cref="Hediff"/>自身的默认值。
    /// </summary>
    public int OverrideDisappearTicks = -1;

    /// <summary>
    /// 创建一个新的<see cref="Hediff"/>给予参数实例。
    /// </summary>
    public HediffGiveParams() { }
    /// <summary>
    /// 使用指定的<see cref="HediffDef"/>创建一个新的<see cref="Hediff"/>给予参数实例。
    /// </summary>
    /// <param name="hediffToGive">要给予的<see cref="HediffDef"/>。</param>
    public HediffGiveParams(HediffDef hediffToGive)
    {
        this.HediffToGive = hediffToGive;
    }

    /// <summary>
    /// 创建此对象的浅表副本。
    /// </summary>
    public HediffGiveParams ShallowCopy() => (HediffGiveParams)MemberwiseClone();

    /// <summary>
    /// 序列化/反序列化此对象需持久保存的字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_Defs.Look(ref HediffToGive, nameof(HediffToGive));

        Scribe_BodyParts.Look(ref BodyPartRecordToGive, nameof(BodyPartRecordToGive));
        Scribe_Values.Look(ref InitSeverity, nameof(InitSeverity), defaultValue: -1f);
        Scribe_Values.Look(ref OverrideDisappearTicks, nameof(OverrideDisappearTicks), defaultValue: -1);
    }
}