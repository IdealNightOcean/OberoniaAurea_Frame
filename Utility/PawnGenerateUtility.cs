using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame.Utility;

public static class PawnGenerateUtility
{
    public static bool TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, IsolatedPawnGroupMakerDef pawnGroupMakerDef, out PawnGroupMaker pawnGroupMaker)
    {
        if (parms.seed.HasValue)
        {
            Rand.PushState(parms.seed.Value);
        }
        bool result = pawnGroupMakerDef.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms)).TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
        if (parms.seed.HasValue)
        {
            Rand.PopState();
        }
        return result;
    }
    public static bool TryGetRandomPawnGroupMaker(PawnGroupKindDef pawnGroupKindDef, IsolatedPawnGroupMakerDef pawnGroupMakerDef, out PawnGroupMaker pawnGroupMaker)
    {
        return pawnGroupMakerDef.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == pawnGroupKindDef).TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
    }

    public static IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker pawnGroupMaker, bool needFaction = true, bool warnOnZeroResults = true)
    {
        if (parms.groupKind == null)
        {
            Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
            yield break;
        }

        if (needFaction && parms.faction == null)
        {
            Log.Error("Tried to generate pawn kinds with null faction. parms=" + parms);
            yield break;
        }

        foreach (Pawn item in pawnGroupMaker.GeneratePawns(parms, warnOnZeroResults))
        {
            yield return item;
        }
    }
}