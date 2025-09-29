using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_FactionUtility
{
    /// <summary>
    /// 是否为鼠族派系
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinFaction(this Faction faction)
    {
        return faction?.def.GetModExtension<RatkinFactionFlag>() is not null;
    }

    /// <summary>
    /// 是否为鼠族王国类型派系
    /// </summary>
    public static bool IsRatkinKindomFaction(this Faction faction)
    {
        return faction?.def.GetModExtension<FactionTagsExtension>()?.HasTag("RatkinKindom") ?? false;
    }

    public static IEnumerable<Faction> GetAvailableFactionsOf(FactionValidationParams validationParams, Predicate<Faction> predicater = null)
    {
        if (predicater is null)
        {
            return Find.FactionManager.AllFactionsListForReading.Where(validationParams.ValidateFaction);
        }
        else
        {
            return Find.FactionManager.AllFactionsListForReading.Where(f => validationParams.ValidateFaction(f) && predicater(f));
        }
    }

    public static Faction FirstAvailableFactionOf(FactionValidationParams validationParams, Predicate<Faction> predicater = null)
    {
        return GetAvailableFactionsOf(validationParams, predicater).FirstOrFallback(null);
    }

    public static Faction RandomAvailableFactionOf(FactionValidationParams validationParams, Predicate<Faction> predicater = null)
    {
        return GetAvailableFactionsOf(validationParams, predicater).RandomElementWithFallback(null);
    }

    public static IEnumerable<Faction> GetAvailableFactionsOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && validationParams.ValidateFaction(f));
    }

    public static Faction FirstAvailableFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableFactionsOfDef(def, validationParams).FirstOrFallback(null);
    }

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

    public static Faction FirstAvailableTempFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableTempFactionsOfDef(def, validationParams).FirstOrFallback(null);
    }

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
