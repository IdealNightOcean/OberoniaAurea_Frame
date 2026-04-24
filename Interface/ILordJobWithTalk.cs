using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 可对话LordJob接口。
/// </summary>
public interface ILordJobWithTalk
{
    Pawn TalkablePawn { get; }
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