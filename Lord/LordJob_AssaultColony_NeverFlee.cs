using RimWorld;

namespace OberoniaAurea_Frame;

public class LordJob_AssaultColony_NeverFlee : LordJob_AssaultColony
{
    public override bool AddFleeToil => false;
    public LordJob_AssaultColony_NeverFlee() : base() { }
    public LordJob_AssaultColony_NeverFlee(SpawnedPawnParams parms) : base(parms) { }
    public LordJob_AssaultColony_NeverFlee(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false) : base(assaulterFaction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal, breachers, canPickUpOpportunisticWeapons) { }
}
