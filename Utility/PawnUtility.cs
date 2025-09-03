using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
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

    /// <summary>
    /// 添加健康状态
    /// </summary>
    public static void AdjustOrAddHediff(this Pawn pawn, HediffDef hediffDef, float severity = -1, int overrideDisappearTicks = -1, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
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

    /// <summary>
    /// 移除第一个相关Def的健康状态
    /// </summary>
    public static void RemoveFirstHediffOfDef(this Pawn pawn, HediffDef def, bool mustBeVisible = false)
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
    public static bool SleepNow(this Pawn pawn)
    {
        return pawn.jobs?.curDriver?.asleep ?? false;
    }

    public static int GetMaxSkillLevelOfPawns(IEnumerable<Pawn> pawns, SkillDef skill)
    {
        if (pawns is null)
        {
            return -1;
        }
        int maxSkillLevel = -1;

        foreach (Pawn pawn in pawns)
        {
            if (pawn.skills is null)
            {
                continue;
            }
            maxSkillLevel = Mathf.Max(maxSkillLevel, pawn.skills.GetSkill(skill).GetLevel());
        }
        return maxSkillLevel;
    }

    public static (Pawn, int) GetMaxSkillLevelPawn(IEnumerable<Pawn> pawns, SkillDef skill)
    {
        if (pawns is null)
        {
            return (null, -1);
        }
        Pawn maxSkillPawn = null;
        int maxSkillLevel = -1;

        int skillLevel;
        foreach (Pawn pawn in pawns)
        {
            if (pawn.skills is null)
            {
                continue;
            }
            skillLevel = pawn.skills.GetSkill(skill).GetLevel();
            if (skillLevel > maxSkillLevel)
            {
                maxSkillLevel = skillLevel;
                maxSkillPawn = pawn;
            }
        }
        return (maxSkillPawn, maxSkillLevel);
    }
}
