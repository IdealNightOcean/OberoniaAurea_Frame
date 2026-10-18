using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 独立商人商队到达事件。
/// </summary>
public class IncidentWorker_IsolatedTraderCaravanArrival : IncidentWorker_NeutralGroup
{
    /// <summary>
    /// 获取用于生成商人商队的独立<see cref="Pawn"/>生成组制造器定义。
    /// </summary>
    protected virtual IsolatedPawnGroupMakerDef PawnGroupMakerDef => null;
    /// <inheritdoc/>
    protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Trader;

    /// <summary>
    /// 检查派系是否可以作为事件来源。
    /// </summary>
    public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
    {
        return !f.HostileTo(Faction.OfPlayer);
    }
    /// <inheritdoc/>
    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        parms.points = TraderCaravanUtility.GenerateGuardPoints();
    }
    /// <inheritdoc/>
    protected override bool TryResolveParmsGeneral(IncidentParms parms)
    {
        if (PawnGroupMakerDef is null)
        {
            return false;
        }
        Map map = (Map)parms.target;
        if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral))
        {
            return false;
        }
        if (!TryResolveFaction(parms))
        {
            return false;
        }

        parms.traderKind ??= PawnGroupMakerDef.traderKind;
        if (parms.traderKind is null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 尝试从参数解析事件来源派系，未指定时随机选取一个非敌对派系。
    /// </summary>
    protected virtual bool TryResolveFaction(IncidentParms parms)
    {
        parms.faction ??= Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
        return parms.faction is not null;
    }
    /// <inheritdoc/>
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (!TryResolveParms(parms))
        {
            return false;
        }
        if (parms.faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        PawnGroupMakerParms groupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true);
        groupMakerParms.tile = PlanetTile.Invalid;
        if (!PawnGroupMakerDef.TryGetRandomPawnGroupMaker(PawnGroupKindDef, out PawnGroupMaker groupMaker))
        {
            return false;
        }
        List<Pawn> pawns = SpawnTradePawns(parms, groupMakerParms, groupMaker);
        if (pawns.Count == 0)
        {
            return false;
        }
        for (int i = 0; i < pawns.Count; i++)
        {
            if (pawns[i].needs is not null && pawns[i].needs.food is not null)
            {
                pawns[i].needs.food.CurLevel = pawns[i].needs.food.MaxLevel;
            }
        }
        SendLetter(parms, pawns);
        RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0].Position, pawns[0].MapHeld, pawns[0], out IntVec3 result, delegate (IntVec3 c)
        {
            for (int k = 0; k < pawns.Count; k++)
            {
                if (!pawns[k].CanReach(c, PathEndMode.OnCell, Danger.Deadly))
                {
                    return false;
                }
            }
            return true;
        });
        LordJob_TradeWithColony lordJob = new(parms.faction, result);
        LordMaker.MakeNewLord(parms.faction, lordJob, map, pawns);
        return true;
    }
    /// <summary>
    /// 生成并放置在事件中心附近的交易商队小人。
    /// </summary>
    /// <param name="parms">事件参数。</param>
    /// <param name="groupMakerParms">用于生成小人的参数集。</param>
    /// <param name="groupMaker">小人组制造器。</param>
    /// <returns>生成的小人列表。</returns>
    protected virtual List<Pawn> SpawnTradePawns(IncidentParms parms, PawnGroupMakerParms groupMakerParms, PawnGroupMaker groupMaker)
    {
        Map map = (Map)parms.target;
        List<Pawn> pawns = Utility.OAFrame_PawnGenerateUtility.GeneratePawns(groupMakerParms, groupMaker, needFaction: false, warnOnZeroResults: false).ToList();
        foreach (Pawn pawn in pawns)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
            GenSpawn.Spawn(pawn, loc, map);
            parms.storeGeneratedNeutralPawns?.Add(pawn);
        }
        return pawns;
    }

    /// <summary>
    /// 发送商人商队到达事件的信件。
    /// </summary>
    /// <param name="parms">事件参数。</param>
    /// <param name="pawns">商队<see cref="Pawn"/>列表。</param>
    protected virtual void SendLetter(IncidentParms parms, List<Pawn> pawns)
    {
        TaggedString letterLabel = "LetterLabelTraderCaravanArrival".Translate(parms.faction.Name, parms.traderKind.label).CapitalizeFirst();
        TaggedString letterText = "LetterTraderCaravanArrival".Translate(parms.faction.NameColored, parms.traderKind.label).CapitalizeFirst();
        letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
        SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
    }

}
