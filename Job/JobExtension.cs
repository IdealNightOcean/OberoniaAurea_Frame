using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 工作扩展记录，保存工作相关属性。
/// </summary>
public class JobExtensionRecord : IExposable
{
    public SkillDef jobSkill;
    public float skillXpPerTick;

    public int defaultWorkAmount;

    public StatDef jobStat;
    public float statFactorForTickAmount = 1f / 60f;

    public EffecterDef jobEffecter;

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
    public JobExtensionRecord jobExtensionRecord;
}
