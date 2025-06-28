using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_PawnUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsChildOfRetentionHolder(this Pawn p)
    {
        return p.ParentHolder is IPawnRetentionHolder;
    }

    //添加健康状态
    public static void AdjustOrAddHediff(Pawn pawn, HediffDef hediffDef, float severity = -1, int overrideDisappearTicks = -1, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
    {
        Hediff hediff = pawn?.health.GetOrAddHediff(hediffDef, part, dinfo, result);
        if (hediff is null)
        {
            return;
        }
        if (severity > 0)
        {
            hediff.Severity = severity;
        }
        if (overrideDisappearTicks > 0)
        {
            HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
            if (comp is not null)
            {
                comp.ticksToDisappear = overrideDisappearTicks;
            }
        }
    }
    //移除第一个健康状态
    public static void RemoveFirstHediffOfDef(Pawn pawn, HediffDef def, bool mustBeVisible = false)
    {
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            Hediff hediff = hediffs[i];
            if (hediff.def == def && (!mustBeVisible || hediff.Visible))
            {
                pawn.health.RemoveHediff(hediff);
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PawnSleepNow(Pawn pawn)
    {
        return pawn.jobs?.curDriver?.asleep ?? false;
    }
}
