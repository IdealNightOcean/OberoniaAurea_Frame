using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class LordJob_AssaultThings_NeverFlee : LordJob_AssaultThings
{
    public override bool AddFleeToil => false;
    public LordJob_AssaultThings_NeverFlee() : base() { }

    public LordJob_AssaultThings_NeverFlee(Faction assaulterFaction, List<Thing> things, float damageFraction = 1f, bool useAvoidGridSmart = false) : base(assaulterFaction, things, damageFraction, useAvoidGridSmart) { }

}
