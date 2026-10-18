using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 攻击指定物体且从不逃跑的职责。
/// </summary>
public class LordJob_AssaultThings_NeverFlee : LordJob_AssaultThings
{
    /// <inheritdoc/>
    public override bool AddFleeToil => false;
    /// <summary>
    /// 初始化攻击指定物体且从不逃跑的职责。
    /// </summary>
    public LordJob_AssaultThings_NeverFlee() : base() { }
    /// <summary>
    /// 根据派系和攻击目标初始化攻击指定物体且从不逃跑的职责。
    /// </summary>
    /// <param name="assaulterFaction">发起攻击的派系。</param>
    /// <param name="things">攻击的目标物体列表。</param>
    /// <param name="damageFraction">摧毁目标的伤害比例。</param>
    /// <param name="useAvoidGridSmart">是否使用智能规避网格。</param>
    public LordJob_AssaultThings_NeverFlee(Faction assaulterFaction, List<Thing> things, float damageFraction = 1f, bool useAvoidGridSmart = false) : base(assaulterFaction, things, damageFraction, useAvoidGridSmart) { }

}
