namespace OberoniaAurea_Frame;

public interface IFixedCaravanAssociate
{
    string FixedCaravanName { get; }
    void PreConvertToCaravanByPlayer(FixedCaravan fixedCaravan);
    string FixedCaravanWorkDesc();
}
