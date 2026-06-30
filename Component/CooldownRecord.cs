using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 冷却记录。
/// </summary>
public struct CooldownRecord : IExposable
{
    public int lastActiveTick;
    public int nextAvailableTick;
    private bool removeWhenExpired;
    public readonly bool ShouldRemove
    {
        get
        {
            if (lastActiveTick < 0)
            {
                return true;
            }
            if (removeWhenExpired && !IsInCooldown)
            {
                return true;
            }
            return false;
        }
    }

    public CooldownRecord()
    {
        lastActiveTick = -1;
        nextAvailableTick = -1;
        removeWhenExpired = true;
    }

    /// <summary>
    /// 使用冷却时间和过期移除标志初始化冷却记录。
    /// </summary>
    public CooldownRecord(int cooldownTicks, bool removeWhenExpired)
    {
        lastActiveTick = Find.TickManager.TicksGame;
        nextAvailableTick = lastActiveTick + cooldownTicks;
        this.removeWhenExpired = removeWhenExpired;
    }

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
    public void ExposeData()
    {
        Scribe_Values.Look(ref lastActiveTick, nameof(lastActiveTick), -1);
        Scribe_Values.Look(ref nextAvailableTick, nameof(nextAvailableTick), -1);
        Scribe_Values.Look(ref removeWhenExpired, nameof(removeWhenExpired), defaultValue: false);
    }

    public readonly int CooldownTicksLeft => lastActiveTick < 0 ? -1 : nextAvailableTick - Find.TickManager.TicksGame;

    public readonly int TicksSinceLastActive => lastActiveTick < 0 ? -1 : Find.TickManager.TicksGame - lastActiveTick;

    public readonly bool IsInCooldown => nextAvailableTick > 0 && nextAvailableTick > Find.TickManager.TicksGame;

    public override readonly string ToString() => $"LastActiveTick: {lastActiveTick}, NextAvailableTick: {nextAvailableTick}";

}