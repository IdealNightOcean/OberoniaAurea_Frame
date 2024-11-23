using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;


[StaticConstructorOnStartup]
public static class OAFrame_FactionUtility
{

    //是否为玩家派系
    public static bool IsPlayerFaction(this Faction faction)
    {
        return faction?.def.isPlayer ?? false;
    }

    //是否为鼠族派系
    public static bool IsRatkinFaction(this Faction faction)
    {
        if (faction == null)
        {
            return false;
        }
        return faction.def.categoryTag?.Equals("RatkinStory") ?? false;
    }
    public static Faction RandomFactionOfDef(FactionDef def, bool allowDefeated = false, bool allowTemporary = false, bool allowNonHumanlike = false)
    {
        Faction faction = Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && ValidFaction(f)).RandomElementWithFallback(null);
        return faction;

        bool ValidFaction(Faction tf)
        {
            if (tf == null)
            {
                return false;
            }
            if (tf.defeated && !allowDefeated)
            {
                return false;
            }
            if (!allowNonHumanlike && !tf.def.humanlikeFaction)
            {
                return false;
            }
            if (tf.temporary && !allowTemporary)
            {
                return false;
            }
            return true;
        }
    }
    public static Faction RandomTempFactionOfDef(FactionDef def, bool allowDefeated = false, bool allowNonHumanlike = false)
    {
        Faction faction = Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && ValidFaction(f)).RandomElementWithFallback(null);
        return faction;

        bool ValidFaction(Faction tf)
        {
            if (tf == null)
            {
                return false;
            }
            if (tf.defeated && !allowDefeated)
            {
                return false;
            }
            if (!allowNonHumanlike && !tf.def.humanlikeFaction)
            {
                return false;
            }
            return tf.temporary;
        }
    }

    public static Faction GenerateTempFaction(FactionDef templateDef)
    {
        if (templateDef == null)
        {
            return null;
        }
        List<FactionRelation> RelationList = [];
        foreach (Faction otherF in Find.FactionManager.AllFactionsListForReading)
        {
            if (!otherF.def.PermanentlyHostileTo(templateDef))
            {
                RelationList.Add(new FactionRelation
                {
                    other = otherF,
                    kind = FactionRelationKind.Neutral
                });
            }
        }
        FactionGeneratorParms parms = new(templateDef, default, true);
        if (ModsConfig.IdeologyActive)
        {
            parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef, forceNoExpansionIdeo: false, DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef p) => p.proselytizes || p.approvesOfCharity).ToList());
        }
        Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, RelationList);
        faction.temporary = true;
        Find.FactionManager.Add(faction);
        return faction;
    }
}
