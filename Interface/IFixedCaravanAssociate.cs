using RimWorld.Planet;

namespace OberoniaAurea_Frame;

public interface IFixedCaravanAssociate
{
    FixedCaravan AssociatedFixedCaravan { get; }
    string FixedCaravanName { get; }
    void PreConvertToCaravanByPlayer();
    void PostConvertToCaravan(Caravan caravan);
    string FixedCaravanWorkDesc();
}
