using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

public class LordJob_AssistColony_NeverFlee : LordJob_AssistColony
{
    public override bool AddFleeToil => false;

    public LordJob_AssistColony_NeverFlee() : base() { }
    public LordJob_AssistColony_NeverFlee(Faction faction, IntVec3 fallbackLocation) : base(faction, fallbackLocation) { }
}
