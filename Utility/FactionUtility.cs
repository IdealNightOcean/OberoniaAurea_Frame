using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;


[StaticConstructorOnStartup]
public static class OAFrame_FactionUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinFaction(this Faction faction)
    {
        return faction?.def.GetModExtension<RatkinFactionFlag>() is not null;
    }

    public static FactionValidationParams AllyNormalFactionParams => new() { AllowNeutral = false, AllyHostile = false };
    public static FactionValidationParams NonHostileNormalFactionParams => new() { AllyHostile = false };
    public static FactionValidationParams HostileNormalFactionParams => new() { AllowAlly = false, AllowNeutral = false };

    public static IEnumerable<Faction> GetAvailableFactionsOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && validationParams.ValidateFaction(f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Faction RandomAvailableFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableFactionsOfDef(def, validationParams).RandomElementWithFallback(null);
    }

    public static IEnumerable<Faction> GetAvailableTempFactionsOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        validationParams.AllTemporary = true;
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def
                                                                        && f.temporary
                                                                        && validationParams.ValidateFaction(f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Faction RandomAvailableTempFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableTempFactionsOfDef(def, validationParams).RandomElementWithFallback(null);
    }

    public static Faction GenerateTempFaction(FactionDef templateDef, FactionRelationKind relationKindWithPlayer = FactionRelationKind.Neutral)
    {
        if (templateDef is null)
        {
            return null;
        }
        List<FactionRelation> RelationList = [];
        Faction ofPlayer = Faction.OfPlayer;
        foreach (Faction otherF in Find.FactionManager.AllFactionsListForReading)
        {
            if (!otherF.def.PermanentlyHostileTo(templateDef))
            {
                FactionRelationKind relationKind = otherF == ofPlayer ? relationKindWithPlayer : FactionRelationKind.Neutral;

                RelationList.Add(new FactionRelation
                {
                    other = otherF,
                    kind = relationKind
                });
            }
        }
        FactionGeneratorParms parms = new(templateDef, default, true);
        if (ModsConfig.IdeologyActive)
        {
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where(p => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, RelationList);
        faction.temporary = true;
        Find.FactionManager.Add(faction);
        return faction;
    }
}
