using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 累加严重度范围Hediff给予器。
/// </summary>
public class RangeHediffGiver_AddServity : RangeHediffGiver
{
    /// <summary>
    /// 每次检查时累加的严重度值。默认值为 0.1。
    /// </summary>
    public float AddSeverity = 0.1f;

    /// <inheritdoc />
    public RangeHediffGiver_AddServity() { }

    /// <inheritdoc />
    public RangeHediffGiver_AddServity(Thing linkedThing, HediffDef hediffToGive, float radius) : base(linkedThing, hediffToGive, radius)
    { }

    /// <inheritdoc />
    protected override void GiveHediffToTarget(Pawn target)
    {
        Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(HediffToGive);
        if (hediff is null)
        {
            hediff = target.health.AddHediff(HediffToGive, BodyPartRecordToGive);
            if (InitSeverity > 0f)
                hediff.Severity = InitSeverity;
        }
        else
        {
            hediff.Severity += AddSeverity;
        }
        HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
        if (hediffComp_Link is not null)
        {
            hediffComp_Link.drawConnection = false;
            hediffComp_Link.other = LinkedThing;
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

    /// <inheritdoc />
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref AddSeverity, nameof(AddSeverity), defaultValue: 0.1f);
    }
}
