using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

    private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
    {
        return from p in bodyModel.GetNotMissingParts()
               where p.depth == BodyPartDepth.Outside || (p.depth == BodyPartDepth.Inside && p.def.IsSolid(p, bodyModel.hediffs))
               select p;
    }

    /// <summary>
    /// 造成不致命、不残疾的伤害
    /// </summary>
    public static void TakeNonLethalDamage(Pawn pawn, int injuriesCount, DamageDef fixedDamageDef = null)
    {
        if (pawn.DeadOrDowned)
        {
            return;
        }

        pawn.health.forceDowned = true;
        IEnumerable<BodyPartRecord> source = from p in HittablePartsViolence(pawn.health.hediffSet)
                                             where !pawn.health.hediffSet.hediffs.Any(h => h.Part == p && h.CurStage != null && h.CurStage.partEfficiencyOffset < 0f)
                                             select p;

        int curInjuryNum = 0;
        while (curInjuryNum < injuriesCount && source.Any())
        {
            curInjuryNum++;
            BodyPartRecord bodyPartRecord = source.RandomElementByWeight(p => p.coverageAbs);
            float maxHealth = bodyPartRecord.def.GetMaxHealth(pawn);
            float partHealth = pawn.health.hediffSet.GetPartHealth(bodyPartRecord);
            float statValue = pawn.GetStatValue(StatDefOf.IncomingDamageFactor);
            if (statValue > 0f)
            {
                maxHealth = (int)((float)maxHealth / statValue);
            }
            maxHealth -= 3f;
            if (maxHealth <= 0f)
            {
                continue;
            }

            int min = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.3f), (int)partHealth - 1);
            int max = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.8f), (int)partHealth - 1);
            int severity = Rand.RangeInclusive(min, max);
            DamageDef damageDef = fixedDamageDef ?? HealthUtility.RandomViolenceDamageType();
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, pawn, bodyPartRecord);
            if (pawn.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, statValue * severity))
            {
                break;
            }
            DamageInfo dinfo = new(damageDef, severity, 999f, -1f, null, bodyPartRecord);
            dinfo.SetAllowDamagePropagation(val: false);
            pawn.TakeDamage(dinfo);
        }
        if (pawn.Dead)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(string.Concat(pawn, " died during take non-lethal damage"));
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                stringBuilder.AppendLine("   -" + pawn.health.hediffSet.hediffs[i].ToString());
            }
            Log.Error(stringBuilder.ToString());
        }
        pawn.health.forceDowned = false;
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

    public static float GetMaxStatOfPawns(IEnumerable<Pawn> pawns, StatDef statDef)
    {
        if (pawns is null)
        {
            return -999f;
        }
        float maxStatValue = -999f;

        foreach (Pawn pawn in pawns)
        {
            if (!statDef.Worker.IsDisabledFor(pawn))
            {
                continue;
            }
            maxStatValue = Mathf.Max(maxStatValue, pawn.GetStatValue(statDef));
        }
        return maxStatValue;
    }

    public static (Pawn, float) GetMaxStatPawn(IEnumerable<Pawn> pawns, StatDef statDef)
    {
        if (pawns is null)
        {
            return (null, -999f);
        }
        Pawn maxStatPawn = null;
        float maxStatValue = -999f;

        float statValue;
        foreach (Pawn pawn in pawns)
        {
            if (!statDef.Worker.IsDisabledFor(pawn))
            {
                continue;
            }
            statValue = pawn.GetStatValue(statDef);
            if (statValue > maxStatValue)
            {
                maxStatValue = statValue;
                maxStatPawn = pawn;
            }
        }
        return (maxStatPawn, maxStatValue);
    }
}