
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// <see cref="Pawn"/>预伤害处理接口。
/// </summary>
public interface IPawnPreApplyDamage
{
    /// <summary>
    /// 获取处理优先级。
    /// </summary>
    int Priority { get; }
    /// <summary>
    /// <see cref="Pawn"/>预伤害处理。
    /// </summary>
    void PawnPreApplyDamage(ref DamageInfo dinfo, out bool absorbed);
}

/// <summary>
/// <see cref="Pawn"/>预伤害处理器组件属性。
/// </summary>
public class CompProperties_PawnPreApplyDamage : CompProperties
{
    public CompProperties_PawnPreApplyDamage()
    {
        compClass = typeof(CompPawnPreApplyDamageHandler);
    }
}

/// <summary>
/// <see cref="Pawn"/>预伤害处理器组件。
/// </summary>
public class CompPawnPreApplyDamageHandler : ThingComp
{
    [Unsaved] private List<IPawnPreApplyDamage> pawnPreApplyDamages;

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;

        if (parent is Pawn && !pawnPreApplyDamages.NullOrEmpty())
        {
            foreach (IPawnPreApplyDamage damageProcessor in pawnPreApplyDamages)
            {
                try
                {
                    damageProcessor.PawnPreApplyDamage(ref dinfo, out absorbed);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error in PawnPreApplyDamage: {ex}");
                }

                if (absorbed)
                {
                    dinfo.SetAmount(0f);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 注册伤害处理器。
    /// </summary>
    public bool RegisterDamageProcessor(IPawnPreApplyDamage damageProcessor)
    {
        if (pawnPreApplyDamages is null)
        {
            pawnPreApplyDamages = [damageProcessor];
            return true;
        }
        else if (!pawnPreApplyDamages.Contains(damageProcessor))
        {
            int insertIndex = 0;
            foreach (IPawnPreApplyDamage d in pawnPreApplyDamages)
            {
                if (d.Priority >= damageProcessor.Priority)
                {
                    break;
                }
                insertIndex++;
            }
            pawnPreApplyDamages.Insert(insertIndex, damageProcessor);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 注销伤害处理器。
    /// </summary>
    public bool DeregisterDamageProcessor(IPawnPreApplyDamage damageProcessor)
    {
        if (pawnPreApplyDamages is not null && pawnPreApplyDamages.Remove(damageProcessor))
        {
            if (pawnPreApplyDamages.Count == 0)
            {
                pawnPreApplyDamages = null;
            }
            return true;
        }
        return false;
    }

    private void CheckDamageProcessor()
    {
        if (pawnPreApplyDamages is null)
        {
            return;
        }

        pawnPreApplyDamages.RemoveAll(d => d is null);
    }
}