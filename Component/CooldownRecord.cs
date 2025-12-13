using Verse;

namespace OberoniaAurea_Frame;

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

    public CooldownRecord(int cooldownTicks, bool removeWhenExpired)
    {
        lastActiveTick = Find.TickManager.TicksGame;
        nextAvailableTick = lastActiveTick + cooldownTicks;
        this.removeWhenExpired = removeWhenExpired;
    }

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