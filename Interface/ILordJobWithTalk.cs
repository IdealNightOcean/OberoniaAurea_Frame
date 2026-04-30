using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 可对话<see cref="Verse.AI.Group.LordJob"/>接口。
/// </summary>
public interface ILordJobWithTalk
{
    /// <summary>
    /// 获取可对话的<see cref="Pawn"/>。
    /// </summary>
    Pawn TalkablePawn { get; }

    /// <summary>
    /// 当前是否可以对话。
    /// </summary>
    bool CanTalkNow { get; }

    /// <summary>
    /// 检查是否可与指定<see cref="Pawn"/>对话。
    /// </summary>
    bool CanTalkWith(Pawn p);

    /// <summary>
    /// 启用对话。
    /// </summary>
    void EnableTalk(Pawn p);

    /// <summary>
    /// 禁用对话。
    /// </summary>
    void DisableTalk(bool dismiss);
}