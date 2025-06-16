using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public abstract class RaidStrategyWorker_ImmediateAttack_NeverFlee : RaidStrategyWorker
{
    protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        IntVec3 originCell = (parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld);
        if (parms.attackTargets is not null && parms.attackTargets.Count > 0)
        {
            return new LordJob_AssaultThings_NeverFlee(parms.faction, parms.attackTargets);
        }
        if (parms.faction.HostileTo(Faction.OfPlayer))
        {
            return new LordJob_AssaultColony_NeverFlee(parms.faction, canTimeoutOrFlee: parms.canTimeoutOrFlee, canKidnap: parms.canKidnap, sappers: false, useAvoidGridSmart: false, canSteal: parms.canSteal);
        }
        RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out IntVec3 result);
        return new LordJob_AssistColony_NeverFlee(parms.faction, result);
    }
}
