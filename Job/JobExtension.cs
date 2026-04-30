using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

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

public class JobExtension : DefModExtension
{
    public JobExtensionRecord jobExtensionRecord;
}
