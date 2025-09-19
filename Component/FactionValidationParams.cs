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

    public readonly bool ValidateFaction(Faction faction)
    {
        if (faction is null)
        {
            return false;
        }
        if (faction.defeated && !AllDefeated)
        {
            return false;
        }
        if (faction.Hidden && !AllHidden)
        {
            return false;
        }
        if (faction.temporary && !AllTemporary)
        {
            return false;
        }
        if (!faction.def.humanlikeFaction && !AllowNonHumanlike)
        {
            return false;
        }
        if (MinTechLevel != TechLevel.Undefined && faction.def.techLevel < MinTechLevel)
        {
            return false;
        }
        if (MaxTechLevel != TechLevel.Undefined && faction.def.techLevel > MinTechLevel)
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