using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 范围Hediff给予器。对关联物体周围指定范围内的所有符合条件的Pawn自动给予Hediff。
/// </summary>
public class RangeHediffGiver : IExposable
{
    protected RangeHediffGiveParams parms;
    /// <summary>
    /// 获取此给予器所使用的参数配置。
    /// </summary>
    public RangeHediffGiveParams Parms => parms;


    protected Thing linkedThing;

    /// <summary>
    /// 关联的物体，用于确定位置和阵营信息。
    /// </summary>
    public Thing LinkedThing
    {
        get => linkedThing;
        set => linkedThing = value;
    }


    /// <summary>
    /// 获取关联物体所在的地图。
    /// </summary>
    public Map TargetMap => linkedThing?.MapHeld;
    /// <summary>
    /// 获取关联物体所属的阵营。
    /// </summary>
    public Faction LinkedFaction => linkedThing?.Faction;

    /// <summary>
    /// 创建一个新的范围Hediff给予器实例。
    /// </summary>
    public RangeHediffGiver() { }

    /// <summary>
    /// 使用指定的关联物体和参数配置创建一个新的范围Hediff给予器实例。
    /// </summary>
    /// <param name="linkedThing">关联的物体，用于确定位置和阵营信息。</param>
    /// <param name="parms">参数配置。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="linkedThing"/> 为 null 时抛出。</exception>
    public RangeHediffGiver(Thing linkedThing, RangeHediffGiveParams parms)
    {
        this.linkedThing = linkedThing ?? throw new ArgumentNullException(nameof(linkedThing));
        this.parms = parms;
    }

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_References.Look(ref linkedThing, nameof(linkedThing));
        Scribe_Deep.Look(ref parms, nameof(parms));
    }

    /// <summary>
    /// 对范围内所有符合条件的Pawn给予Hediff。
    /// 根据 <see href="Parms.Radius"/> 确定范围，并根据 <see href="Parms.TargetRace"/> 和 <see href="Parms.TargetRelation"/> 过滤目标。
    /// </summary>
    public void GiveHediffToRange()
    {
        if (linkedThing is null)
        {
            Log.Error($"[OARK] {nameof(linkedThing)} 不能为空。");
            return;
        }

        if (Parms.Radius <= 0f || !linkedThing.Spawned)
            return;

        IntVec3 linkedThingPos = linkedThing.Position;
        float radiusSquared = Parms.Radius * Parms.Radius;

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
        Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Parms.HediffToGive);
        if (hediff is null)
        {
            hediff = target.health.AddHediff(Parms.HediffToGive, Parms.BodyPartRecordToGive);
            if (Parms.InitSeverity > 0f)
                hediff.Severity = Parms.InitSeverity;

            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link is not null)
            {
                hediffComp_Link.drawConnection = Parms.DrawConnection;
                hediffComp_Link.other = linkedThing;
            }
        }

        if (Parms.OverrideDisappearTicks > 0)
        {
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears is null)
            {
                Log.Error($"[OAFrame] {hediff} 没有 {nameof(HediffComp_Disappears)} 组件。");
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = Parms.OverrideDisappearTicks;
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

        if (target == linkedThing && !OARO_EnumUtility.ContainsFlag(Parms.TargetRelation, TargetRelationType.Self))
            return false;

        if (center.DistanceToSquared(target.Position) > radiusSquared)
            return false;

        RaceType targetRace = Parms.TargetRace;
        if (!targetRace.ContainsFlag(RaceType.All))
        {
            RaceType raceType = target.GetRaceType();
            if (raceType == RaceType.None)
                return false;

            if (!targetRace.ContainsFlag(raceType))
                return false;
        }

        TargetRelationType targetRelation = Parms.TargetRelation;
        if (!targetRelation.ContainsFlag(TargetRelationType.All))
        {
            Faction targetFacion = target.Faction;
            if (targetFacion is null)
            {
                if (target.HostileTo(linkedThing))
                {
                    if (!targetRelation.ContainsFlag(TargetRelationType.Hostile))
                        return false;
                }
                else
                {
                    if (!targetRelation.ContainsAnyFlag(TargetRelationType.NonHostile))
                        return false;
                }
            }
            else
            {
                if (targetFacion == LinkedFaction && !targetRelation.ContainsFlag(TargetRelationType.SameFaction))
                    return false;

                FactionRelationKind relation = LinkedFaction?.RelationKindWith(targetFacion) ?? FactionRelationKind.Neutral;
                switch (relation)
                {
                    case FactionRelationKind.Ally:
                        if (!targetRelation.ContainsFlag(TargetRelationType.Ally)) return false;
                        break;
                    case FactionRelationKind.Neutral:
                        if (!targetRelation.ContainsFlag(TargetRelationType.Neutral)) return false;
                        break;
                    case FactionRelationKind.Hostile:
                        if (!targetRelation.ContainsFlag(TargetRelationType.Hostile)) return false;
                        break;
                    default:
                        if (!targetRelation.ContainsFlag(TargetRelationType.Neutral)) return false;
                        break;
                }
            }
        }

        return true;
    }
}