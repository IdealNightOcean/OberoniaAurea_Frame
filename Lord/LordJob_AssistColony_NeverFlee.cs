using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 协助殖民地且从不逃跑的职责。
/// </summary>
public class LordJob_AssistColony_NeverFlee : LordJob_AssistColony
{
    public override bool AddFleeToil => false;

    public LordJob_AssistColony_NeverFlee() : base() { }
    public LordJob_AssistColony_NeverFlee(Faction faction, IntVec3 fallbackLocation) : base(faction, fallbackLocation) { }
}
