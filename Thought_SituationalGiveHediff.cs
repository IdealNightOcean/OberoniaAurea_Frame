using RimWorld;

namespace OberoniaAurea_Frame;

public class Thought_SituationalGiveHediff : Thought_Situational
{
    protected override void Notify_BecameActive()
    {
        base.Notify_BecameActive();
        pawn.health.AddHediff(def.hediff);
    }

    protected override void Notify_BecameInactive()
    {
        base.Notify_BecameInactive();
        pawn.RemoveFirstHediffOfDef(def.hediff);
    }
}