using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 可累加严重度的范围型<see cref="Hediff"/>给予参数。继承自 <see cref="RangeHediffGiveParams"/>，额外包含每次检查时累加的严重度值。
/// </summary>
public class RangeHediffGiveParams_AddServity : RangeHediffGiveParams
{
    private float addSeverity = 0.1f;

    /// <summary>
    /// 每次检查时累加的严重度值。默认值为 0.1。
    /// </summary>
    public float AddSeverity
    {
        get => addSeverity;
        set => addSeverity = value;
    }

    /// <inheritdoc />
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref addSeverity, nameof(addSeverity), defaultValue: 0.1f);
    }
}
