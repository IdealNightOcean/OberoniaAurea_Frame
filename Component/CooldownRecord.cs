using Verse;

namespace OberoniaAurea_Frame;

public struct CooldownRecord : IExposable
{
    public int lastActiveTick;
    public int nextAvailableTick;
    private bool shouldRemoveWhenExpired;
    public readonly bool ShouldRemove
    {
        get
        {
            if (lastActiveTick < 0)
            {
                return true;
            }
            if (shouldRemoveWhenExpired && !IsInCooldown)
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
        shouldRemoveWhenExpired = true;
    }

    public CooldownRecord(int cooldownTicks, bool shouldRemoveWhenExpired)
    {
        lastActiveTick = Find.TickManager.TicksGame;
        nextAvailableTick = lastActiveTick + cooldownTicks;
        this.shouldRemoveWhenExpired = shouldRemoveWhenExpired;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref lastActiveTick, "lastActiveTick", -1);
        Scribe_Values.Look(ref nextAvailableTick, "nextAvailableTick", -1);
        Scribe_Values.Look(ref shouldRemoveWhenExpired, "shouldRemoveWhenExpired", defaultValue: false);
    }

    public readonly int CooldownTicksLeft => lastActiveTick < 0 ? -1 : nextAvailableTick - Find.TickManager.TicksGame;

    public readonly int TicksSinceLastActive => lastActiveTick < 0 ? -1 : Find.TickManager.TicksGame - lastActiveTick;

    public readonly bool IsInCooldown => nextAvailableTick > 0 && nextAvailableTick > Find.TickManager.TicksGame;

    public override readonly string ToString() => $"LastActiveTick: {lastActiveTick}, NextAvailableTick: {nextAvailableTick}";

}