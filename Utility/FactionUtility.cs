using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 派系工具类。 
/// </summary>
[StaticConstructorOnStartup]
public static class OAFrame_FactionUtility
{
    /// <summary>
    /// 是否为鼠族派系
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinFaction(this FactionDef factionDef) => factionDef?.GetModExtension<RatkinFactionFlag>() is not null;
    /// <summary>
    /// 是否为鼠族派系
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinFaction(this Faction faction) => IsRatkinFaction(faction?.def);

    /// <summary>
    /// 是否为鼠族王国类型派系
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinKindomFaction(this FactionDef factionDef) => factionDef?.GetModExtension<FactionTagsExtension>()?.HasTag("RatkinKindom") ?? false;
    /// <summary>
    /// 是否为鼠族王国类型派系
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRatkinKindomFaction(this Faction faction) => IsRatkinKindomFaction(faction?.def);

    /// <summary>
    /// 获取符合验证参数的可用派系列表。
    /// </summary>
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

    /// <summary>
    /// 获取第一个符合验证参数的可用派系。
    /// </summary>
    public static Faction FirstAvailableFactionOf(FactionValidationParams validationParams, Predicate<Faction> predicater = null)
    {
        return GetAvailableFactionsOf(validationParams, predicater).FirstOrFallback(null);
    }

    /// <summary>
    /// 获取随机符合验证参数的可用派系。
    /// </summary>
    public static Faction RandomAvailableFactionOf(FactionValidationParams validationParams, Predicate<Faction> predicater = null)
    {
        return GetAvailableFactionsOf(validationParams, predicater).RandomElementWithFallback(null);
    }

    /// <summary>
    /// 获取指定def的可用派系列表。
    /// </summary>
    public static IEnumerable<Faction> GetAvailableFactionsOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && validationParams.ValidateFaction(f));
    }

    /// <summary>
    /// 获取第一个指定def的可用派系。
    /// </summary>
    public static Faction FirstAvailableFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableFactionsOfDef(def, validationParams).FirstOrFallback(null);
    }

    /// <summary>
    /// 获取随机指定def的可用派系。
    /// </summary>
    public static Faction RandomAvailableFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableFactionsOfDef(def, validationParams).RandomElementWithFallback(null);
    }

    /// <summary>
    /// 获取指定def的临时派系列表。
    /// </summary>
    public static IEnumerable<Faction> GetAvailableTempFactionsOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        validationParams.AllTemporary = true;
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def
                                                                        && f.temporary
                                                                        && validationParams.ValidateFaction(f));
    }

    /// <summary>
    /// 获取第一个指定def的临时派系。
    /// </summary>
    public static Faction FirstAvailableTempFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableTempFactionsOfDef(def, validationParams).FirstOrFallback(null);
    }

    /// <summary>
    /// 获取随机指定 <see cref="FactionDef"/> 的临时派系。
    /// </summary>
    public static Faction RandomAvailableTempFactionOfDef(FactionDef def, FactionValidationParams validationParams)
    {
        return GetAvailableTempFactionsOfDef(def, validationParams).RandomElementWithFallback(null);
    }

    /// <summary>
    /// 生成指定 <see cref="FactionDef"/> 和 <see href="玩家关系"/> 的临时派系。
    /// </summary>
    public static Faction GenerateTempFaction(FactionDef factionDef, FactionRelationKind relationKindWithPlayer = FactionRelationKind.Neutral)
    {
        if (factionDef is null)
            return null;

        try
        {
            List<FactionRelation> RelationList = [];
            Faction ofPlayer = Faction.OfPlayer;
            foreach (Faction otherF in Find.FactionManager.AllFactionsListForReading)
            {
                if (!otherF.def.PermanentlyHostileTo(factionDef))
                {
                    FactionRelationKind relationKind = otherF == ofPlayer ? relationKindWithPlayer : FactionRelationKind.Neutral;

                    RelationList.Add(new FactionRelation
                    {
                        other = otherF,
                        kind = relationKind
                    });
                }
            }
            FactionGeneratorParms parms = new(factionDef, default, true);
            if (ModsConfig.IdeologyActive)
            {
                parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where(p => p.proselytizes || p.approvesOfCharity).ToList());
            }
            Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, RelationList);
            faction.temporary = true;
            Find.FactionManager.Add(faction);
            return faction;
        }
        catch (Exception ex)
        {
            Log.Error($"[OAFrame] 在创建临时派系时出现异常，异常：{ex}");
            return null;
        }
    }
}
