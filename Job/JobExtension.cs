using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 工作扩展记录，保存工作相关属性。
/// </summary>
public class JobExtensionRecord : IExposable
{
    /// <summary>
    /// 相关技能定义。
    /// </summary>
    public SkillDef jobSkill;
    /// <summary>
    /// 每刻获得的技能经验值。
    /// </summary>
    public float skillXpPerTick;

    /// <summary>
    /// 默认工作量。
    /// </summary>
    public int defaultWorkAmount;

    /// <summary>
    /// 相关工作属性。
    /// </summary>
    public StatDef jobStat;
    /// <summary>
    /// 工作量计算所用刻数的转换因子。
    /// </summary>
    public float statFactorForTickAmount = 1f / 60f;

    /// <summary>
    /// 工作效果器。
    /// </summary>
    public EffecterDef jobEffecter;

    /// <inheritdoc/>
    public void ExposeData()
    {
        Scribe_Defs.Look(ref jobSkill, nameof(jobSkill));
        Scribe_Values.Look(ref skillXpPerTick, nameof(skillXpPerTick), 0f);
        Scribe_Values.Look(ref defaultWorkAmount, nameof(defaultWorkAmount), 0);
        Scribe_Defs.Look(ref jobStat, nameof(jobStat));
        Scribe_Values.Look(ref statFactorForTickAmount, nameof(statFactorForTickAmount), 1f);
        Scribe_Defs.Look(ref jobEffecter, nameof(jobEffecter));
    }
}

/// <summary>
/// 工作定义扩展，包含工作扩展记录。
/// </summary>
public class JobExtension : DefModExtension
{
    /// <summary>
    /// 工作扩展记录。
    /// </summary>
    public JobExtensionRecord jobExtensionRecord;
}
