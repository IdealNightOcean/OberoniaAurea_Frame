using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 派系验证参数。
/// </summary>
public struct FactionValidationParams : IExposable
{
    /// <summary>
    /// 允许同盟关系。
    /// </summary>
    public bool AllowAlly = true;
    /// <summary>
    /// 允许中立关系。
    /// </summary>
    public bool AllowNeutral = true;
    /// <summary>
    /// 允许敌对关系。
    /// </summary>
    public bool AllyHostile = true;

    /// <summary>
    /// 允许已击败派系。
    /// </summary>
    public bool AllDefeated = false;
    /// <summary>
    /// 允许隐藏派系。
    /// </summary>
    public bool AllHidden = false;
    /// <summary>
    /// 允许临时派系。
    /// </summary>
    public bool AllTemporary = false;
    /// <summary>
    /// 允许非人派系。
    /// </summary>
    public bool AllowNonHumanlike = false;

    /// <summary>
    /// 最小科技等级。
    /// </summary>
    public TechLevel MinTechLevel = TechLevel.Undefined;
    /// <summary>
    /// 最大科技等级。
    /// </summary>
    public TechLevel MaxTechLevel = TechLevel.Undefined;

    public FactionValidationParams() { }

    public void ExposeData()
    {
        Scribe_Values.Look(ref AllowAlly, nameof(AllowAlly), defaultValue: true);
        Scribe_Values.Look(ref AllowNeutral, nameof(AllowNeutral), defaultValue: true);
        Scribe_Values.Look(ref AllyHostile, nameof(AllyHostile), defaultValue: true);

        Scribe_Values.Look(ref AllDefeated, nameof(AllDefeated), defaultValue: false);
        Scribe_Values.Look(ref AllHidden, nameof(AllHidden), defaultValue: false);
        Scribe_Values.Look(ref AllTemporary, nameof(AllTemporary), defaultValue: false);
        Scribe_Values.Look(ref AllowNonHumanlike, nameof(AllowNonHumanlike), defaultValue: false);

        Scribe_Values.Look(ref MinTechLevel, nameof(MinTechLevel), defaultValue: TechLevel.Undefined);
        Scribe_Values.Look(ref MaxTechLevel, nameof(MaxTechLevel), defaultValue: TechLevel.Undefined);
    }


    /// <summary>
    /// 获取默认派系验证参数。
    /// </summary>
    public static FactionValidationParams DefaultFaction => new();
    /// <summary>
    /// 获取盟友普通派系验证参数。
    /// </summary>
    public static FactionValidationParams AllyNormalFaction => new() { AllowNeutral = false, AllyHostile = false };
    /// <summary>
    /// 获取非敌对普通派系验证参数。
    /// </summary>
    public static FactionValidationParams NonHostileNormalFaction => new() { AllyHostile = false };
    /// <summary>
    /// 获取敌对普通派系验证参数。
    /// </summary>
    public static FactionValidationParams HostileNormalFaction => new() { AllowAlly = false, AllowNeutral = false };

    /// <summary>
    /// 验证派系是否符合条件。
    /// </summary>
    public readonly bool ValidateFaction(Faction faction)
    {
        if (faction is null || faction == Faction.OfPlayer)
        {
            return false;
        }
        if (!AllDefeated && faction.defeated)
        {
            return false;
        }
        if (!AllHidden && faction.Hidden)
        {
            return false;
        }
        if (!AllTemporary && faction.temporary)
        {
            return false;
        }
        if (!AllowNonHumanlike && !faction.def.humanlikeFaction)
        {
            return false;
        }
        if (MinTechLevel != TechLevel.Undefined && faction.def.techLevel < MinTechLevel)
        {
            return false;
        }
        if (MaxTechLevel != TechLevel.Undefined && faction.def.techLevel > MaxTechLevel)
        {
            return false;
        }

        return faction.PlayerRelationKind switch
        {
            FactionRelationKind.Ally => AllowAlly,
            FactionRelationKind.Neutral => AllowNeutral,
            FactionRelationKind.Hostile => AllyHostile,
            _ => true
        };
    }
}