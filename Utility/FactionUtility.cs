﻿using RimWorld;
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

    public static List<Faction> ValidFactionsOfDef(FactionDef def, bool allowDefeated = false, bool allowTemporary = false, bool allowNonHumanlike = false)
    {
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && ValidFaction(f)).ToList();
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
    public static Faction RandomFactionOfDef(FactionDef def, bool allowDefeated = false, bool allowTemporary = false, bool allowNonHumanlike = false)
    {
        return ValidFactionsOfDef(def, allowDefeated, allowTemporary, allowNonHumanlike).RandomElementWithFallback(null);
    }

    public static List<Faction> ValidTempFactionsOfDef(FactionDef def, bool allowDefeated = false, bool allowNonHumanlike = false)
    {
        return Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && ValidFaction(f)).ToList();
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
    public static Faction RandomTempFactionOfDef(FactionDef def, bool allowDefeated = false, bool allowNonHumanlike = false)
    {
        return ValidTempFactionsOfDef(def, allowDefeated, allowNonHumanlike).RandomElementWithFallback(null);
    }

    public static Faction GenerateTempFaction(FactionDef templateDef, FactionRelationKind relationKindWithPlayer = FactionRelationKind.Neutral)
    {
        if (templateDef == null)
        {
            return null;
        }
        List<FactionRelation> RelationList = [];
        Faction ofPlayer = Faction.OfPlayer;
        foreach (Faction otherF in Find.FactionManager.AllFactionsListForReading)
        {
            if (!otherF.def.PermanentlyHostileTo(templateDef))
            {
                FactionRelationKind relationKind = otherF == ofPlayer ? relationKindWithPlayer : FactionRelationKind.Neutral;

                RelationList.Add(new FactionRelation
                {
                    other = otherF,
                    kind = relationKind
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
