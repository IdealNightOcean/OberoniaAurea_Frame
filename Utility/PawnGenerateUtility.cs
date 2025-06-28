using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame.Utility;

public static class OAFrame_PawnGenerateUtility
{
    public static PawnGenerationRequest CommonPawnGenerationRequest(PawnKindDef kindDef, Faction faction = null, bool forceNew = false, bool allowChild = false)
    {
        PawnGenerationRequest request = new(kindDef, faction)
        {
            Context = PawnGenerationContext.NonPlayer,
            AllowDead = false,
            AllowDowned = false,
            ForceGenerateNewPawn = forceNew,
            CanGeneratePawnRelations = false,
            RelationWithExtraPawnChanceFactor = 0f,
            AllowedDevelopmentalStages = DevelopmentalStage.Adult,
        };
        if (allowChild && Find.Storyteller.difficulty.ChildrenAllowed)
        {
            request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
        }
        return request;
    }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetRandomPawnGroupMaker(PawnGroupKindDef pawnGroupKindDef, IsolatedPawnGroupMakerDef pawnGroupMakerDef, out PawnGroupMaker pawnGroupMaker)
    {
        return pawnGroupMakerDef.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == pawnGroupKindDef).TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
    }

    public static IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker pawnGroupMaker, bool needFaction = true, bool warnOnZeroResults = true)
    {
        if (parms.groupKind is null)
        {
            Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
            yield break;
        }

        if (needFaction && parms.faction is null)
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