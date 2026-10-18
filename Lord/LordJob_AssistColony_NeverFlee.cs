using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 协助殖民地且从不逃跑的职责。
/// </summary>
public class LordJob_AssistColony_NeverFlee : LordJob_AssistColony
{
    /// <inheritdoc/>
    public override bool AddFleeToil => false;

    /// <summary>
    /// 初始化协助殖民地且从不逃跑的职责。
    /// </summary>
    public LordJob_AssistColony_NeverFlee() : base() { }
    /// <summary>
    /// 根据派系和回退位置初始化协助殖民地且从不逃跑的职责。
    /// </summary>
    /// <param name="faction">协助殖民地的派系。</param>
    /// <param name="fallbackLocation">无法抵达时的回退位置。</param>
    public LordJob_AssistColony_NeverFlee(Faction faction, IntVec3 fallbackLocation) : base(faction, fallbackLocation) { }
}
