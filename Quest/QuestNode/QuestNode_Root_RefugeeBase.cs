using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public abstract class QuestNode_Root_RefugeeBase : QuestNode
{
    protected static readonly QuestGen_Pawns.GetPawnParms FactionOpponentPawnParams = new()
    {
        mustBeWorldPawn = true,
        mustBeFactionLeader = false,
        mustBeNonHostileToPlayer = true
    };

    protected const float MidEventSelWeight_None = 0.5f;

    protected const float MidEventSelWeight_Mutiny = 0f;

    protected const float MidEventSelWeight_BetrayalOffer = 0.25f;

    protected const float RewardPostLeaveChance = 0.5f;

    protected const float RewardFactor_Postleave = 55f;

    public const float RewardFactor_BetrayalOffer = 300f;

    public const int BetrayalOfferGoodwillReward = 10;

    protected static FloatRange BetrayalOfferTimeRange = new(0.25f, 0.5f);

    protected static FloatRange MutinyTimeRange = new(0.2f, 1f);

    protected virtual IntRange QuestDurationDaysRange => new(5, 20);
    protected virtual IntRange LodgerCount => new(2, 3);

    protected virtual Faction GetOrGenerateFaction()
    {
        List<FactionRelation> list = [];
        foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
        {
            if (!item.def.permanentEnemy)
            {
                list.Add(new FactionRelation
                {
                    other = item,
                    kind = FactionRelationKind.Neutral
                });
            }
        }
        FactionGeneratorParms parms = new(FactionDefOf.OutlanderRefugee, default, true);
        if (ModsConfig.IdeologyActive)
        {
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef p) => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
        Find.FactionManager.Add(faction);
        return faction;
    }

    protected override bool TestRunInt(Slate slate)
    {
        return QuestGen_Get.GetMap() is not null;
    }

    protected virtual List<Pawn> GeneratePawns(int lodgerCount, Faction faction, Map map, Quest quest, string lodgerRecruitedSignal = null)
    {
        List<Pawn> pawns = [];
        for (int i = 0; i < lodgerCount; i++)
        {
            Pawn pawn = quest.GeneratePawn(PawnKindDefOf.Refugee, faction, allowAddictions: true, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, DevelopmentalStage.Adult, allowPregnant: true);
            pawns.Add(pawn);
            quest.PawnJoinOffer(pawn, "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")), "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")), "LetterJoinOfferText".Translate(pawn.Named("PAWN"), map.Parent.Named("MAP")), delegate
            {
                quest.JoinPlayer(map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                quest.SignalPass(null, null, lodgerRecruitedSignal);
            }, delegate
            {
                quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
            }, null, null, null, charity: true);
        }
        return pawns;
    }

    protected virtual List<Pawn> GeneratePawns(int lodgerCount, int childCount, Faction faction, Map map, Quest quest, string lodgerRecruitedSignal = null)
    {
        List<Pawn> pawns = [];
        for (int i = 0; i < lodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = (i > 0 && i >= lodgerCount - childCount) ? DevelopmentalStage.Child : DevelopmentalStage.Adult;
            Pawn pawn = quest.GeneratePawn(PawnKindDefOf.Refugee, faction, allowAddictions: true, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, developmentalStages, allowPregnant: true);
            pawns.Add(pawn);
            quest.PawnJoinOffer(pawn, "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")), "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")), "LetterJoinOfferText".Translate(pawn.Named("PAWN"), map.Parent.Named("MAP")), delegate
            {
                quest.JoinPlayer(map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                quest.SignalPass(null, null, lodgerRecruitedSignal);
            }, delegate
            {
                quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
            }, null, null, null, charity: true);
        }
        return pawns;
    }
}
