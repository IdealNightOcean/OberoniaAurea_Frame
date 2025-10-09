using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_QuestUtility
{
    public static QuestPart_InvolvedFactions AddInvolvedFaction(Quest quest, Faction faction)
    {
        QuestPart_InvolvedFactions questPart_InvolvedFactions = quest.PartsListForReading.OfType<QuestPart_InvolvedFactions>().FirstOrFallback(null);
        if (questPart_InvolvedFactions is null)
        {
            questPart_InvolvedFactions = new();
            quest.AddPart(questPart_InvolvedFactions);
        }

        questPart_InvolvedFactions.factions.AddDistinct(faction);
        return questPart_InvolvedFactions;
    }

    public static MapParent GetAvailableMapParent(this Quest quest, MapParent originParent)
    {
        if (originParent is not null && originParent.HasMap && quest.IsParentSuitableForQuest(originParent))
        {
            return originParent;
        }
        else
        {
            MapParent mapParent = quest.TryFindNewSuitableMapParentForRetarget();
            if (mapParent is null || !mapParent.HasMap)
            {
                return null;
            }
            return mapParent;
        }
    }

    public static bool TryGenerateQuestAndMakeAvailable(out Quest quest, QuestScriptDef scriptDef, float points, bool forced = false, IIncidentTarget target = null, bool sendAvailableLetter = true)
    {
        Slate slate = new();
        slate.Set("points", points);
        return TryGenerateQuestAndMakeAvailable(out quest, scriptDef, slate, forced, target, sendAvailableLetter);
    }

    public static bool TryGenerateQuestAndMakeAvailable(out Quest quest, QuestScriptDef scriptDef, Slate slate, bool forced = false, IIncidentTarget target = null, bool sendAvailableLetter = true)
    {
        quest = null;
        if (scriptDef is null)
        {
            return false;
        }

        slate ??= new Slate();
        try
        {
            if (forced || scriptDef.CanRun(slate, target ?? Find.World))
            {
                quest = QuestUtility.GenerateQuestAndMakeAvailable(scriptDef, slate);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to generate quest {scriptDef}: {ex.Message}");
            quest = null;
            return false;
        }

        if (IsQuestAvailable(quest))
        {
            if (sendAvailableLetter)
            {
                SendLetterQuestAvailable(quest);
            }
            return true;
        }
        else
        {
            quest = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsQuestAvailable(this Quest quest)
    {
        return quest is not null && (quest.State == QuestState.NotYetAccepted || quest.State == QuestState.Ongoing);
    }

    /// <summary>
    /// 任务可用时发送信件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SendLetterQuestAvailable(this Quest quest)
    {
        if (!quest.hidden && quest.root.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
    }

    public static List<string> GetCommonPawnNegativeSiganls(bool addTag, string tagToAdd = null)
    {
        addTag = addTag && !tagToAdd.NullOrEmpty();

        List<string> processedSignals = [
                 ProcessSignal("Destroyed"),
                 ProcessSignal("Arrested"),
                 ProcessSignal("BecameMutant"),
                 ProcessSignal("SurgeryViolation"),
                 ProcessSignal("PsychicRitualTarget"),
                 ProcessSignal("Kidnapped"),
                 ProcessSignal("Banished"),
                 ProcessSignal("LeftBehind"),
             ];

        return processedSignals;

        string ProcessSignal(string signalTag)
        {
            if (addTag)
            {
                return QuestGenUtility.HardcodedSignalWithQuestID((tagToAdd + "." + signalTag));
            }
            else
            {
                return QuestGenUtility.HardcodedSignalWithQuestID(signalTag);
            }
        }
    }
}