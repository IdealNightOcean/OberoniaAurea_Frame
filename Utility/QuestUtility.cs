using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_QuestUtility
{
    public static bool TryGenerateQuestAndMakeAvailable(out Quest quest, QuestScriptDef scriptDef, float points, bool forced = false, IIncidentTarget target = null, bool sendAvailableLetter = true)
    {
        Slate slate = new();
        slate.Set("points", points);
        return TryGenerateQuestAndMakeAvailable(out quest, scriptDef, slate, sendAvailableLetter);
    }

    public static bool TryGenerateQuestAndMakeAvailable(out Quest quest, QuestScriptDef scriptDef, Slate slate, bool forced = false, IIncidentTarget target = null, bool sendAvailableLetter = true)
    {
        quest = null;
        if (scriptDef is null)
        {
            return false;
        }

        slate ??= new Slate();
        target ??= Find.World;
        try
        {
            if (forced || scriptDef.CanRun(slate, target))
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
    public static bool IsQuestAvailable(Quest quest)
    {
        return quest is not null && (quest.State == QuestState.NotYetAccepted || quest.State == QuestState.Ongoing);
    }

    // 任务可用时发送信件
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SendLetterQuestAvailable(Quest quest)
    {
        if (!quest.hidden && quest.root.sendAvailableLetter)
        {
            QuestUtility.SendLetterQuestAvailable(quest);
        }
    }
}