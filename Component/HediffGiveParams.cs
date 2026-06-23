using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 用于配置给予Hediff的参数集合。
/// </summary>
public class HediffGiveParams : IExposable
{

    private HediffDef hediffToGive;
    /// <summary>
    /// 要给予的Hediff定义。
    /// </summary>
    public HediffDef HediffToGive
    {
        get => hediffToGive;
        set => hediffToGive = value;
    }

    private BodyPartRecord bodyPartRecordToGive;
    /// <summary>
    /// 要给予Hediff的目标身体部位。为null时使用默认部位。
    /// </summary>
    public BodyPartRecord BodyPartRecordToGive
    {
        get => bodyPartRecordToGive;
        set => bodyPartRecordToGive = value;
    }

    private float initSeverity = -1f;
    /// <summary>
    /// 初始严重度。小于等于0时使用Hediff定义的默认值。
    /// </summary>
    public float InitSeverity
    {
        get => initSeverity;
        set => initSeverity = value;
    }

    private int overrideDisappearTicks = -1;
    /// <summary>
    /// 覆盖Hediff的消失时间（Tick数）。小于等于0时使用Hediff定义的默认值。
    /// </summary>
    public int OverrideDisappearTicks
    {
        get => overrideDisappearTicks;
        set => overrideDisappearTicks = value;
    }

    /// <summary>
    /// 创建此对象的浅表副本。
    /// </summary>
    public HediffGiveParams ShallowCopy() => (HediffGiveParams)MemberwiseClone();

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_Defs.Look(ref hediffToGive, nameof(hediffToGive));

        Scribe_BodyParts.Look(ref bodyPartRecordToGive, nameof(bodyPartRecordToGive));
        Scribe_Values.Look(ref initSeverity, nameof(initSeverity), defaultValue: -1f);
        Scribe_Values.Look(ref overrideDisappearTicks, nameof(overrideDisappearTicks), defaultValue: -1);

    }
}