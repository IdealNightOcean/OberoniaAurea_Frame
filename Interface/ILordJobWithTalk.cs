using Verse;

namespace OberoniaAurea_Frame;

public interface ILordJobWithTalk
{
    Pawn TalkablePawn { get; }
    bool CanTalkNow { get; }

    bool CanTalkWith(Pawn p);
    void EnableTalk(Pawn p);
    void DisableTalk();
}