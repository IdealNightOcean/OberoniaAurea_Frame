using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public interface IPawnPreApplyDamage
{
    public int Priority { get; }
    public void PawnPreApplyDamage(ref DamageInfo dinfo, out bool absorbed);
}

public class CompProperties_PawnPreApplyDamage : CompProperties
{
    public CompProperties_PawnPreApplyDamage()
    {
        compClass = typeof(CompPawnPreApplyDamageHandler);
    }
}

public class CompPawnPreApplyDamageHandler : ThingComp
{

    [Unsaved] List<IPawnPreApplyDamage> pawnPreApplyDamages;

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
                if (d.Priority <= damageProcessor.Priority)
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