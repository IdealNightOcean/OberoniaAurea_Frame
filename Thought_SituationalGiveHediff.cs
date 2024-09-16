using RimWorld;
using Verse;

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
        RemoveHediff(pawn, def.hediff);
    }

    public static void RemoveHediff(Pawn pawn, HediffDef hediffDef)
    {
        Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
        if (firstHediffOfDef != null)
        {
            pawn.health.RemoveHediff(firstHediffOfDef);
        }
    }

}