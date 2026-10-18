using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：任务唯一性检查。 
/// </summary>
public class QuestNode_QuestUnique : QuestNode
{
    /// <summary>
    /// 任务唯一性检查的约束级别。
    /// </summary>
    public enum CheckRestriction
    {
        /// <summary>
        /// 只要不存在进行中的同名任务即可。
        /// </summary>
        Normal,
        /// <summary>
        /// 要求未接受或进行中的任务中都不存在同名任务。
        /// </summary>
        UniqueAvailable,
        /// <summary>
        /// 要求未接受、进行中或已成功结束的任务中都不存在同名任务。
        /// </summary>
        UniqueSuccess,
        /// <summary>
        /// 要求在任意状态的任务中都不存在同名任务。
        /// </summary>
        Absolute
    }

    /// <summary>
    /// 用于检查唯一性的任务标记。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> tag;
    /// <summary>
    /// 参与唯一性判定的派系（为空时不带派系后缀）。
    /// </summary>
    public SlateRef<Faction> faction;

    /// <summary>
    /// 唯一性检查的约束级别。
    /// </summary>
    public SlateRef<CheckRestriction> restriction = CheckRestriction.Normal;

    /// <summary>
    /// 存储标签：处理后的任务标记。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeProcessedTagAs;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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