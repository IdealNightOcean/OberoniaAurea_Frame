using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_QuestUnique : QuestNode
{
    public enum CheckRestriction
    {
        Normal,
        UniqueAvailable,
        UniqueSuccess,
        Absolute
    }

    [NoTranslate]
    public SlateRef<string> tag;
    public SlateRef<Faction> faction;

    public SlateRef<CheckRestriction> restriction = CheckRestriction.Normal;

    [NoTranslate]
    public SlateRef<string> storeProcessedTagAs;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        string processedTag = GetProcessedTag(tag.GetValue(slate), faction.GetValue(slate));
        QuestUtility.AddQuestTag(ref QuestGen.quest.tags, processedTag);
        if (storeProcessedTagAs.GetValue(slate) is not null)
        {
            slate.Set(storeProcessedTagAs.GetValue(slate), processedTag);
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        string processedTag = GetProcessedTag(tag.GetValue(slate), faction.GetValue(slate));
        if (storeProcessedTagAs.GetValue(slate) is not null)
        {
            slate.Set(storeProcessedTagAs.GetValue(slate), processedTag);
        }

        return restriction.GetValue(slate) switch
        {
            CheckRestriction.Normal => NormalCheck(processedTag),
            CheckRestriction.UniqueAvailable => UniqueAvailableCheck(processedTag),
            CheckRestriction.UniqueSuccess => UniqueSuccessCheck(processedTag),
            CheckRestriction.Absolute => AbsoluteCheck(processedTag),
            _ => NormalCheck(processedTag),
        };
    }

    private static bool NormalCheck(string tag)
    {
        List<Quest> allQuests = Find.QuestManager.QuestsListForReading;
        for (int i = 0; i < allQuests.Count; i++)
        {
            QuestState state = allQuests[i].State;
            if (allQuests[i].State == QuestState.Ongoing && allQuests[i].tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    private static bool UniqueAvailableCheck(string tag)
    {
        List<Quest> allQuests = Find.QuestManager.QuestsListForReading;
        for (int i = 0; i < allQuests.Count; i++)
        {
            QuestState state = allQuests[i].State;
            if ((state == QuestState.NotYetAccepted || state == QuestState.Ongoing) && allQuests[i].tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    private static bool UniqueSuccessCheck(string tag)
    {
        List<Quest> allQuests = Find.QuestManager.QuestsListForReading;
        for (int i = 0; i < allQuests.Count; i++)
        {
            QuestState state = allQuests[i].State;
            if ((state == QuestState.NotYetAccepted || state == QuestState.Ongoing || state == QuestState.EndedSuccess) && allQuests[i].tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    private static bool AbsoluteCheck(string tag)
    {
        List<Quest> allQuests = Find.QuestManager.QuestsListForReading;
        for (int i = 0; i < allQuests.Count; i++)
        {
            if (allQuests[i].tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }

    private static string GetProcessedTag(string tag, Faction faction)
    {
        if (faction is null)
        {
            return tag;
        }
        return tag + "_" + faction.Name;
    }
}