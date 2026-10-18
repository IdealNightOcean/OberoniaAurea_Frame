using RimWorld;

namespace OberoniaAurea_Frame;

/// <summary>
/// 攻击殖民地且从不逃跑的职责。
/// </summary>
public class LordJob_AssaultColony_NeverFlee : LordJob_AssaultColony
{
    /// <inheritdoc/>
    public override bool AddFleeToil => false;
    /// <summary>
    /// 初始化攻击殖民地且从不逃跑的职责。
    /// </summary>
    public LordJob_AssaultColony_NeverFlee() : base() { }
    /// <summary>
    /// 根据生成的<see cref="Verse.Pawn"/>初始化攻击殖民地且从不逃跑的职责。
    /// </summary>
    /// <param name="parms">生成的<see cref="Verse.Pawn"/>参数。</param>
    public LordJob_AssaultColony_NeverFlee(SpawnedPawnParams parms) : base(parms) { }
    /// <summary>
    /// 根据派系和攻击选项初始化攻击殖民地且从不逃跑的职责。
    /// </summary>
    /// <param name="assaulterFaction">发起攻击的派系。</param>
    /// <param name="canKidnap">是否允许绑架。</param>
    /// <param name="canTimeoutOrFlee">是否允许超时或逃跑。</param>
    /// <param name="sappers">是否为工兵攻击。</param>
    /// <param name="useAvoidGridSmart">是否使用智能规避网格。</param>
    /// <param name="canSteal">是否允许偷窃。</param>
    /// <param name="breachers">是否为破墙攻击。</param>
    /// <param name="canPickUpOpportunisticWeapons">是否允许拾取机会性武器。</param>
    public LordJob_AssaultColony_NeverFlee(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false) : base(assaulterFaction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal, breachers, canPickUpOpportunisticWeapons) { }
}
