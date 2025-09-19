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
        Scribe_Defs.Look(ref jobSkill, "jobSkill");
        Scribe_Values.Look(ref skillXpPerTick, "skillXpPerTick", 0f);
        Scribe_Values.Look(ref defaultWorkAmount, "defaultWorkAmount", 0);
        Scribe_Defs.Look(ref jobStat, "jobStat");
        Scribe_Values.Look(ref statFactorForTickAmount, "statFactorForTickAmount", 1f);
        Scribe_Defs.Look(ref jobEffecter, "jobEffecter");
    }
}

public class JobExtension : DefModExtension
{
    public JobExtensionRecord jobExtensionRecord;
}
