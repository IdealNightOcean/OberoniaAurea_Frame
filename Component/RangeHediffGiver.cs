using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class RangeHediffGiver : IExposable
{
    /// <summary>
    /// 关联的物体，用于确定位置和阵营信息。
    /// </summary>
    public Thing LinkedThing;
    /// <summary>
    /// 要给予的Hediff定义。
    /// </summary>
    public HediffDef HediffToGive;
    /// <summary>
    /// 影响范围半径。小于等于0时不会生效。
    /// </summary>
    public float Radius = -1f;

    /// <summary>
    /// 要给予Hediff的目标身体部位。为null时使用默认部位。
    /// </summary>
    public BodyPartRecord BodyPartRecordToGive;
    /// <summary>
    /// 初始严重度。小于等于0时使用Hediff定义的默认值。
    /// </summary>
    public float InitSeverity = -1f;
    /// <summary>
    /// 覆盖Hediff的消失时间（Tick数）。小于等于0时使用Hediff定义的默认值。
    /// </summary>
    public int OverrideDisappearTicks = -1;

    /// <summary>
    /// 是否绘制Hediff与关联物体之间的连接线。
    /// </summary>
    public bool DrawConnection = false;

    /// <summary>
    /// 获取关联物体所在的地图。
    /// </summary>
    public Map TargetMap => LinkedThing?.MapHeld;
    /// <summary>
    /// 获取关联物体所属的阵营。
    /// </summary>
    public Faction LinkedFaction => LinkedThing?.Faction;


    /// <summary>
    /// 目标种族的筛选条件。默认为 <see cref="RaceType.Humanlike"/>。
    /// </summary>
    public RaceType TargetRace = RaceType.Humanlike;

    /// <summary>
    /// 目标阵营关系的筛选条件。默认为 <see cref="TargetRelationType.Default"/>。
    /// </summary>
    public TargetRelationType TargetRelation = TargetRelationType.Default;

    /// <summary>
    /// 创建一个新的范围Hediff给予器实例。
    /// </summary>
    public RangeHediffGiver() { }

    /// <summary>
    /// 使用指定的关联物体、Hediff定义和范围半径创建一个新的范围Hediff给予器实例。
    /// </summary>
    /// <param name="linkedThing">关联的物体，用于确定位置和阵营信息。</param>
    /// <param name="hediffToGive">要给予的Hediff定义。</param>
    /// <param name="radius">影响范围半径。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="linkedThing"/> 或 <paramref name="hediffToGive"/> 为 null 时抛出。</exception>
    public RangeHediffGiver(Thing linkedThing, HediffDef hediffToGive, float radius)
    {
        LinkedThing = linkedThing ?? throw new ArgumentNullException(nameof(linkedThing));
        HediffToGive = hediffToGive ?? throw new ArgumentNullException(nameof(hediffToGive));
        Radius = radius;
    }

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_References.Look(ref LinkedThing, nameof(LinkedThing));
        Scribe_Defs.Look(ref HediffToGive, nameof(HediffToGive));
        Scribe_Values.Look(ref Radius, nameof(Radius), defaultValue: -1f);

        Scribe_Values.Look(ref InitSeverity, nameof(InitSeverity), defaultValue: -1f);
        Scribe_Values.Look(ref OverrideDisappearTicks, nameof(OverrideDisappearTicks), defaultValue: -1);
        Scribe_Values.Look(ref DrawConnection, nameof(DrawConnection), defaultValue: false);

        Scribe_Values.Look(ref TargetRace, nameof(TargetRace), defaultValue: RaceType.Humanlike);
        Scribe_Values.Look(ref TargetRelation, nameof(TargetRelation), defaultValue: TargetRelationType.Default);
    }

    /// <summary>
    /// 对范围内所有符合条件的Pawn给予Hediff。
    /// 根据 <see cref="Radius"/> 确定范围，并根据 <see cref="TargetRace"/> 和 <see cref="TargetRelation"/> 过滤目标。
    /// </summary>
    public void GiveHediffToRange()
    {
        if (LinkedThing is null)
        {
            Log.Error($"[OARK] {nameof(LinkedThing)} 不能为空。");
            return;
        }

        if (Radius <= 0f || !LinkedThing.Spawned)
            return;

        IntVec3 linkedThingPos = LinkedThing.Position;
        float radiusSquared = Radius * Radius;

        IEnumerable<Pawn> potentialPawns = TargetMap.mapPawns.AllPawnsSpawned;
        foreach (Pawn target in potentialPawns)
        {
            if (CanApplyOnPawn(target, linkedThingPos, radiusSquared))
            {
                GiveHediffToTarget(target);
            }
        }
    }

    /// <summary>
    /// 对指定的单个Pawn给予Hediff。如果目标已有同类型Hediff则跳过添加，但会更新消失时间。
    /// </summary>
    /// <param name="target">要给予Hediff的目标Pawn。</param>
    protected virtual void GiveHediffToTarget(Pawn target)
    {
        Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(HediffToGive);
        if (hediff is null)
        {
            hediff = target.health.AddHediff(HediffToGive, BodyPartRecordToGive);
            if (InitSeverity > 0f)
                hediff.Severity = InitSeverity;

            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link is not null)
            {
                hediffComp_Link.drawConnection = false;
                hediffComp_Link.other = LinkedThing;
            }
        }

        if (OverrideDisappearTicks > 0)
        {
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears is null)
            {
                Log.Error($"[OAFrame] {hediff} 没有 {nameof(HediffComp_Disappears)} 组件。");
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = OverrideDisappearTicks;
            }
        }
    }

    /// <summary>
    /// 检查指定的Pawn是否符合所有条件（距离、种族、阵营关系），以确定是否对其给予Hediff。
    /// </summary>
    /// <param name="target">要检查的目标Pawn。</param>
    /// <param name="center">范围中心位置。</param>
    /// <param name="radiusSquared">范围半径的平方。</param>
    /// <returns>如果目标符合条件则返回 <see langword="true"/>。</returns>
    protected virtual bool CanApplyOnPawn(Pawn target, IntVec3 center, float radiusSquared)
    {
        if (target is null)
            return false;

        if (target == LinkedThing && !OARO_EnumUtility.ContainsFlag(TargetRelation, TargetRelationType.Self))
            return false;

        if (center.DistanceToSquared(target.Position) > radiusSquared)
            return false;

        if (!TargetRace.ContainsFlag(RaceType.All))
        {
            RaceType raceType = target.GetRaceType();
            if (raceType == RaceType.None)
                return false;

            if (!TargetRace.ContainsFlag(raceType))
                return false;
        }

        if (!TargetRelation.ContainsFlag(TargetRelationType.All))
        {
            Faction targetFacion = target.Faction;
            if (targetFacion is null)
            {
                if (target.HostileTo(LinkedThing))
                {
                    if (!TargetRelation.ContainsFlag(TargetRelationType.Hostile))
                        return false;
                }
                else
                {
                    if (!TargetRelation.ContainsAnyFlag(TargetRelationType.NonHostile))
                        return false;
                }
            }
            else
            {
                if (targetFacion == LinkedFaction && !TargetRelation.ContainsFlag(TargetRelationType.SameFaction))
                    return false;

                FactionRelationKind relation = LinkedFaction?.RelationKindWith(targetFacion) ?? FactionRelationKind.Neutral;
                switch (relation)
                {
                    case FactionRelationKind.Ally:
                        if (!TargetRelation.ContainsFlag(TargetRelationType.Ally)) return false;
                        break;
                    case FactionRelationKind.Neutral:
                        if (!TargetRelation.ContainsFlag(TargetRelationType.Neutral)) return false;
                        break;
                    case FactionRelationKind.Hostile:
                        if (!TargetRelation.ContainsFlag(TargetRelationType.Hostile)) return false;
                        break;
                    default:
                        if (!TargetRelation.ContainsFlag(TargetRelationType.Neutral)) return false;
                        break;
                }
            }
        }

        return true;
    }
}