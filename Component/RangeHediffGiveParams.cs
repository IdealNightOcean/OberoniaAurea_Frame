using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 用于配置范围型Hediff给予的参数集合。继承自 <see cref="HediffGiveParams"/>，额外包含影响范围、连接线绘制、目标种族和阵营关系等筛选条件。
/// </summary>
public class RangeHediffGiveParams : HediffGiveParams
{
    private float radius = -1f;
    /// <summary>
    /// 影响范围半径。小于等于0时不会生效。
    /// </summary>
    public float Radius
    {
        get => radius;
        set => radius = value;
    }

    private bool drawConnection = false;
    /// <summary>
    /// 是否绘制<see cref="Hediff"/>所有者与关联物体之间的连接线。
    /// </summary>
    public bool DrawConnection
    {
        get => drawConnection;
        set => drawConnection = value;
    }

    private RaceType targetRace = RaceType.Humanlike;
    /// <summary>
    /// 目标种族的筛选条件。默认为 <see cref="RaceType.Humanlike"/>。
    /// </summary>
    public RaceType TargetRace
    {
        get => targetRace;
        set => targetRace = value;
    }

    private TargetRelationType targetRelation = TargetRelationType.Default;
    /// <summary>
    /// 目标阵营关系的筛选条件。默认为 <see cref="TargetRelationType.Default"/>。
    /// </summary>
    public TargetRelationType TargetRelation
    {
        get => targetRelation;
        set => targetRelation = value;
    }

    /// <summary>
    /// 创建此对象的浅表副本。
    /// </summary>
    public new RangeHediffGiveParams ShallowCopy() => (RangeHediffGiveParams)MemberwiseClone();


    /// <inheritdoc />
    public override void ExposeData()
    {
        Scribe_Values.Look(ref radius, nameof(radius), defaultValue: -1f);

        Scribe_Values.Look(ref drawConnection, nameof(drawConnection), defaultValue: false);

        Scribe_Values.Look(ref targetRace, nameof(targetRace), defaultValue: RaceType.Humanlike);
        Scribe_Values.Look(ref targetRelation, nameof(targetRelation), defaultValue: TargetRelationType.Default);
    }
}