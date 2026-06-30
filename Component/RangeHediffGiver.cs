using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 范围<see cref="Hediff"/>给予器。对关联物体周围指定范围内的所有符合条件的<see cref="Pawn"/>给予<see cref="Hediff"/>。
/// </summary>
public class RangeHediffGiver : IExposable
{
    private RangeHediffGiveParams parms;
    /// <summary>
    /// 获取此给予器所使用的参数配置。
    /// </summary>
    public RangeHediffGiveParams Parms => parms;


    private Thing linkedThing;

    /// <summary>
    /// 关联的物体，用于确定位置和阵营信息。
    /// </summary>
    public Thing LinkedThing
    {
        get => linkedThing;
        set => linkedThing = value;
    }

    /// <summary>
    /// 额外的目标验证委托。在已有条件基础上进行额外筛选，返回 <see langword="true"/> 时表示目标通过验证。
    /// </summary>
    public Predicate<Pawn> ExtraTargetValiator { get; set; }

    /// <summary>
    /// 获取关联物体所在的地图。
    /// </summary>
    public Map TargetMap => linkedThing?.MapHeld;
    /// <summary>
    /// 获取关联物体所属的阵营。
    /// </summary>
    public Faction LinkedFaction => linkedThing?.Faction;

    /// <summary>
    /// 创建一个新的范围<see cref="Hediff"/>给予器实例。
    /// </summary>
    public RangeHediffGiver() { }

    /// <summary>
    /// 使用指定的关联物体和参数配置创建一个新的范围<see cref="Hediff"/>给予器实例。
    /// </summary>
    /// <param name="linkedThing">关联的物体，用于确定位置和阵营信息。</param>
    /// <param name="parms">参数配置。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="linkedThing"/> 为 <see langword="null"/> 时抛出。</exception>
    public RangeHediffGiver(Thing linkedThing, RangeHediffGiveParams parms)
    {
        this.linkedThing = linkedThing ?? throw new ArgumentNullException(nameof(linkedThing));
        this.parms = parms;
    }

    public void SetGiveParams(RangeHediffGiveParams parms) => this.parms = parms;

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_References.Look(ref linkedThing, nameof(linkedThing));
        Scribe_Deep.Look(ref parms, nameof(parms));
    }

    /// <summary>
    /// 对范围内所有符合条件的<see cref="Pawn"/>给予<see cref="Hediff"/>。
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

        IEnumerable<Pawn> potentialPawns = null;
        if (linkedThing.Faction is null || parms.TargetRelation.ContainsAnyFlag(TargetRelationType.NonSameFaction))
            potentialPawns = TargetMap.mapPawns.AllPawnsSpawned;
        else
            potentialPawns = TargetMap.mapPawns.SpawnedPawnsInFaction(linkedThing.Faction);

        foreach (Pawn target in potentialPawns)
        {
            if (CanApplyOnPawn(target, linkedThingPos, radiusSquared))
            {
                GiveHediffToTarget(target);
            }
        }
    }

    /// <summary>
    /// 对指定的单个<see cref="Pawn"/>给予<see cref="Hediff"/>。如果目标已有同类型<see cref="Hediff"/>则跳过添加，但会更新消失时间。
    /// </summary>
    /// <param name="target">要给予<see cref="Hediff"/>的目标<see cref="Pawn"/>。</param>
    protected void GiveHediffToTarget(Pawn target)
    {
        Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Parms.HediffToGive);
        bool isNewHediff = false;
        if (hediff is null)
        {
            hediff = target.health.AddHediff(Parms.HediffToGive, Parms.BodyPartRecordToGive);
            isNewHediff = true;
        }

        PostGiveHediff(target, hediff, isNewHediff);
    }

    /// <summary>
    /// 在给予<see cref="Hediff"/>后调用，用于执行后续处理（调整严重度、设置连接线、覆盖消失时间）。
    /// </summary>
    /// <param name="target">被给予<see cref="Hediff"/>的目标<see cref="Pawn"/>。</param>
    /// <param name="hediffGived">被给予的<see cref="Hediff"/>实例。</param>
    /// <param name="isNewHediff">指示是否为新增的<see cref="Hediff"/>。</param>
    protected virtual void PostGiveHediff(Pawn target, Hediff hediffGived, bool isNewHediff)
    {
        if (isNewHediff)
        {
            if (Parms.InitSeverity > 0f)
                hediffGived.Severity = Parms.InitSeverity;
        }
        else if (Parms.AddSeverityIfExist.HasValue)
        {
            hediffGived.Severity += Parms.AddSeverityIfExist.Value;
        }

        HediffComp_Link hediffComp_Link = hediffGived.TryGetComp<HediffComp_Link>();
        if (hediffComp_Link is not null)
        {
            hediffComp_Link.drawConnection = Parms.DrawConnection;
            hediffComp_Link.other = linkedThing;
        }

        if (Parms.OverrideDisappearTicks > 0)
        {
            HediffComp_Disappears hediffComp_Disappears = hediffGived.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears is null)
            {
                Log.Error($"[OAFrame] {hediffGived} 没有 {nameof(HediffComp_Disappears)} 组件。");
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = Parms.OverrideDisappearTicks;
            }
        }
    }

    /// <summary>
    /// 检查指定的<see cref="Pawn"/>是否符合所有条件（距离、种族、阵营关系），以确定是否对其给予<see cref="Hediff"/>。
    /// </summary>
    /// <param name="target">要检查的目标<see cref="Pawn"/>。</param>
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

        if (ExtraTargetValiator is not null)
        {
            try
            {
                return ExtraTargetValiator.Invoke(target);
            }
            catch (Exception ex)
            {
                Log.Error($"[OAFrame] 对目标进行额外判定{nameof(ExtraTargetValiator)}时出现异常，目标已被排除，异常：{ex}");
                return false;
            }
        }

        return true;
    }
}