using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_PawnGenerateUtility
{
    public static PawnGenerationRequest CommonPawnGenerationRequest(PawnKindDef kindDef, Faction faction = null, PlanetTile? tile = null, bool forceNew = false, bool allowChild = false)
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
        if (tile.HasValue)
        {
            request.Tile = tile.Value;
        }
        if (allowChild && Find.Storyteller.difficulty.ChildrenAllowed)
        {
            request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
        }
        return request;
    }

    public static PawnGroupMaker GetRandomPawnGroupMakerOfFaction(Faction faction, PawnGroupKindDef groupKindDef, Predicate<PawnGroupMaker> predicater = null)
    {
        if (predicater is null)
        {
            return faction?.def.pawnGroupMakers?.Where(g => g.kindDef == groupKindDef).RandomElementByWeightWithFallback(g => g.commonality, fallback: null);
        }
        else
        {
            return faction?.def.pawnGroupMakers?.Where(g => g.kindDef == groupKindDef && predicater(g)).RandomElementByWeightWithFallback(g => g.commonality, fallback: null);
        }
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