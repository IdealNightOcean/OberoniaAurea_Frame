using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 用于配置范围型<see cref="Hediff"/>给予的参数集合。继承自 <see cref="HediffGiveParams"/>，额外包含影响范围、连接线绘制、目标种族和阵营关系等筛选条件。
/// </summary>
public class RangeHediffGiveParams : HediffGiveParams
{
    /// <summary>
    /// 影响范围半径。小于等于0时全图生效。
    /// </summary>
    public float Radius;
    /// <summary>
    /// 影响范围半径的平方。小于等于0时全图生效。
    /// </summary>
    public float RadiusSquared => Radius <= 0f ? -1f : Radius * Radius;

    /// <summary>
    /// 是否设置<see cref="HediffComp_Link"/>
    /// </summary>
    public bool AddlinkWithThing = true;

    /// <summary>
    /// 是否绘制 <see cref="Hediff"/> 所有者与关联物体之间的连接线。
    /// </summary>
    public bool DrawConnection = false;

    /// <summary>
    /// 目标种族的筛选条件。默认为 <see cref="RaceType.Humanlike"/>。
    /// </summary>
    public RaceType TargetRace = RaceType.Humanlike;

    /// <summary>
    /// 目标阵营关系的筛选条件。默认为 <see cref="TargetRelationType.NonHostile"/>。
    /// </summary>
    public TargetRelationType TargetRelation = TargetRelationType.NonHostile;

    /// <summary>
    /// 创建一个新的范围型<see cref="Hediff"/>给予参数实例。
    /// </summary>
    public RangeHediffGiveParams() { }
    /// <summary>
    /// 使用指定的<see cref="HediffDef"/>和影响范围半径创建一个新的范围型<see cref="Hediff"/>给予参数实例。
    /// </summary>
    /// <param name="hediffToGive">要给予的<see cref="HediffDef"/>。</param>
    /// <param name="radius">影响范围半径。</param>
    public RangeHediffGiveParams(HediffDef hediffToGive, float radius) : base(hediffToGive)
    {
        this.Radius = radius;
    }

    /// <summary>
    /// 创建此对象的浅表副本。
    /// </summary>
    public new RangeHediffGiveParams ShallowCopy() => (RangeHediffGiveParams)MemberwiseClone();


    /// <inheritdoc />
    public override void ExposeData()
    {
        Scribe_Values.Look(ref Radius, nameof(Radius), defaultValue: -1f);

        Scribe_Values.Look(ref AddlinkWithThing, nameof(AddlinkWithThing), defaultValue: true);
        Scribe_Values.Look(ref DrawConnection, nameof(DrawConnection), defaultValue: false);

        Scribe_Values.Look(ref TargetRace, nameof(TargetRace), defaultValue: RaceType.Humanlike);
        Scribe_Values.Look(ref TargetRelation, nameof(TargetRelation), defaultValue: TargetRelationType.NonHostile);
    }
}