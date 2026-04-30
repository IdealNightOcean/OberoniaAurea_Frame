using RimWorld;

namespace OberoniaAurea_Frame;

public struct FactionValidationParams
{
    public bool AllowAlly = true;
    public bool AllowNeutral = true;
    public bool AllyHostile = true;

    public bool AllDefeated = false;
    public bool AllHidden = false;
    public bool AllTemporary = false;
    public bool AllowNonHumanlike = false;

    public TechLevel MinTechLevel = TechLevel.Undefined;
    public TechLevel MaxTechLevel = TechLevel.Undefined;

    public FactionValidationParams() { }

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