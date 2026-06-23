using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 累加严重度范围<see cref="Hediff"/>给予器。
/// </summary>
public class RangeHediffGiver_AddServity : RangeHediffGiver
{
    /// <inheritdoc />
    public new RangeHediffGiveParams_AddServity Parms => (RangeHediffGiveParams_AddServity)parms;

    /// <inheritdoc />
    public RangeHediffGiver_AddServity() { }

    /// <inheritdoc />
    public RangeHediffGiver_AddServity(Thing linkedThing, RangeHediffGiveParams_AddServity parms) : base(linkedThing, parms)
    { }

    /// <inheritdoc />
    protected override void GiveHediffToTarget(Pawn target)
    {
        Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Parms.HediffToGive);
        if (hediff is null)
        {
            hediff = target.health.AddHediff(Parms.HediffToGive, Parms.BodyPartRecordToGive);
            if (Parms.InitSeverity > 0f)
                hediff.Severity = Parms.InitSeverity;
        }
        else
        {
            hediff.Severity += Parms.AddSeverity;
        }
        HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
        if (hediffComp_Link is not null)
        {
            hediffComp_Link.drawConnection = false;
            hediffComp_Link.other = linkedThing;
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
}
