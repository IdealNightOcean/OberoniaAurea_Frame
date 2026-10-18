using RimWorld;

namespace OberoniaAurea_Frame;

/// <summary>
/// 任务关联接口。
/// </summary>
public interface IQuestAssociate
{
    /// <summary>
    /// 获取关联的任务。
    /// </summary>
    Quest AssociatedQuest { get; }
    /// <summary>
    /// 设置关联任务。
    /// </summary>
    void SetAssociatedQuest(Quest quest);
}
