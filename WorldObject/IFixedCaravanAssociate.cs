namespace OberoniaAurea_Frame;

public interface IFixedCaravanAssociate
{
    FixedCaravan AssociatedFixedCaravan { get; }
    string FixedCaravanName { get; }
    void PreConvertToCaravanByPlayer();
    string FixedCaravanWorkDesc();
}
